namespace MegaSenaHub.Application.DTOs;

public sealed record BetHistoryCheckResultDto(
    bool ExactCombinationAlreadyDrawn,
    int? ExactCombinationContestNumber,
    int BestHits,
    int? BestContestNumber,
    IReadOnlyCollection<int> MatchedNumbers,
    decimal? PrizeAmount);
