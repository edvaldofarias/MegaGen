using FluentAssertions;
using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.Exceptions;
using MegaSenaHub.Application.Queries;
using MegaSenaHub.Application.UseCases;
using MegaSenaHub.Domain.Entities;
using NSubstitute;

namespace MegaSenaHub.Application.Tests.UseCases;

public sealed class CheckCombinationHistoryUseCaseTests
{
    private readonly IContestRepository _contestRepository = Substitute.For<IContestRepository>();
    private readonly CheckCombinationHistoryUseCase _useCase;
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly DateOnly DrawDate = new(2023, 10, 14);

    public CheckCombinationHistoryUseCaseTests()
    {
        _useCase = new CheckCombinationHistoryUseCase(_contestRepository);
        _contestRepository
            .FindContestsWithAnyNumbersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns([]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowDomainException_WhenNumbersAreInvalid()
    {
        // Arrange
        var query = new CheckCombinationHistoryQuery([1, 2, 3, 4, 5]); // apenas 5 números (inválido para Mega-Sena)

        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(query, Ct);

        // Assert
        await act.Should().ThrowAsync<MegaSenaHub.Domain.Exceptions.DomainException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnExactMatch_WhenCombinationWasDrawn()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5, 6 };
        var contest = LotteryContest.Create(2800, DrawDate, numbers);

        _contestRepository
            .FindContestsWithAnyNumbersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns([contest]);

        // Act
        var result = await _useCase.ExecuteAsync(new CheckCombinationHistoryQuery(numbers), Ct);

        // Assert
        result.ExactCombinationAlreadyDrawn.Should().BeTrue();
        result.ExactCombinationContestNumber.Should().Be(2800);
        result.BestHits.Should().Be(6);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnBestHits_WhenPartialMatchExists()
    {
        // Arrange — aposta: 1,2,3,4,5,6; concurso sorteou 1,2,3,4,5,7 → 5 acertos
        var betNumbers = new[] { 1, 2, 3, 4, 5, 6 };
        var contestNumbers = new[] { 1, 2, 3, 4, 5, 7 };
        var contest = LotteryContest.Create(2800, DrawDate, contestNumbers);

        _contestRepository
            .FindContestsWithAnyNumbersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns([contest]);

        // Act
        var result = await _useCase.ExecuteAsync(new CheckCombinationHistoryQuery(betNumbers), Ct);

        // Assert
        result.ExactCombinationAlreadyDrawn.Should().BeFalse();
        result.BestHits.Should().Be(5);
        result.BestContestNumber.Should().Be(2800);
        result.MatchedNumbers.Should().BeEquivalentTo([1, 2, 3, 4, 5]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPrizeAmount_WhenPrizeRangeExists()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5, 6 };
        var contest = LotteryContest.Create(2800, DrawDate, numbers);
        contest.AddPrizeRange(PrizeRange.Create(contest.Id, 6, 1, 50_000_000m));

        _contestRepository
            .FindContestsWithAnyNumbersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns([contest]);

        // Act
        var result = await _useCase.ExecuteAsync(new CheckCombinationHistoryQuery(numbers), Ct);

        // Assert
        result.PrizeAmount.Should().Be(50_000_000m);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnZeroBestHits_WhenNoMatchExists()
    {
        // Arrange
        _contestRepository
            .FindContestsWithAnyNumbersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _useCase.ExecuteAsync(
            new CheckCombinationHistoryQuery([1, 2, 3, 4, 5, 6]), Ct);

        // Assert
        result.ExactCombinationAlreadyDrawn.Should().BeFalse();
        result.BestHits.Should().Be(0);
        result.BestContestNumber.Should().BeNull();
        result.MatchedNumbers.Should().BeEmpty();
    }
}
