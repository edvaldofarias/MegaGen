using FluentAssertions;
using MegaSenaHub.Domain.Entities;
using MegaSenaHub.Domain.Exceptions;
using MegaSenaHub.Domain.ValueObjects;

namespace MegaSenaHub.Domain.Tests.Entities;

public sealed class LotteryContestTests
{
    private static readonly DateOnly DrawDate = new(2024, 1, 13);
    private static readonly int[] ValidNumbers = [1, 2, 3, 4, 5, 6];

    [Fact]
    public void Create_ShouldCreateContest_WhenDataIsValid()
    {
        // Arrange & Act
        var contest = LotteryContest.Create(2800, DrawDate, ValidNumbers);

        // Assert
        contest.ContestNumber.Should().Be(2800);
        contest.DrawDate.Should().Be(DrawDate);
        contest.DrawnNumbers.Should().NotBeNull();
        contest.Id.Should().NotBe(Guid.Empty);
        contest.CreatedAt.Should().NotBe(default);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowDomainException_WhenContestNumberIsInvalid(int contestNumber)
    {
        // Arrange & Act
        Action act = () => LotteryContest.Create(contestNumber, DrawDate, ValidNumbers);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenDrawnNumbersAreInvalid()
    {
        // Arrange — 7 números (mais do que o permitido)
        var invalid = new[] { 1, 2, 3, 4, 5, 6, 7 };

        // Act
        Action act = () => LotteryContest.Create(2800, DrawDate, invalid);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldGenerateCombinationHash_WhenContestIsCreated()
    {
        // Arrange & Act
        var contest = LotteryContest.Create(2800, DrawDate, new[] { 6, 3, 1, 4, 2, 5 });

        // Assert
        contest.CombinationHash.Value.Should().Be("01-02-03-04-05-06");
    }

    [Fact]
    public void CountHits_ShouldReturnCorrectHits_WhenBetNumbersAreCompared()
    {
        // Arrange
        var contest = LotteryContest.Create(2800, DrawDate, new[] { 1, 2, 3, 4, 5, 6 });
        var bet = MegaSenaNumbers.Create(new[] { 1, 2, 3, 4, 7, 8 });

        // Act
        var hits = contest.CountHits(bet);

        // Assert
        hits.Should().Be(4);
    }

    [Fact]
    public void IsExactCombination_ShouldReturnTrue_WhenCombinationMatchesContest()
    {
        // Arrange
        var contest = LotteryContest.Create(2800, DrawDate, new[] { 1, 2, 3, 4, 5, 6 });
        var exact = MegaSenaNumbers.Create(new[] { 1, 2, 3, 4, 5, 6 });

        // Act
        var result = contest.IsExactCombination(exact);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExactCombination_ShouldReturnFalse_WhenCombinationDiffersFromContest()
    {
        // Arrange
        var contest = LotteryContest.Create(2800, DrawDate, new[] { 1, 2, 3, 4, 5, 6 });
        var different = MegaSenaNumbers.Create(new[] { 1, 2, 3, 4, 5, 7 });

        // Act
        var result = contest.IsExactCombination(different);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AddPrizeRange_ShouldAddPrizeRange_WhenPrizeRangeIsValid()
    {
        // Arrange
        var contest = LotteryContest.Create(2800, DrawDate, ValidNumbers);
        var range = PrizeRange.Create(contest.Id, hits: 6, winners: 1, prizeAmount: 50_000_000m);

        // Act
        contest.AddPrizeRange(range);

        // Assert
        contest.PrizeRanges.Should().HaveCount(1);
        contest.PrizeRanges[0].Hits.Should().Be(6);
    }

    [Fact]
    public void AddPrizeRange_ShouldThrowDomainException_WhenPrizeRangeIsDuplicated()
    {
        // Arrange
        var contest = LotteryContest.Create(2800, DrawDate, ValidNumbers);
        var first = PrizeRange.Create(contest.Id, hits: 6, winners: 1, prizeAmount: 50_000_000m);
        var duplicate = PrizeRange.Create(contest.Id, hits: 6, winners: 2, prizeAmount: 25_000_000m);

        contest.AddPrizeRange(first);

        // Act
        Action act = () => contest.AddPrizeRange(duplicate);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenTotalPrizeIsNegative()
    {
        // Arrange & Act
        Action act = () => LotteryContest.Create(2800, DrawDate, ValidNumbers, totalPrize: -1m);

        // Assert
        act.Should().Throw<DomainException>();
    }
}
