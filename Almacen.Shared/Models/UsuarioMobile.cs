namespace Almacen.Shared.Models;

public class UsuarioMobile
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public int? InstitucionId { get; set; }
    public string? InstitucionNombre { get; set; }
    public int? RolId { get; set; }
    public string? RolNombre { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? UltimaSync { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}