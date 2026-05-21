namespace MegaSenaHub.Infrastructure.Data.Models;

internal sealed class PrizeRangeData
{
    public Guid Id { get; set; }
    public Guid ContestId { get; set; }
    public int Hits { get; set; }
    public int Winners { get; set; }
    public decimal PrizeAmount { get; set; }

    public LotteryContestData Contest { get; set; } = null!;
}
