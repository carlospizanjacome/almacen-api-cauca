using Almacen.Shared.Models;

namespace Almacen.Api.Repositories;

/// <summary>
/// Acceso a datos de usuarios del móvil (schema mobile).
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>
    /// Busca un usuario por email (case-insensitive).
    /// </summary>
    Task<UsuarioMobile?> ObtenerPorEmailAsync(string email);

    /// <summary>
    /// Busca un usuario por ID.
    /// </summary>
    Task<UsuarioMobile?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Actualiza la fecha de última sincronización del usuario.
    /// </summary>
    Task ActualizarUltimaSyncAsync(int usuarioId);
}