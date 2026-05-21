using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace MegaSenaHub.Infrastructure.Identity;

/// <summary>
/// Implementa IAuthService usando ASP.NET Core Identity + JwtTokenService.
/// Mantém toda a dependência de Identity confinada à camada Infrastructure.
/// </summary>
internal sealed class IdentityAuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    JwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthResultDto> RegisterAsync(
        RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            Name = command.Name,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new DomainException(errors);
        }

        await userManager.AddToRoleAsync(user, "User");

        var (accessToken, expiresAt) = await jwtTokenService.GenerateTokenAsync(user, cancellationToken);

        return new AuthResultDto(
            accessToken,
            expiresAt,
            user.Id,
            user.Name,
            user.Email!,
            user.PhoneNumber);
    }

    public async Task<AuthResultDto?> LoginAsync(
        LoginUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return null;

        var result = await signInManager.CheckPasswordSignInAsync(
            user, command.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
            return null;

        var (accessToken, expiresAt) = await jwtTokenService.GenerateTokenAsync(user, cancellationToken);

        return new AuthResultDto(
            accessToken,
            expiresAt,
            user.Id,
            user.Name,
            user.Email!,
            user.PhoneNumber);
    }

    public async Task<UserProfileDto?> GetProfileAsync(
        string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return null;

        return new UserProfileDto(user.Id, user.Name, user.Email!, user.PhoneNumber);
    }
}
