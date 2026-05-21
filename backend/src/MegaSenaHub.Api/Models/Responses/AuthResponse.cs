namespace MegaSenaHub.Api.Models.Responses;

/// <summary>Resposta de autenticação com o token JWT e dados do usuário.</summary>
public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    UserInfo User);

/// <summary>Dados públicos do usuário autenticado.</summary>
public sealed record UserInfo(
    string Id,
    string Name,
    string Email,
    string? PhoneNumber);
