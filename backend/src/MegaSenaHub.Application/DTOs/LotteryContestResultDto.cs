namespace MegaSenaHub.Application.DTOs;

/// <summary>Resultado recebido do provider externo de resultados da Mega-Sena.</summary>
public sealed record LotteryContestResultDto(
    int ContestNumber,
    DateTime DrawDate,
    IReadOnlyCollection<int> DrawnNumbers,
    bool Accumulated,
    decimal TotalPrize,
    string Source,
    IReadOnlyCollection<PrizeRangeDto> PrizeRanges);
