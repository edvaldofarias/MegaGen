using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.Commands;
using MegaSenaHub.Application.DTOs;
using MegaSenaHub.Application.Exceptions;
using MegaSenaHub.Domain.Entities;

namespace MegaSenaHub.Application.UseCases;

/// <summary>
/// Registra uma nova aposta para o usuário autenticado.
/// Valida a identidade e delega a criação ao domain.
/// </summary>
public sealed class RegisterUserBetUseCase(
    IUserBetRepository userBetRepository,
    ICurrentUser currentUser)
{
    public async Task<UserBetDto> ExecuteAsync(
        RegisterUserBetCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException("User is not authenticated.");

        if (string.IsNullOrEmpty(currentUser.UserId))
            throw new UnauthorizedException("User ID is not available.");

        var userId = Guid.Parse(currentUser.UserId);

        // DomainException é lançada pelo domain se os dados forem inválidos
        var bet = UserBet.Create(userId, command.ContestNumber, command.Numbers, command.AmountPaid);

        await userBetRepository.AddAsync(bet, cancellationToken);

        return MapToDto(bet);
    }

    private static UserBetDto MapToDto(UserBet bet) =>
        new(bet.Id,
            bet.UserId.ToString(),
            bet.ContestNumber,
            bet.Numbers.AsReadOnly().Select(n => n.Value).ToList().AsReadOnly(),
            bet.AmountPaid,
            bet.Status.ToString(),
            bet.CreatedAt,
            bet.CheckedAt);
}
