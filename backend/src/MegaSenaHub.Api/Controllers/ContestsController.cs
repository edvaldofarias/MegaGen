using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.UseCases;
using MegaSenaHub.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaSenaHub.Api.Controllers;

[ApiController]
[Route("api/mega-sena/contests")]
[Produces("application/json")]
public sealed class ContestsController(
    GetContestByNumberUseCase getByNumber,
    GetLatestContestUseCase getLatest,
    SyncMissingContestsUseCase syncMissing) : ControllerBase
{
    /// <summary>Retorna um concurso pelo número do sorteio.</summary>
    [HttpGet("{contestNumber:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNumber(int contestNumber, CancellationToken cancellationToken)
    {
        var result = await getByNumber.ExecuteAsync(
            new GetContestByNumberQuery(contestNumber), cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Retorna o concurso mais recente registrado na base.</summary>
    [HttpGet("latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatest(CancellationToken cancellationToken)
    {
        var result = await getLatest.ExecuteAsync(cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Detecta concursos ausentes e publica mensagens de sincronização. Requer role Admin.</summary>
    [HttpPost("sync")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Sync(CancellationToken cancellationToken)
    {
        var result = await syncMissing.ExecuteAsync(new SyncMissingContestsCommand(), cancellationToken);
        return Ok(result);
    }
}
