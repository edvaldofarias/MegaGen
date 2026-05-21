using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;

namespace MegaSenaHub.Infrastructure.Adapters;

/// <summary>
/// Implementação no-op do provider externo de resultados da Mega-Sena.
/// Retorna 0 como número de concurso mais recente (nenhum sync ocorre).
/// Provider real com integração à API da Caixa será implementado na Etapa 7.
/// </summary>
internal sealed class NoOpLotteryResultProvider : ILotteryResultProvider
{
    public Task<int> GetLatestContestNumberAsync(CancellationToken cancellationToken)
        => Task.FromResult(0);

    public Task<LotteryContestResultDto> GetContestResultAsync(
        int contestNumber, CancellationToken cancellationToken)
        => throw new NotSupportedException(
            "External lottery provider is not configured. This will be implemented in Etapa 7.");
}
