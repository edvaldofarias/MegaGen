namespace MegaSenaHub.Application.DTOs;

/// <summary>Detalhe de um concurso da Mega-Sena retornado pelos use cases de consulta.</summary>
public sealed record ContestDetailDto(
    int ContestNumber,
    DateOnly DrawDate,
    IReadOnlyCollection<int> DrawnNumbers,
    bool Accumulated,
    decimal TotalPrize,
    string Source,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<PrizeRangeDto> PrizeRanges);
