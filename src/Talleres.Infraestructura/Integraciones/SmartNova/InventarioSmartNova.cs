using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.DTOs.Inventario;
using Talleres.Dominio.Excepciones;

namespace Talleres.Infraestructura.Integraciones.SmartNova;

public sealed class InventarioSmartNova(SmartNovaDbContext contexto) : IInventarioSmartNova
{
    public async Task<IReadOnlyList<BodegaInventarioDto>> ObtenerBodegasAsync(
        int empresaNovaId,
        CancellationToken cancellationToken)
    {
        var filas = await contexto.Database.SqlQueryRaw<FilaBodega>(
            "EXEC dbo.sp_GetBodegasByEmpresa @EmpresaId={0}", empresaNovaId)
            .ToListAsync(cancellationToken);
        return filas
            .Select(item => new BodegaInventarioDto(item.Id, item.Nombre, item.EsPrincipal))
            .ToList();
    }

    public async Task<IReadOnlyList<ArticuloInventarioDto>> ObtenerExistenciasAsync(
        int empresaNovaId,
        int bodegaId,
        string? criterio,
        CancellationToken cancellationToken)
    {
        var filas = await ConsultarArticulosAsync(
            empresaNovaId,
            bodegaId,
            null,
            criterio,
            cancellationToken);
        return filas.Select(ConvertirArticulo).ToList();
    }

    public async Task<ArticuloInventarioDto?> ObtenerArticuloAsync(
        int empresaNovaId,
        int bodegaId,
        int productoId,
        CancellationToken cancellationToken)
    {
        var filas = await ConsultarArticulosAsync(
            empresaNovaId,
            bodegaId,
            productoId,
            null,
            cancellationToken);
        return filas.Select(ConvertirArticulo).SingleOrDefault();
    }

