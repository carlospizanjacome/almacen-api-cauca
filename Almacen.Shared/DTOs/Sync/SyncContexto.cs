namespace Almacen.Shared.DTOs.Sync;

/// <summary>
/// Datos del usuario y dispositivo que se adjuntan al sync
/// para registrar la auditoria correspondiente.
/// </summary>
public class SyncContexto
{
    public int UsuarioId { get; set; }
    public string? UsuarioEmail { get; set; }
    public string? UsuarioNombre { get; set; }
    public int InstitucionId { get; set; }
    public string? DispositivoId { get; set; }
    public string? UserAgent { get; set; }
}