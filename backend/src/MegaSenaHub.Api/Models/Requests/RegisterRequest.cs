using System.ComponentModel.DataAnnotations;

namespace MegaSenaHub.Api.Models.Requests;

/// <summary>Dados para criação de conta.</summary>
public sealed record RegisterRequest(
    [Required, MaxLength(150)] string Name,
    [Required, EmailAddress] string Email,
    [Required, Phone] string PhoneNumber,
    [Required, MinLength(8)] string Password);
