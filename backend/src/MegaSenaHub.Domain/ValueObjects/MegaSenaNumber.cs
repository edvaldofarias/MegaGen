using MegaSenaHub.Domain.Exceptions;

namespace MegaSenaHub.Domain.ValueObjects;

/// <summary>
/// Value object que representa uma dezena da Mega-Sena (1 a 60).
/// Usar record garante igualdade por valor sem implementação manual.
/// </summary>
public sealed record MegaSenaNumber : IComparable<MegaSenaNumber>
{
    public int Value { get; }

    private MegaSenaNumber(int value) => Value = value;

    public static MegaSenaNumber Create(int value)
    {
        if (value < 1 || value > 60)
            throw new DomainException($"Mega-Sena number must be between 1 and 60. Value: {value}.");

        return new MegaSenaNumber(value);
    }

    public int CompareTo(MegaSenaNumber? other) => other is null ? 1 : Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString("D2");
}
