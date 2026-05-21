using System.ComponentModel.DataAnnotations;

namespace MegaSenaHub.Api.Models.Requests;

/// <summary>Dados para registrar uma nova aposta.</summary>
public sealed record RegisterUserBetRequest(
    [Range(1, int.MaxValue)] int ContestNumber,
    [Required, MinLength(6), MaxLength(6)] IReadOnlyCollection<int> Numbers,
    [Range(0.01, 100_000)] decimal AmountPaid);
