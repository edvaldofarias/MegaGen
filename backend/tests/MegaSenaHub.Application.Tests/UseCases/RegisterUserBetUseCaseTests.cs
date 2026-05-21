using FluentAssertions;
using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.Exceptions;
using MegaSenaHub.Application.UseCases;
using MegaSenaHub.Domain.Enums;
using NSubstitute;

namespace MegaSenaHub.Application.Tests.UseCases;

public sealed class RegisterUserBetUseCaseTests
{
    private readonly IUserBetRepository _repository = Substitute.For<IUserBetRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly RegisterUserBetUseCase _useCase;
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly Guid UserId = Guid.NewGuid();

    public RegisterUserBetUseCaseTests()
    {
        _useCase = new RegisterUserBetUseCase(_repository, _currentUser);
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.UserId.Returns(UserId.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _currentUser.IsAuthenticated.Returns(false);

        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(
            new RegisterUserBetCommand(2800, [1, 2, 3, 4, 5, 6], 5m), Ct);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
        await _repository.DidNotReceive().AddAsync(Arg.Any<Domain.Entities.UserBet>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnDto_WhenCommandIsValid()
    {
        // Arrange
        var command = new RegisterUserBetCommand(2800, [3, 12, 25, 37, 48, 56], 10m);

        // Act
        var result = await _useCase.ExecuteAsync(command, Ct);

        // Assert
        result.Should().NotBeNull();
        result.ContestNumber.Should().Be(2800);
        result.Numbers.Should().BeEquivalentTo(new[] { 3, 12, 25, 37, 48, 56 });
        result.AmountPaid.Should().Be(10m);
        result.Status.Should().Be(nameof(UserBetStatus.Pending));
        result.UserId.Should().Be(UserId.ToString());

        await _repository.Received(1)
            .AddAsync(Arg.Any<Domain.Entities.UserBet>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldThrow_WhenContestNumberIsInvalid(int contestNumber)
    {
        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(
            new RegisterUserBetCommand(contestNumber, [1, 2, 3, 4, 5, 6], 5m), Ct);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _repository.DidNotReceive().AddAsync(Arg.Any<Domain.Entities.UserBet>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenAmountPaidIsNegative()
    {
        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(
            new RegisterUserBetCommand(2800, [1, 2, 3, 4, 5, 6], -1m), Ct);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenNumbersAreInvalid()
    {
        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(
            new RegisterUserBetCommand(2800, [1, 2, 3, 4, 5], 5m), Ct); // apenas 5 números

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
