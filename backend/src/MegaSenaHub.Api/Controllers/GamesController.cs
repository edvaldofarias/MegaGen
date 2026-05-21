using MegaSenaHub.Api.Models.Requests;
using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace MegaSenaHub.Api.Controllers;

[ApiController]
[Route("api/mega-sena/games")]
[Produces("application/json")]
public sealed class GamesController(GenerateMegaSenaGamesUseCase generateGames) : ControllerBase
{
    /// <summary>Gera jogos da Mega-Sena com a estratégia e quantidade solicitadas.</summary>
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateGamesRequest request, CancellationToken cancellationToken)
    {
        var command = new GenerateMegaSenaGamesCommand(
            request.Quantity,
            request.NumbersPerGame,
            request.Strategy,
            request.AvoidAlreadyDrawnCombination,
            request.AvoidAlreadyWonCombination);

        var result = await generateGames.ExecuteAsync(command, cancellationToken);
        return Ok(result);
    }
}
