namespace MegaSenaHub.Application.DTOs;

public sealed record UserBetDto(
    Guid Id,
    string UserId,
    int ContestNumber,
    IReadOnlyCollection<int> Numbers,
    decimal AmountPaid,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CheckedAt);
