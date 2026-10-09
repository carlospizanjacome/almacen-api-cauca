using System.Security.Claims;
using Almacen.Api.Repositories;
using Almacen.Shared.DTOs.Common;
using Almacen.Shared.DTOs.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Almacen.Api.Controllers;

[ApiController]
[Route("api/sync")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly ISyncRepository _syncRepo;
    private readonly ILogger<SyncController> _logger;

    public SyncController(ISyncRepository syncRepo, ILogger<SyncController> logger)
    {
        _syncRepo = syncRepo;
        _logger = logger;
    }

    [HttpPost("kardex")]
    public async Task<IActionResult> SincronizarKardex([FromBody] SyncKardexRequest request)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? "desconocido";

        _logger.LogInformation("Sync kardex: usuario={Usuario} operaciones={Total}",
            usuarioId, request.Operaciones?.Count ?? 0);

        if (request.Operaciones is null || request.Operaciones.Count == 0)
        {
            return Ok(ApiResponse.Ok(new SyncKardexResponse
            {
                TotalRecibidas = 0,
                TotalOk = 0,
                TotalError = 0,
                TotalDuplicadas = 0
            }, "Sin operaciones para procesar"));
        }

        var respuesta = new SyncKardexResponse
        {
            TotalRecibidas = request.Operaciones.Count
        };

        foreach (var op in request.Operaciones)
        {
            try
            {
                var resultado = await _syncRepo.ProcesarOperacionAsync(op);
                respuesta.Resultados.Add(resultado);

                switch (resultado.Estado)
                {
                    case "OK":         respuesta.TotalOk++;         break;
                    case "DUPLICADO":  respuesta.TotalDuplicadas++; break;
                    default:           respuesta.TotalError++;      break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando operacion {IdLocal}", op.IdLocalMovil);
                respuesta.Resultados.Add(new SyncResultado
                {
                    IdLocalMovil = op.IdLocalMovil,
                    Estado = "ERROR",
                    Mensaje = ex.Message
                });
                respuesta.TotalError++;
            }
        }

        _logger.LogInformation(
            "Sync kardex resultado: OK={Ok} Duplicadas={Dup} Error={Err}",
            respuesta.TotalOk, respuesta.TotalDuplicadas, respuesta.TotalError);

        return Ok(ApiResponse.Ok(respuesta, "Sincronizacion procesada"));
    }
}