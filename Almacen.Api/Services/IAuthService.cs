using Almacen.Shared.DTOs.Auth;

namespace Almacen.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<UsuarioDto?> ObtenerUsuarioAsync(int usuarioId);
}