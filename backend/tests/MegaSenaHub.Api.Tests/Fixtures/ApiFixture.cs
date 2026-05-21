using MegaSenaHub.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace MegaSenaHub.Api.Tests.Fixtures;

/// <summary>
/// Fixture de integração que sobe um container PostgreSQL e uma instância real da API.
/// Compartilhada entre todos os testes de uma coleção para economizar tempo de startup.
/// </summary>
public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("megasena_hub_api_test")
        .WithUsername("megasena")
        .WithPassword("megasena123")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Aplica o schema via EF Core (EnsureCreated usa as configurações do DbContext)
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MegaSenaHubDbContext>();
        await db.Database.EnsureCreatedAsync();

        // Seed de roles via RoleManager já acontece no Program.cs (SeedRolesAsync)
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Substitui o DbContext para apontar ao container de teste
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MegaSenaHubDbContext>));

            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<MegaSenaHubDbContext>(options =>
                options
                    .UseNpgsql(_container.GetConnectionString())
                    .UseSnakeCaseNamingConvention());
        });
    }

    async Task IAsyncLifetime.DisposeAsync() => await _container.DisposeAsync();
}
