using FluentAssertions;
using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.UseCases;
using MegaSenaHub.Domain.Entities;
using MegaSenaHub.Domain.Enums;
using NSubstitute;

namespace MegaSenaHub.Application.Tests.UseCases;

public sealed class GenerateMegaSenaGamesUseCaseTests
{
    private readonly IContestRepository _contestRepository = Substitute.For<IContestRepository>();
    private readonly GenerateMegaSenaGamesUseCase _useCase;
    private static readonly CancellationToken Ct = CancellationToken.None;

    public GenerateMegaSenaGamesUseCaseTests()
    {
        _useCase = new GenerateMegaSenaGamesUseCase(_contestRepository);

        // Padrão: hash não existe, sem concursos relacionados
        _contestRepository
            .CombinationHashExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _contestRepository
            .FindContestsWithAnyNumbersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns([]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateCorrectQuantity_WhenQuantityIsValid()
    {
        // Arrange
        var command = new GenerateMegaSenaGamesCommand(5, 6, GameGenerationStrategy.Random, false, false);

        // Act
        var result = await _useCase.ExecuteAsync(command, Ct);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateGamesWith6Numbers()
    {
        // Arrange
        var command = new GenerateMegaSenaGamesCommand(3, 6, GameGenerationStrategy.Random, false, false);

        // Act
        var result = await _useCase.ExecuteAsync(command, Ct);

        // Assert
        result.All(g => g.Numbers.Count == 6).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateNumbersBetween1And60()
    {
        // Arrange
        var command = new GenerateMegaSenaGamesCommand(3, 6, GameGenerationStrategy.Random, false, false);

        // Act
        var result = await _useCase.ExecuteAsync(command, Ct);

        // Assert
        result.SelectMany(g => g.Numbers).All(n => n >= 1 && n <= 60).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldGenerateNoDuplicatesWithinGame()
    {
        // Arrange
        var command = new GenerateMegaSenaGamesCommand(10, 6, GameGenerationStrategy.Random, false, false);

        // Act
        var result = await _useCase.ExecuteAsync(command, Ct);

        // Assert
        result.All(g => g.Numbers.Distinct().Count() == 6).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNumbersInAscendingOrder()
    {
        // Arrange
        var command = new GenerateMegaSenaGamesCommand(5, 6, GameGenerationStrategy.Random, false, false);

        // Act
        var result = await _useCase.ExecuteAsync(command, Ct);

        // Assert
        result.All(g =>
        {
            var nums = g.Numbers.ToList();
            return nums.SequenceEqual(nums.OrderBy(n => n));
        }).Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task ExecuteAsync_ShouldThrow_WhenQuantityIsOutOfRange(int quantity)
    {
        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(
            new GenerateMegaSenaGamesCommand(quantity, 6, GameGenerationStrategy.Random, false, false), Ct);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(7)]
    public async Task ExecuteAsync_ShouldThrow_WhenNumbersPerGameIsNot6(int numbersPerGame)
    {
        // Act
        Func<Task> act = () => _useCase.ExecuteAsync(
            new GenerateMegaSenaGamesCommand(1, numbersPerGame, GameGenerationStrategy.Random, false, false), Ct);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseMostDrawnNumbers_WhenStrategyIsMostDrawn()
    {
        // Arrange — números 1-6 têm frequência 100; os demais têm frequência 1
        var frequencies = Enumerable.Range(1, 60)
            .Select(n => new NumberFrequencyDto(n, n <= 6 ? 100 : 1))
            .ToList()
            .AsReadOnly();

        _contestRepository.GetNumberFrequenciesAsync(Ct).Returns(frequencies);

        var command = new GenerateMegaSenaGamesCommand(
            1, 6, GameGenerationStrategy.MostDrawn, false, false);

        // Act
        var result = await _useCase.ExecuteAsync(command, Ct);

        // Assert
        result.First().Numbers.All(n => n <= 6).Should().BeTrue(
            "estratégia MostDrawn deve preferir os números mais frequentes");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseLeastDrawnNumbers_WhenStrategyIsLeastDrawn()
    {
        // Arrange — números 55-60 têm frequência 0; os demais têm frequência 100
        var frequencies = Enumerable.Range(1, 60)
            .Select(n => new NumberFrequencyDto(n, n >= 55 ? 0 : 100))
            .ToList()
            .AsReadOnly();

        _contestRepository.GetNumberFrequenciesAsync(Ct).Returns(frequencies);

        var command = new GenerateMegaSenaGamesCommand(
            1, 6, GameGenerationStrategy.LeastDrawn, false, false);

        // Act
        var result = await _useCase.ExecuteAsync(command, Ct);

        // Assert
        result.First().Numbers.All(n => n >= 55).Should().BeTrue(
            "estratégia LeastDrawn deve preferir os números menos frequentes");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRegenerateGame_WhenCombinationIsAlreadyDrawn()
    {
        // Arrange — primeira chamada retorna true (hash existe), segunda retorna false
        _contestRepository
            .CombinationHashExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true, false);

        var command = new GenerateMegaSenaGamesCommand(
            1, 6, GameGenerationStrategy.Random, AvoidAlreadyDrawnCombination: true, false);

        // Act
        var result = await _useCase.ExecuteAsync(command, Ct);

        // Assert
        result.First().AlreadyDrawn.Should().BeFalse(
            "a combinação retornada não deve estar no histórico de sorteios");

        await _contestRepository.Received()
            .CombinationHashExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAlreadyWonFalse_WhenHistoryIsEmpty()
    {
        // Arrange — nenhum concurso no histórico → alreadyWon sempre false
        _contestRepository
            .FindContestsWithAnyNumbersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new GenerateMegaSenaGamesCommand(
            1, 6, GameGenerationStrategy.Random, false, AvoidAlreadyWonCombination: true);

        // Act
        var result = await _useCase.ExecuteAsync(command, Ct);

        // Assert
        result.First().AlreadyWon.Should().BeFalse(
            "sem histórico não há combinação vencedora anterior");

        await _contestRepository.Received()
            .FindContestsWithAnyNumbersAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>());
    }
}
