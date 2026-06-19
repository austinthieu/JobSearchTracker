using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth.DTOs;
using Application.JobApplications.DTOs;
using Api.Tests.Integration;

namespace Api.Tests.Integration;

public class ApplicationsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ApplicationsTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid CompanyId)> CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();

        var email = $"app-{Guid.NewGuid():N}@test.com";
        var regResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { Email = email, Password = "Test123!", Name = "Test" }
        );
        regResponse.EnsureSuccessStatusCode();

        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", auth!.Token
        );

        var compResponse = await client.PostAsJsonAsync("/api/companies", new { Name = "Acme Corp" });
        compResponse.EnsureSuccessStatusCode();
        var compContent = await compResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        return (client, compContent!["id"]);
    }

    [Fact]
    public async Task Create_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/applications",
            new { CompanyId = Guid.NewGuid(), Position = "Engineer" }
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidData_Returns201()
    {
        var (client, companyId) = await CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            "/api/applications",
            new
            {
                CompanyId = companyId,
                Position = "Senior Engineer",
                JobUrl = "https://example.com/job",
                Source = "LinkedIn",
                SalaryMin = 100000m,
                SalaryMax = 150000m,
            }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        Assert.NotNull(content);
        Assert.NotEqual(Guid.Empty, content["id"]);
    }

    [Fact]
    public async Task GetAll_ReturnsApplications()
    {
        var (client, companyId) = await CreateAuthenticatedClient();

        await client.PostAsJsonAsync("/api/applications", new { CompanyId = companyId, Position = "Engineer I" });
        await client.PostAsJsonAsync("/api/applications", new { CompanyId = companyId, Position = "Engineer II" });

        var response = await client.GetAsync("/api/applications");
        var apps = await response.Content.ReadFromJsonAsync<List<ApplicationDto>>();

        Assert.NotNull(apps);
        Assert.Equal(2, apps.Count);
    }

    [Fact]
    public async Task GetAll_IsScopedToUser()
    {
        var (client1, companyId1) = await CreateAuthenticatedClient();
        await client1.PostAsJsonAsync("/api/applications", new { CompanyId = companyId1, Position = "My App" });

        var (client2, companyId2) = await CreateAuthenticatedClient();
        await client2.PostAsJsonAsync("/api/applications", new { CompanyId = companyId2, Position = "Other App" });

        var response = await client1.GetAsync("/api/applications");
        var apps = await response.Content.ReadFromJsonAsync<List<ApplicationDto>>();

        Assert.NotNull(apps);
        Assert.Single(apps);
        Assert.Equal("My App", apps[0].Position);
    }

    [Fact]
    public async Task GetById_ReturnsApplication()
    {
        var (client, companyId) = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/applications",
            new { CompanyId = companyId, Position = "Data Scientist" }
        );
        var createContent = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var response = await client.GetAsync($"/api/applications/{createContent!["id"]}");
        var app = await response.Content.ReadFromJsonAsync<ApplicationDto>();

        Assert.NotNull(app);
        Assert.Equal("Data Scientist", app.Position);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var (client, _) = await CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/applications/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ValidStatus_Returns204()
    {
        var (client, companyId) = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/applications",
            new { CompanyId = companyId, Position = "Dev" }
        );
        var createContent = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var appId = createContent!["id"];

        var response = await client.PatchAsJsonAsync(
            $"/api/applications/{appId}/status",
            new { Status = "Applied" }
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_InvalidStatus_Returns400()
    {
        var (client, companyId) = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/applications",
            new { CompanyId = companyId, Position = "Dev" }
        );
        var createContent = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var response = await client.PatchAsJsonAsync(
            $"/api/applications/{createContent!["id"]}/status",
            new { Status = "NotARealStatus" }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ForOtherUser_Returns404()
    {
        var (client, companyId) = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/applications",
            new { CompanyId = companyId, Position = "Dev" }
        );
        var createContent = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var (otherClient, _) = await CreateAuthenticatedClient();

        var response = await otherClient.PatchAsJsonAsync(
            $"/api/applications/{createContent!["id"]}/status",
            new { Status = "Applied" }
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddNote_Returns201()
    {
        var (client, companyId) = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/applications",
            new { CompanyId = companyId, Position = "Dev" }
        );
        var createContent = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var response = await client.PostAsJsonAsync(
            $"/api/applications/{createContent!["id"]}/notes",
            new { Content = "Followed up with recruiter" }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        Assert.NotNull(content);
        Assert.NotEqual(Guid.Empty, content["noteId"]);
    }

    [Fact]
    public async Task Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/applications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Detail endpoint ─────────────────────────────────────

    [Fact]
    public async Task GetDetail_ReturnsFullDetail()
    {
        var (client, companyId) = await CreateAuthenticatedClient();

        // Create application
        var createResponse = await client.PostAsJsonAsync(
            "/api/applications",
            new
            {
                CompanyId = companyId,
                Position = "Senior Engineer",
                Source = "LinkedIn",
                SalaryMin = 100000m,
                SalaryMax = 150000m,
            }
        );
        var createContent = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var appId = createContent!["id"];

        // Add a note
        await client.PostAsJsonAsync(
            $"/api/applications/{appId}/notes",
            new { Content = "Spoke with recruiter" }
        );

        // Update status to create history
        await client.PatchAsJsonAsync(
            $"/api/applications/{appId}/status",
            new { Status = "Applied" }
        );

        // Create an interview
        await client.PostAsJsonAsync(
            $"/api/applications/{appId}/interviews",
            new
            {
                Type = "Phone",
                ScheduledAt = DateTime.UtcNow.AddDays(7),
                DurationMinutes = 30,
                Location = "Zoom",
            }
        );

        // Create a contact
        await client.PostAsJsonAsync(
            $"/api/applications/{appId}/contacts",
            new { Name = "Jane Doe", Email = "jane@acme.com", Title = "Recruiter" }
        );

        // Fetch detail
        var response = await client.GetAsync($"/api/applications/{appId}/detail");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var detail = await response.Content.ReadFromJsonAsync<ApplicationDetailDto>();
        Assert.NotNull(detail);
        Assert.Equal("Senior Engineer", detail.Position);
        Assert.Equal("LinkedIn", detail.Source);
        Assert.Equal("Acme Corp", detail.CompanyName);

        Assert.Single(detail.Notes);
        Assert.Equal("Spoke with recruiter", detail.Notes[0].Content);

        Assert.NotEmpty(detail.StatusHistory);
        Assert.Equal("Saved", detail.StatusHistory[0].FromStatus);

        Assert.Single(detail.Interviews);
        Assert.Equal("Phone", detail.Interviews[0].Type);

        Assert.Single(detail.Contacts);
        Assert.Equal("Jane Doe", detail.Contacts[0].Name);
    }

    [Fact]
    public async Task GetDetail_NotFound_Returns404()
    {
        var (client, _) = await CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/applications/{Guid.NewGuid()}/detail");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDetail_ForOtherUser_Returns404()
    {
        var (client, companyId) = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/applications",
            new { CompanyId = companyId, Position = "My App" }
        );
        var createContent = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var (otherClient, _) = await CreateAuthenticatedClient();

        var response = await otherClient.GetAsync($"/api/applications/{createContent!["id"]}/detail");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetDetail_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/applications/{Guid.NewGuid()}/detail");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
