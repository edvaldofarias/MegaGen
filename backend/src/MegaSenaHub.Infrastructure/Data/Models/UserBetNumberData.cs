namespace MegaSenaHub.Infrastructure.Data.Models;

/// <summary>
/// Modelo de persistência para cada dezena de uma aposta do usuário.
/// Chave primária composta: (UserBetId, Number).
/// </summary>
internal sealed class UserBetNumberData
{
    public Guid UserBetId { get; set; }
    public int Number { get; set; }

    public UserBetData UserBet { get; set; } = null!;
}
