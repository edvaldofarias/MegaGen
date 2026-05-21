using MegaSenaHub.Application.DTOs;

namespace MegaSenaHub.Application.Abstractions;

/// <summary>
/// Abstração das operações de autenticação e gerenciamento de conta.
/// Permite que a camada Application orquestre auth sem depender de Infrastructure.
/// </summary>
public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken);
    Task<AuthResultDto?> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken);
    Task<UserProfileDto?> GetProfileAsync(string userId, CancellationToken cancellationToken);
}
