using FluentAssertions;
using MegaSenaHub.Domain.Entities;
using MegaSenaHub.Infrastructure.Repositories;
using MegaSenaHub.Infrastructure.Tests.Fixtures;

namespace MegaSenaHub.Infrastructure.Tests.Repositories;

public sealed class ContestRepositoryTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public ContestRepositoryTests(PostgreSqlFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task AddAsync_ShouldPersistContestWithNumbersAndPrizeRanges()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new ContestRepository(context);
        var contest = LotteryContest.Create(1001, new DateOnly(2024, 1, 10), [1, 2, 3, 4, 5, 6]);
        contest.AddPrizeRange(PrizeRange.Create(contest.Id, 6, 1, 50_000_000m));
        contest.AddPrizeRange(PrizeRange.Create(contest.Id, 5, 10, 25_000m));

        // Act
        await repository.AddAsync(contest, CancellationToken.None);

        // Assert
        await using var readContext = _fixture.CreateDbContext();
        var readRepository = new ContestRepository(readContext);
        var persisted = await readRepository.GetByContestNumberAsync(1001, CancellationToken.None);

        persisted.Should().NotBeNull();
        persisted!.ContestNumber.Should().Be(1001);
        persisted.DrawnNumbers.AsReadOnly().Select(n => n.Value).Should().BeEquivalentTo([1, 2, 3, 4, 5, 6]);
        persisted.PrizeRanges.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenContestExists()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new ContestRepository(context);
        var contest = LotteryContest.Create(1002, new DateOnly(2024, 1, 11), [7, 8, 9, 10, 11, 12]);
        await repository.AddAsync(contest, CancellationToken.None);

        // Act
        await using var readContext = _fixture.CreateDbContext();
        var readRepository = new ContestRepository(readContext);
        var exists = await readRepository.ExistsAsync(1002, CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenContestDoesNotExist()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new ContestRepository(context);

        // Act
        var exists = await repository.ExistsAsync(99999, CancellationToken.None);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByContestNumberAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new ContestRepository(context);

        // Act
        var result = await repository.GetByContestNumberAsync(88888, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetExistingContestNumbersAsync_ShouldContainAddedContestNumber()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new ContestRepository(context);
        var contest = LotteryContest.Create(1003, new DateOnly(2024, 1, 12), [13, 14, 15, 16, 17, 18]);
        await repository.AddAsync(contest, CancellationToken.None);

        // Act
        await using var readContext = _fixture.CreateDbContext();
        var readRepository = new ContestRepository(readContext);
        var numbers = await readRepository.GetExistingContestNumbersAsync(CancellationToken.None);

        // Assert
        numbers.Should().Contain(1003);
    }

    [Fact]
    public async Task CombinationHashExistsAsync_ShouldReturnTrue_WhenHashExists()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new ContestRepository(context);
        var contest = LotteryContest.Create(1004, new DateOnly(2024, 1, 13), [19, 20, 21, 22, 23, 24]);
        await repository.AddAsync(contest, CancellationToken.None);

        // Act
        await using var readContext = _fixture.CreateDbContext();
        var readRepository = new ContestRepository(readContext);
        var exists = await readRepository.CombinationHashExistsAsync(
            contest.CombinationHash.Value, CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task FindContestsWithAnyNumbersAsync_ShouldReturnContests_ContainingMatchingNumbers()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new ContestRepository(context);
        var contest = LotteryContest.Create(1005, new DateOnly(2024, 1, 14), [25, 26, 27, 28, 29, 30]);
        await repository.AddAsync(contest, CancellationToken.None);

        // Act
        await using var readContext = _fixture.CreateDbContext();
        var readRepository = new ContestRepository(readContext);
        var results = await readRepository.FindContestsWithAnyNumbersAsync([25, 26], CancellationToken.None);

        // Assert
        results.Should().ContainSingle(c => c.ContestNumber == 1005);
    }

    [Fact]
    public async Task GetNumberFrequenciesAsync_ShouldReturnFrequencies_ForPersistedNumbers()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new ContestRepository(context);
        var contest = LotteryContest.Create(1006, new DateOnly(2024, 1, 15), [31, 32, 33, 34, 35, 36]);
        await repository.AddAsync(contest, CancellationToken.None);

        // Act
        await using var readContext = _fixture.CreateDbContext();
        var readRepository = new ContestRepository(readContext);
        var frequencies = await readRepository.GetNumberFrequenciesAsync(CancellationToken.None);

        // Assert
        frequencies.Should().NotBeEmpty();
        frequencies.Should().AllSatisfy(f =>
        {
            f.Number.Should().BeInRange(1, 60);
            f.Frequency.Should().BeGreaterThan(0);
        });
    }
}
