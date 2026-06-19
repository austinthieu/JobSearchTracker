using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth.DTOs;
using Application.JobApplications.DTOs;
using Api.Tests.Integration;

namespace Api.Tests.Integration;

public class ContactsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ContactsTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid AppId)> CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();

        var email = $"con-{Guid.NewGuid():N}@test.com";
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
    public async Task GetContacts_Empty_ReturnsEmptyList()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/applications/{appId}/contacts");
        var contacts = await response.Content.ReadFromJsonAsync<List<ContactDto>>();

        Assert.NotNull(contacts);
        Assert.Empty(contacts);
    }

    [Fact]
    public async Task CreateContact_Returns201()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            $"/api/applications/{appId}/contacts",
            new
            {
                Name = "Jane Recruiter",
                Email = "jane@company.com",
                Phone = "+1234567890",
                Title = "Technical Recruiter",
                Notes = "Referred by LinkedIn",
            }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        Assert.NotNull(content);
        Assert.NotEqual(Guid.Empty, content["contactId"]);
    }

    [Fact]
    public async Task CreateContact_ForOtherUser_Returns404()
    {
        var (client1, appId) = await CreateAuthenticatedClient();
        var (client2, _) = await CreateAuthenticatedClient();

        var response = await client2.PostAsJsonAsync(
            $"/api/applications/{appId}/contacts",
            new { Name = "Sneaky Contact" }
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetContacts_ReturnsCreatedContacts()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        await client.PostAsJsonAsync(
            $"/api/applications/{appId}/contacts",
            new { Name = "Alice" }
        );
        await client.PostAsJsonAsync(
            $"/api/applications/{appId}/contacts",
            new { Name = "Bob" }
        );

        var response = await client.GetAsync($"/api/applications/{appId}/contacts");
        var contacts = await response.Content.ReadFromJsonAsync<List<ContactDto>>();

        Assert.NotNull(contacts);
        Assert.Equal(2, contacts.Count);
    }

    [Fact]
    public async Task DeleteContact_Returns204()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            $"/api/applications/{appId}/contacts",
            new { Name = "Delete Me" }
        );
        var content = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var response = await client.DeleteAsync(
            $"/api/applications/{appId}/contacts/{content!["contactId"]}"
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteContact_ForOtherUser_Returns404()
    {
        var (client1, appId) = await CreateAuthenticatedClient();

        var createResponse = await client1.PostAsJsonAsync(
            $"/api/applications/{appId}/contacts",
            new { Name = "Mine" }
        );
        var content = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var (client2, _) = await CreateAuthenticatedClient();
        var response = await client2.DeleteAsync(
            $"/api/applications/{appId}/contacts/{content!["contactId"]}"
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteContact_NotFound_Returns404()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var response = await client.DeleteAsync(
            $"/api/applications/{appId}/contacts/{Guid.NewGuid()}"
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
