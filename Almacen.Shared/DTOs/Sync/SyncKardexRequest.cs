namespace Almacen.Shared.DTOs.Sync;

/// <summary>
/// Peticion que envia el movil con el lote de operaciones pendientes.
/// </summary>
public class SyncKardexRequest
{
    public List<OperacionSync> Operaciones { get; set; } = new();

    /// <summary>
    /// Nombre del dispositivo (ej: SM-A600). Para auditoria.
    /// </summary>
    public string? DispositivoId { get; set; }
}