using System.Security.Claims;
using Almacen.Api.Repositories;
using Almacen.Shared.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Almacen.Api.Controllers;

[ApiController]
[Route("api/bienes")]
[Authorize]
[Produces("application/json")]
public class BienesController : ControllerBase
{
    private readonly IBienRepository _bienRepository;
    private readonly ILogger<BienesController> _logger;

    public BienesController(IBienRepository bienRepository, ILogger<BienesController> logger)
    {
        _bienRepository = bienRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerBienes([FromQuery] int sedeId = 0, [FromQuery] string? filtro = null)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var institucionIdClaim = User.FindFirst("institucionId")?.Value
                              ?? User.FindFirst("institucion_id")?.Value;

        if (string.IsNullOrEmpty(institucionIdClaim) || !int.TryParse(institucionIdClaim, out var institucionId))
        {
            _logger.LogWarning("Token sin institucionId para usuario {UsuarioId}", usuarioId);
            return Unauthorized(ApiResponse.Error("Token sin institucionId"));
        }

        _logger.LogInformation("Usuario {UsuarioId} (inst {InstId}) consulta bienes sede {SedeId}",
            usuarioId, institucionId, sedeId);

        var bienes = await _bienRepository.ObtenerPorSedeAsync(institucionId, sedeId, filtro);
        return Ok(bienes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerBien(int id)
    {
        var bien = await _bienRepository.ObtenerPorIdAsync(id);
        if (bien is null)
            return NotFound(ApiResponse.Error($"Bien {id} no encontrado"));

        return Ok(bien);
    }

    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        return Ok(new { status = "pong", timestamp = DateTime.UtcNow });
    }
}