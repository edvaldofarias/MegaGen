namespace MegaSenaHub.Application.DTOs;

public sealed record GeneratedGameDto(
    IReadOnlyCollection<int> Numbers,
    string CombinationHash,
    bool AlreadyDrawn,
    bool AlreadyWon);
