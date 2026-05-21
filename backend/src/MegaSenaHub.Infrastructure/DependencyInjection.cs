using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Infrastructure.Adapters;
using MegaSenaHub.Infrastructure.Data;
using MegaSenaHub.Infrastructure.Identity;
using MegaSenaHub.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
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

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<MegaSenaHubDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IContestRepository, ContestRepository>();
        services.AddScoped<IUserBetRepository, UserBetRepository>();
        services.AddScoped<IMessagePublisher, NoOpMessagePublisher>();
        services.AddScoped<ILotteryResultProvider, NoOpLotteryResultProvider>();
        services.AddScoped<IAuthService, IdentityAuthService>();
        services.AddScoped<JwtTokenService>();

        return services;
    }
}
