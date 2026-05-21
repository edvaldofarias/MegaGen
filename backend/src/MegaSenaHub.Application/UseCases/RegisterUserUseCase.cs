using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;

namespace MegaSenaHub.Application.UseCases;

/// <summary>Orquestra o registro de um novo usuário e retorna o token JWT gerado.</summary>
public sealed class RegisterUserUseCase(IAuthService authService)
{
    public Task<AuthResultDto> ExecuteAsync(
        RegisterUserCommand command, CancellationToken cancellationToken)
        => authService.RegisterAsync(command, cancellationToken);
}
