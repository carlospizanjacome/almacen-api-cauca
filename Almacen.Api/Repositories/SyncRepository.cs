using Almacen.Shared.DTOs.Sync;
using Dapper;
using Npgsql;

namespace Almacen.Api.Repositories;

public interface ISyncRepository
{
    Task<SyncResultado> ProcesarOperacionAsync(OperacionSync op, SyncContexto contexto);
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

    public async Task<SyncResultado> ProcesarOperacionAsync(OperacionSync op, SyncContexto contexto)
    {
        var tipo = (op.Tipo ?? "").ToUpperInvariant();

        return tipo switch
        {
            "ENTRADA" => await ProcesarEntradaAsync(op, contexto),
            "SALIDA"  => await ProcesarSalidaAsync(op, contexto),
            "CONSUMO" => await ProcesarConsumoAsync(op, contexto),
            _ => new SyncResultado
            {
                IdLocalMovil = op.IdLocalMovil,
                Estado = "ERROR",
                Mensaje = $"Tipo no reconocido: {op.Tipo}"
            }
        };
    }

    // ============================================================
    // ENTRADAS
    // ============================================================
    private async Task<SyncResultado> ProcesarEntradaAsync(OperacionSync op, SyncContexto ctx)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

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

        // AUDITORIA
        await RegistrarAuditoriaAsync(conn, ctx,
            modulo: "entradas",
            objetoId: nuevoId,
            objetoCodigo: op.BienCodigo,
            descripcion: $"Entrada sincronizada desde movil: {op.BienNombre}",
            documentoReferencia: op.NumeroFactura);

        _logger.LogInformation("Entrada sync OK: idLocal={IdLocal} idServidor={IdServidor}",
            op.IdLocalMovil, nuevoId);

        return new SyncResultado
        {
            IdLocalMovil = op.IdLocalMovil,
            Estado = "OK",
            IdServidor = nuevoId
        };
    }

    // ============================================================
    // SALIDAS
    // ============================================================
    private async Task<SyncResultado> ProcesarSalidaAsync(OperacionSync op, SyncContexto ctx)
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

        // AUDITORIA
        await RegistrarAuditoriaAsync(conn, ctx,
            modulo: "salidas",
            objetoId: nuevoId,
            objetoCodigo: op.BienCodigo,
            descripcion: $"Salida sincronizada desde movil: {op.BienNombre}",
            documentoReferencia: op.NumeroActaComite);

        _logger.LogInformation("Salida sync OK: idLocal={IdLocal} idServidor={IdServidor}",
            op.IdLocalMovil, nuevoId);

        return new SyncResultado
        {
            IdLocalMovil = op.IdLocalMovil,
            Estado = "OK",
            IdServidor = nuevoId
        };
    }

    // ============================================================
    // CONSUMO
    // ============================================================
    private async Task<SyncResultado> ProcesarConsumoAsync(OperacionSync op, SyncContexto ctx)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

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

        var saldoAnterior = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT saldo_cantidad, saldo_valor, cpp
            FROM public.movimientos_consumo
            WHERE bien_id = @BienId AND anulada = false
            ORDER BY id DESC LIMIT 1;",
            new { op.BienId });

        decimal saldoCantAnterior = saldoAnterior?.saldo_cantidad ?? 0m;
        decimal saldoValorAnterior = saldoAnterior?.saldo_valor ?? 0m;
        decimal cppAnterior = saldoAnterior?.cpp ?? 0m;

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
            nuevoSaldoCant = saldoCantAnterior - cantidad;
            nuevoCpp = cppAnterior > 0 ? cppAnterior : costoUnit;
            nuevoSaldoValor = nuevoSaldoCant * nuevoCpp;
        }

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

        // Actualizar stock del bien
        const string sqlUpdateBien = @"
            UPDATE public.bienes
            SET stock_actual = @StockActual,
                cpp_actual = @CppActual,
                valor_stock = @ValorStock,
                updated_at = NOW()
            WHERE id = @BienId;";

        await conn.ExecuteAsync(sqlUpdateBien, new
        {
            op.BienId,
            StockActual = nuevoSaldoCant,
            CppActual = nuevoCpp,
            ValorStock = nuevoSaldoValor
        });

        // AUDITORIA
        await RegistrarAuditoriaAsync(conn, ctx,
            modulo: "consumo",
            objetoId: nuevoId,
            objetoCodigo: op.BienCodigo,
            descripcion: $"Consumo {tipoMov} sincronizado desde movil: {op.BienNombre} ({cantidad} unidades)",
            documentoReferencia: op.DocumentoReferencia);

        _logger.LogInformation("Consumo sync OK: idLocal={IdLocal} idServidor={IdServidor}",
            op.IdLocalMovil, nuevoId);

        return new SyncResultado
        {
            IdLocalMovil = op.IdLocalMovil,
            Estado = "OK",
            IdServidor = nuevoId
        };
    }

    // ============================================================
    // AUDITORIA
    // ============================================================
    private async Task RegistrarAuditoriaAsync(
        NpgsqlConnection conn,
        SyncContexto ctx,
        string modulo,
        int objetoId,
        string? objetoCodigo,
        string descripcion,
        string? documentoReferencia)
    {
        try
        {
            const string sql = @"
                INSERT INTO public.auditoria (
                    institucion_id, usuario_id, usuario_email, usuario_nombre,
                    operacion, modulo, severidad,
                    objeto_tipo, objeto_id, objeto_codigo, objeto_descripcion,
                    documento_referencia, ip, user_agent
                ) VALUES (
                    @InstitucionId, @UsuarioId, @UsuarioEmail, @UsuarioNombre,
                    'CREAR', @Modulo, 'INFO',
                    'Movil', @ObjetoId, @ObjetoCodigo, @Descripcion,
                    @DocumentoReferencia, @Ip, @UserAgent
                );";

            await conn.ExecuteAsync(sql, new
            {
                ctx.InstitucionId,
                ctx.UsuarioId,
                ctx.UsuarioEmail,
                ctx.UsuarioNombre,
                Modulo = modulo,
                ObjetoId = objetoId,
                ObjetoCodigo = objetoCodigo,
                Descripcion = descripcion,
                DocumentoReferencia = documentoReferencia,
                Ip = "movil",
                UserAgent = $"MAUI Android ({ctx.DispositivoId ?? "desconocido"})"
            });
        }
        catch (Exception ex)
        {
            // No romper el sync si falla la auditoria
            _logger.LogWarning(ex, "No se pudo registrar auditoria para {Modulo} id={Id}", modulo, objetoId);
        }
    }
}