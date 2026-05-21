using MegaSenaHub.Domain.Enums;

namespace MegaSenaHub.Application.Commands;

/// <summary>Solicita a geração de jogos da Mega-Sena com a estratégia informada.</summary>
public sealed record GenerateMegaSenaGamesCommand(
    int Quantity,
    int NumbersPerGame,
    GameGenerationStrategy Strategy,
    bool AvoidAlreadyDrawnCombination,
    bool AvoidAlreadyWonCombination);
