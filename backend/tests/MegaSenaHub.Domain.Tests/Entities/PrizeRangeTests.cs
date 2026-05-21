using FluentAssertions;
using MegaSenaHub.Domain.Entities;
using MegaSenaHub.Domain.Exceptions;

namespace MegaSenaHub.Domain.Tests.Entities;

public sealed class PrizeRangeTests
{
    private static readonly Guid ContestId = Guid.NewGuid();

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Create_ShouldCreatePrizeRange_WhenHitsIsFourFiveOrSix(int hits)
    {
        // Arrange & Act
        var range = PrizeRange.Create(ContestId, hits, winners: 10, prizeAmount: 500m);

        // Assert
        range.Hits.Should().Be(hits);
        range.Winners.Should().Be(10);
        range.PrizeAmount.Should().Be(500m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(-1)]
    public void Create_ShouldThrowDomainException_WhenHitsIsInvalid(int hits)
    {
        // Arrange & Act
        Action act = () => PrizeRange.Create(ContestId, hits, winners: 0, prizeAmount: 0m);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenWinnersIsNegative()
    {
        // Arrange & Act
        Action act = () => PrizeRange.Create(ContestId, hits: 6, winners: -1, prizeAmount: 0m);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenPrizeAmountIsNegative()
    {
        // Arrange & Act
        Action act = () => PrizeRange.Create(ContestId, hits: 6, winners: 0, prizeAmount: -1m);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldAllowZeroWinnersAndZeroPrize_WhenNoOneWon()
    {
        // Arrange & Act
        var range = PrizeRange.Create(ContestId, hits: 6, winners: 0, prizeAmount: 0m);

        // Assert
        range.Winners.Should().Be(0);
        range.PrizeAmount.Should().Be(0m);
    }
}
