using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.Queries;

namespace MegaSenaHub.Application.UseCases;

/// <summary>
/// Retorna o detalhe de um concurso específico pelo número do sorteio.
/// Retorna null quando o concurso não existe na base.
/// </summary>
public sealed class GetContestByNumberUseCase(IContestRepository contestRepository)
{
    public async Task<ContestDetailDto?> ExecuteAsync(
        GetContestByNumberQuery query, CancellationToken cancellationToken)
    {
        var contest = await contestRepository.GetByContestNumberAsync(query.ContestNumber, cancellationToken);
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
