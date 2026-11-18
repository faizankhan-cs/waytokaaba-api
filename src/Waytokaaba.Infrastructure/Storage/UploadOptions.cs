namespace Waytokaaba.Infrastructure.Storage;

/// <summary>Settings for admin photo uploads (configuration section <c>Uploads</c>).</summary>
public sealed class UploadOptions
{
    public const string SectionName = "Uploads";

    /// <summary>
    /// Folder the photos are written to. Relative paths are resolved against the app's content root, so the default
    /// is <c>/app/uploads</c> in the Docker image. In production this must be a persistent volume.
    /// </summary>
    public string Directory { get; set; } = "uploads";

    /// <summary>
    /// Public origin of the API used to build photo URLs, e.g. <c>https://api.way2kaaba.com</c>. When empty the
    /// origin of the upload request is used, which is wrong if the website calls the API on an internal address.
    /// </summary>
    public string? PublicBaseUrl { get; set; }

    /// <summary>Largest accepted upload in bytes.</summary>
    public long MaxBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>Longest side of the stored photo in pixels; larger photos are scaled down.</summary>
    public int MaxDimension { get; set; } = 1920;

    /// <summary>URL path the photos are served under.</summary>
    public const string RequestPath = "/uploads";

    public string ResolveDirectory(string contentRoot) =>
        Path.GetFullPath(Path.IsPathRooted(Directory) ? Directory : Path.Combine(contentRoot, Directory));
}
