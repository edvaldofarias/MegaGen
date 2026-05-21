using MegaSenaHub.Domain.Exceptions;

namespace MegaSenaHub.Domain.Entities;

/// <summary>
/// Representa o resultado calculado de uma aposta após verificação contra um concurso.
/// </summary>
public sealed class UserBetResult
{
    public Guid Id { get; }
    public Guid UserBetId { get; }
    public Guid ContestId { get; }
    public int Hits { get; }
    public decimal PrizeAmount { get; }
    public DateTimeOffset CheckedAt { get; }

    private UserBetResult(
        Guid id,
        Guid userBetId,
        Guid contestId,
        int hits,
        decimal prizeAmount,
        DateTimeOffset checkedAt)
    {
        Id = id;
        UserBetId = userBetId;
        ContestId = contestId;
        Hits = hits;
        PrizeAmount = prizeAmount;
        CheckedAt = checkedAt;
    }

    public static UserBetResult Create(
        Guid userBetId,
        Guid contestId,
        int hits,
        decimal prizeAmount,
        DateTimeOffset checkedAt)
    {
        if (hits < 0 || hits > 6)
            throw new DomainException($"Hits must be between 0 and 6. Value: {hits}.");

        if (prizeAmount < 0)
            throw new DomainException($"Prize amount cannot be negative. Value: {prizeAmount}.");

        return new UserBetResult(
            Guid.NewGuid(),
            userBetId,
            contestId,
            hits,
            prizeAmount,
            checkedAt);
    }
}
