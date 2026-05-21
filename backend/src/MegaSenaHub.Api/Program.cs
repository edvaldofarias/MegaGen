using System.Text;
using MegaSenaHub.Api.Middleware;
using MegaSenaHub.Api.Services;
using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.UseCases;
using MegaSenaHub.Infrastructure;
using MegaSenaHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Configuração ────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// ── Infrastructure ───────────────────────────────────────────────────────────
builder.Services.AddInfrastructure(connectionString);

// ── Application Use Cases ────────────────────────────────────────────────────
builder.Services.AddScoped<RegisterUserUseCase>();
builder.Services.AddScoped<LoginUserUseCase>();
builder.Services.AddScoped<GetCurrentUserProfileUseCase>();
builder.Services.AddScoped<GenerateMegaSenaGamesUseCase>();
builder.Services.AddScoped<CheckCombinationHistoryUseCase>();
builder.Services.AddScoped<GetContestByNumberUseCase>();
builder.Services.AddScoped<GetLatestContestUseCase>();
builder.Services.AddScoped<RegisterUserBetUseCase>();
builder.Services.AddScoped<GetUserBetsUseCase>();
builder.Services.AddScoped<GetUserBetByIdUseCase>();
builder.Services.AddScoped<CheckUserBetResultUseCase>();
builder.Services.AddScoped<GetUserBetSummaryUseCase>();
builder.Services.AddScoped<SyncMissingContestsUseCase>();

// ── Clock ────────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IClock, SystemClock>();

// ── HTTP Context / CurrentUser ───────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// ── JWT Authentication ───────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MegaSena Hub API",
        Version = "v1",
        Description = "API para análise histórica, geração de jogos e controle de apostas da Mega-Sena."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Informe: Bearer {seu-token-jwt}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Seed de Roles ─────────────────────────────────────────────────────────────
await SeedRolesAsync(app);

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MegaSena Hub API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// ── Seed de Roles ─────────────────────────────────────────────────────────────
static async Task SeedRolesAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var role in new[] { "User", "Admin" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// Necessário para o WebApplicationFactory nos testes de integração
public partial class Program;
