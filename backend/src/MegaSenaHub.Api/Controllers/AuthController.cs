using MegaSenaHub.Api.Models.Requests;
using MegaSenaHub.Api.Models.Responses;
using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaSenaHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(
    RegisterUserUseCase registerUser,
    LoginUserUseCase loginUser,
    GetCurrentUserProfileUseCase getCurrentUserProfile) : ControllerBase
{
    /// <summary>Cria uma nova conta de usuário e retorna o token JWT.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Name, request.Email, request.PhoneNumber, request.Password);

        var result = await registerUser.ExecuteAsync(command, cancellationToken);

        return Ok(new AuthResponse(
            result.AccessToken,
            result.ExpiresAt,
            new UserInfo(result.UserId, result.Name, result.Email, result.PhoneNumber)));
    }

    /// <summary>Autentica o usuário e retorna o token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request.Email, request.Password);
        var result = await loginUser.ExecuteAsync(command, cancellationToken);

        if (result is null)
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication Failed",
                Detail = "Invalid credentials.",
                Status = StatusCodes.Status401Unauthorized
            });

        return Ok(new AuthResponse(
            result.AccessToken,
            result.ExpiresAt,
            new UserInfo(result.UserId, result.Name, result.Email, result.PhoneNumber)));
    }

    /// <summary>Retorna os dados do usuário autenticado.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var profile = await getCurrentUserProfile.ExecuteAsync(cancellationToken);
        return Ok(new UserInfo(profile.UserId, profile.Name, profile.Email, profile.PhoneNumber));
    }
}

