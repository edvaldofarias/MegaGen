using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Domain.Entities;
using MegaSenaHub.Domain.Enums;
using MegaSenaHub.Infrastructure.Data;
using MegaSenaHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaSenaHub.Infrastructure.Repositories;

internal sealed class UserBetRepository : IUserBetRepository
{
    private readonly MegaSenaHubDbContext _context;

    public UserBetRepository(MegaSenaHubDbContext context) => _context = context;

    public async Task AddAsync(UserBet bet, CancellationToken cancellationToken)
    {
        var data = ToData(bet);
        _context.UserBets.Add(data);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserBet?> GetByIdAsync(Guid id, string userId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return null;

        var data = await _context.UserBets
            .Include(b => b.Numbers)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userGuid, cancellationToken);

        return data is null ? null : ToDomain(data);
    }

    public async Task<IReadOnlyCollection<UserBet>> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return [];

        var data = await _context.UserBets
            .Include(b => b.Numbers)
            .AsNoTracking()
            .Where(b => b.UserId == userGuid)
            .ToListAsync(cancellationToken);

        return data.ConvertAll(ToDomain).AsReadOnly();
    }

    public async Task UpdateAsync(UserBet bet, CancellationToken cancellationToken)
    {
        var data = await _context.UserBets
            .FirstOrDefaultAsync(b => b.Id == bet.Id, cancellationToken);

        if (data is null)
            return;

        data.Status = (int)bet.Status;
        data.CheckedAt = bet.CheckedAt;
        data.PrizeWon = bet.PrizeWon;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static UserBetData ToData(UserBet bet) =>
        new()
        {
            Id = bet.Id,
            UserId = bet.UserId,
            ContestNumber = bet.ContestNumber,
            AmountPaid = bet.AmountPaid,
            Status = (int)bet.Status,
            PrizeWon = bet.PrizeWon,
            CreatedAt = bet.CreatedAt,
            CheckedAt = bet.CheckedAt,
            Numbers = [.. bet.Numbers.AsReadOnly().Select(n =>
                new UserBetNumberData
                {
                    UserBetId = bet.Id,
                    Number = n.Value
                })]
        };

    private static UserBet ToDomain(UserBetData data) =>
        UserBet.Reconstitute(
            data.Id,
            data.UserId,
            data.ContestNumber,
            data.Numbers.Select(n => n.Number),
            data.AmountPaid,
            data.CreatedAt,
            data.CheckedAt,
            (UserBetStatus)data.Status,
            data.PrizeWon);
}
