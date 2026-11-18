using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Waytokaaba.API.Infrastructure;
using Waytokaaba.Application.Common;
using Waytokaaba.Infrastructure.Storage;

namespace Waytokaaba.API.Controllers;

public sealed record UploadedImageDto(string Url, int Width, int Height);

[ApiController]
[Route("api/uploads")]
[Produces("application/json")]
[Authorize(Policy = AppPolicies.Admin)]
public sealed class UploadsController(IImageStorage storage, IOptions<UploadOptions> options) : ControllerBase
{
    /// <summary>
    /// Uploads a photo (JPG, PNG or WebP, up to 10 MB). It is turned upright, scaled down to at most 1920 px and stored
    /// as WebP. Returns the absolute URL to put in an image field.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 11 * 1024 * 1024)]
    [ProducesResponseType<UploadedImageDto>(200)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<UploadedImageDto> Upload(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new RequestValidationException("file", "Please choose a photo to upload.");
        if (file.Length > options.Value.MaxBytes)
            throw new RequestValidationException("file", $"Photo is larger than {options.Value.MaxBytes / (1024 * 1024)} MB.");

        await using var stream = file.OpenReadStream();
        var stored = await storage.SaveAsync(stream, ct);

        var origin = string.IsNullOrWhiteSpace(options.Value.PublicBaseUrl)
            ? $"{Request.Scheme}://{Request.Host}"
            : options.Value.PublicBaseUrl.TrimEnd('/');
        return new UploadedImageDto($"{origin}{UploadOptions.RequestPath}/{stored.RelativePath}", stored.Width, stored.Height);
    }
}
