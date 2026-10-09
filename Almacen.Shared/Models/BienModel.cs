namespace Almacen.Shared.Models;

public class BienModel
{
    public int Id { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? CategoriaId { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? TipoBien { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Serie { get; set; }
    public decimal? ValorAdquisicion { get; set; }
    public DateTime? FechaAdquisicion { get; set; }
    public string? EstadoFisico { get; set; }
    public string? Ubicacion { get; set; }
    public string? Responsable { get; set; }
    public bool Activo { get; set; }
    public int? InstitucionId { get; set; }
    public int? SedeId { get; set; }
    public string? SedeNombre { get; set; }
    public int? AulaId { get; set; }
    public string? CodigoQr { get; set; }
    public int Cantidad { get; set; }
    public string? UnidadMedida { get; set; }
    public decimal? StockActual { get; set; }
    public decimal? CppActual { get; set; }
    public decimal? ValorStock { get; set; }
}
