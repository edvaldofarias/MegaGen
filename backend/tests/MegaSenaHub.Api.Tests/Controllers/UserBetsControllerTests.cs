using System.Net;
using System.Net.Http.Json;
using MegaSenaHub.Api.Tests.Fixtures;

namespace MegaSenaHub.Api.Tests.Controllers;

[Collection("Api")]
public sealed class UserBetsControllerTests(ApiFixture fixture)
    : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/me/bets");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync($"/api/me/bets/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_WithoutToken_Returns401()
    {
        var body = new { contestNumber = 2800, numbers = new[] { 1, 2, 3, 4, 5, 6 }, amountPaid = 5.00m };
        var response = await _client.PostAsJsonAsync("/api/me/bets", body);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
