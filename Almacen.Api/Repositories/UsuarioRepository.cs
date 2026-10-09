using Almacen.Shared.Models;
using Dapper;
using Npgsql;

namespace Almacen.Api.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MobileDb")
            ?? throw new InvalidOperationException("Connection string 'MobileDb' no configurada.");
    }

    public async Task<UsuarioMobile?> ObtenerPorEmailAsync(string email)
    {
        const string sql = @"
            SELECT 
                id              AS Id,
                email           AS Email,
                password_hash   AS PasswordHash,
                nombre_completo AS NombreCompleto,
                institucion_id  AS InstitucionId,
                institucion_nombre AS InstitucionNombre,
                rol_id          AS RolId,
                rol_nombre      AS RolNombre,
                activo          AS Activo,
                ultima_sync     AS UltimaSync,
                created_at      AS CreatedAt,
                updated_at      AS UpdatedAt
            FROM mobile.usuarios_mobile
            WHERE LOWER(email) = LOWER(@Email)
            LIMIT 1;";

        await using var conn = new NpgsqlConnection(_connectionString);
        return await conn.QueryFirstOrDefaultAsync<UsuarioMobile>(sql, new { Email = email });
    }

    public async Task<UsuarioMobile?> ObtenerPorIdAsync(int id)
    {
        const string sql = @"
            SELECT 
                id              AS Id,
                email           AS Email,
                password_hash   AS PasswordHash,
                nombre_completo AS NombreCompleto,
                institucion_id  AS InstitucionId,
                institucion_nombre AS InstitucionNombre,
                rol_id          AS RolId,
                rol_nombre      AS RolNombre,
                activo          AS Activo,
                ultima_sync     AS UltimaSync,
                created_at      AS CreatedAt,
                updated_at      AS UpdatedAt
            FROM mobile.usuarios_mobile
            WHERE id = @Id
            LIMIT 1;";

        await using var conn = new NpgsqlConnection(_connectionString);
        return await conn.QueryFirstOrDefaultAsync<UsuarioMobile>(sql, new { Id = id });
    }

    public async Task ActualizarUltimaSyncAsync(int usuarioId)
    {
        const string sql = @"
            UPDATE mobile.usuarios_mobile
            SET ultima_sync = NOW(), updated_at = NOW()
            WHERE id = @UsuarioId;";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.ExecuteAsync(sql, new { UsuarioId = usuarioId });
    }
}