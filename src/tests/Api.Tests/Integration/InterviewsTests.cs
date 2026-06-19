using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth.DTOs;
using Application.JobApplications.DTOs;
using Api.Tests.Integration;

namespace Api.Tests.Integration;

public class InterviewsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public InterviewsTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid AppId)> CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();

        var email = $"int-{Guid.NewGuid():N}@test.com";
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
    public async Task GetInterviews_Empty_ReturnsEmptyList()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/applications/{appId}/interviews");
        var interviews = await response.Content.ReadFromJsonAsync<List<InterviewDto>>();

        Assert.NotNull(interviews);
        Assert.Empty(interviews);
    }

    [Fact]
    public async Task CreateInterview_Returns201()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            $"/api/applications/{appId}/interviews",
            new
            {
                Type = "Phone",
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                DurationMinutes = 30,
                Location = "Zoom",
                Interviewers = "Alice",
                Notes = "Technical screen",
            }
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        Assert.NotNull(content);
        Assert.NotEqual(Guid.Empty, content["interviewId"]);
    }

    [Fact]
    public async Task CreateInterview_InvalidType_Returns400()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            $"/api/applications/{appId}/interviews",
            new
            {
                Type = "NotARealType",
                ScheduledAt = DateTime.UtcNow.AddDays(1),
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateInterview_ForOtherUser_Returns404()
    {
        var (client1, appId) = await CreateAuthenticatedClient();
        var (client2, _) = await CreateAuthenticatedClient();

        var response = await client2.PostAsJsonAsync(
            $"/api/applications/{appId}/interviews",
            new
            {
                Type = "Phone",
                ScheduledAt = DateTime.UtcNow.AddDays(1),
            }
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetInterviews_ReturnsCreatedInterviews()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        await client.PostAsJsonAsync(
            $"/api/applications/{appId}/interviews",
            new { Type = "Phone", ScheduledAt = DateTime.UtcNow.AddDays(1) }
        );
        await client.PostAsJsonAsync(
            $"/api/applications/{appId}/interviews",
            new { Type = "Video", ScheduledAt = DateTime.UtcNow.AddDays(2) }
        );

        var response = await client.GetAsync($"/api/applications/{appId}/interviews");
        var interviews = await response.Content.ReadFromJsonAsync<List<InterviewDto>>();

        Assert.NotNull(interviews);
        Assert.Equal(2, interviews.Count);
    }

    [Fact]
    public async Task RescheduleInterview_Returns204()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            $"/api/applications/{appId}/interviews",
            new { Type = "Video", ScheduledAt = DateTime.UtcNow.AddDays(1) }
        );
        var content = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var interviewId = content!["interviewId"];

        var newDate = DateTime.UtcNow.AddDays(3);
        var response = await client.PatchAsJsonAsync(
            $"/api/applications/{appId}/interviews/{interviewId}/reschedule",
            new { NewScheduledAt = newDate }
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RescheduleInterview_ForOtherUser_Returns404()
    {
        var (client1, appId) = await CreateAuthenticatedClient();

        var createResponse = await client1.PostAsJsonAsync(
            $"/api/applications/{appId}/interviews",
            new { Type = "Video", ScheduledAt = DateTime.UtcNow.AddDays(1) }
        );
        var content = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var (client2, _) = await CreateAuthenticatedClient();
        var response = await client2.PatchAsJsonAsync(
            $"/api/applications/{appId}/interviews/{content!["interviewId"]}/reschedule",
            new { NewScheduledAt = DateTime.UtcNow.AddDays(5) }
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkFollowUpSent_Returns204()
    {
        var (client, appId) = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            $"/api/applications/{appId}/interviews",
            new { Type = "InPerson", ScheduledAt = DateTime.UtcNow.AddDays(1) }
        );
        var content = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var response = await client.PostAsync(
            $"/api/applications/{appId}/interviews/{content!["interviewId"]}/follow-up",
            null
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
