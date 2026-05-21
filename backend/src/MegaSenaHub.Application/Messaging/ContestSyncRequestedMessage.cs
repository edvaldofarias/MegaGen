namespace MegaSenaHub.Application.Messaging;

/// <summary>
/// Mensagem publicada no broker quando um concurso precisa ser importado assincronamente.
/// </summary>
public sealed record ContestSyncRequestedMessage(
    string LotteryType,
    int ContestNumber,
    DateTimeOffset RequestedAt,
    Guid CorrelationId);
