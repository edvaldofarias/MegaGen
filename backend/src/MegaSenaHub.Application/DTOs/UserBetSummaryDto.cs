namespace MegaSenaHub.Application.DTOs;

public sealed record UserBetSummaryDto(
    decimal TotalSpent,
    decimal TotalWon,
    decimal Balance,
    int TotalBets,
    int WinningBets,
    int LosingBets,
    int PendingBets,
    int BestHits);
