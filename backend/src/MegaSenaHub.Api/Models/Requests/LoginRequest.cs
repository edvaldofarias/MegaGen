using System.ComponentModel.DataAnnotations;

namespace MegaSenaHub.Api.Models.Requests;

/// <summary>Credenciais de autenticação.</summary>
public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);
