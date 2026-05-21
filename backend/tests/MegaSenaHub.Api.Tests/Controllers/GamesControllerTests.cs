using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MegaSenaHub.Api.Tests.Fixtures;

namespace MegaSenaHub.Api.Tests.Controllers;

[Collection("Api")]
public sealed class GamesControllerTests(ApiFixture fixture)
    : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task Generate_ValidRequest_Returns200WithGames()
    {
        // Arrange
        var body = new
        {
            quantity = 3,
            numbersPerGame = 6,
            strategy = "Random",
            avoidAlreadyDrawnCombination = false,
            avoidAlreadyWonCombination = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/mega-sena/games/generate", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("games").GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task Generate_ZeroQuantity_Returns400()
    {
        // Arrange
        var body = new
        {
            quantity = 0,
            numbersPerGame = 6,
            strategy = "Random",
            avoidAlreadyDrawnCombination = false,
            avoidAlreadyWonCombination = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/mega-sena/games/generate", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
