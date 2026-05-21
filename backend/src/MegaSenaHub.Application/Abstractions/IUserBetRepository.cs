using MegaSenaHub.Domain.Entities;

namespace MegaSenaHub.Application.Abstractions;

/// <summary>Abstrai a persistência de apostas do usuário.</summary>
public interface IUserBetRepository
{
    Task AddAsync(UserBet bet, CancellationToken cancellationToken);

    /// <summary>Busca uma aposta pelo Id garantindo que pertença ao userId informado.</summary>
    Task<UserBet?> GetByIdAsync(Guid id, string userId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserBet>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task UpdateAsync(UserBet bet, CancellationToken cancellationToken);
}
