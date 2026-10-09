using System.ComponentModel.DataAnnotations;

namespace Almacen.Shared.DTOs.Auth;

/// <summary>
/// Petición de login desde la app móvil.
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Identificador único del dispositivo móvil.
    /// Opcional en el primer login.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Nombre del dispositivo (ej: "Samsung Galaxy A54").
    /// </summary>
    public string? DeviceName { get; set; }
}