namespace Almacen.Shared.DTOs.Sync;

/// <summary>
/// Respuesta de la API al lote de sincronizacion.
/// </summary>
public class SyncKardexResponse
{
    public int TotalRecibidas { get; set; }
    public int TotalOk { get; set; }
    public int TotalError { get; set; }
    public int TotalDuplicadas { get; set; }
    public List<SyncResultado> Resultados { get; set; } = new();
}