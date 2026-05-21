using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Domain.Entities;

namespace MegaSenaHub.Application.Abstractions;

/// <summary>Abstrai a persistência de concursos da Mega-Sena.</summary>
public interface IContestRepository
{
    Task<bool> ExistsAsync(int contestNumber, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<int>> GetExistingContestNumbersAsync(CancellationToken cancellationToken);
    Task<LotteryContest?> GetByContestNumberAsync(int contestNumber, CancellationToken cancellationToken);
    Task AddAsync(LotteryContest contest, CancellationToken cancellationToken);

    /// <summary>Retorna concursos que contenham pelo menos um dos números informados.</summary>
    Task<IReadOnlyCollection<LotteryContest>> FindContestsWithAnyNumbersAsync(
        IReadOnlyCollection<int> numbers, CancellationToken cancellationToken);

    Task<bool> CombinationHashExistsAsync(string combinationHash, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<NumberFrequencyDto>> GetNumberFrequenciesAsync(CancellationToken cancellationToken);

    /// <summary>Retorna o concurso com o maior número de sorteio registrado.</summary>
    Task<LotteryContest?> GetLatestAsync(CancellationToken cancellationToken);
}
