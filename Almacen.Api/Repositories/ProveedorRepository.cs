using Almacen.Shared.Models;
using Dapper;
using Npgsql;

namespace Almacen.Api.Repositories;

public class ProveedorRepository : IProveedorRepository
{
    private readonly string _connectionString;

    public ProveedorRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MobileDb")
            ?? throw new InvalidOperationException("Connection string 'MobileDb' no configurada.");
    }

    public async Task<IEnumerable<ProveedorModel>> ObtenerActivosAsync()
    {
        const string sql = @"
            SELECT
                id              AS Id,
                nit             AS Nit,
                nombre          AS Nombre,
                direccion       AS Direccion,
                telefono        AS Telefono,
                email           AS Email,
                contacto        AS Contacto,
                observaciones   AS Observaciones,
                activo          AS Activo
            FROM public.proveedores
            WHERE activo = true
            ORDER BY nombre;";

        await using var conn = new NpgsqlConnection(_connectionString);
        return await conn.QueryAsync<ProveedorModel>(sql);
    }
}