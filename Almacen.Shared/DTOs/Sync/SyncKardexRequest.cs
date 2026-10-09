namespace Almacen.Shared.DTOs.Sync;

/// <summary>
/// Peticion que envia el movil con el lote de operaciones pendientes.
/// </summary>
public class SyncKardexRequest
{
    public List<OperacionSync> Operaciones { get; set; } = new();
}