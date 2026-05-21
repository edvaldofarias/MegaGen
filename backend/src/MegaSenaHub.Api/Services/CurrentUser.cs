using System.Security.Claims;
using MegaSenaHub.Application.Abstractions;

namespace MegaSenaHub.Api.Services;

/// <summary>
/// Implementação de ICurrentUser que lê o usuário autenticado do HttpContext.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
