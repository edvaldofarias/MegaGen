using MegaSenaHub.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MegaSenaHub.Infrastructure.Data.Configurations;

internal sealed class UserBetNumberConfiguration : IEntityTypeConfiguration<UserBetNumberData>
{
    public void Configure(EntityTypeBuilder<UserBetNumberData> builder)
    {
        builder.ToTable("user_bet_numbers");

        builder.HasKey(n => new { n.UserBetId, n.Number });

        builder.Property(n => n.UserBetId).IsRequired();
        builder.Property(n => n.Number).IsRequired();
    }
}
