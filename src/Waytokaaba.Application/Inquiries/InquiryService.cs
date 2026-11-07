using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Waytokaaba.Application.Common;
using Waytokaaba.Core.Entities;
using Waytokaaba.Core.Enums;

namespace Waytokaaba.Application.Inquiries;

public sealed class CreateInquiryRequest
{
    public string CustomerName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public InquiryType InquiryType { get; set; } = InquiryType.General;
    public int? PackageId { get; set; }
    public int? HotelId { get; set; }
    public DateOnly? CheckIn { get; set; }
    public DateOnly? CheckOut { get; set; }
    public int? Guests { get; set; }
    public string Message { get; set; } = "";

    /// <summary>Honeypot: real users never fill this hidden field.</summary>
    public string? Website { get; set; }
}

public sealed record InquiryCreatedDto(int Id, DateTimeOffset CreatedAt);

public sealed record InquiryDto(
    int Id,
    string CustomerName,
    string Email,
    string Phone,
    InquiryType InquiryType,
    int? PackageId,
    string? PackageName,
    int? HotelId,
    string? HotelName,
    City? HotelCity,
    DateOnly? CheckIn,
    DateOnly? CheckOut,
    int? Guests,
    string Message,
    InquiryStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed class InquiryQuery : PagedQuery
{
    public InquiryStatus? Status { get; set; }
    public InquiryType? Type { get; set; }
}

public sealed record UpdateInquiryStatusRequest(InquiryStatus Status);

public sealed class CreateInquiryRequestValidator : AbstractValidator<CreateInquiryRequest>
{
    public CreateInquiryRequestValidator(TimeProvider clock)
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(200).EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().Matches(ValidationRules.PhonePattern)
            .WithMessage("Please enter a valid phone number (7–20 digits, optional +).");
        RuleFor(x => x.InquiryType).IsInEnum();
        RuleFor(x => x.PackageId).NotNull().When(x => x.InquiryType == InquiryType.Package)
            .WithMessage("Please select a package.");
        RuleFor(x => x.HotelId).NotNull().When(x => x.InquiryType == InquiryType.Hotel)
            .WithMessage("Please select a hotel.");
        RuleFor(x => x.Guests).InclusiveBetween(1, 50);
        RuleFor(x => x.CheckIn)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime.AddDays(-1)))
            .WithMessage("Check-in date cannot be in the past.");
        RuleFor(x => x.CheckOut).GreaterThan(x => x.CheckIn)
            .When(x => x.CheckIn.HasValue && x.CheckOut.HasValue)
            .WithMessage("Check-out must be after check-in.");
        RuleFor(x => x.Message).NotEmpty().MinimumLength(10).MaximumLength(2000);
        RuleFor(x => x.Website).MaximumLength(200);
    }
}

public sealed class UpdateInquiryStatusRequestValidator : AbstractValidator<UpdateInquiryStatusRequest>
{
    public UpdateInquiryStatusRequestValidator() => RuleFor(x => x.Status).IsInEnum();
}

public interface IInquiryService
{
    /// <returns>The created inquiry, or null when the request was discarded as spam.</returns>
    Task<InquiryCreatedDto?> CreateAsync(CreateInquiryRequest request, CancellationToken ct = default);
    Task<PagedResult<InquiryDto>> ListAsync(InquiryQuery query, CancellationToken ct = default);
    Task<InquiryDto> UpdateStatusAsync(int id, InquiryStatus status, CancellationToken ct = default);
}

public sealed class InquiryService(IAppDbContext db) : IInquiryService
{
    public async Task<InquiryCreatedDto?> CreateAsync(CreateInquiryRequest request, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
            return null; // honeypot tripped – silently drop

        if (request.PackageId is { } packageId && !await db.Packages.AnyAsync(p => p.Id == packageId && p.IsActive, ct))
            throw new RequestValidationException("PackageId", "The selected package does not exist.");

        if (request.HotelId is { } hotelId && !await db.Hotels.AnyAsync(h => h.Id == hotelId && h.IsActive, ct))
            throw new RequestValidationException("HotelId", "The selected hotel does not exist.");

        var inquiry = new Inquiry
        {
            CustomerName = request.CustomerName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            InquiryType = request.InquiryType,
            PackageId = request.PackageId,
            HotelId = request.HotelId,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            Guests = request.Guests,
            Message = request.Message.Trim(),
            Status = InquiryStatus.New,
        };
        db.Inquiries.Add(inquiry);
        await db.SaveChangesAsync(ct);
        return new InquiryCreatedDto(inquiry.Id, inquiry.CreatedAt);
    }

    public Task<PagedResult<InquiryDto>> ListAsync(InquiryQuery query, CancellationToken ct = default)
    {
        var inquiries = db.Inquiries.AsNoTracking();
        if (query.Status is { } status) inquiries = inquiries.Where(i => i.Status == status);
        if (query.Type is { } type) inquiries = inquiries.Where(i => i.InquiryType == type);

        return inquiries
            .OrderByDescending(i => i.CreatedAt).ThenByDescending(i => i.Id)
            .Select(i => new InquiryDto(
                i.Id, i.CustomerName, i.Email, i.Phone, i.InquiryType,
                i.PackageId, i.Package != null ? i.Package.Name : null,
                i.HotelId, i.Hotel != null ? i.Hotel.Name : null, i.Hotel != null ? i.Hotel.City : null,
                i.CheckIn, i.CheckOut, i.Guests, i.Message, i.Status, i.CreatedAt, i.UpdatedAt))
            .ToPagedResultAsync(query.Page, query.PageSize, ct);
    }

    public async Task<InquiryDto> UpdateStatusAsync(int id, InquiryStatus status, CancellationToken ct = default)
    {
        var inquiry = await db.Inquiries
            .Include(i => i.Package)
            .Include(i => i.Hotel)
            .FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new NotFoundException("Inquiry", id);

        inquiry.Status = status;
        await db.SaveChangesAsync(ct);

        return new InquiryDto(
            inquiry.Id, inquiry.CustomerName, inquiry.Email, inquiry.Phone, inquiry.InquiryType,
            inquiry.PackageId, inquiry.Package?.Name, inquiry.HotelId, inquiry.Hotel?.Name, inquiry.Hotel?.City,
            inquiry.CheckIn, inquiry.CheckOut, inquiry.Guests, inquiry.Message, inquiry.Status,
            inquiry.CreatedAt, inquiry.UpdatedAt);
    }
}
