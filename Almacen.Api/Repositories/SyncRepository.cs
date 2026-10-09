using Almacen.Shared.DTOs.Sync;
using Dapper;
using Npgsql;

namespace Almacen.Api.Repositories;

public interface ISyncRepository
{
    Task<SyncResultado> ProcesarOperacionAsync(OperacionSync op);
}

public class SyncRepository : ISyncRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SyncRepository> _logger;

    public SyncRepository(IConfiguration config, ILogger<SyncRepository> logger)
    {
        _connectionString = config.GetConnectionString("MobileDb")
            ?? throw new InvalidOperationException("ConnectionStrings:MobileDb no configurada");
        _logger = logger;
    }

    public async Task<SyncResultado> ProcesarOperacionAsync(OperacionSync op)
    {
        var tipo = (op.Tipo ?? "").ToUpperInvariant();

        return tipo switch
        {
            "ENTRADA" => await ProcesarEntradaAsync(op),
            "SALIDA"  => await ProcesarSalidaAsync(op),
            "CONSUMO" => await ProcesarConsumoAsync(op),
            _ => new SyncResultado
            {
                IdLocalMovil = op.IdLocalMovil,
                Estado = "ERROR",
                Mensaje = $"Tipo no reconocido: {op.Tipo}"
            }
        };
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // ENTRADAS
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private async Task<SyncResultado> ProcesarEntradaAsync(OperacionSync op)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // 1. Verificar duplicado
        var existente = await conn.QueryFirstOrDefaultAsync<int?>(
            "SELECT id FROM public.entradas WHERE id_local_movil = @IdLocalMovil LIMIT 1;",
            new { op.IdLocalMovil });

        if (existente.HasValue)
        {
            return new SyncResultado
            {
                IdLocalMovil = op.IdLocalMovil,
                Estado = "DUPLICADO",
                IdServidor = existente.Value,
                Mensaje = "Ya procesado previamente"
            };
        }

        // 2. Insertar
        const string sql = @"
            INSERT INTO public.entradas (
                bien_id, tipo_fuente, numero_factura, fecha_entrada, valor,
                proveedor, proveedor_id, funcionario_recibe_id, observaciones,
                institucion_id, created_at, updated_at, anulada, id_local_movil
            ) VALUES (
                @BienId, @TipoFuente, @NumeroFactura, @FechaEntrada, @Valor,
                @Proveedor, @ProveedorId, @FuncionarioRecibeId, @Observaciones,
                @InstitucionId, NOW(), NOW(), FALSE, @IdLocalMovil
            ) RETURNING id;";

        var nuevoId = await conn.ExecuteScalarAsync<int>(sql, new
        {
            op.BienId,
            op.TipoFuente,
            op.NumeroFactura,
            FechaEntrada = op.FechaOperacion,
            op.Valor,
            op.Proveedor,
            op.ProveedorId,
            op.FuncionarioRecibeId,
            op.Observaciones,
            op.InstitucionId,
            op.IdLocalMovil
        });

        _logger.LogInformation("Entrada sync OK: idLocal={IdLocal} idServidor={IdServidor}",
            op.IdLocalMovil, nuevoId);

        return new SyncResultado
        {
            IdLocalMovil = op.IdLocalMovil,
            Estado = "OK",
            IdServidor = nuevoId
        };
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // SALIDAS
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private async Task<SyncResultado> ProcesarSalidaAsync(OperacionSync op)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var existente = await conn.QueryFirstOrDefaultAsync<int?>(
            "SELECT id FROM public.salidas WHERE id_local_movil = @IdLocalMovil LIMIT 1;",
            new { op.IdLocalMovil });

        if (existente.HasValue)
        {
            return new SyncResultado
            {
                IdLocalMovil = op.IdLocalMovil,
                Estado = "DUPLICADO",
                IdServidor = existente.Value,
                Mensaje = "Ya procesado previamente"
            };
        }

        const string sql = @"
            INSERT INTO public.salidas (
                bien_id, tipo_baja, motivo, fecha_salida, numero_acta_comite,
                valor_salida, funcionario_aprueba_id, observaciones,
                institucion_id, created_at, anulada, id_local_movil
            ) VALUES (
                @BienId, @TipoBaja, @Motivo, @FechaSalida, @NumeroActaComite,
                @ValorSalida, @FuncionarioApruebaId, @Observaciones,
                @InstitucionId, NOW(), FALSE, @IdLocalMovil
            ) RETURNING id;";

        var nuevoId = await conn.ExecuteScalarAsync<int>(sql, new
        {
            op.BienId,
            op.TipoBaja,
            op.Motivo,
            FechaSalida = op.FechaOperacion,
            op.NumeroActaComite,
            op.ValorSalida,
            op.FuncionarioApruebaId,
            op.Observaciones,
            op.InstitucionId,
            op.IdLocalMovil
        });

        _logger.LogInformation("Salida sync OK: idLocal={IdLocal} idServidor={IdServidor}",
            op.IdLocalMovil, nuevoId);

        return new SyncResultado
        {
            IdLocalMovil = op.IdLocalMovil,
            Estado = "OK",
            IdServidor = nuevoId
        };
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // CONSUMO
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private async Task<SyncResultado> ProcesarConsumoAsync(OperacionSync op)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // 1. Verificar duplicado
        var existente = await conn.QueryFirstOrDefaultAsync<int?>(
            "SELECT id FROM public.movimientos_consumo WHERE id_local_movil = @IdLocalMovil LIMIT 1;",
            new { op.IdLocalMovil });

        if (existente.HasValue)
        {
            return new SyncResultado
            {
                IdLocalMovil = op.IdLocalMovil,
                Estado = "DUPLICADO",
                IdServidor = existente.Value,
                Mensaje = "Ya procesado previamente"
            };
        }

        // 2. Obtener saldo anterior del bien
        var saldoAnterior = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT saldo_cantidad, saldo_valor, cpp
            FROM public.movimientos_consumo
            WHERE bien_id = @BienId AND anulada = false
            ORDER BY id DESC LIMIT 1;",
            new { op.BienId });

        decimal saldoCantAnterior = saldoAnterior?.saldo_cantidad ?? 0m;
        decimal saldoValorAnterior = saldoAnterior?.saldo_valor ?? 0m;
        decimal cppAnterior = saldoAnterior?.cpp ?? 0m;

        // 3. Calcular nuevo saldo y CPP
        var tipoMov = (op.TipoMovimiento ?? "ENTRADA").ToUpperInvariant();
        decimal cantidad = op.Cantidad ?? 0m;
        decimal costoUnit = op.CostoUnitario ?? 0m;
        decimal valorMov = op.ValorMovimiento ?? (cantidad * costoUnit);

        decimal nuevoSaldoCant;
        decimal nuevoSaldoValor;
        decimal nuevoCpp;

        if (tipoMov == "ENTRADA")
        {
            nuevoSaldoCant = saldoCantAnterior + cantidad;
            nuevoSaldoValor = saldoValorAnterior + valorMov;
            nuevoCpp = nuevoSaldoCant > 0
                ? Math.Round(nuevoSaldoValor / nuevoSaldoCant, 4)
                : 0m;
        }
        else
        {
            // SALIDA
            nuevoSaldoCant = saldoCantAnterior - cantidad;
            nuevoCpp = cppAnterior > 0 ? cppAnterior : costoUnit;
            nuevoSaldoValor = nuevoSaldoCant * nuevoCpp;
        }

        // 4. Insertar movimiento con saldos calculados
        const string sql = @"
            INSERT INTO public.movimientos_consumo (
                bien_id, institucion_id, tipo_movimiento, fecha_movimiento, cantidad,
                costo_unitario, valor_movimiento, saldo_cantidad, saldo_valor, cpp,
                documento_referencia, proveedor_id, funcionario_recibe_id, observaciones,
                created_at, anulada, id_local_movil
            ) VALUES (
                @BienId, @InstitucionId, @TipoMovimiento, @FechaMovimiento, @Cantidad,
                @CostoUnitario, @ValorMovimiento, @SaldoCantidad, @SaldoValor, @Cpp,
                @DocumentoReferencia, @ProveedorId, @FuncionarioRecibeId, @Observaciones,
                NOW(), FALSE, @IdLocalMovil
            ) RETURNING id;";

        var nuevoId = await conn.ExecuteScalarAsync<int>(sql, new
        {
            op.BienId,
            op.InstitucionId,
            op.TipoMovimiento,
            FechaMovimiento = op.FechaOperacion,
            op.Cantidad,
            op.CostoUnitario,
            op.ValorMovimiento,
            SaldoCantidad = nuevoSaldoCant,
            SaldoValor = nuevoSaldoValor,
            Cpp = nuevoCpp,
            op.DocumentoReferencia,
            op.ProveedorId,
            op.FuncionarioRecibeId,
            op.Observaciones,
            op.IdLocalMovil
        });

        _logger.LogInformation("Consumo sync OK: idLocal={IdLocal} idServidor={IdServidor}",
            op.IdLocalMovil, nuevoId);

        return new SyncResultado
        {
            IdLocalMovil = op.IdLocalMovil,
            Estado = "OK",
            IdServidor = nuevoId
        };
    }
}