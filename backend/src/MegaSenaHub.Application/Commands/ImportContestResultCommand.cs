namespace MegaSenaHub.Application.Commands;

/// <summary>Importa o resultado de um concurso específico do provider externo.</summary>
public sealed record ImportContestResultCommand(int ContestNumber);
