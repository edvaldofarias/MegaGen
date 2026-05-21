using System.Text.RegularExpressions;
using MegaSenaHub.Domain.Exceptions;

namespace MegaSenaHub.Domain.ValueObjects;

/// <summary>
/// Value object que representa a combinação normalizada de uma aposta ou resultado da Mega-Sena.
/// Formato: "01-02-03-04-05-06" — sempre com dois dígitos por dezena e separados por hífen.
/// </summary>
public sealed record CombinationHash
{
    // Seis grupos de exatamente dois dígitos separados por hífen.
    private static readonly Regex ValidFormat =
        new(@"^\d{2}(-\d{2}){5}$", RegexOptions.Compiled);

    public string Value { get; }

    private CombinationHash(string value) => Value = value;

    public static CombinationHash Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("CombinationHash cannot be empty.");

        if (!ValidFormat.IsMatch(value))
            throw new DomainException(
                $"CombinationHash has an invalid format: '{value}'. Expected: '01-02-03-04-05-06'.");

        return new CombinationHash(value);
    }

    /// <summary>
    /// Cria o hash a partir de uma sequência de inteiros, ordenando-os antes de formatar.
    /// </summary>
    public static CombinationHash CreateFromNumbers(IEnumerable<int> numbers)
    {
        var formatted = numbers
            .OrderBy(n => n)
            .Select(n => n.ToString("D2"));

        return Create(string.Join("-", formatted));
    }

    public override string ToString() => Value;
}
