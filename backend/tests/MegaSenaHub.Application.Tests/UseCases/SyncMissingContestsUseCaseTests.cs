using FluentAssertions;
using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.Messaging;
using MegaSenaHub.Application.UseCases;
using NSubstitute;

namespace MegaSenaHub.Application.Tests.UseCases;

public sealed class SyncMissingContestsUseCaseTests
{
    private readonly ILotteryResultProvider _provider = Substitute.For<ILotteryResultProvider>();
    private readonly IContestRepository _contestRepository = Substitute.For<IContestRepository>();
    private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly SyncMissingContestsUseCase _useCase;
    private static readonly CancellationToken Ct = CancellationToken.None;

    public SyncMissingContestsUseCaseTests()
    {
        _useCase = new SyncMissingContestsUseCase(_provider, _contestRepository, _publisher, _clock);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPublishZeroMessages_WhenNoContestsMissing()
    {
        // Arrange
        _provider.GetLatestContestNumberAsync(Ct).Returns(5);
        _contestRepository.GetExistingContestNumbersAsync(Ct)
            .Returns(new[] { 1, 2, 3, 4, 5 }.AsReadOnly<int>());

        // Act
        var result = await _useCase.ExecuteAsync(new SyncMissingContestsCommand(), Ct);

        // Assert
        result.MissingCount.Should().Be(0);
        result.LatestContestNumber.Should().Be(5);
        result.PublishedContestNumbers.Should().BeEmpty();
        await _publisher.DidNotReceive()
            .PublishAsync(Arg.Any<ContestSyncRequestedMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPublishMissingContests_WhenGapsExist()
    {
        // Arrange
        _provider.GetLatestContestNumberAsync(Ct).Returns(10);
        _contestRepository.GetExistingContestNumbersAsync(Ct)
            .Returns(new[] { 1, 2, 3, 7, 8, 10 }.AsReadOnly<int>());

        // Act
        var result = await _useCase.ExecuteAsync(new SyncMissingContestsCommand(), Ct);

        // Assert
        result.MissingCount.Should().Be(4);
        result.PublishedContestNumbers.Should().BeEquivalentTo([4, 5, 6, 9]);
        await _publisher.Received(4)
            .PublishAsync(Arg.Any<ContestSyncRequestedMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPublish1109Messages_WhenDatabaseHasLargeGaps()
    {
        // Arrange — existing: [1..987] + [1002..3002] = 2988 concursos; missing: [988..1001] (14) + [3003..4097] (1095) = 1109
        _provider.GetLatestContestNumberAsync(Ct).Returns(4097);

        var existing = Enumerable.Range(1, 987)
            .Concat(Enumerable.Range(1002, 2001))
            .ToArray()
            .AsReadOnly<int>();
        _contestRepository.GetExistingContestNumbersAsync(Ct).Returns(existing);

        // Act
        var result = await _useCase.ExecuteAsync(new SyncMissingContestsCommand(), Ct);

        // Assert
        result.MissingCount.Should().Be(1109);
        result.PublishedContestNumbers.Should().HaveCount(1109);
        result.PublishedContestNumbers.First().Should().Be(988);
        result.PublishedContestNumbers.Last().Should().Be(4097);

        await _publisher.Received(1109)
            .PublishAsync(Arg.Any<ContestSyncRequestedMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseClockTime_AndSetNonEmptyCorrelationId()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        _clock.UtcNow.Returns(fixedTime);

        _provider.GetLatestContestNumberAsync(Ct).Returns(2);
        _contestRepository.GetExistingContestNumbersAsync(Ct)
            .Returns(new[] { 1 }.AsReadOnly<int>());

        var publishedMessages = new List<ContestSyncRequestedMessage>();
        _publisher
            .When(p => p.PublishAsync(Arg.Any<ContestSyncRequestedMessage>(), Arg.Any<CancellationToken>()))
            .Do(call => publishedMessages.Add(call.Arg<ContestSyncRequestedMessage>()));

        // Act
        await _useCase.ExecuteAsync(new SyncMissingContestsCommand(), Ct);

        // Assert
        publishedMessages.Should().HaveCount(1);
        publishedMessages[0].RequestedAt.Should().Be(fixedTime);
        publishedMessages[0].CorrelationId.Should().NotBe(Guid.Empty);
        publishedMessages[0].LotteryType.Should().Be("MegaSena");
    }
}
