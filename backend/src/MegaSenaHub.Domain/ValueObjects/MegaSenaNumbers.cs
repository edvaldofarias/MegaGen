using MegaSenaHub.Domain.Exceptions;

namespace MegaSenaHub.Domain.ValueObjects;

/// <summary>
/// Value object que representa uma combinação válida de dezenas da Mega-Sena.
/// Mantém os números ordenados e garante unicidade.
/// </summary>
public sealed class MegaSenaNumbers
{
    public static readonly int RequiredCount = 6;

    private readonly IReadOnlyList<MegaSenaNumber> _numbers;

    private MegaSenaNumbers(IReadOnlyList<MegaSenaNumber> numbers)
    {
        _numbers = numbers;
    }

    public static MegaSenaNumbers Create(IEnumerable<int> numbers)
    {
        var rawList = numbers.ToList();

        if (rawList.Count != RequiredCount)
            throw new DomainException(
                $"A Mega-Sena combination must have exactly {RequiredCount} numbers. Count: {rawList.Count}.");

        // MegaSenaNumber.Create lança DomainException se o número for inválido (fora de 1-60).
        var megaNumbers = rawList.Select(MegaSenaNumber.Create).ToList();

        if (megaNumbers.Select(n => n.Value).Distinct().Count() != RequiredCount)
            throw new DomainException("A Mega-Sena combination cannot have duplicate numbers.");

        var sorted = megaNumbers.OrderBy(n => n.Value).ToList().AsReadOnly();

        return new MegaSenaNumbers(sorted);
    }

    public IReadOnlyList<MegaSenaNumber> AsReadOnly() => _numbers;

    public bool Contains(MegaSenaNumber number) => _numbers.Contains(number);

    /// <summary>
    /// Retorna quantas dezenas desta combinação estão presentes em <paramref name="other"/>.
    /// </summary>
    public int CountHits(MegaSenaNumbers other) =>
        _numbers.Count(n => other.Contains(n));

    public CombinationHash ToCombinationHash() =>
        CombinationHash.CreateFromNumbers(_numbers.Select(n => n.Value));
}
