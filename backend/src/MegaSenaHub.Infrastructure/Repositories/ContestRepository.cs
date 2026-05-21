using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Domain.Entities;
using MegaSenaHub.Infrastructure.Data;
using MegaSenaHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaSenaHub.Infrastructure.Repositories;

internal sealed class ContestRepository : IContestRepository
{
    private readonly MegaSenaHubDbContext _context;

    public ContestRepository(MegaSenaHubDbContext context) => _context = context;

    public Task<bool> ExistsAsync(int contestNumber, CancellationToken cancellationToken)
        => _context.LotteryContests.AnyAsync(c => c.ContestNumber == contestNumber, cancellationToken);

    public async Task<IReadOnlyCollection<int>> GetExistingContestNumbersAsync(CancellationToken cancellationToken)
    {
        var numbers = await _context.LotteryContests
            .AsNoTracking()
            .Select(c => c.ContestNumber)
            .ToListAsync(cancellationToken);
        return numbers.AsReadOnly();
    }

    public async Task<LotteryContest?> GetByContestNumberAsync(int contestNumber, CancellationToken cancellationToken)
    {
        var data = await _context.LotteryContests
            .Include(c => c.Numbers)
            .Include(c => c.PrizeRanges)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ContestNumber == contestNumber, cancellationToken);

        return data is null ? null : ToDomain(data);
    }

    public async Task AddAsync(LotteryContest contest, CancellationToken cancellationToken)
    {
        var data = ToData(contest);
        _context.LotteryContests.Add(data);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LotteryContest>> FindContestsWithAnyNumbersAsync(
        IReadOnlyCollection<int> numbers, CancellationToken cancellationToken)
    {
        var data = await _context.LotteryContests
            .Include(c => c.Numbers)
            .Include(c => c.PrizeRanges)
            .AsNoTracking()
            .Where(c => c.Numbers.Any(n => numbers.Contains(n.Number)))
            .ToListAsync(cancellationToken);

        return data.ConvertAll(ToDomain).AsReadOnly();
    }

    public Task<bool> CombinationHashExistsAsync(string combinationHash, CancellationToken cancellationToken)
        => _context.LotteryContests.AnyAsync(c => c.CombinationHash == combinationHash, cancellationToken);

    public async Task<IReadOnlyCollection<NumberFrequencyDto>> GetNumberFrequenciesAsync(CancellationToken cancellationToken)
    {
        var raw = await _context.LotteryContestNumbers
            .GroupBy(n => n.Number)
            .Select(g => new { Number = g.Key, Frequency = g.Count() })
            .OrderByDescending(g => g.Frequency)
            .ToListAsync(cancellationToken);

        return raw
            .ConvertAll(r => new NumberFrequencyDto(r.Number, r.Frequency))
            .AsReadOnly();
    }

    private static LotteryContestData ToData(LotteryContest contest) =>
        new()
        {
            Id = contest.Id,
            ContestNumber = contest.ContestNumber,
            DrawDate = contest.DrawDate,
            Accumulated = contest.Accumulated,
            TotalPrize = contest.TotalPrize,
            Source = contest.Source,
            CombinationHash = contest.CombinationHash.Value,
            CreatedAt = contest.CreatedAt,
            UpdatedAt = contest.UpdatedAt,
            Numbers = [.. contest.DrawnNumbers.AsReadOnly().Select(n =>
                new LotteryContestNumberData
                {
                    ContestId = contest.Id,
                    Number = n.Value
                })],
            PrizeRanges = [.. contest.PrizeRanges.Select(pr =>
                new PrizeRangeData
                {
                    Id = pr.Id,
                    ContestId = pr.ContestId,
                    Hits = pr.Hits,
                    Winners = pr.Winners,
                    PrizeAmount = pr.PrizeAmount
                })]
        };

    private static LotteryContest ToDomain(LotteryContestData data)
    {
        var prizeRanges = data.PrizeRanges.Select(pr =>
            PrizeRange.Reconstitute(pr.Id, pr.ContestId, pr.Hits, pr.Winners, pr.PrizeAmount));

        return LotteryContest.Reconstitute(
            data.Id,
            data.ContestNumber,
            data.DrawDate,
            data.Numbers.Select(n => n.Number),
            data.Accumulated,
            data.TotalPrize,
            data.Source,
            data.CreatedAt,
            data.UpdatedAt,
            prizeRanges);
    }
}
