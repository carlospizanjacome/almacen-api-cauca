using Almacen.Shared.Models;

namespace Almacen.Api.Repositories;

public interface IProveedorRepository
{
    Task<IEnumerable<ProveedorModel>> ObtenerActivosAsync();
}