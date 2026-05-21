using MegaSenaHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MegaSenaHub.Infrastructure.Data.Configurations;

internal sealed class LotteryContestConfiguration : IEntityTypeConfiguration<LotteryContestData>
{
    public void Configure(EntityTypeBuilder<LotteryContestData> builder)
    {
        builder.ToTable("lottery_contests");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ContestNumber).IsRequired();
        builder.HasIndex(c => c.ContestNumber).IsUnique();

        builder.Property(c => c.DrawDate).IsRequired();
        builder.HasIndex(c => c.DrawDate);

        builder.Property(c => c.Accumulated).IsRequired();

        builder.Property(c => c.TotalPrize)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.Source)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.CombinationHash)
            .HasMaxLength(17)
            .IsRequired();
        builder.HasIndex(c => c.CombinationHash).IsUnique();

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasMany(c => c.Numbers)
            .WithOne(n => n.Contest)
            .HasForeignKey(n => n.ContestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.PrizeRanges)
            .WithOne(p => p.Contest)
            .HasForeignKey(p => p.ContestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
