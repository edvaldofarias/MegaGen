using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MegaSenaHub.Api.Tests.Fixtures;

namespace MegaSenaHub.Api.Tests.Controllers;

[Collection("Api")]
public sealed class AuthControllerTests(ApiFixture fixture)
    : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task Register_ValidData_Returns200WithToken()
    {
        // Arrange
        var request = new
        {
            name = "Teste Usuario",
            email = $"test_{Guid.NewGuid():N}@example.com",
            phoneNumber = "11999999999",
            password = "Password1!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        // Arrange
        var request = new { email = "nobody@example.com", password = "WrongPass1!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        // Arrange
        var email = $"dup_{Guid.NewGuid():N}@example.com";
        var request = new
        {
            name = "Duplicado",
            email,
            phoneNumber = "11888888888",
            password = "Password1!"
        };

        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act — segunda tentativa com mesmo e-mail
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
