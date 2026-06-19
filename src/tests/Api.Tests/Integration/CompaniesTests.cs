using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth.DTOs;
using Application.Companies.DTOs;

namespace Api.Tests.Integration;

public class CompaniesTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CompaniesTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();

        var email = $"comp-{Guid.NewGuid():N}@test.com";
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                Email = email,
                Password = "Test123!",
                Name = "Test",
            }
        );

        var body = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            auth!.Token
        );

        return client;
    }

    [Fact]
    public async Task Create_WithValidData_Returns201AndId()
    {
        var client = await CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            "/api/companies",
            new
            {
                Name = "Google",
                Website = "https://google.com",
                Notes = "Search",
            }
        );

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        Assert.NotNull(content);
        Assert.NotEqual(Guid.Empty, content["id"]);
    }

    [Fact]
    public async Task Create_WithoutName_Returns400()
    {
        var client = await CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            "/api/companies",
            new
            {
                Name = "",
                Website = "",
                Notes = "",
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsCompanies()
    {
        var client = await CreateAuthenticatedClient();

        await client.PostAsJsonAsync("/api/companies", new { Name = "Google" });
        await client.PostAsJsonAsync("/api/companies", new { Name = "Apple" });

        var response = await client.GetAsync("/api/companies");
        var companies = await response.Content.ReadFromJsonAsync<List<CompanyDto>>();

        Assert.NotNull(companies);
        Assert.Equal(2, companies.Count);
    }

    [Fact]
    public async Task GetById_ReturnsCorrectCompany()
    {
        var client = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync("/api/companies", new { Name = "Meta" });
        var createContent = await createResponse.Content.ReadFromJsonAsync<
            Dictionary<string, Guid>
        >();

        var response = await client.GetAsync($"/api/companies/{createContent!["id"]}");
        var company = await response.Content.ReadFromJsonAsync<CompanyDto>();

        Assert.NotNull(company);
        Assert.Equal("Meta", company.Name);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var client = await CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/companies/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingCompany_Returns204()
    {
        var client = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/companies",
            new { Name = "OldName" }
        );
        var createContent = await createResponse.Content.ReadFromJsonAsync<
            Dictionary<string, Guid>
        >();
        var id = createContent!["id"];

        var response = await client.PutAsJsonAsync(
            $"/api/companies/{id}",
            new
            {
                Id = id,
                Name = "NewName",
                Website = "",
                Notes = "",
            }
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_NonExistent_Returns404()
    {
        var client = await CreateAuthenticatedClient();

        var id = Guid.NewGuid();
        var response = await client.PutAsJsonAsync(
            $"/api/companies/{id}",
            new
            {
                Id = id,
                Name = "Ghost",
                Website = "",
                Notes = "",
            }
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingCompany_Returns204()
    {
        var client = await CreateAuthenticatedClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/companies",
            new { Name = "ToDelete" }
        );
        var createContent = await createResponse.Content.ReadFromJsonAsync<
            Dictionary<string, Guid>
        >();

        var response = await client.DeleteAsync($"/api/companies/{createContent!["id"]}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistent_Returns404()
    {
        var client = await CreateAuthenticatedClient();

        var response = await client.DeleteAsync($"/api/companies/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/companies");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
