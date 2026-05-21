using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.Exceptions;

namespace MegaSenaHub.Application.UseCases;

/// <summary>
/// Retorna todas as apostas do usuário autenticado.
/// </summary>
public sealed class GetUserBetsUseCase(
    IUserBetRepository userBetRepository,
    ICurrentUser currentUser)
{
    public async Task<IReadOnlyCollection<UserBetDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException("User is not authenticated.");

        var bets = await userBetRepository.GetByUserIdAsync(currentUser.UserId!, cancellationToken);

        return bets.Select(b => new UserBetDto(
            b.Id,
            b.UserId.ToString(),
            b.ContestNumber,
            b.Numbers.AsReadOnly().Select(n => n.Value).ToList().AsReadOnly(),
            b.AmountPaid,
            b.Status.ToString(),
            b.CreatedAt,
            b.CheckedAt)).ToList().AsReadOnly();
    }
}
