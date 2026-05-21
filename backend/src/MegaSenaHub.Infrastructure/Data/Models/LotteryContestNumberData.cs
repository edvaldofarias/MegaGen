namespace MegaSenaHub.Infrastructure.Data.Models;

/// <summary>
/// Modelo de persistência para cada dezena sorteada de um concurso.
/// Chave primária composta: (ContestId, Number).
/// </summary>
internal sealed class LotteryContestNumberData
{
    public Guid ContestId { get; set; }
    public int Number { get; set; }

    public LotteryContestData Contest { get; set; } = null!;
}
