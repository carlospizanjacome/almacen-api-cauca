using Almacen.Shared.Models;
using Dapper;
using Npgsql;

namespace Almacen.Api.Repositories;

public class BienRepository : IBienRepository
{
    private readonly string _connectionString;

    public BienRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MobileDb")
            ?? throw new InvalidOperationException("Connection string 'MobileDb' no configurada.");
    }

    public async Task<IEnumerable<BienModel>> ObtenerPorSedeAsync(int institucionId, int sedeId, string? filtro = null)
    {
        var sql = @"
            SELECT
                b.id              AS Id,
                b.codigo          AS Codigo,
                b.nombre          AS Nombre,
                b.descripcion     AS Descripcion,
                b.categoria_id    AS CategoriaId,
                c.nombre          AS CategoriaNombre,
                b.tipo_bien       AS TipoBien,
                b.marca           AS Marca,
                b.modelo          AS Modelo,
                b.serie           AS Serie,
                b.valor_adquisicion AS ValorAdquisicion,
                b.fecha_adquisicion AS FechaAdquisicion,
                b.estado_fisico   AS EstadoFisico,
                b.ubicacion       AS Ubicacion,
                b.responsable     AS Responsable,
                b.activo          AS Activo,
                b.institucion_id  AS InstitucionId,
                s.id              AS SedeId,
                s.nombre          AS SedeNombre,
                b.aula_id         AS AulaId,
                b.codigo_qr       AS CodigoQR,
                b.cantidad        AS Cantidad,
                b.unidad_medida   AS UnidadMedida,
                b.stock_actual    AS StockActual,
                b.cpp_actual      AS CppActual,
                b.valor_stock     AS ValorStock
            FROM public.bienes b
            LEFT JOIN public.categorias c ON c.id = b.categoria_id
            LEFT JOIN public.aulas a ON a.id = b.aula_id
            LEFT JOIN public.bloques bl ON bl.id = a.bloque_id
            LEFT JOIN public.sedes s ON s.id = bl.sede_id
            WHERE b.activo = true
              AND b.institucion_id = @InstitucionId";

        if (sedeId > 0)
            sql += " AND s.id = @SedeId";

        if (!string.IsNullOrWhiteSpace(filtro))
            sql += " AND (b.nombre ILIKE @Filtro OR b.codigo ILIKE @Filtro)";

        sql += " ORDER BY b.nombre LIMIT 200;";

        await using var conn = new NpgsqlConnection(_connectionString);
        return await conn.QueryAsync<BienModel>(sql, new
        {
            InstitucionId = institucionId,
            SedeId = sedeId,
            Filtro = $"%{filtro}%"
        });
    }

    public async Task<BienModel?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"
            SELECT
                b.id              AS Id,
                b.codigo          AS Codigo,
                b.nombre          AS Nombre,
                b.descripcion     AS Descripcion,
                b.categoria_id    AS CategoriaId,
                c.nombre          AS CategoriaNombre,
                b.tipo_bien       AS TipoBien,
                b.marca           AS Marca,
                b.modelo          AS Modelo,
                b.serie           AS Serie,
                b.valor_adquisicion AS ValorAdquisicion,
                b.fecha_adquisicion AS FechaAdquisicion,
                b.estado_fisico   AS EstadoFisico,
                b.ubicacion       AS Ubicacion,
                b.responsable     AS Responsable,
                b.activo          AS Activo,
                b.institucion_id  AS InstitucionId,
                s.id              AS SedeId,
                s.nombre          AS SedeNombre,
                b.aula_id         AS AulaId,
                b.codigo_qr       AS CodigoQR,
                b.cantidad        AS Cantidad,
                b.unidad_medida   AS UnidadMedida,
                b.stock_actual    AS StockActual,
                b.cpp_actual      AS CppActual,
                b.valor_stock     AS ValorStock
            FROM public.bienes b
            LEFT JOIN public.categorias c ON c.id = b.categoria_id
            LEFT JOIN public.aulas a ON a.id = b.aula_id
            LEFT JOIN public.bloques bl ON bl.id = a.bloque_id
            LEFT JOIN public.sedes s ON s.id = bl.sede_id
            WHERE b.id = @Id
            LIMIT 1;";

        await using var conn = new NpgsqlConnection(_connectionString);
        return await conn.QueryFirstOrDefaultAsync<BienModel>(sql, new { Id = id });
    }
}