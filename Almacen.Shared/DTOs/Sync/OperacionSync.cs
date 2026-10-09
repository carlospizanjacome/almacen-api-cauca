namespace Almacen.Shared.DTOs.Sync;

/// <summary>
/// Representa una operacion offline que el movil envia a la API para sincronizar.
/// </summary>
public class OperacionSync
{
    public string IdLocalMovil { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int BienId { get; set; }
    public string? BienCodigo { get; set; }
    public string? BienNombre { get; set; }
    public int InstitucionId { get; set; }
    public DateTime FechaOperacion { get; set; }
    public int UsuarioId { get; set; }
    public string? Observaciones { get; set; }

    // ENTRADAS
    public string? TipoFuente { get; set; }
    public string? NumeroFactura { get; set; }
    public decimal? Valor { get; set; }
    public string? Proveedor { get; set; }
    public int? ProveedorId { get; set; }
    public int? FuncionarioRecibeId { get; set; }

    // SALIDAS
    public string? TipoBaja { get; set; }
    public string? Motivo { get; set; }
    public decimal? ValorSalida { get; set; }
    public string? NumeroActaComite { get; set; }
    public int? FuncionarioApruebaId { get; set; }

    // CONSUMO
    public string? TipoMovimiento { get; set; }
    public decimal? Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public decimal? ValorMovimiento { get; set; }
    public string? DocumentoReferencia { get; set; }

    // SYNC
    public int Intentos { get; set; }
    public string? DispositivoId { get; set; }
}