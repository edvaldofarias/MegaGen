using FluentAssertions;
using MegaSenaHub.Domain.Entities;
using MegaSenaHub.Domain.Enums;
using MegaSenaHub.Infrastructure.Repositories;
using MegaSenaHub.Infrastructure.Tests.Fixtures;

namespace MegaSenaHub.Infrastructure.Tests.Repositories;

public sealed class UserBetRepositoryTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public UserBetRepositoryTests(PostgreSqlFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task AddAsync_ShouldPersistBetWithNumbers()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new UserBetRepository(context);
        var userId = Guid.NewGuid();
        var bet = UserBet.Create(userId, 2001, [1, 2, 3, 4, 5, 6], 5.00m);

        // Act
        await repository.AddAsync(bet, CancellationToken.None);

        // Assert
        await using var readContext = _fixture.CreateDbContext();
        var readRepository = new UserBetRepository(readContext);
        var persisted = await readRepository.GetByIdAsync(bet.Id, userId.ToString(), CancellationToken.None);

        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(bet.Id);
        persisted.Numbers.AsReadOnly().Select(n => n.Value).Should().BeEquivalentTo([1, 2, 3, 4, 5, 6]);
        persisted.Status.Should().Be(UserBetStatus.Pending);
        persisted.AmountPaid.Should().Be(5.00m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBet_WhenBelongsToUser()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new UserBetRepository(context);
        var userId = Guid.NewGuid();
        var bet = UserBet.Create(userId, 2002, [7, 8, 9, 10, 11, 12], 5.00m);
        await repository.AddAsync(bet, CancellationToken.None);

        // Act
        await using var readContext = _fixture.CreateDbContext();
        var readRepository = new UserBetRepository(readContext);
        var result = await readRepository.GetByIdAsync(bet.Id, userId.ToString(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(bet.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserIdDoesNotMatch()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new UserBetRepository(context);
        var userId = Guid.NewGuid();
        var bet = UserBet.Create(userId, 2003, [13, 14, 15, 16, 17, 18], 5.00m);
        await repository.AddAsync(bet, CancellationToken.None);

        // Act
        await using var readContext = _fixture.CreateDbContext();
        var readRepository = new UserBetRepository(readContext);
        var result = await readRepository.GetByIdAsync(bet.Id, Guid.NewGuid().ToString(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnAllBets_ForGivenUser()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new UserBetRepository(context);
        var userId = Guid.NewGuid();
        var bet1 = UserBet.Create(userId, 2004, [19, 20, 21, 22, 23, 24], 5.00m);
        var bet2 = UserBet.Create(userId, 2005, [25, 26, 27, 28, 29, 30], 7.50m);
        await repository.AddAsync(bet1, CancellationToken.None);
        await repository.AddAsync(bet2, CancellationToken.None);

        // Act
        await using var readContext = _fixture.CreateDbContext();
        var readRepository = new UserBetRepository(readContext);
        var bets = await readRepository.GetByUserIdAsync(userId.ToString(), CancellationToken.None);

        // Assert
        bets.Should().HaveCount(2);
        bets.Should().AllSatisfy(b => b.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdate_Status_CheckedAt_AndPrizeWon()
    {
        // Arrange
        await using var addContext = _fixture.CreateDbContext();
        var addRepository = new UserBetRepository(addContext);
        var userId = Guid.NewGuid();
        var bet = UserBet.Create(userId, 2006, [31, 32, 33, 34, 35, 36], 5.00m);
        await addRepository.AddAsync(bet, CancellationToken.None);

        // Reconstitui e verifica contra um concurso para alterar o status
        await using var loadContext = _fixture.CreateDbContext();
        var loadRepository = new UserBetRepository(loadContext);
        var loaded = await loadRepository.GetByIdAsync(bet.Id, userId.ToString(), CancellationToken.None);
        var contest = LotteryContest.Create(2006, new DateOnly(2024, 2, 1), [31, 32, 33, 34, 35, 36]);
        var checkedAt = DateTimeOffset.UtcNow;
        loaded!.CheckAgainstContest(contest, checkedAt, 50_000_000m);

        // Act
        await using var updateContext = _fixture.CreateDbContext();
        var updateRepository = new UserBetRepository(updateContext);
        await updateRepository.UpdateAsync(loaded, CancellationToken.None);

        // Assert
        await using var readContext = _fixture.CreateDbContext();
        var readRepository = new UserBetRepository(readContext);
        var updated = await readRepository.GetByIdAsync(bet.Id, userId.ToString(), CancellationToken.None);

        updated.Should().NotBeNull();
        updated!.Status.Should().Be(UserBetStatus.WonSena);
        updated.PrizeWon.Should().Be(50_000_000m);
        updated.CheckedAt.Should().NotBeNull();
    }
}
