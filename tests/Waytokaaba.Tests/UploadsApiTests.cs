using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SkiaSharp;
using Waytokaaba.API.Controllers;

namespace Waytokaaba.Tests;

[Collection(ApiCollection.Name)]
public sealed class UploadsApiTests(ApiFactory factory)
{
    private static byte[] Png(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Goldenrod);
            canvas.DrawRect(0, 0, width / 2f, height / 2f, new SKPaint { Color = SKColors.Black });
        }
        using var image = SKImage.FromBitmap(bitmap);
        return image.Encode(SKEncodedImageFormat.Png, 100).ToArray();
    }

    private static MultipartFormDataContent Form(byte[] bytes, string fileName, string contentType)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", fileName } };
    }

    [Fact]
    public async Task Admin_upload_is_scaled_down_stored_as_webp_and_served()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsync("/api/uploads", Form(Png(3000, 2000), "big.png", "image/png"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var uploaded = (await response.Content.ReadFromJsonAsync<UploadedImageDto>(ApiFactory.Json))!;
        uploaded.Width.Should().Be(1920);
        uploaded.Height.Should().Be(1280);
        uploaded.Url.Should().Contain("/uploads/").And.EndWith(".webp");

        var served = await admin.GetAsync(new Uri(uploaded.Url).PathAndQuery);
        served.StatusCode.Should().Be(HttpStatusCode.OK);
        served.Content.Headers.ContentType!.MediaType.Should().Be("image/webp");
        served.Headers.CacheControl!.MaxAge.Should().Be(TimeSpan.FromDays(365));

        using var stored = SKBitmap.Decode(await served.Content.ReadAsByteArrayAsync());
        stored.Width.Should().Be(1920);
    }

    [Fact]
    public async Task Small_photos_are_not_enlarged()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsync("/api/uploads", Form(Png(400, 300), "small.png", "image/png"));

        var uploaded = (await response.Content.ReadFromJsonAsync<UploadedImageDto>(ApiFactory.Json))!;
        (uploaded.Width, uploaded.Height).Should().Be((400, 300));
    }

    [Fact]
    public async Task Anonymous_cannot_upload()
    {
        var response = await factory.CreateClient().PostAsync("/api/uploads", Form(Png(10, 10), "a.png", "image/png"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Non_image_file_is_rejected()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsync("/api/uploads", Form("not really a photo"u8.ToArray(), "fake.jpg", "image/jpeg"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Empty_file_is_rejected()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsync("/api/uploads", Form([], "empty.png", "image/png"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
