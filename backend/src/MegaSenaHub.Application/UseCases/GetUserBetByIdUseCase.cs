using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.Exceptions;
using MegaSenaHub.Application.Queries;

namespace MegaSenaHub.Application.UseCases;

/// <summary>
/// Retorna uma aposta específica do usuário autenticado pelo Id.
/// Retorna null quando a aposta não existe ou não pertence ao usuário.
/// </summary>
public sealed class GetUserBetByIdUseCase(
    IUserBetRepository userBetRepository,
    ICurrentUser currentUser)
{
    public async Task<UserBetDto?> ExecuteAsync(
        GetUserBetByIdQuery query, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException("User is not authenticated.");

        var bet = await userBetRepository.GetByIdAsync(query.BetId, currentUser.UserId!, cancellationToken);

        if (bet is null)
            return null;

        return new UserBetDto(
            bet.Id,
            bet.UserId.ToString(),
            bet.ContestNumber,
            bet.Numbers.AsReadOnly().Select(n => n.Value).ToList().AsReadOnly(),
            bet.AmountPaid,
            bet.Status.ToString(),
            bet.CreatedAt,
            bet.CheckedAt);
    }
}
