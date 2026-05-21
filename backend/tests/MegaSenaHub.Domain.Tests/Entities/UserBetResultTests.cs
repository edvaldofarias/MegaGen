using FluentAssertions;
using MegaSenaHub.Domain.Entities;
using MegaSenaHub.Domain.Exceptions;

namespace MegaSenaHub.Domain.Tests.Entities;

public sealed class UserBetResultTests
{
    private static readonly Guid UserBetId = Guid.NewGuid();
    private static readonly Guid ContestId = Guid.NewGuid();
    private static readonly DateTimeOffset CheckedAt = DateTimeOffset.UtcNow;

    // decimal literals não são permitidos em [InlineData], por isso usamos double e convertemos.
    [Theory]
    [InlineData(0, 0.0)]
    [InlineData(3, 0.0)]
    [InlineData(4, 1500.0)]
    [InlineData(5, 25000.0)]
    [InlineData(6, 50_000_000.0)]
    public void Create_ShouldCreateResult_WhenDataIsValid(int hits, double prizeAmountDouble)
    {
        // Arrange
        var prizeAmount = (decimal)prizeAmountDouble;

        // Act
        var result = UserBetResult.Create(UserBetId, ContestId, hits, prizeAmount, CheckedAt);

        // Assert
        result.Hits.Should().Be(hits);
        result.PrizeAmount.Should().Be(prizeAmount);
        result.CheckedAt.Should().Be(CheckedAt);
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_ShouldThrowDomainException_WhenHitsIsLessThanZero(int hits)
    {
        // Arrange & Act
        Action act = () => UserBetResult.Create(UserBetId, ContestId, hits, prizeAmount: 0m, CheckedAt);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(7)]
    [InlineData(100)]
    public void Create_ShouldThrowDomainException_WhenHitsIsGreaterThanSix(int hits)
    {
        // Arrange & Act
        Action act = () => UserBetResult.Create(UserBetId, ContestId, hits, prizeAmount: 0m, CheckedAt);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenPrizeAmountIsNegative()
    {
        // Arrange & Act
        Action act = () => UserBetResult.Create(UserBetId, ContestId, hits: 6, prizeAmount: -1m, CheckedAt);

        // Assert
        act.Should().Throw<DomainException>();
    }
}
