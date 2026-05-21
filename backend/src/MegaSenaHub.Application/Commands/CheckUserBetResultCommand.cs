namespace MegaSenaHub.Application.Commands;

/// <summary>Verifica o resultado de uma aposta registrada pelo usuário autenticado.</summary>
public sealed record CheckUserBetResultCommand(Guid UserBetId);
