using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.DTOs.Inventario;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.Servicios;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Enumeraciones;
using Talleres.Dominio.Excepciones;
using Talleres.Infraestructura.Persistencia;
using Talleres.Pruebas.Soporte;

namespace Talleres.Pruebas.Servicios;

public sealed class DetalleOrdenServicioPruebas
{
    [Fact]
    public async Task AgregarInventarioAsync_ConExistencia_DescuentaYCalculaTotal()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var orden = await CrearOrdenAsync(dbContext, empresa.EmpresaId, EstadoOrdenServicio.Reparacion);
        var inventario = new InventarioPrueba(new ArticuloInventarioDto(
            20, "REP-20", "Pastillas de freno", "unidad", 4, 125.50m));
        var servicio = new Talleres.Aplicacion.Servicios.DetalleOrdenServicio(
            dbContext, empresa, inventario);

        var resumen = await servicio.AgregarInventarioAsync(
            orden.Id,
            new AgregarDetalleInventarioSolicitud
            {
                ProductoId = 20,
                BodegaId = 3,
                Cantidad = 2
            },
            1,
            "usuario-prueba",
            CancellationToken.None);

        var detalle = Assert.Single(resumen.Detalles);
        Assert.True(detalle.ExistenciaDescontada);
        Assert.Equal(251m, resumen.Total);
        Assert.Equal(1, inventario.SalidasRegistradas);
        Assert.Equal(900, (await dbContext.DetallesOrdenesServicio.SingleAsync()).SalidaInventarioId);
    }

    [Fact]
    public async Task AgregarInventarioAsync_SinExistencia_AgregaSinDescontar()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var orden = await CrearOrdenAsync(dbContext, empresa.EmpresaId, EstadoOrdenServicio.Reparacion);
        var inventario = new InventarioPrueba(new ArticuloInventarioDto(
            30, "REP-30", "Filtro especial", "unidad", 0, 80m));
        var servicio = new Talleres.Aplicacion.Servicios.DetalleOrdenServicio(
            dbContext, empresa, inventario);

        var resumen = await servicio.AgregarInventarioAsync(
            orden.Id,
            new AgregarDetalleInventarioSolicitud
            {
                ProductoId = 30,
                BodegaId = 3,
                Cantidad = 1
            },
            1,
            "usuario-prueba",
            CancellationToken.None);

        var detalle = Assert.Single(resumen.Detalles);
        Assert.False(detalle.ExistenciaDescontada);
        Assert.Equal(80m, resumen.Total);
        Assert.Equal(0, inventario.SalidasRegistradas);
    }

    [Fact]
    public async Task AgregarManualAsync_FueraDeReparacion_LanzaReglaNegocio()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var orden = await CrearOrdenAsync(dbContext, empresa.EmpresaId, EstadoOrdenServicio.Diagnostico);
        var inventario = new InventarioPrueba(null);
        var servicio = new Talleres.Aplicacion.Servicios.DetalleOrdenServicio(
            dbContext, empresa, inventario);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.AgregarManualAsync(
            orden.Id,
            new AgregarDetalleManualSolicitud
            {
                Descripcion = "Mano de obra",
                Cantidad = 2,
                PrecioUnitario = 50m
            },
            CancellationToken.None));
    }

    [Fact]
    public async Task AgregarManualAsync_ListaParaEntrega_PermiteCorregirCargos()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var orden = await CrearOrdenAsync(dbContext, empresa.EmpresaId, EstadoOrdenServicio.ListaParaEntrega);
        var servicio = new Talleres.Aplicacion.Servicios.DetalleOrdenServicio(
            dbContext, empresa, new InventarioPrueba(null));

        var resumen = await servicio.AgregarManualAsync(
            orden.Id,
            new AgregarDetalleManualSolicitud
            {
                Descripcion = "Ajuste final de mano de obra",
                Cantidad = 1,
                PrecioUnitario = 75m
            },
            CancellationToken.None);

        Assert.Equal(75m, resumen.Total);
        Assert.Equal("Ajuste final de mano de obra", Assert.Single(resumen.Detalles).Descripcion);
    }

    [Fact]
    public async Task EliminarAsync_ProductoConSalida_DevuelveExistenciaYEliminaDetalle()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var orden = await CrearOrdenAsync(dbContext, empresa.EmpresaId, EstadoOrdenServicio.ListaParaEntrega);
        var detalle = new Talleres.Dominio.Entidades.DetalleOrdenServicio
        {
            EmpresaId = empresa.EmpresaId,
            OrdenServicioId = orden.Id,
            Tipo = TipoDetalleOrdenServicio.Inventario,
            ProductoInventarioId = 20,
            BodegaInventarioId = 3,
            SalidaInventarioId = 900,
            CodigoProducto = "REP-20",
            Descripcion = "Pastillas de freno",
            UnidadMedida = "unidad",
            Cantidad = 2,
            PrecioUnitario = 125.50m,
            ExistenciaDescontada = true,
            FechaCreacion = DateTime.UtcNow
        };
        dbContext.DetallesOrdenesServicio.Add(detalle);
        await dbContext.SaveChangesAsync();
        var inventario = new InventarioPrueba(null);
        var servicio = new Talleres.Aplicacion.Servicios.DetalleOrdenServicio(
            dbContext, empresa, inventario);

        var resumen = await servicio.EliminarAsync(
            orden.Id,
            detalle.Id,
            1,
            "usuario-prueba",
            CancellationToken.None);

        Assert.Empty(resumen.Detalles);
        Assert.Equal(0m, resumen.Total);
        Assert.Equal(1, inventario.AnulacionesRegistradas);
        Assert.Equal(900, inventario.UltimaSalidaAnuladaId);
        Assert.Empty(await dbContext.DetallesOrdenesServicio.ToListAsync());
    }

    [Fact]
    public async Task EliminarAsync_SiNoPuedeDevolverExistencia_ConservaDetalle()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var orden = await CrearOrdenAsync(dbContext, empresa.EmpresaId, EstadoOrdenServicio.Reparacion);
        var detalle = new Talleres.Dominio.Entidades.DetalleOrdenServicio
        {
            EmpresaId = empresa.EmpresaId,
            OrdenServicioId = orden.Id,
            Tipo = TipoDetalleOrdenServicio.Inventario,
            ProductoInventarioId = 20,
            BodegaInventarioId = 3,
            SalidaInventarioId = 900,
            Descripcion = "Pastillas de freno",
            Cantidad = 1,
            PrecioUnitario = 100m,
            ExistenciaDescontada = true,
            FechaCreacion = DateTime.UtcNow
        };
        dbContext.DetallesOrdenesServicio.Add(detalle);
        await dbContext.SaveChangesAsync();
        var inventario = new InventarioPrueba(null, fallarAnulacion: true);
        var servicio = new Talleres.Aplicacion.Servicios.DetalleOrdenServicio(
            dbContext, empresa, inventario);

        await Assert.ThrowsAsync<IntegracionNoDisponibleException>(() => servicio.EliminarAsync(
            orden.Id,
            detalle.Id,
            1,
            "usuario-prueba",
            CancellationToken.None));

        Assert.Equal(1, await dbContext.DetallesOrdenesServicio.CountAsync());
    }

    private static TallerDbContext CrearDbContext(ContextoEmpresaPrueba empresa)
    {
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TallerDbContext(opciones, empresa);
    }

    private static async Task<OrdenServicio> CrearOrdenAsync(
        TallerDbContext contexto,
        long empresaId,
        EstadoOrdenServicio estado)
    {
        var cliente = new Cliente
        {
            EmpresaId = empresaId,
            Nombre = "Cliente prueba",
            Telefono = "8888-0000",
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
        var marca = new MarcaVehiculo
        {
            EmpresaId = empresaId,
            Nombre = "Marca prueba",
            FechaCreacion = DateTime.UtcNow
        };
        var modelo = new ModeloVehiculo
        {
            EmpresaId = empresaId,
            Marca = marca,
            Nombre = "Modelo prueba",
            FechaCreacion = DateTime.UtcNow
        };
        var vehiculo = new Vehiculo
        {
            EmpresaId = empresaId,
            Cliente = cliente,
            Placa = $"P{Guid.NewGuid():N}"[..7],
            Modelo = modelo,
            Anio = 2024,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
        var orden = new OrdenServicio
        {
            EmpresaId = empresaId,
            Numero = $"OS-{Guid.NewGuid():N}"[..25],
            Cliente = cliente,
            Vehiculo = vehiculo,
            Estado = estado,
            FechaIngreso = DateTime.UtcNow
        };
        contexto.OrdenesServicio.Add(orden);
        await contexto.SaveChangesAsync();
        return orden;
    }

    private sealed class InventarioPrueba(
        ArticuloInventarioDto? articulo,
        bool fallarAnulacion = false) : IInventarioSmartNova
    {
        public int SalidasRegistradas { get; private set; }
        public int AnulacionesRegistradas { get; private set; }
        public int? UltimaSalidaAnuladaId { get; private set; }

        public Task<IReadOnlyList<BodegaInventarioDto>> ObtenerBodegasAsync(
            int empresaNovaId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<BodegaInventarioDto>>([]);

        public Task<IReadOnlyList<ArticuloInventarioDto>> ObtenerExistenciasAsync(
            int empresaNovaId,
            int bodegaId,
            string? criterio,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ArticuloInventarioDto>>(
                articulo is null ? [] : [articulo]);

        public Task<ArticuloInventarioDto?> ObtenerArticuloAsync(
            int empresaNovaId,
            int bodegaId,
            int productoId,
            CancellationToken cancellationToken) => Task.FromResult(articulo);

        public Task<int> RegistrarSalidaAsync(
            RegistrarSalidaInventarioSolicitud solicitud,
            CancellationToken cancellationToken)
        {
            SalidasRegistradas++;
            return Task.FromResult(900);
        }

        public Task AnularSalidaAsync(
            int empresaNovaId,
            int salidaId,
            string usuarioId,
            string motivo,
            CancellationToken cancellationToken)
        {
            if (fallarAnulacion)
            {
                throw new IntegracionNoDisponibleException(
                    "No fue posible devolver la existencia.");
            }

            AnulacionesRegistradas++;
            UltimaSalidaAnuladaId = salidaId;
            return Task.CompletedTask;
        }
    }
}
