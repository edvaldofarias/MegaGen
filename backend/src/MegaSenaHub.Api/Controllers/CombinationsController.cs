using MegaSenaHub.Api.Models.Requests;
using MegaSenaHub.Application.Queries;
using MegaSenaHub.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace MegaSenaHub.Api.Controllers;

[ApiController]
[Route("api/mega-sena/combinations")]
[Produces("application/json")]
public sealed class CombinationsController(CheckCombinationHistoryUseCase checkHistory) : ControllerBase
{
    /// <summary>Verifica se uma combinação já foi sorteada e retorna o melhor resultado histórico.</summary>
    [HttpPost("check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Check(
        [FromBody] CheckCombinationRequest request, CancellationToken cancellationToken)
    {
        var query = new CheckCombinationHistoryQuery(request.Numbers);
        var result = await checkHistory.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }
}
