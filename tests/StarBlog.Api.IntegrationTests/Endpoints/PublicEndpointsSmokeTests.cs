using System.Net;
using StarBlog.Api.IntegrationTests.Infrastructure;
using StarBlog.Testing;

namespace StarBlog.Api.IntegrationTests.Endpoints;

public sealed class PublicEndpointsSmokeTests : IClassFixture<StarBlogApiApplicationFactory>, IAsyncLifetime {
    private readonly StarBlogApiApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicEndpointsSmokeTests(StarBlogApiApplicationFactory factory) {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() {
        await TestDatabaseSeeder.EnsureEfCoreDatabaseAsync(_factory.Services);
        await TestDatabaseSeeder.SeedMinimalBlogDataAsync(_factory.Services);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task BlogPostList_ReturnsOk() {
        var response = await _client.GetAsync("/Api/BlogPost?page=1&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CategoryNodes_ReturnsOk() {
        var response = await _client.GetAsync("/Api/Category/Nodes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Theme_ReturnsOk() {
        var response = await _client.GetAsync("/Api/Theme");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PublicLinks_ReturnsOk() {
        var response = await _client.GetAsync("/Api/Link/Public");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
