using MegaSenaHub.Api.Models.Requests;
using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.Queries;
using MegaSenaHub.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaSenaHub.Api.Controllers;

[ApiController]
[Route("api/me/bets")]
[Authorize]
[Produces("application/json")]
public sealed class UserBetsController(
    RegisterUserBetUseCase registerBet,
    GetUserBetsUseCase getUserBets,
    GetUserBetByIdUseCase getUserBetById,
    CheckUserBetResultUseCase checkResult) : ControllerBase
{
    /// <summary>Registra uma nova aposta para o usuário autenticado.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserBetRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterUserBetCommand(
            request.ContestNumber, request.Numbers, request.AmountPaid);

        var result = await registerBet.ExecuteAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Retorna todas as apostas do usuário autenticado.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await getUserBets.ExecuteAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Retorna uma aposta específica do usuário autenticado.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getUserBetById.ExecuteAsync(
            new GetUserBetByIdQuery(id), cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Verifica o resultado da aposta contra o concurso correspondente.</summary>
    [HttpPost("{id:guid}/check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Check(Guid id, CancellationToken cancellationToken)
    {
        var result = await checkResult.ExecuteAsync(
            new CheckUserBetResultCommand(id), cancellationToken);

        return Ok(result);
    }
}
