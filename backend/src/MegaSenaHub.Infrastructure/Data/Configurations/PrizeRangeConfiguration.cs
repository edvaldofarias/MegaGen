using MegaSenaHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MegaSenaHub.Infrastructure.Data.Configurations;

internal sealed class PrizeRangeConfiguration : IEntityTypeConfiguration<PrizeRangeData>
{
    public void Configure(EntityTypeBuilder<PrizeRangeData> builder)
    {
        builder.ToTable("prize_ranges");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ContestId).IsRequired();
        builder.Property(p => p.Hits).IsRequired();
        builder.Property(p => p.Winners).IsRequired();

        builder.Property(p => p.PrizeAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(p => new { p.ContestId, p.Hits }).IsUnique();
    }
}
