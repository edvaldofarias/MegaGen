using MegaSenaHub.Domain.Enums;
using MegaSenaHub.Domain.Exceptions;
using MegaSenaHub.Domain.ValueObjects;

namespace MegaSenaHub.Domain.Entities;

/// <summary>
/// Representa uma aposta cadastrada por um usuário para um concurso da Mega-Sena.
/// </summary>
public sealed class UserBet
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public int ContestNumber { get; }
    public MegaSenaNumbers Numbers { get; }
    public decimal AmountPaid { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? CheckedAt { get; private set; }
    public UserBetStatus Status { get; private set; }

    private UserBet(
        Guid id,
        Guid userId,
        int contestNumber,
        MegaSenaNumbers numbers,
        decimal amountPaid,
        DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        ContestNumber = contestNumber;
        Numbers = numbers;
        AmountPaid = amountPaid;
        CreatedAt = createdAt;
        Status = UserBetStatus.Pending;
    }

    public static UserBet Create(
        Guid userId,
        int contestNumber,
        IEnumerable<int> numbers,
        decimal amountPaid)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (contestNumber <= 0)
            throw new DomainException($"Contest number must be greater than zero. Value: {contestNumber}.");

        if (amountPaid < 0)
            throw new DomainException($"Amount paid cannot be negative. Value: {amountPaid}.");

        // MegaSenaNumbers.Create valida contagem, faixa e unicidade dos números.
        var megaNumbers = MegaSenaNumbers.Create(numbers);

        return new UserBet(
            Guid.NewGuid(),
            userId,
            contestNumber,
            megaNumbers,
            amountPaid,
            DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Verifica a aposta contra o resultado de um concurso e atualiza o status e CheckedAt.
    /// </summary>
    public void CheckAgainstContest(LotteryContest contest)
    {
        var hits = contest.CountHits(Numbers);

        Status = hits switch
        {
            6 => UserBetStatus.WonSena,
            5 => UserBetStatus.WonQuina,
            4 => UserBetStatus.WonQuadra,
            _ => UserBetStatus.Lost
        };

        CheckedAt = DateTimeOffset.UtcNow;
    }
}
