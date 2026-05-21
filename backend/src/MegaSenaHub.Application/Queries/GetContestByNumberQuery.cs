namespace MegaSenaHub.Application.Queries;

/// <summary>Consulta um concurso específico pelo número do sorteio.</summary>
public sealed record GetContestByNumberQuery(int ContestNumber);
