using System.Net;
using StarBlog.Api.IntegrationTests.Infrastructure;

namespace StarBlog.Api.IntegrationTests.Endpoints;

public sealed class SwaggerTests : IClassFixture<StarBlogApiApplicationFactory> {
    private readonly HttpClient _client;

    public SwaggerTests(StarBlogApiApplicationFactory factory) {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task BlogSwaggerJson_ReturnsOk() {
        var response = await _client.GetAsync("/swagger/blog/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
