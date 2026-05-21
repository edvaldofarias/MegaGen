using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.Queries;
using MegaSenaHub.Domain.ValueObjects;

namespace MegaSenaHub.Application.UseCases;

/// <summary>
/// Consulta o histórico de sorteios para uma combinação de dezenas informada,
/// retornando se já foi sorteada exatamente, o melhor resultado e o prêmio correspondente.
/// </summary>
public sealed class CheckCombinationHistoryUseCase(IContestRepository contestRepository)
{
    public async Task<BetHistoryCheckResultDto> ExecuteAsync(
        CheckCombinationHistoryQuery query, CancellationToken cancellationToken)
    {
        // DomainException se a combinação for inválida (< 6, duplicados, fora de 1-60)
        var megaNumbers = MegaSenaNumbers.Create(query.Numbers);
        var numbersList = megaNumbers.AsReadOnly().Select(n => n.Value).ToArray();

        var relatedContests = await contestRepository
            .FindContestsWithAnyNumbersAsync(numbersList, cancellationToken);

        bool exactDrawn = false;
        int? exactContestNumber = null;
        int bestHits = 0;
        int? bestContestNumber = null;
        IReadOnlyCollection<int> matchedNumbers = Array.Empty<int>();
        decimal? prizeAmount = null;

        foreach (var contest in relatedContests)
        {
            if (contest.IsExactCombination(megaNumbers))
            {
                exactDrawn = true;
                exactContestNumber = contest.ContestNumber;
            }

            var hits = contest.CountHits(megaNumbers);
            if (hits > bestHits)
            {
                bestHits = hits;
                bestContestNumber = contest.ContestNumber;
                matchedNumbers = contest.DrawnNumbers.AsReadOnly()
                    .Where(n => megaNumbers.Contains(n))
                    .Select(n => n.Value)
                    .ToList()
                    .AsReadOnly();
                prizeAmount = contest.PrizeRanges
                    .FirstOrDefault(pr => pr.Hits == hits)?.PrizeAmount;
            }
        }

        return new BetHistoryCheckResultDto(
            ExactCombinationAlreadyDrawn: exactDrawn,
            ExactCombinationContestNumber: exactContestNumber,
            BestHits: bestHits,
            BestContestNumber: bestContestNumber,
            MatchedNumbers: matchedNumbers,
            PrizeAmount: prizeAmount);
    }
}
