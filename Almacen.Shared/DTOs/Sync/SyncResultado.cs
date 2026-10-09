namespace Almacen.Shared.DTOs.Sync;

/// <summary>
/// Resultado del procesamiento de UNA operacion en el servidor.
/// </summary>
public class SyncResultado
{
    public string IdLocalMovil { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty; // OK | ERROR | DUPLICADO
    public int? IdServidor { get; set; }
    public string? Mensaje { get; set; }
}