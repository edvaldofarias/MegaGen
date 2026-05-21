using MegaSenaHub.Domain.Exceptions;
using MegaSenaHub.Domain.ValueObjects;

namespace MegaSenaHub.Domain.Entities;

/// <summary>
/// Representa um concurso oficial da Mega-Sena.
/// </summary>
public sealed class LotteryContest
{
    public Guid Id { get; }
    public int ContestNumber { get; }
    public DateOnly DrawDate { get; }
    public MegaSenaNumbers DrawnNumbers { get; }
    public bool Accumulated { get; }
    public decimal TotalPrize { get; }
    public string Source { get; }
    public CombinationHash CombinationHash { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<PrizeRange> _prizeRanges = [];
    public IReadOnlyList<PrizeRange> PrizeRanges => _prizeRanges.AsReadOnly();

    private LotteryContest(
        Guid id,
        int contestNumber,
        DateOnly drawDate,
        MegaSenaNumbers drawnNumbers,
        bool accumulated,
        decimal totalPrize,
        string source,
        DateTimeOffset createdAt)
    {
        Id = id;
        ContestNumber = contestNumber;
        DrawDate = drawDate;
        DrawnNumbers = drawnNumbers;
        Accumulated = accumulated;
        TotalPrize = totalPrize;
        Source = source;
        CombinationHash = drawnNumbers.ToCombinationHash();
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static LotteryContest Create(
        int contestNumber,
        DateOnly drawDate,
        IEnumerable<int> drawnNumbers,
        bool accumulated = false,
        decimal totalPrize = 0m,
        string source = "")
    {
        if (contestNumber <= 0)
            throw new DomainException($"Contest number must be greater than zero. Value: {contestNumber}.");

        if (totalPrize < 0)
            throw new DomainException($"Total prize cannot be negative. Value: {totalPrize}.");

        // MegaSenaNumbers.Create valida contagem, faixa e unicidade dos números.
        var numbers = MegaSenaNumbers.Create(drawnNumbers);

        return new LotteryContest(
            Guid.NewGuid(),
            contestNumber,
            drawDate,
            numbers,
            accumulated,
            totalPrize,
            source ?? string.Empty,
            DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Adiciona uma faixa de premiação. Não permite duplicar a mesma quantidade de acertos.
    /// </summary>
    public void AddPrizeRange(PrizeRange prizeRange)
    {
        if (_prizeRanges.Any(p => p.Hits == prizeRange.Hits))
            throw new DomainException(
                $"A prize range for {prizeRange.Hits} hits already exists in contest {ContestNumber}.");

        _prizeRanges.Add(prizeRange);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Retorna quantas dezenas da aposta coincidem com o resultado deste concurso.
    /// </summary>
    public int CountHits(MegaSenaNumbers betNumbers) =>
        DrawnNumbers.CountHits(betNumbers);

    /// <summary>
    /// Verifica se uma combinação é exatamente igual ao resultado sorteado.
    /// </summary>
    public bool IsExactCombination(MegaSenaNumbers betNumbers) =>
        CombinationHash.Value == betNumbers.ToCombinationHash().Value;

    /// <summary>
    /// Reconstitui um concurso a partir de dados de persistência.
    /// Não valida regras de negócio — use exclusivamente na camada de Infrastructure.
    /// </summary>
    public static LotteryContest Reconstitute(
        Guid id,
        int contestNumber,
        DateOnly drawDate,
        IEnumerable<int> drawnNumbers,
        bool accumulated,
        decimal totalPrize,
        string source,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IEnumerable<PrizeRange> prizeRanges)
    {
        var numbers = MegaSenaNumbers.Create(drawnNumbers);
        var contest = new LotteryContest(id, contestNumber, drawDate, numbers, accumulated, totalPrize, source, createdAt);
        contest.UpdatedAt = updatedAt;
        foreach (var pr in prizeRanges)
            contest._prizeRanges.Add(pr);
        return contest;
    }
}
