using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.Exceptions;
using MegaSenaHub.Application.Queries;
using MegaSenaHub.Domain.Enums;

namespace MegaSenaHub.Application.UseCases;

/// <summary>
/// Retorna um resumo consolidado das apostas do usuário autenticado:
/// total gasto, total ganho, saldo, contagens e melhor resultado.
/// </summary>
public sealed class GetUserBetSummaryUseCase(
    IUserBetRepository userBetRepository,
    ICurrentUser currentUser)
{
    public async Task<UserBetSummaryDto> ExecuteAsync(
        GetUserBetSummaryQuery query, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException("User is not authenticated.");

        var bets = await userBetRepository.GetByUserIdAsync(currentUser.UserId!, cancellationToken);

        if (!bets.Any())
            return new UserBetSummaryDto(0m, 0m, 0m, 0, 0, 0, 0, 0);

        var totalSpent = bets.Sum(b => b.AmountPaid);

        var winningBets = bets
            .Where(b => b.Status is UserBetStatus.WonQuadra
                or UserBetStatus.WonQuina
                or UserBetStatus.WonSena)
            .ToList();

        var totalWon = winningBets.Sum(b => b.PrizeWon);
        var balance = totalWon - totalSpent;
        var losingBets = bets.Count(b => b.Status == UserBetStatus.Lost);
        var pendingBets = bets.Count(b => b.Status == UserBetStatus.Pending);
        var bestHits = bets.Max(b => StatusToApproximateHits(b.Status));

        return new UserBetSummaryDto(
            TotalSpent: totalSpent,
            TotalWon: totalWon,
            Balance: balance,
            TotalBets: bets.Count,
            WinningBets: winningBets.Count,
            LosingBets: losingBets,
            PendingBets: pendingBets,
            BestHits: bestHits);
    }

    private static int StatusToApproximateHits(UserBetStatus status) => status switch
    {
        UserBetStatus.WonSena => 6,
        UserBetStatus.WonQuina => 5,
        UserBetStatus.WonQuadra => 4,
        _ => 0
    };
}
