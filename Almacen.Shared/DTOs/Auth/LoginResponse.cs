namespace Almacen.Shared.DTOs.Auth;

/// <summary>
/// Respuesta del login exitoso.
/// Contiene el access token (JWT), refresh token y datos del usuario.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT para autenticar peticiones HTTP.
    /// Expira en 60 minutos por defecto.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token para renovar el access token.
    /// Expira en 30 días. Se guarda en SecureStorage del dispositivo.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de expiración del access token.
    /// </summary>
    public DateTime ExpiraEn { get; set; }

    /// <summary>
    /// Datos públicos del usuario autenticado.
    /// </summary>
    public UsuarioDto Usuario { get; set; } = new();
}