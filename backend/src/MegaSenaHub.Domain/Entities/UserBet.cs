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

    /// <summary>Valor ganho com esta aposta após verificação. Zero enquanto pendente ou quando perdeu.</summary>
    public decimal PrizeWon { get; private set; }

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
    /// Verifica a aposta contra o resultado de um concurso e atualiza o status, CheckedAt e PrizeWon.
    /// Overload de conveniência que usa DateTimeOffset.UtcNow como timestamp.
    /// </summary>
    public void CheckAgainstContest(LotteryContest contest) =>
        CheckAgainstContest(contest, DateTimeOffset.UtcNow, 0m);

    /// <summary>
    /// Verifica a aposta contra o resultado de um concurso.
    /// Use este overload em use cases que injetam IClock e conhecem o prêmio.
    /// </summary>
    public void CheckAgainstContest(LotteryContest contest, DateTimeOffset checkedAt, decimal prizeWon = 0m)
    {
        if (prizeWon < 0)
            throw new DomainException($"Prize won cannot be negative. Value: {prizeWon}.");

        var hits = contest.CountHits(Numbers);

        Status = hits switch
        {
            6 => UserBetStatus.WonSena,
            5 => UserBetStatus.WonQuina,
            4 => UserBetStatus.WonQuadra,
            _ => UserBetStatus.Lost
        };

        CheckedAt = checkedAt;
        PrizeWon = prizeWon;
    }

    /// <summary>
    /// Reconstitui uma aposta a partir de dados de persistência.
    /// Não valida regras de negócio — use exclusivamente na camada de Infrastructure.
    /// </summary>
    public static UserBet Reconstitute(
        Guid id,
        Guid userId,
        int contestNumber,
        IEnumerable<int> numbers,
        decimal amountPaid,
        DateTimeOffset createdAt,
        DateTimeOffset? checkedAt,
        UserBetStatus status,
        decimal prizeWon)
    {
        var megaNumbers = MegaSenaNumbers.Create(numbers);
        var bet = new UserBet(id, userId, contestNumber, megaNumbers, amountPaid, createdAt);
        bet.Status = status;
        bet.CheckedAt = checkedAt;
        bet.PrizeWon = prizeWon;
        return bet;
    }
}
