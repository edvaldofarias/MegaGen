using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Domain.Entities;

namespace MegaSenaHub.Application.Abstractions;

/// <summary>Abstrai a fonte externa de resultados da Mega-Sena.</summary>
public interface ILotteryResultProvider
{
    Task<int> GetLatestContestNumberAsync(CancellationToken cancellationToken);
    Task<LotteryContestResultDto> GetContestResultAsync(int contestNumber, CancellationToken cancellationToken);
}
