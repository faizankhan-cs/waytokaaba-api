using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Waytokaaba.Application.Auth;
using Waytokaaba.Application.Common;

namespace Waytokaaba.API.Infrastructure;

/// <summary>Maps exceptions to RFC 7807 ProblemDetails responses. Never leaks internals in production.</summary>
public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem = exception switch
        {
            ValidationException ve => Validation(ve.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => ToCamel(g.Key), g => g.Select(e => e.ErrorMessage).Distinct().ToArray())),
            RequestValidationException rve => Validation(rve.Errors.ToDictionary(kv => ToCamel(kv.Key), kv => kv.Value)),
            NotFoundException nf => new ProblemDetails { Status = 404, Title = "Not found", Detail = nf.Message },
            ConflictException c => new ProblemDetails { Status = 409, Title = "Conflict", Detail = c.Message },
            UnauthorizedException u => new ProblemDetails { Status = 401, Title = "Unauthorized", Detail = u.Message },
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } =>
                new ProblemDetails { Status = 409, Title = "Conflict", Detail = "A record with the same unique value already exists." },
            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } } =>
                new ProblemDetails { Status = 409, Title = "Conflict", Detail = "The operation conflicts with related records." },
            BadHttpRequestException bad => new ProblemDetails { Status = bad.StatusCode, Title = "Bad request" },
            OperationCanceledException when cancellationToken.IsCancellationRequested => new ProblemDetails { Status = 499, Title = "Client closed request" },
            _ => new ProblemDetails
            {
                Status = 500,
                Title = "An unexpected error occurred.",
                Detail = "Please try again later. If the problem persists, contact support.",
            },
        };

        if (problem.Status >= 500)
            LogUnhandled(logger, exception, httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = problem.Status ?? 500;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static ValidationProblemDetails Validation(IDictionary<string, string[]> errors) =>
        new(errors) { Status = 400, Title = "One or more validation errors occurred." };

    private static string ToCamel(string name) =>
        string.Join('.', name.Split('.').Select(p => p.Length == 0 ? p : char.ToLowerInvariant(p[0]) + p[1..]));

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception for {Method} {Path}")]
    private static partial void LogUnhandled(ILogger logger, Exception ex, string method, string path);
}
