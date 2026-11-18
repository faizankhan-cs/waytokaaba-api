using Microsoft.Extensions.Options;
using SkiaSharp;
using Waytokaaba.Application.Common;

namespace Waytokaaba.Infrastructure.Storage;

/// <summary>
/// Saves uploaded photos to a local folder (a persistent volume in production). Every photo is decoded, turned upright
/// using its EXIF orientation, scaled down to <see cref="UploadOptions.MaxDimension"/> and re-encoded as WebP, so phone
/// photos of several MB become a few hundred KB and nothing but a clean image is ever stored or served.
/// </summary>
public sealed class LocalImageStorage(IOptions<UploadOptions> options, string rootDirectory) : IImageStorage
{
    private const long MaxPixels = 40_000_000;
    private const int WebpQuality = 80;

    public async Task<StoredImage> SaveAsync(Stream content, CancellationToken ct = default)
    {
        // SKCodec needs a seekable stream; uploads are capped at a few MB, so buffering in memory is fine.
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        using var data = SKData.Create(buffer);
        using var codec = data is null ? null : SKCodec.Create(data);
        if (codec is null || codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp))
            throw Invalid("Please upload a JPG, PNG or WebP photo.");

        var info = codec.Info;
        if ((long)info.Width * info.Height > MaxPixels)
            throw Invalid("This photo is too large. Please use a photo under 40 megapixels.");

        using var decoded = SKBitmap.Decode(codec);
        if (decoded is null) throw Invalid("This photo could not be read. Please try another one.");

        using var upright = ApplyOrientation(decoded, codec.EncodedOrigin);
        using var resized = Resize(upright, options.Value.MaxDimension);
        using var image = SKImage.FromBitmap(resized);
        using var webp = image.Encode(SKEncodedImageFormat.Webp, WebpQuality)
            ?? throw new InvalidOperationException("WebP encoding failed.");

        var now = DateTime.UtcNow;
        var relative = $"{now:yyyy}/{now:MM}/{Guid.NewGuid():N}.webp";
        var fullPath = Path.Combine(rootDirectory, relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using (var file = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await file.WriteAsync(webp.ToArray(), ct);
        }

        return new StoredImage(relative, resized.Width, resized.Height);
    }

    private static RequestValidationException Invalid(string message) => new("file", message);

    private static SKBitmap Resize(SKBitmap source, int maxDimension)
    {
        var longest = Math.Max(source.Width, source.Height);
        if (longest <= maxDimension) return source.Copy();

        var scale = (double)maxDimension / longest;
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));
        return source.Resize(new SKImageInfo(width, height, source.ColorType, source.AlphaType), new SKSamplingOptions(SKCubicResampler.Mitchell))
            ?? throw new InvalidOperationException("Resizing the photo failed.");
    }

    /// <summary>Returns a copy drawn the right way up for the camera's EXIF orientation.</summary>
    private static SKBitmap ApplyOrientation(SKBitmap source, SKEncodedOrigin origin)
    {
        if (origin is SKEncodedOrigin.TopLeft) return source.Copy();

        var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var (w, h) = swap ? (source.Height, source.Width) : (source.Width, source.Height);
        var result = new SKBitmap(new SKImageInfo(w, h, source.ColorType, source.AlphaType));

        using var canvas = new SKCanvas(result);
        switch (origin)
        {
            case SKEncodedOrigin.TopRight: canvas.Scale(-1, 1, w / 2f, 0); break;
            case SKEncodedOrigin.BottomRight: canvas.RotateDegrees(180, w / 2f, h / 2f); break;
            case SKEncodedOrigin.BottomLeft: canvas.Scale(1, -1, 0, h / 2f); break;
            // Transpose: (x, y) -> (y, x)
            case SKEncodedOrigin.LeftTop: canvas.RotateDegrees(90); canvas.Scale(1, -1); break;
            // 90° clockwise: (x, y) -> (w - y, x)
            case SKEncodedOrigin.RightTop: canvas.Translate(w, 0); canvas.RotateDegrees(90); break;
            // Transverse: (x, y) -> (w - y, h - x)
            case SKEncodedOrigin.RightBottom: canvas.Translate(w, h); canvas.RotateDegrees(-90); canvas.Scale(1, -1); break;
            // 90° anticlockwise: (x, y) -> (y, h - x)
            case SKEncodedOrigin.LeftBottom: canvas.Translate(0, h); canvas.RotateDegrees(-90); break;
        }
        canvas.DrawBitmap(source, 0, 0);
        return result;
    }
}
