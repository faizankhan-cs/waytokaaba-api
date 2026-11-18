namespace Waytokaaba.Application.Common;

/// <summary>A stored photo: its path under the uploads root (forward slashes) and its final size in pixels.</summary>
public sealed record StoredImage(string RelativePath, int Width, int Height);

/// <summary>Stores admin-uploaded photos (resized and re-encoded for the web).</summary>
public interface IImageStorage
{
    /// <summary>
    /// Decodes, normalises and saves an uploaded image.
    /// Throws <see cref="RequestValidationException"/> when the content is not a usable JPEG, PNG or WebP image.
    /// </summary>
    Task<StoredImage> SaveAsync(Stream content, CancellationToken ct = default);
}
