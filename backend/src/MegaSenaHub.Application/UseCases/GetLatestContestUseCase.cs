using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;

namespace MegaSenaHub.Application.UseCases;

/// <summary>
/// Retorna o concurso mais recente registrado na base de dados (maior número de sorteio).
/// Retorna null quando ainda não há concursos importados.
/// </summary>
public sealed class GetLatestContestUseCase(IContestRepository contestRepository)
{
    public async Task<ContestDetailDto?> ExecuteAsync(CancellationToken cancellationToken)
    {
        var contest = await contestRepository.GetLatestAsync(cancellationToken);
        return contest is null ? null : MapToDto(contest);
    }

    private static ContestDetailDto MapToDto(Domain.Entities.LotteryContest c) =>
        new(c.ContestNumber,
            c.DrawDate,
            c.DrawnNumbers.AsReadOnly().Select(n => n.Value).ToList().AsReadOnly(),
            c.Accumulated,
            c.TotalPrize,
            c.Source,
            c.CreatedAt,
            c.PrizeRanges.Select(pr => new PrizeRangeDto(pr.Hits, pr.Winners, pr.PrizeAmount))
                .ToList().AsReadOnly());
}
