using MegaSenaHub.Application.Queries;
using MegaSenaHub.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaSenaHub.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
[Produces("application/json")]
public sealed class UserProfileController(GetUserBetSummaryUseCase getSummary) : ControllerBase
{
    /// <summary>Retorna o resumo financeiro das apostas do usuário autenticado.</summary>
    [HttpGet("balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBalance(CancellationToken cancellationToken)
    {
        var result = await getSummary.ExecuteAsync(new GetUserBetSummaryQuery(), cancellationToken);
        return Ok(result);
    }
}
