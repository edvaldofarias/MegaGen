using System.Reflection;
using MegaSenaHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MegaSenaHub.Infrastructure.Data;

public sealed class MegaSenaHubDbContext : DbContext
{
    public MegaSenaHubDbContext(DbContextOptions<MegaSenaHubDbContext> options) : base(options) { }

    internal DbSet<LotteryContestData> LotteryContests => Set<LotteryContestData>();
    internal DbSet<LotteryContestNumberData> LotteryContestNumbers => Set<LotteryContestNumberData>();
    internal DbSet<PrizeRangeData> PrizeRanges => Set<PrizeRangeData>();
    internal DbSet<UserBetData> UserBets => Set<UserBetData>();
    internal DbSet<UserBetNumberData> UserBetNumbers => Set<UserBetNumberData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
