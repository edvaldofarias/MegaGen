using MegaSenaHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MegaSenaHub.Infrastructure.Data.Configurations;

internal sealed class LotteryContestNumberConfiguration : IEntityTypeConfiguration<LotteryContestNumberData>
{
    public void Configure(EntityTypeBuilder<LotteryContestNumberData> builder)
    {
        builder.ToTable("lottery_contest_numbers");

        builder.HasKey(n => new { n.ContestId, n.Number });

        builder.Property(n => n.ContestId).IsRequired();
        builder.Property(n => n.Number).IsRequired();
    }
}
