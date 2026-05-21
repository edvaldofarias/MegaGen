namespace MegaSenaHub.Application.DTOs;

/// <summary>Dados do perfil do usuário autenticado.</summary>
public sealed record UserProfileDto(
    string UserId,
    string Name,
    string Email,
    string? PhoneNumber);
