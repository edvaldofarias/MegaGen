using System.Reflection;
using MegaSenaHub.Infrastructure.Data.Models;
using MegaSenaHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MegaSenaHub.Infrastructure.Data;

public sealed class MegaSenaHubDbContext : IdentityDbContext<ApplicationUser>
{
    public MegaSenaHubDbContext(DbContextOptions<MegaSenaHubDbContext> options) : base(options) { }

    internal DbSet<LotteryContestData> LotteryContests => Set<LotteryContestData>();
    internal DbSet<LotteryContestNumberData> LotteryContestNumbers => Set<LotteryContestNumberData>();
    internal DbSet<PrizeRangeData> PrizeRanges => Set<PrizeRangeData>();
    internal DbSet<UserBetData> UserBets => Set<UserBetData>();
    internal DbSet<UserBetNumberData> UserBetNumbers => Set<UserBetNumberData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
