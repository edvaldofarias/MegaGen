using FluentAssertions;
using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.Exceptions;
using MegaSenaHub.Application.Queries;
using MegaSenaHub.Application.UseCases;
using MegaSenaHub.Domain.Entities;
using NSubstitute;

namespace MegaSenaHub.Application.Tests.UseCases;

public sealed class GetUserBetSummaryUseCaseTests
{
    private readonly IUserBetRepository _betRepository = Substitute.For<IUserBetRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly GetUserBetSummaryUseCase _useCase;
    private static readonly CancellationToken Ct = CancellationToken.None;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateOnly DrawDate = new(2023, 10, 14);

    public GetUserBetSummaryUseCaseTests()
    {
        _useCase = new GetUserBetSummaryUseCase(_betRepository, _currentUser);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(UserId.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _currentUser.IsAuthenticated.Returns(false);

        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(new GetUserBetSummaryQuery(), Ct);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnZeroSummary_WhenUserHasNoBets()
    {
        // Arrange
        _betRepository.GetByUserIdAsync(UserId.ToString(), Ct)
            .Returns(Array.Empty<UserBet>());

        // Act
        var result = await _useCase.ExecuteAsync(new GetUserBetSummaryQuery(), Ct);

        // Assert
        result.TotalBets.Should().Be(0);
        result.TotalSpent.Should().Be(0);
        result.TotalWon.Should().Be(0);
        result.Balance.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateTotalSpent_WhenUserHasBets()
    {
        // Arrange
        var bet1 = UserBet.Create(UserId, 2800, [1, 2, 3, 4, 5, 6], 10m);
        var bet2 = UserBet.Create(UserId, 2801, [7, 8, 9, 10, 11, 12], 15m);

        _betRepository.GetByUserIdAsync(UserId.ToString(), Ct)
            .Returns(new[] { bet1, bet2 });

        // Act
        var result = await _useCase.ExecuteAsync(new GetUserBetSummaryQuery(), Ct);

        // Assert
        result.TotalSpent.Should().Be(25m);
        result.TotalBets.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateTotalWon_WhenUserHasWinningBets()
    {
        // Arrange
        var bet = UserBet.Create(UserId, 2800, [1, 2, 3, 4, 5, 6], 10m);
        var contest = LotteryContest.Create(2800, DrawDate, [1, 2, 3, 4, 5, 6]);
        bet.CheckAgainstContest(contest, DateTimeOffset.UtcNow, 50_000_000m);

        _betRepository.GetByUserIdAsync(UserId.ToString(), Ct)
            .Returns(new[] { bet });

        // Act
        var result = await _useCase.ExecuteAsync(new GetUserBetSummaryQuery(), Ct);

        // Assert
        result.TotalWon.Should().Be(50_000_000m);
        result.WinningBets.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateBalance_WhenUserHasSpentAndWon()
    {
        // Arrange
        var bet = UserBet.Create(UserId, 2800, [1, 2, 3, 4, 5, 6], 10m);
        var contest = LotteryContest.Create(2800, DrawDate, [1, 2, 3, 4, 5, 6]);
        bet.CheckAgainstContest(contest, DateTimeOffset.UtcNow, 1500m);

        _betRepository.GetByUserIdAsync(UserId.ToString(), Ct)
            .Returns(new[] { bet });

        // Act
        var result = await _useCase.ExecuteAsync(new GetUserBetSummaryQuery(), Ct);

        // Assert
        result.Balance.Should().Be(1490m); // 1500 - 10
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCountWinningLosingAndPendingBets_Correctly()
    {
        // Arrange
        var pending = UserBet.Create(UserId, 2799, [1, 2, 3, 4, 5, 6], 5m);

        var won = UserBet.Create(UserId, 2800, [1, 2, 3, 4, 5, 6], 5m);
        var contestWon = LotteryContest.Create(2800, DrawDate, [1, 2, 3, 4, 5, 6]);
        won.CheckAgainstContest(contestWon, DateTimeOffset.UtcNow, 50_000_000m);

        var lost = UserBet.Create(UserId, 2801, [7, 8, 9, 10, 11, 12], 5m);
        var contestLost = LotteryContest.Create(2801, DrawDate, [1, 2, 3, 4, 5, 6]);
        lost.CheckAgainstContest(contestLost, DateTimeOffset.UtcNow, 0m);

        _betRepository.GetByUserIdAsync(UserId.ToString(), Ct)
            .Returns(new[] { pending, won, lost });

        // Act
        var result = await _useCase.ExecuteAsync(new GetUserBetSummaryQuery(), Ct);

        // Assert
        result.TotalBets.Should().Be(3);
        result.WinningBets.Should().Be(1);
        result.LosingBets.Should().Be(1);
        result.PendingBets.Should().Be(1);
        result.BestHits.Should().Be(6);
    }
}
