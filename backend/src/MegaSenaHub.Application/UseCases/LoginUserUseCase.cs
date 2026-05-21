using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;

namespace MegaSenaHub.Application.UseCases;

/// <summary>Autentica um usuário existente e retorna o token JWT.</summary>
public sealed class LoginUserUseCase(IAuthService authService)
{
    /// <returns>O resultado com token JWT, ou <c>null</c> se as credenciais forem inválidas.</returns>
    public Task<AuthResultDto?> ExecuteAsync(
        LoginUserCommand command, CancellationToken cancellationToken)
        => authService.LoginAsync(command, cancellationToken);
}
