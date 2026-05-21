using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.Exceptions;
using MegaSenaHub.Domain.Entities;

namespace MegaSenaHub.Application.UseCases;

/// <summary>
/// Verifica o resultado de uma aposta contra o concurso correspondente,
/// atualiza status, CheckedAt e PrizeWon usando IClock.
/// </summary>
public sealed class CheckUserBetResultUseCase(
    IUserBetRepository userBetRepository,
    IContestRepository contestRepository,
    ICurrentUser currentUser,
    IClock clock)
{
    public async Task<UserBetDto> ExecuteAsync(
        CheckUserBetResultCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException("User is not authenticated.");

        var bet = await userBetRepository.GetByIdAsync(
            command.UserBetId, currentUser.UserId!, cancellationToken)
            ?? throw new NotFoundException(
                $"Bet '{command.UserBetId}' not found for the current user.");

        var contest = await contestRepository.GetByContestNumberAsync(bet.ContestNumber, cancellationToken)
            ?? throw new NotFoundException(
                $"Contest {bet.ContestNumber} not found. Import it before checking results.");

        var hits = contest.CountHits(bet.Numbers);
        var prizeRange = contest.PrizeRanges.FirstOrDefault(pr => pr.Hits == hits);

        bet.CheckAgainstContest(contest, clock.UtcNow, prizeRange?.PrizeAmount ?? 0m);

        await userBetRepository.UpdateAsync(bet, cancellationToken);

        return MapToDto(bet);
    }

    private static UserBetDto MapToDto(UserBet bet) =>
        new(bet.Id,
            bet.UserId.ToString(),
            bet.ContestNumber,
            bet.Numbers.AsReadOnly().Select(n => n.Value).ToList().AsReadOnly(),
            bet.AmountPaid,
            bet.Status.ToString(),
            bet.CreatedAt,
            bet.CheckedAt);
}