    public async Task<int> RegistrarSalidaAsync(
        RegistrarSalidaInventarioSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var detalles = new DataTable();
        detalles.Columns.Add("ProductoId", typeof(int));
        detalles.Columns.Add("Cantidad", typeof(decimal));
        detalles.Columns.Add("CostoUnitario", typeof(decimal));
        detalles.Columns.Add("Observacion", typeof(string));
        detalles.Columns.Add("UnidadMedida", typeof(string));
        detalles.Rows.Add(
            solicitud.ProductoId,
            solicitud.Cantidad,
            DBNull.Value,
            $"Consumo para orden {solicitud.NumeroOrden}",
            solicitud.UnidadMedida);

        // EF Core no expone TVP ni parámetros OUTPUT. Esta llamada ADO.NET queda
        // aislada en la integración y usa el procedimiento transaccional oficial de NOVA.
        await using var comando = (SqlCommand)contexto.Database.GetDbConnection().CreateCommand();
        comando.CommandText = "dbo.sp_RegistrarSalida";
        comando.CommandType = CommandType.StoredProcedure;
        comando.Parameters.Add(new SqlParameter("@Fecha", SqlDbType.Date) { Value = DateTime.UtcNow.Date });
        comando.Parameters.Add(new SqlParameter("@Observacion", SqlDbType.NVarChar, 500) { Value = $"Consumo de taller para {solicitud.NumeroOrden}" });
        comando.Parameters.Add(new SqlParameter("@BodegaId", SqlDbType.Int) { Value = solicitud.BodegaId });
        comando.Parameters.Add(new SqlParameter("@EmpresaId", SqlDbType.Int) { Value = solicitud.EmpresaNovaId });
        comando.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 450) { Value = solicitud.UsuarioId });
        comando.Parameters.Add(new SqlParameter("@FacturaId", SqlDbType.Int) { Value = DBNull.Value });
        comando.Parameters.Add(new SqlParameter("@TipoSalidaId", SqlDbType.Int) { Value = 5 });
        comando.Parameters.Add(new SqlParameter("@EstadoId", SqlDbType.Int) { Value = 1 });
        comando.Parameters.Add(new SqlParameter("@NumeroDocumento", SqlDbType.NVarChar, 80) { Value = solicitud.NumeroOrden });
        comando.Parameters.Add(new SqlParameter("@ReferenciaExterna", SqlDbType.NVarChar, 120) { Value = solicitud.NumeroOrden });
        comando.Parameters.Add(new SqlParameter("@ObservacionInterna", SqlDbType.NVarChar, 500) { Value = "Generado por el módulo independiente de Talleres." });
        comando.Parameters.Add(new SqlParameter("@Detalles", SqlDbType.Structured)
        {
            TypeName = "dbo.DetalleSalidaType",
            Value = detalles
        });
        var indicador = new SqlParameter("@flag", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        var mensaje = new SqlParameter("@msg", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output };
        comando.Parameters.Add(indicador);
        comando.Parameters.Add(mensaje);

        var cerrarConexion = comando.Connection!.State != ConnectionState.Open;
        if (cerrarConexion)
        {
            await comando.Connection.OpenAsync(cancellationToken);
        }

        try
        {
            int salidaId = 0;
            await using var lector = await comando.ExecuteReaderAsync(cancellationToken);
            if (await lector.ReadAsync(cancellationToken))
            {
                salidaId = lector.GetInt32(0);
            }

            await lector.CloseAsync();
            if (!Convert.ToBoolean(indicador.Value) || salidaId <= 0)
            {
                throw new ReglaNegocioException(
                    Convert.ToString(mensaje.Value) ?? "No fue posible descontar la existencia.");
            }

            return salidaId;
        }
        finally
        {
            if (cerrarConexion)
            {
                await comando.Connection.CloseAsync();
            }
        }
    }

    public async Task AnularSalidaAsync(
        int empresaNovaId,
        int salidaId,
        string usuarioId,
        string motivo,
        CancellationToken cancellationToken)
    {
        await using var comando = (SqlCommand)contexto.Database.GetDbConnection().CreateCommand();
        comando.CommandText = "dbo.sp_AnularSalida";
        comando.CommandType = CommandType.StoredProcedure;
        comando.Parameters.Add(new SqlParameter("@SalidaId", SqlDbType.Int) { Value = salidaId });
        comando.Parameters.Add(new SqlParameter("@EmpresaId", SqlDbType.Int) { Value = empresaNovaId });
        comando.Parameters.Add(new SqlParameter("@UserIdAnulacion", SqlDbType.NVarChar, 450) { Value = usuarioId });
        comando.Parameters.Add(new SqlParameter("@MotivoAnulacion", SqlDbType.NVarChar, 500) { Value = motivo });
        var indicador = new SqlParameter("@flag", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        var mensaje = new SqlParameter("@msg", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output };
        comando.Parameters.Add(indicador);
        comando.Parameters.Add(mensaje);

        var cerrarConexion = comando.Connection!.State != ConnectionState.Open;
        if (cerrarConexion)
        {
            await comando.Connection.OpenAsync(cancellationToken);
        }

        try
        {
            await comando.ExecuteNonQueryAsync(cancellationToken);
            if (!Convert.ToBoolean(indicador.Value))
            {
                throw new IntegracionNoDisponibleException(
                    Convert.ToString(mensaje.Value) ?? "No fue posible anular la salida de inventario.");
            }
        }
        finally
        {
            if (cerrarConexion)
            {
                await comando.Connection.CloseAsync();
            }
        }
    }

    private async Task<IReadOnlyList<FilaArticulo>> ConsultarArticulosAsync(
        int empresaNovaId,
        int bodegaId,
        int? productoId,
        string? criterio,
        CancellationToken cancellationToken)
    {
        var filtro = string.IsNullOrWhiteSpace(criterio) ? null : criterio.Trim();
        return await contexto.Database.SqlQuery<FilaArticulo>($$"""
            SELECT TOP (200)
                ProductoId = p.Id,
                Codigo = COALESCE(NULLIF(LTRIM(RTRIM(p.CodigoBarra)), ''), NULLIF(LTRIM(RTRIM(p.Referencia)), ''), CONVERT(varchar(50), p.Id)),
                Nombre = COALESCE(p.Nombre, ''),
                UnidadMedida = COALESCE(CONVERT(nvarchar(50), p.UnidadVentaID), ''),
                Existencia = CONVERT(decimal(18, 4), COALESCE(eb.Cantidad, 0)),
                PrecioUnitario = CONVERT(decimal(18, 4), COALESCE(precio.Precio, p.CostoActual, p.UltimoCosto, p.ValorCompra))
            FROM Inventario.Producto p
            INNER JOIN Inventario.Bodega b
                ON b.Id = {{bodegaId}}
               AND b.EmpresaId = {{empresaNovaId}}
            LEFT JOIN Inventario.ExistenciasBodega eb
                ON eb.ProductoId = p.Id
               AND eb.BodegaId = b.Id
            OUTER APPLY
            (
                SELECT TOP (1) lista.Precio
                FROM Inventario.ListaPreciosProducto lista
                INNER JOIN General.Tarifa tarifa
                    ON tarifa.Id = lista.TarifaId
                   AND tarifa.EmpresaId = {{empresaNovaId}}
                WHERE lista.ProductoId = p.Id
                ORDER BY tarifa.Id
            ) precio
            WHERE p.EmpresaId = {{empresaNovaId}}
              AND COALESCE(p.TipoId, '1') <> '2'
              AND ({{productoId}} IS NULL OR p.Id = {{productoId}})
              AND ({{filtro}} IS NULL
                   OR COALESCE(p.Nombre, '') LIKE '%' + {{filtro}} + '%'
                   OR COALESCE(p.CodigoBarra, '') LIKE '%' + {{filtro}} + '%'
                   OR COALESCE(p.Referencia, '') LIKE '%' + {{filtro}} + '%')
            ORDER BY p.Nombre
            """).ToListAsync(cancellationToken);
    }

    private static ArticuloInventarioDto ConvertirArticulo(FilaArticulo item) =>
        new(
            item.ProductoId,
            item.Codigo,
            item.Nombre,
            item.UnidadMedida,
            item.Existencia,
            item.PrecioUnitario);

    private sealed class FilaBodega
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool EsPrincipal { get; set; }
    }

    private sealed class FilaArticulo
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal Existencia { get; set; }
        public decimal? PrecioUnitario { get; set; }
    }
}
