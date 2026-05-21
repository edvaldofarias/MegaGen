using FluentAssertions;
using MegaSenaHub.Domain.Entities;
using MegaSenaHub.Domain.Enums;
using MegaSenaHub.Domain.Exceptions;

namespace MegaSenaHub.Domain.Tests.Entities;

public sealed class UserBetTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly int[] ValidNumbers = [10, 20, 30, 40, 50, 60];
    private static readonly DateOnly DrawDate = new(2024, 1, 13);

    [Fact]
    public void Create_ShouldCreateUserBet_WhenDataIsValid()
    {
        // Arrange & Act
        var bet = UserBet.Create(UserId, contestNumber: 2800, ValidNumbers, amountPaid: 5m);

        // Assert
        bet.UserId.Should().Be(UserId);
        bet.ContestNumber.Should().Be(2800);
        bet.AmountPaid.Should().Be(5m);
        bet.Id.Should().NotBe(Guid.Empty);
        bet.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void Create_ShouldSetStatusAsPending_WhenBetIsCreated()
    {
        // Arrange & Act
        var bet = UserBet.Create(UserId, 2800, ValidNumbers, 5m);

        // Assert
        bet.Status.Should().Be(UserBetStatus.Pending);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenUserIdIsEmpty()
    {
        // Arrange & Act
        Action act = () => UserBet.Create(Guid.Empty, 2800, ValidNumbers, 5m);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowDomainException_WhenContestNumberIsInvalid(int contestNumber)
    {
        // Arrange & Act
        Action act = () => UserBet.Create(UserId, contestNumber, ValidNumbers, 5m);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenNumbersAreInvalid()
    {
        // Arrange — apenas 5 números (menos do que o exigido)
        var invalid = new[] { 1, 2, 3, 4, 5 };

        // Act
        Action act = () => UserBet.Create(UserId, 2800, invalid, 5m);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenAmountPaidIsNegative()
    {
        // Arrange & Act
        Action act = () => UserBet.Create(UserId, 2800, ValidNumbers, amountPaid: -1m);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CheckAgainstContest_ShouldSetStatusWonQuadra_WhenHitsIsFour()
    {
        // Arrange — bet tem 4 acertos: 1, 2, 3, 4
        var bet = UserBet.Create(UserId, 2800, new[] { 1, 2, 3, 4, 7, 8 }, 5m);
        var contest = LotteryContest.Create(2800, DrawDate, new[] { 1, 2, 3, 4, 5, 6 });

        // Act
        bet.CheckAgainstContest(contest);

        // Assert
        bet.Status.Should().Be(UserBetStatus.WonQuadra);
    }

    [Fact]
    public void CheckAgainstContest_ShouldSetStatusWonQuina_WhenHitsIsFive()
    {
        // Arrange — bet tem 5 acertos: 1, 2, 3, 4, 5
        var bet = UserBet.Create(UserId, 2800, new[] { 1, 2, 3, 4, 5, 9 }, 5m);
        var contest = LotteryContest.Create(2800, DrawDate, new[] { 1, 2, 3, 4, 5, 6 });

        // Act
        bet.CheckAgainstContest(contest);

        // Assert
        bet.Status.Should().Be(UserBetStatus.WonQuina);
    }

    [Fact]
    public void CheckAgainstContest_ShouldSetStatusWonSena_WhenHitsIsSix()
    {
        // Arrange — combinação idêntica
        var bet = UserBet.Create(UserId, 2800, new[] { 1, 2, 3, 4, 5, 6 }, 5m);
        var contest = LotteryContest.Create(2800, DrawDate, new[] { 1, 2, 3, 4, 5, 6 });

        // Act
        bet.CheckAgainstContest(contest);

        // Assert
        bet.Status.Should().Be(UserBetStatus.WonSena);
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3, 7, 8, 9 })]   // 3 acertos
    [InlineData(new[] { 7, 8, 9, 10, 11, 12 })] // 0 acertos
    public void CheckAgainstContest_ShouldSetStatusLost_WhenHitsIsLessThanFour(int[] betNumbers)
    {
        // Arrange
        var bet = UserBet.Create(UserId, 2800, betNumbers, 5m);
        var contest = LotteryContest.Create(2800, DrawDate, new[] { 1, 2, 3, 4, 5, 6 });

        // Act
        bet.CheckAgainstContest(contest);

        // Assert
        bet.Status.Should().Be(UserBetStatus.Lost);
    }

    [Fact]
    public void CheckAgainstContest_ShouldSetCheckedAt_WhenBetIsChecked()
    {
        // Arrange
        var bet = UserBet.Create(UserId, 2800, ValidNumbers, 5m);
        var contest = LotteryContest.Create(2800, DrawDate, new[] { 1, 2, 3, 4, 5, 6 });

        bet.CheckedAt.Should().BeNull();

        // Act
        bet.CheckAgainstContest(contest);

        // Assert
        bet.CheckedAt.Should().NotBeNull();
    }
}
