namespace MegaSenaHub.Application.Abstractions;

/// <summary>Dados necessários para criar uma nova conta de usuário.</summary>
public sealed record RegisterUserCommand(
    string Name,
    string Email,
    string PhoneNumber,
    string Password);
