namespace Waytokaaba.Application.Common;

/// <summary>Requested resource does not exist (HTTP 404).</summary>
public sealed class NotFoundException(string resource, object key)
    : Exception($"{resource} '{key}' was not found.")
{
    public string Resource { get; } = resource;
}

/// <summary>Request conflicts with existing state, e.g. duplicate slug (HTTP 409).</summary>
public sealed class ConflictException(string message) : Exception(message);

/// <summary>Business-rule validation failure (HTTP 400).</summary>
public sealed class RequestValidationException(IDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;

    public RequestValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] }) { }
}
