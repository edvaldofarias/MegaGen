using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Domain.Enums;
using MegaSenaHub.Domain.ValueObjects;

namespace MegaSenaHub.Application.UseCases;

/// <summary>
/// Gera jogos da Mega-Sena com base na estratégia solicitada,
/// verificando histórico de sorteios e premiações conforme as flags de evitação.
/// </summary>
public sealed class GenerateMegaSenaGamesUseCase(IContestRepository contestRepository)
{
    private const int MaxQuantity = 100;
    private const int MaxAttempts = 1_000;

    public async Task<IReadOnlyCollection<GeneratedGameDto>> ExecuteAsync(
        GenerateMegaSenaGamesCommand command, CancellationToken cancellationToken)
    {
        if (command.Quantity <= 0 || command.Quantity > MaxQuantity)
            throw new ArgumentOutOfRangeException(
                nameof(command.Quantity),
                $"Quantity must be between 1 and {MaxQuantity}.");

        if (command.NumbersPerGame != MegaSenaNumbers.RequiredCount)
            throw new ArgumentOutOfRangeException(
                nameof(command.NumbersPerGame),
                $"NumbersPerGame must be exactly {MegaSenaNumbers.RequiredCount}.");

        IReadOnlyList<NumberFrequencyDto>? frequencies = null;

        if (command.Strategy is GameGenerationStrategy.MostDrawn
            or GameGenerationStrategy.LeastDrawn
            or GameGenerationStrategy.Mixed)
        {
            frequencies = (await contestRepository.GetNumberFrequenciesAsync(cancellationToken)).ToList();
        }

        var results = new List<GeneratedGameDto>(command.Quantity);

        for (int i = 0; i < command.Quantity; i++)
        {
            var game = await GenerateGameWithAttemptsAsync(command, frequencies, cancellationToken);
            results.Add(game);
        }

        return results.AsReadOnly();
    }

    private async Task<GeneratedGameDto> GenerateGameWithAttemptsAsync(
        GenerateMegaSenaGamesCommand command,
        IReadOnlyList<NumberFrequencyDto>? frequencies,
        CancellationToken ct)
    {
        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var rawNumbers = GenerateNumbers(command.Strategy, command.NumbersPerGame, frequencies);
            var megaNumbers = MegaSenaNumbers.Create(rawNumbers);
            var hash = megaNumbers.ToCombinationHash();

            var alreadyDrawn = await contestRepository.CombinationHashExistsAsync(hash.Value, ct);

            bool shouldSkipDrawn = alreadyDrawn &&
                (command.AvoidAlreadyDrawnCombination ||
                 command.Strategy == GameGenerationStrategy.NeverDrawn);

            if (shouldSkipDrawn) continue;

            var numbersList = megaNumbers.AsReadOnly().Select(n => n.Value).ToArray();
            var relatedContests = await contestRepository
                .FindContestsWithAnyNumbersAsync(numbersList, ct);

            var alreadyWon = relatedContests.Any(c => c.CountHits(megaNumbers) >= 4);

            bool shouldSkipWon = alreadyWon &&
                (command.AvoidAlreadyWonCombination ||
                 command.Strategy == GameGenerationStrategy.NeverWon);

            if (shouldSkipWon) continue;

            return new GeneratedGameDto(
                Numbers: numbersList.OrderBy(n => n).ToList().AsReadOnly(),
                CombinationHash: hash.Value,
                AlreadyDrawn: alreadyDrawn,
                AlreadyWon: alreadyWon);
        }

        throw new InvalidOperationException(
            $"Could not generate a valid combination within {MaxAttempts} attempts.");
    }

    private static List<int> GenerateNumbers(
        GameGenerationStrategy strategy, int count, IReadOnlyList<NumberFrequencyDto>? frequencies) =>
        strategy switch
        {
            GameGenerationStrategy.Random => GenerateRandom(count),
            GameGenerationStrategy.MostDrawn => GenerateByFrequency(count, frequencies!, ascending: false),
            GameGenerationStrategy.LeastDrawn => GenerateByFrequency(count, frequencies!, ascending: true),
            GameGenerationStrategy.Mixed => GenerateMixed(count, frequencies!),
            GameGenerationStrategy.NeverDrawn => GenerateRandom(count),
            GameGenerationStrategy.NeverWon => GenerateRandom(count),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy))
        };

    private static List<int> GenerateRandom(int count)
    {
        var pool = Enumerable.Range(1, 60).ToList();
        var result = new List<int>(count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Shared.Next(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    private static List<int> GenerateByFrequency(
        int count, IReadOnlyList<NumberFrequencyDto> frequencies, bool ascending)
    {
        var ordered = ascending
            ? frequencies.OrderBy(f => f.Frequency).ThenBy(f => f.Number)
            : (IEnumerable<NumberFrequencyDto>)frequencies
                .OrderByDescending(f => f.Frequency).ThenBy(f => f.Number);

        var selected = ordered.Take(count).Select(f => f.Number).ToList();

        // Fallback: se a lista de frequências tiver menos de count entradas
        if (selected.Count < count)
        {
            var existing = selected.ToHashSet();
            var remaining = Enumerable.Range(1, 60).Where(n => !existing.Contains(n)).ToList();

            while (selected.Count < count && remaining.Count > 0)
            {
                int idx = Random.Shared.Next(0, remaining.Count);
                selected.Add(remaining[idx]);
                remaining.RemoveAt(idx);
            }
        }

        return selected;
    }

    private static List<int> GenerateMixed(int count, IReadOnlyList<NumberFrequencyDto> frequencies)
    {
        int topHalf = count / 2;
        var mostDrawn = frequencies
            .OrderByDescending(f => f.Frequency)
            .ThenBy(f => f.Number)
            .Take(topHalf)
            .Select(f => f.Number)
            .ToList();

        var remaining = frequencies
            .Where(f => !mostDrawn.Contains(f.Number))
            .Select(f => f.Number)
            .ToList();

        var result = new List<int>(count);
        result.AddRange(mostDrawn);

        int needed = count - topHalf;
        for (int i = 0; i < needed && remaining.Count > 0; i++)
        {
            int idx = Random.Shared.Next(0, remaining.Count);
            result.Add(remaining[idx]);
            remaining.RemoveAt(idx);
        }

        // Fallback adicional se remaining ficou vazio
        if (result.Count < count)
        {
            var existing = result.ToHashSet();
            var pool = Enumerable.Range(1, 60).Where(n => !existing.Contains(n)).ToList();
            while (result.Count < count && pool.Count > 0)
            {
                int idx = Random.Shared.Next(0, pool.Count);
                result.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
        }

        return result;
    }
}
