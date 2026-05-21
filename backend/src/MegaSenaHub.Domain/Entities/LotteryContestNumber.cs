using MegaSenaHub.Domain.Exceptions;
using MegaSenaHub.Domain.ValueObjects;

namespace MegaSenaHub.Domain.Entities;

/// <summary>
/// Representa um número individualmente sorteado de um concurso.
/// Entidade auxiliar que pode ser usada na camada de persistência futura.
/// </summary>
public sealed class LotteryContestNumber
{
    public Guid Id { get; }
    public Guid ContestId { get; }
    public MegaSenaNumber Number { get; }

    private LotteryContestNumber(Guid id, Guid contestId, MegaSenaNumber number)
    {
        Id = id;
        ContestId = contestId;
        Number = number;
    }

    public static LotteryContestNumber Create(Guid contestId, int number)
    {
        if (contestId == Guid.Empty)
            throw new DomainException("ContestId cannot be empty.");

        var megaNumber = MegaSenaNumber.Create(number);

        return new LotteryContestNumber(Guid.NewGuid(), contestId, megaNumber);
    }
}
