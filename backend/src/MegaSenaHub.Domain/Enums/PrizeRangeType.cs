namespace MegaSenaHub.Domain.Enums;

/// <summary>
/// Representa as faixas de premiação da Mega-Sena pelo número de acertos.
/// Os valores inteiros refletem diretamente a quantidade de acertos,
/// facilitando comparações diretas sem conversão.
/// </summary>
public enum PrizeRangeType
{
    Quadra = 4,
    Quina = 5,
    Sena = 6
}
