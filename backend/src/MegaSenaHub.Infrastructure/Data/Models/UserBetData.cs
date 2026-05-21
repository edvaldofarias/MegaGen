namespace MegaSenaHub.Infrastructure.Data.Models;

internal sealed class UserBetData
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int ContestNumber { get; set; }
    public decimal AmountPaid { get; set; }
    public int Status { get; set; }
    public decimal PrizeWon { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CheckedAt { get; set; }

    public List<UserBetNumberData> Numbers { get; set; } = [];
}
