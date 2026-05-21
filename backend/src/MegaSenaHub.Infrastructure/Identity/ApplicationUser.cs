using Microsoft.AspNetCore.Identity;

namespace MegaSenaHub.Infrastructure.Identity;

/// <summary>
/// Entidade de usuário da aplicação.
/// Herda de IdentityUser e adiciona campos específicos do domínio MegaSena Hub.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    /// <summary>Nome completo do usuário.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Data e hora de criação da conta.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
