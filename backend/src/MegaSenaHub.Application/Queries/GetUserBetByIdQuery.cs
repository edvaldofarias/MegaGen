namespace MegaSenaHub.Application.Queries;

/// <summary>Consulta a aposta de um usuário pelo Id.</summary>
public sealed record GetUserBetByIdQuery(Guid BetId);
