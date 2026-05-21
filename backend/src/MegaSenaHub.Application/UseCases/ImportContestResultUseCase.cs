using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Domain.Entities;

namespace MegaSenaHub.Application.UseCases;

/// <summary>
/// Importa o resultado de um concurso específico a partir do provider externo
/// e persiste na base de dados caso ainda não exista.
/// </summary>
public sealed class ImportContestResultUseCase(
    IContestRepository contestRepository,
    ILotteryResultProvider provider)
{
    public async Task<ImportContestResultDto> ExecuteAsync(
        ImportContestResultCommand command, CancellationToken cancellationToken)
    {
        if (command.ContestNumber <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(command.ContestNumber),
                "Contest number must be greater than zero.");

        var exists = await contestRepository.ExistsAsync(command.ContestNumber, cancellationToken);
        if (exists)
            return new ImportContestResultDto(command.ContestNumber, Imported: false, AlreadyExists: true);

        var dto = await provider.GetContestResultAsync(command.ContestNumber, cancellationToken);

        var contest = LotteryContest.Create(
            dto.ContestNumber,
            DateOnly.FromDateTime(dto.DrawDate),
            dto.DrawnNumbers,
            dto.Accumulated,
            dto.TotalPrize,
            dto.Source);

        foreach (var prizeDto in dto.PrizeRanges)
        {
            var prizeRange = PrizeRange.Create(
                contest.Id,
                prizeDto.Hits,
                prizeDto.Winners,
                prizeDto.PrizeAmount);

            contest.AddPrizeRange(prizeRange);
        }

        await contestRepository.AddAsync(contest, cancellationToken);

        return new ImportContestResultDto(command.ContestNumber, Imported: true, AlreadyExists: false);
    }
}
