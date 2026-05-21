namespace MegaSenaHub.Infrastructure.Identity;

/// <summary>Configurações JWT lidas do appsettings.json.</summary>
public sealed class JwtSettings
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Secret { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;
}
