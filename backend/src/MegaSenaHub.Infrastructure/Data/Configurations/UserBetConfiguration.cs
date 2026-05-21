using MegaSenaHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MegaSenaHub.Infrastructure.Data.Configurations;

internal sealed class UserBetConfiguration : IEntityTypeConfiguration<UserBetData>
{
    public void Configure(EntityTypeBuilder<UserBetData> builder)
    {
        builder.ToTable("user_bets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.UserId).IsRequired();
        builder.HasIndex(b => b.UserId);

        builder.Property(b => b.ContestNumber).IsRequired();
        builder.HasIndex(b => b.ContestNumber);

        builder.HasIndex(b => new { b.UserId, b.ContestNumber });

        builder.Property(b => b.AmountPaid)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.Status).IsRequired();

        builder.Property(b => b.PrizeWon)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.CreatedAt).IsRequired();

        builder.HasMany(b => b.Numbers)
            .WithOne(n => n.UserBet)
            .HasForeignKey(n => n.UserBetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
