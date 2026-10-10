using System.Security.Claims;
using Almacen.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Almacen.Api.Controllers;

[ApiController]
[Route("api/proveedores")]
[Authorize]
[Produces("application/json")]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorRepository _proveedorRepository;
    private readonly ILogger<ProveedoresController> _logger;

    public ProveedoresController(IProveedorRepository proveedorRepository, ILogger<ProveedoresController> logger)
    {
        _proveedorRepository = proveedorRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerProveedores()
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Usuario {UsuarioId} consulta proveedores", usuarioId);

        var proveedores = await _proveedorRepository.ObtenerActivosAsync();
        return Ok(proveedores);
    }
}