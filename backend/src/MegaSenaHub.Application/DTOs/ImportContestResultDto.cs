namespace MegaSenaHub.Application.DTOs;

public sealed record ImportContestResultDto(
    int ContestNumber,
    bool Imported,
    bool AlreadyExists);
