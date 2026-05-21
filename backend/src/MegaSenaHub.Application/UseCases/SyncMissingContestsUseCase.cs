using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.Messaging;
using MegaSenaHub.Application.Commands;

namespace MegaSenaHub.Application.UseCases;

/// <summary>
/// Detecta concursos ausentes na base comparando com o último número disponível
/// e publica uma mensagem para cada número faltante ser importado assincronamente.
/// </summary>
public sealed class SyncMissingContestsUseCase(
    ILotteryResultProvider provider,
    IContestRepository contestRepository,
    IMessagePublisher publisher,
    IClock clock)
{
    public async Task<SyncMissingContestsResultDto> ExecuteAsync(
        SyncMissingContestsCommand command, CancellationToken cancellationToken)
    {
        var latestContestNumber = await provider.GetLatestContestNumberAsync(cancellationToken);

        var existingNumbers = await contestRepository.GetExistingContestNumbersAsync(cancellationToken);
        var existingSet = existingNumbers.ToHashSet();

        var missing = Enumerable.Range(1, latestContestNumber)
            .Where(n => !existingSet.Contains(n))
            .ToList();

        foreach (var contestNumber in missing)
        {
            var message = new ContestSyncRequestedMessage(
                LotteryType: "MegaSena",
                ContestNumber: contestNumber,
                RequestedAt: clock.UtcNow,
                CorrelationId: Guid.NewGuid());

            await publisher.PublishAsync(message, cancellationToken);
        }

        return new SyncMissingContestsResultDto(
            LatestContestNumber: latestContestNumber,
            MissingCount: missing.Count,
            PublishedContestNumbers: missing.AsReadOnly());
    }
}
