namespace MegaSenaHub.Application.Abstractions;

/// <summary>Abstrai o usuário autenticado na sessão atual.</summary>
public interface ICurrentUser
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
}
