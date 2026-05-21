namespace MegaSenaHub.Application.Abstractions;

/// <summary>Credenciais para autenticação de um usuário existente.</summary>
public sealed record LoginUserCommand(
    string Email,
    string Password);
