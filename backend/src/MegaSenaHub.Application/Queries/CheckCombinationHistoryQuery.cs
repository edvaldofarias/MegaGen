namespace MegaSenaHub.Application.Queries;

/// <summary>Consulta o histórico de sorteios para uma combinação de dezenas.</summary>
public sealed record CheckCombinationHistoryQuery(IReadOnlyCollection<int> Numbers);
