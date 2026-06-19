using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth.DTOs;
using Application.JobApplications.DTOs;
using Api.Tests.Integration;

namespace Api.Tests.Integration;

public class DashboardTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DashboardTests(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, Guid CompanyId)> CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();

        var email = $"dash-{Guid.NewGuid():N}@test.com";
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

        return (client, compContent!["id"]);
    }

    private async Task<HttpClient> CreateClientWithApplications(params (string Position, string? Status)[] apps)
    {
        var (client, companyId) = await CreateAuthenticatedClient();

        foreach (var (position, status) in apps)
        {
            var createResponse = await client.PostAsJsonAsync(
                "/api/applications",
                new { CompanyId = companyId, Position = position }
            );
            createResponse.EnsureSuccessStatusCode();
            var content = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

            if (status is not null)
            {
                var statusResponse = await client.PatchAsJsonAsync(
                    $"/api/applications/{content!["id"]}/status",
                    new { Status = status }
                );
                statusResponse.EnsureSuccessStatusCode();
            }
        }

        return client;
    }

    [Fact]
    public async Task GetStats_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard/stats");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetStats_NoApplications_ReturnsEmptyStats()
    {
        var (client, _) = await CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/dashboard/stats");
        var stats = await response.Content.ReadFromJsonAsync<DashboardDto>();

        Assert.NotNull(stats);
        Assert.Equal(0, stats.TotalApplications);
        Assert.Equal(0, stats.ActiveApplications);
        Assert.Equal(0, stats.InterviewsScheduled);
        Assert.Equal(0, stats.Offers);
        Assert.Equal(0, stats.Rejected);
        Assert.Null(stats.ResponseRate);
        Assert.Null(stats.InterviewToOfferRate);
        Assert.Empty(stats.StatusBreakdown);
    }

    [Fact]
    public async Task GetStats_WithVariousStatuses_ReturnsCorrectCounts()
    {
        var client = await CreateClientWithApplications(
            ("Saved App", null),
            ("Applied App", "Applied"),
            ("Phone Screen", "PhoneScreen"),
            ("On-Site", "OnSiteInterview"),
            ("Offer", "Offer"),
            ("Accepted", "Accepted"),
            ("Rejected", "Rejected"),
            ("Withdrawn", "Withdrawn")
        );

        var response = await client.GetAsync("/api/dashboard/stats");
        var stats = await response.Content.ReadFromJsonAsync<DashboardDto>();

        Assert.NotNull(stats);
        Assert.Equal(8, stats.TotalApplications);
        Assert.Equal(5, stats.ActiveApplications);     // Saved, Applied, PhoneScreen, OnSiteInterview, Offer
        Assert.Equal(4, stats.InterviewsScheduled);     // PhoneScreen, OnSiteInterview, Offer, Accepted
        Assert.Equal(1, stats.Offers);
        Assert.Equal(1, stats.Rejected);
        Assert.Equal(8, stats.StatusBreakdown.Count);
    }

    [Fact]
    public async Task GetStats_IsScopedToUser()
    {
        var (client1, _) = await CreateAuthenticatedClient();

        var (client2, companyId2) = await CreateAuthenticatedClient();
        await client2.PostAsJsonAsync(
            "/api/applications",
            new { CompanyId = companyId2, Position = "Other's App" }
        );

        var response = await client1.GetAsync("/api/dashboard/stats");
        var stats = await response.Content.ReadFromJsonAsync<DashboardDto>();

        Assert.NotNull(stats);
        Assert.Equal(0, stats.TotalApplications);
    }
}
