using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.Exceptions;

namespace MegaSenaHub.Application.UseCases;

/// <summary>Retorna o perfil do usuário autenticado na sessão atual.</summary>
public sealed class GetCurrentUserProfileUseCase(
    IAuthService authService,
    ICurrentUser currentUser)
{
    public async Task<UserProfileDto> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new UnauthorizedException("Usuário não autenticado.");

        var profile = await authService.GetProfileAsync(currentUser.UserId, cancellationToken);

        return profile ?? throw new NotFoundException("Usuário não encontrado.");
    }
}
