namespace MegaSenaHub.Infrastructure.Data.Models;

internal sealed class LotteryContestData
{
    public Guid Id { get; set; }
    public int ContestNumber { get; set; }
    public DateOnly DrawDate { get; set; }
    public bool Accumulated { get; set; }
    public decimal TotalPrize { get; set; }
    public string Source { get; set; } = string.Empty;
    public string CombinationHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public List<LotteryContestNumberData> Numbers { get; set; } = [];
    public List<PrizeRangeData> PrizeRanges { get; set; } = [];
}
