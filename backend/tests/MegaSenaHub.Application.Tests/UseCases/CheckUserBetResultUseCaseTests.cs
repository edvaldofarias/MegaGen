using FluentAssertions;
using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.Exceptions;
using MegaSenaHub.Application.UseCases;
using MegaSenaHub.Domain.Entities;
using MegaSenaHub.Domain.Enums;
using NSubstitute;

namespace MegaSenaHub.Application.Tests.UseCases;

public sealed class CheckUserBetResultUseCaseTests
{
    private readonly IUserBetRepository _betRepository = Substitute.For<IUserBetRepository>();
    private readonly IContestRepository _contestRepository = Substitute.For<IContestRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly CheckUserBetResultUseCase _useCase;
    private static readonly CancellationToken Ct = CancellationToken.None;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateOnly DrawDate = new(2023, 10, 14);

    public CheckUserBetResultUseCaseTests()
    {
        _useCase = new CheckUserBetResultUseCase(_betRepository, _contestRepository, _currentUser, _clock);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(UserId.ToString());
        _clock.UtcNow.Returns(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _currentUser.IsAuthenticated.Returns(false);

        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(
            new CheckUserBetResultCommand(Guid.NewGuid()), Ct);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenBetDoesNotExist()
    {
        // Arrange
        var betId = Guid.NewGuid();
        _betRepository.GetByIdAsync(betId, UserId.ToString(), Ct)
            .Returns((UserBet?)null);

        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(new CheckUserBetResultCommand(betId), Ct);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFound_WhenContestDoesNotExist()
    {
        // Arrange
        var bet = UserBet.Create(UserId, 2800, [1, 2, 3, 4, 5, 6], 5m);

        _betRepository.GetByIdAsync(bet.Id, UserId.ToString(), Ct).Returns(bet);
        _contestRepository.GetByContestNumberAsync(2800, Ct).Returns((LotteryContest?)null);

        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(new CheckUserBetResultCommand(bet.Id), Ct);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3, 4, 5, 6 }, new[] { 1, 2, 3, 4, 5, 6 }, "WonSena")]
    [InlineData(new[] { 1, 2, 3, 4, 5, 7 }, new[] { 1, 2, 3, 4, 5, 6 }, "WonQuina")]
    [InlineData(new[] { 1, 2, 3, 4, 7, 8 }, new[] { 1, 2, 3, 4, 5, 6 }, "WonQuadra")]
    [InlineData(new[] { 1, 2, 3, 7, 8, 9 }, new[] { 1, 2, 3, 4, 5, 6 }, "Lost")]
    public async Task ExecuteAsync_ShouldSetCorrectStatus_BasedOnHits(
        int[] betNumbers, int[] drawnNumbers, string expectedStatus)
    {
        // Arrange
        var bet = UserBet.Create(UserId, 2800, betNumbers, 5m);
        var contest = LotteryContest.Create(2800, DrawDate, drawnNumbers);

        _betRepository.GetByIdAsync(bet.Id, UserId.ToString(), Ct).Returns(bet);
        _contestRepository.GetByContestNumberAsync(2800, Ct).Returns(contest);

        // Act
        var result = await _useCase.ExecuteAsync(new CheckUserBetResultCommand(bet.Id), Ct);

        // Assert
        result.Status.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdateCheckedAtUsingClock()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero);
        _clock.UtcNow.Returns(fixedTime);

        var bet = UserBet.Create(UserId, 2800, [1, 2, 3, 4, 5, 6], 5m);
        var contest = LotteryContest.Create(2800, DrawDate, [7, 8, 9, 10, 11, 12]);

        _betRepository.GetByIdAsync(bet.Id, UserId.ToString(), Ct).Returns(bet);
        _contestRepository.GetByContestNumberAsync(2800, Ct).Returns(contest);

        // Act
        await _useCase.ExecuteAsync(new CheckUserBetResultCommand(bet.Id), Ct);

        // Assert — a instância do bet foi modificada in-place pelo use case
        bet.CheckedAt.Should().Be(fixedTime);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetPrizeWon_WhenBetMatchesPrizeRange()
    {
        // Arrange
        var bet = UserBet.Create(UserId, 2800, [1, 2, 3, 4, 5, 6], 5m);
        var contest = LotteryContest.Create(2800, DrawDate, [1, 2, 3, 4, 5, 6]);
        contest.AddPrizeRange(PrizeRange.Create(contest.Id, 6, 1, 50_000_000m));

        _betRepository.GetByIdAsync(bet.Id, UserId.ToString(), Ct).Returns(bet);
        _contestRepository.GetByContestNumberAsync(2800, Ct).Returns(contest);

        // Act
        await _useCase.ExecuteAsync(new CheckUserBetResultCommand(bet.Id), Ct);

        // Assert
        bet.PrizeWon.Should().Be(50_000_000m);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallUpdateAsync_AfterCheckingBet()
    {
        // Arrange
        var bet = UserBet.Create(UserId, 2800, [1, 2, 3, 4, 5, 6], 5m);
        var contest = LotteryContest.Create(2800, DrawDate, [7, 8, 9, 10, 11, 12]);

        _betRepository.GetByIdAsync(bet.Id, UserId.ToString(), Ct).Returns(bet);
        _contestRepository.GetByContestNumberAsync(2800, Ct).Returns(contest);

        // Act
        await _useCase.ExecuteAsync(new CheckUserBetResultCommand(bet.Id), Ct);

        // Assert
        await _betRepository.Received(1).UpdateAsync(bet, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnZeroPrize_WhenBetIsLost()
    {
        // Arrange
        var bet = UserBet.Create(UserId, 2800, [1, 2, 3, 7, 8, 9], 5m);
        var contest = LotteryContest.Create(2800, DrawDate, [10, 11, 12, 13, 14, 15]);

        _betRepository.GetByIdAsync(bet.Id, UserId.ToString(), Ct).Returns(bet);
        _contestRepository.GetByContestNumberAsync(2800, Ct).Returns(contest);

        // Act
        var result = await _useCase.ExecuteAsync(new CheckUserBetResultCommand(bet.Id), Ct);

        // Assert
        result.Status.Should().Be("Lost");
        bet.PrizeWon.Should().Be(0m);
    }
}
