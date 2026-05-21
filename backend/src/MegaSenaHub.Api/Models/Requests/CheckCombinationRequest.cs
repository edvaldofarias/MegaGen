using System.ComponentModel.DataAnnotations;

namespace MegaSenaHub.Api.Models.Requests;

/// <summary>Combinação de dezenas para verificar histórico de sorteios.</summary>
public sealed record CheckCombinationRequest(
    [Required, MinLength(6), MaxLength(6)] IReadOnlyCollection<int> Numbers);
