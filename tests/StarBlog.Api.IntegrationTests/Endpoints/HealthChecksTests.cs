using System.Net;
using StarBlog.Api.IntegrationTests.Infrastructure;

namespace StarBlog.Api.IntegrationTests.Endpoints;

public sealed class HealthChecksTests : IClassFixture<StarBlogApiApplicationFactory> {
    private readonly HttpClient _client;

    public HealthChecksTests(StarBlogApiApplicationFactory factory) {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Live_ReturnsOk() {
        var response = await _client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Ready_ReturnsOk() {
        var response = await _client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
