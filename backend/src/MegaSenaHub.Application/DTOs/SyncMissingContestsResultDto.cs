namespace MegaSenaHub.Application.DTOs;

public sealed record SyncMissingContestsResultDto(
    int LatestContestNumber,
    int MissingCount,
    IReadOnlyCollection<int> PublishedContestNumbers);
