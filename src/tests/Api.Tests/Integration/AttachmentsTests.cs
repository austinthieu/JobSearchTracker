using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Attachments.Queries;
using Application.Auth.DTOs;
using Api.Tests.Integration;

namespace Api.Tests.Integration;

public class AttachmentsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AttachmentsTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid AppId)> CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();

        var email = $"att-{Guid.NewGuid():N}@test.com";
        var regResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { Email = email, Password = "Test123!", Name = "Test" }
        );
        regResponse.EnsureSuccessStatusCode();

        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", auth!.Token
        );

        var compResponse = await client.PostAsJsonAsync(
            "/api/companies",
            new { Name = $"Co-{Guid.NewGuid():N}" }
        );
        compResponse.EnsureSuccessStatusCode();
        var compContent = await compResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var appResponse = await client.PostAsJsonAsync(
            "/api/applications",
            new { CompanyId = compContent!["id"], Position = "Engineer" }
        );
        appResponse.EnsureSuccessStatusCode();
        var appContent = await appResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        return (client, appContent!["id"]);
    }

    [Fact]
    public async Task GetAttachments_Empty_ReturnsEmptyList()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/applications/{appId}/attachments");
        var attachments = await response.Content.ReadFromJsonAsync<List<AttachmentItemDto>>();

        Assert.NotNull(attachments);
        Assert.Empty(attachments);
    }

    [Fact]
    public async Task UploadAttachment_Returns201()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            $"/api/applications/{appId}/attachments",
            new
            {
                FileName = "resume_v2.pdf",
                OriginalName = "My_Resume_2026.pdf",
                ContentType = "application/pdf",
                SizeBytes = 102400L,
            }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        Assert.NotNull(content);
        Assert.NotEqual(Guid.Empty, content["attachmentId"]);
    }

    [Fact]
    public async Task UploadAttachment_ForOtherUser_Returns404()
    {
        var (client1, appId) = await CreateAuthenticatedClient();
        var (client2, _) = await CreateAuthenticatedClient();

        var response = await client2.PostAsJsonAsync(
            $"/api/applications/{appId}/attachments",
            new
            {
                FileName = "resume.pdf",
                OriginalName = "resume.pdf",
                ContentType = "application/pdf",
                SizeBytes = 50000L,
            }
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAttachments_ReturnsUploadedAttachments()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        await client.PostAsJsonAsync(
            $"/api/applications/{appId}/attachments",
            new { FileName = "a.pdf", OriginalName = "a.pdf", ContentType = "application/pdf", SizeBytes = 100L }
        );
        await client.PostAsJsonAsync(
            $"/api/applications/{appId}/attachments",
            new { FileName = "b.pdf", OriginalName = "b.pdf", ContentType = "application/pdf", SizeBytes = 200L }
        );

        var response = await client.GetAsync($"/api/applications/{appId}/attachments");
        var attachments = await response.Content.ReadFromJsonAsync<List<AttachmentItemDto>>();

        Assert.NotNull(attachments);
        Assert.Equal(2, attachments.Count);
    }

    [Fact]
    public async Task DeleteAttachment_Returns204()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            $"/api/applications/{appId}/attachments",
            new { FileName = "del.pdf", OriginalName = "del.pdf", ContentType = "application/pdf", SizeBytes = 100L }
        );
        var content = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var response = await client.DeleteAsync(
            $"/api/applications/{appId}/attachments/{content!["attachmentId"]}"
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAttachment_ForOtherUser_Returns404()
    {
        var (client1, appId) = await CreateAuthenticatedClient();

        var createResponse = await client1.PostAsJsonAsync(
            $"/api/applications/{appId}/attachments",
            new { FileName = "mine.pdf", OriginalName = "mine.pdf", ContentType = "application/pdf", SizeBytes = 100L }
        );
        var content = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var (client2, _) = await CreateAuthenticatedClient();
        var response = await client2.DeleteAsync(
            $"/api/applications/{appId}/attachments/{content!["attachmentId"]}"
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAttachment_NotFound_Returns404()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var response = await client.DeleteAsync(
            $"/api/applications/{appId}/attachments/{Guid.NewGuid()}"
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
