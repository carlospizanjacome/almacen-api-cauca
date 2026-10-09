using Almacen.Api.Repositories;
using Almacen.Shared.DTOs.Auth;

namespace Almacen.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var usuario = await _usuarioRepository.ObtenerPorEmailAsync(request.Email);
        if (usuario is null)
        {
            _logger.LogWarning("Login fallido: email no encontrado {Email}", request.Email);
            return null;
        }

        if (!usuario.Activo)
        {
            _logger.LogWarning("Login fallido: usuario inactivo {Email}", request.Email);
            return null;
        }

        bool passwordValida;
        try
        {
            passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando password para {Email}", request.Email);
            return null;
        }

        if (!passwordValida)
        {
            _logger.LogWarning("Login fallido: password incorrecta para {Email}", request.Email);
            return null;
        }

        var accessToken = _tokenService.GenerarAccessToken(usuario);
        var refreshToken = _tokenService.GenerarRefreshToken();

        await _usuarioRepository.ActualizarUltimaSyncAsync(usuario.Id);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiraEn = _tokenService.ObtenerFechaExpiracion(),
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Email = usuario.Email,
                NombreCompleto = usuario.NombreCompleto,
                InstitucionId = usuario.InstitucionId,
                InstitucionNombre = usuario.InstitucionNombre,
                RolId = usuario.RolId,
                RolNombre = usuario.RolNombre,
                Activo = usuario.Activo
            }
        };

        _logger.LogInformation("Login exitoso: {Email} (id={Id})", usuario.Email, usuario.Id);
        return response;
    }

    public async Task<UsuarioDto?> ObtenerUsuarioAsync(int usuarioId)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId);
        if (usuario is null) return null;

        return new UsuarioDto
        {
            Id = usuario.Id,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto,
            InstitucionId = usuario.InstitucionId,
            InstitucionNombre = usuario.InstitucionNombre,
            RolId = usuario.RolId,
            RolNombre = usuario.RolNombre,
            Activo = usuario.Activo
        };
    }
}