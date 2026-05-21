namespace MegaSenaHub.Application.Commands;

/// <summary>Registra uma nova aposta para o usuário autenticado.</summary>
public sealed record RegisterUserBetCommand(
    int ContestNumber,
    IReadOnlyCollection<int> Numbers,
    decimal AmountPaid);
