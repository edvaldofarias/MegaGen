using MegaSenaHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MegaSenaHub.Infrastructure.Factories;

/// <summary>
/// Fábrica usada pelas ferramentas de design-time do EF Core (dotnet ef migrations add).
/// Necessária até que o projeto MegaSenaHub.Api seja criado na Etapa 4.
/// </summary>
public sealed class MegaSenaHubDbContextFactory : IDesignTimeDbContextFactory<MegaSenaHubDbContext>
{
    public MegaSenaHubDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MegaSenaHubDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=megasena_hub;Username=megasena;Password=megasena123",
                o => o.MigrationsAssembly(typeof(MegaSenaHubDbContext).Assembly.GetName().Name))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new MegaSenaHubDbContext(options);
    }
}
