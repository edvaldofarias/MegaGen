using System.ComponentModel.DataAnnotations;
using MegaSenaHub.Domain.Enums;

namespace MegaSenaHub.Api.Models.Requests;

/// <summary>Solicitação de geração de jogos da Mega-Sena.</summary>
public sealed record GenerateGamesRequest(
    [Range(1, 100)] int Quantity = 1,
    [Range(6, 6)] int NumbersPerGame = 6,
    GameGenerationStrategy Strategy = GameGenerationStrategy.Random,
    bool AvoidAlreadyDrawnCombination = false,
    bool AvoidAlreadyWonCombination = false);
