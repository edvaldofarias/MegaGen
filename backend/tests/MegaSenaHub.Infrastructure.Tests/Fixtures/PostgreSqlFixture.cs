using MegaSenaHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace MegaSenaHub.Infrastructure.Tests.Fixtures;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("megasena_hub_test")
        .WithUsername("megasena")
        .WithPassword("megasena123")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public MegaSenaHubDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MegaSenaHubDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        return new MegaSenaHubDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
