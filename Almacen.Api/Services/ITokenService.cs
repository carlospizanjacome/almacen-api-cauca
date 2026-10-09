using Almacen.Shared.Models;

namespace Almacen.Api.Services;

public interface ITokenService
{
    string GenerarAccessToken(UsuarioMobile usuario);
    string GenerarRefreshToken();
    DateTime ObtenerFechaExpiracion();
}