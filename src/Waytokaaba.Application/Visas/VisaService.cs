using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Waytokaaba.Application.Common;
using Waytokaaba.Application.Hotels;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Application.Visas;

public interface IVisaService
{
    Task<PagedResult<VisaSummaryDto>> SearchAsync(VisaQuery query, bool adminView = false, CancellationToken ct = default);
    Task<VisaDetailDto> GetAsync(string idOrSlug, bool adminView = false, CancellationToken ct = default);
    Task<VisaDetailDto> CreateAsync(UpsertVisaRequest request, CancellationToken ct = default);
    Task<VisaDetailDto> UpdateAsync(int id, UpsertVisaRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public sealed class VisaService(IAppDbContext db) : IVisaService
{
    private const string Currency = "PKR";

    public async Task<PagedResult<VisaSummaryDto>> SearchAsync(VisaQuery query, bool adminView = false, CancellationToken ct = default)
    {
        var visas = db.Visas.AsNoTracking();

        if (!adminView)
            visas = visas.Where(v => v.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var pattern = $"%{HotelService.EscapeLike(query.Q.Trim().ToLowerInvariant())}%";
            visas = visas.Where(v => EF.Functions.Like(v.Country.ToLower(), pattern, "\\"));
        }

        if (query.Format is { Length: > 0 } formats) visas = visas.Where(v => formats.Contains(v.VisaFormat));
        if (query.Featured is { } featured) visas = visas.Where(v => v.IsFeatured == featured);

        return await visas
            .OrderBy(v => v.SortOrder).ThenBy(v => v.Country).ThenBy(v => v.Id)
            .Select(v => new VisaSummaryDto(
                v.Id, v.Slug, v.Country, v.CountryCode, v.ImageUrl, v.VisaFormat, v.Summary, v.ProcessingTime,
                v.Tiers.Min(t => (decimal?)t.Price), Currency, v.IsFeatured, v.IsActive, v.IsDemoContent))
            .ToPagedResultAsync(query.Page, query.PageSize, ct);
    }

    public async Task<VisaDetailDto> GetAsync(string idOrSlug, bool adminView = false, CancellationToken ct = default)
    {
        IQueryable<Visa> visas = db.Visas.AsNoTracking().Include(v => v.Tiers);

        if (!adminView)
            visas = visas.Where(v => v.IsActive);

        var slug = idOrSlug.ToLowerInvariant();

        var visa = int.TryParse(idOrSlug, out var id)
            ? await visas.FirstOrDefaultAsync(v => v.Id == id, ct)
            : await visas.FirstOrDefaultAsync(v => v.Slug == slug, ct);

        return visa is null ? throw new NotFoundException("Visa", idOrSlug) : ToDetail(visa);
    }

    public async Task<VisaDetailDto> CreateAsync(UpsertVisaRequest request, CancellationToken ct = default)
    {
        var slug = await SlugHelper.ResolveUniqueAsync(request.Slug, $"{request.Country} visa", "visa",
            s => db.Visas.AnyAsync(v => v.Slug == s, ct));

        var visa = new Visa
        {
            Slug = slug,
            Country = request.Country,
            CountryCode = request.CountryCode,
            ImageUrl = request.ImageUrl,
            Summary = request.Summary,
            ProcessingTime = request.ProcessingTime,
            Validity = request.Validity,
            AllowedStay = request.AllowedStay,
        };
        Apply(visa, request);
        db.Visas.Add(visa);
        await db.SaveChangesAsync(ct);
        return await GetAsync(visa.Id.ToString(CultureInfo.InvariantCulture), adminView: true, ct);
    }

    public async Task<VisaDetailDto> UpdateAsync(int id, UpsertVisaRequest request, CancellationToken ct = default)
    {
        var visa = await db.Visas.Include(v => v.Tiers).FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new NotFoundException("Visa", id);

        visa.Slug = await SlugHelper.ResolveUniqueAsync(request.Slug ?? visa.Slug, $"{request.Country} visa", "visa",
            s => db.Visas.AnyAsync(v => v.Slug == s && v.Id != id, ct));

        db.VisaTiers.RemoveRange(visa.Tiers);
        visa.Tiers.Clear();

        Apply(visa, request);
        await db.SaveChangesAsync(ct);
        return await GetAsync(id.ToString(CultureInfo.InvariantCulture), adminView: true, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var visa = await db.Visas.FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new NotFoundException("Visa", id);
        db.Visas.Remove(visa);
        await db.SaveChangesAsync(ct);
    }

    private static void Apply(Visa v, UpsertVisaRequest r)
    {
        v.Country = r.Country.Trim();
        v.CountryCode = r.CountryCode.Trim().ToUpperInvariant();
        v.ImageUrl = r.ImageUrl.Trim();
        v.VisaFormat = r.VisaFormat;
        v.Summary = r.Summary.Trim();
        v.ProcessingTime = r.ProcessingTime.Trim();
        v.Validity = r.Validity.Trim();
        v.AllowedStay = r.AllowedStay.Trim();
        v.EntryType = r.EntryType;
        v.IncludesEmbassyFee = r.IncludesEmbassyFee;
        v.IncludesInsurance = r.IncludesInsurance;
        v.IncludesServiceCharge = r.IncludesServiceCharge;
        v.RequiredDocuments = r.RequiredDocuments.Select(d => d.Trim()).Where(d => d.Length > 0).ToList();
        v.Disclaimer = string.IsNullOrWhiteSpace(r.Disclaimer) ? null : r.Disclaimer.Trim();
        v.SortOrder = r.SortOrder;
        v.IsFeatured = r.IsFeatured;
        v.IsActive = r.IsActive;
        v.IsDemoContent = r.IsDemoContent;
        v.Tiers = r.Tiers
            .Select((t, i) => new VisaTier { Name = t.Name.Trim(), Description = t.Description?.Trim(), Price = t.Price, SortOrder = i })
            .ToList();
    }

    private static VisaDetailDto ToDetail(Visa v) => new(
        v.Id, v.Slug, v.Country, v.CountryCode, v.ImageUrl, v.VisaFormat, v.Summary, v.ProcessingTime,
        v.Validity, v.AllowedStay, v.EntryType, v.IncludesEmbassyFee, v.IncludesInsurance, v.IncludesServiceCharge,
        v.RequiredDocuments, v.Disclaimer,
        v.Tiers.Count == 0 ? null : v.Tiers.Min(t => t.Price), Currency,
        v.SortOrder, v.IsFeatured, v.IsActive, v.IsDemoContent,
        v.Tiers.OrderBy(t => t.SortOrder).Select(t => new VisaTierDto(t.Id, t.Name, t.Description, t.Price, t.SortOrder)).ToList(),
        v.CreatedAt, v.UpdatedAt);
}
