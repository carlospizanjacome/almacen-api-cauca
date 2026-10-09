using Almacen.Shared.Models;

namespace Almacen.Api.Repositories;

public interface IBienRepository
{
    Task<IEnumerable<BienModel>> ObtenerPorSedeAsync(int institucionId, int sedeId, string? filtro = null);
    Task<BienModel?> ObtenerPorIdAsync(int id);
}