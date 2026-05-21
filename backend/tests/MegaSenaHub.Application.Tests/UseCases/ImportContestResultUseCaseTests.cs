using FluentAssertions;
using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.UseCases;
using MegaSenaHub.Domain.Entities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MegaSenaHub.Application.Tests.UseCases;

public sealed class ImportContestResultUseCaseTests
{
    private readonly IContestRepository _contestRepository = Substitute.For<IContestRepository>();
    private readonly ILotteryResultProvider _provider = Substitute.For<ILotteryResultProvider>();
    private readonly ImportContestResultUseCase _useCase;
    private static readonly CancellationToken Ct = CancellationToken.None;

    public ImportContestResultUseCaseTests()
    {
        _useCase = new ImportContestResultUseCase(_contestRepository, _provider);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAlreadyExists_WhenContestAlreadyImported()
    {
        // Arrange
        _contestRepository.ExistsAsync(2800, Ct).Returns(true);

        // Act
        var result = await _useCase.ExecuteAsync(new ImportContestResultCommand(2800), Ct);

        // Assert
        result.Imported.Should().BeFalse();
        result.AlreadyExists.Should().BeTrue();
        result.ContestNumber.Should().Be(2800);

        await _provider.DidNotReceive()
            .GetContestResultAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _contestRepository.DidNotReceive()
            .AddAsync(Arg.Any<LotteryContest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldImportContest_WhenContestDoesNotExist()
    {
        // Arrange
        var dto = new LotteryContestResultDto(
            ContestNumber: 2800,
            DrawDate: new DateTime(2023, 10, 14),
            DrawnNumbers: [3, 12, 25, 37, 48, 56],
            Accumulated: false,
            TotalPrize: 50_000_000m,
            Source: "caixa",
            PrizeRanges:
            [
                new PrizeRangeDto(6, 1, 40_000_000m),
                new PrizeRangeDto(5, 50, 50_000m),
                new PrizeRangeDto(4, 1200, 1_500m)
            ]);

        _contestRepository.ExistsAsync(2800, Ct).Returns(false);
        _provider.GetContestResultAsync(2800, Ct).Returns(dto);

        // Act
        var result = await _useCase.ExecuteAsync(new ImportContestResultCommand(2800), Ct);

        // Assert
        result.Imported.Should().BeTrue();
        result.AlreadyExists.Should().BeFalse();
        result.ContestNumber.Should().Be(2800);

        await _contestRepository.Received(1)
            .AddAsync(Arg.Is<LotteryContest>(c =>
                c.ContestNumber == 2800 &&
                c.PrizeRanges.Count == 3), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenContestNumberIsZero()
    {
        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(new ImportContestResultCommand(0), Ct);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();

        await _contestRepository.DidNotReceive()
            .ExistsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotSaveContest_WhenProviderThrowsException()
    {
        // Arrange
        _contestRepository.ExistsAsync(2800, Ct).Returns(false);
        _provider.GetContestResultAsync(2800, Ct)
            .Throws(new Exception("Provider unavailable"));

        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(new ImportContestResultCommand(2800), Ct);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Provider unavailable");

        await _contestRepository.DidNotReceive()
            .AddAsync(Arg.Any<LotteryContest>(), Arg.Any<CancellationToken>());
    }
}
