using MegaSenaHub.Domain.Exceptions;

namespace MegaSenaHub.Domain.Entities;

/// <summary>
/// Representa uma faixa de premiação de um concurso (Quadra = 4, Quina = 5, Sena = 6).
/// </summary>
public sealed class PrizeRange
{
    private static readonly HashSet<int> ValidHits = [4, 5, 6];

    public Guid Id { get; }
    public Guid ContestId { get; }

    /// <summary>Quantidade de acertos desta faixa: 4, 5 ou 6.</summary>
    public int Hits { get; }

    public int Winners { get; }
    public decimal PrizeAmount { get; }

    private PrizeRange(Guid id, Guid contestId, int hits, int winners, decimal prizeAmount)
    {
        Id = id;
        ContestId = contestId;
        Hits = hits;
        Winners = winners;
        PrizeAmount = prizeAmount;
    }

    public static PrizeRange Create(Guid contestId, int hits, int winners, decimal prizeAmount)
    {
        if (!ValidHits.Contains(hits))
            throw new DomainException($"PrizeRange hits must be 4, 5, or 6. Value: {hits}.");

        if (winners < 0)
            throw new DomainException($"PrizeRange winners cannot be negative. Value: {winners}.");

        if (prizeAmount < 0)
            throw new DomainException($"PrizeRange prize amount cannot be negative. Value: {prizeAmount}.");

        return new PrizeRange(Guid.NewGuid(), contestId, hits, winners, prizeAmount);
    }

    /// <summary>
    /// Reconstitui uma faixa de premiação a partir de dados de persistência.
    /// Não valida regras de negócio — use exclusivamente na camada de Infrastructure.
    /// </summary>
    public static PrizeRange Reconstitute(Guid id, Guid contestId, int hits, int winners, decimal prizeAmount)
        => new PrizeRange(id, contestId, hits, winners, prizeAmount);
}
