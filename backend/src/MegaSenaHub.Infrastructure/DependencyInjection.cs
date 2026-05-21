using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Infrastructure.Data;
using MegaSenaHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MegaSenaHub.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra os serviços de Infrastructure no contêiner de DI.
    /// </summary>
    /// <param name="services">O contêiner de serviços.</param>
    /// <param name="connectionString">String de conexão com o PostgreSQL.</param>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<MegaSenaHubDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IContestRepository, ContestRepository>();
        services.AddScoped<IUserBetRepository, UserBetRepository>();

        return services;
    }
}
