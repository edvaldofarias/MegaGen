namespace MegaSenaHub.Application.DTOs;

/// <summary>Resultado de uma operação de autenticação (registro ou login).</summary>
public sealed record AuthResultDto(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string UserId,
    string Name,
    string Email,
    string? PhoneNumber);
