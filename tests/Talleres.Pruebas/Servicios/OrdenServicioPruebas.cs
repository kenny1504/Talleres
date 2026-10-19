using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.DTOs.Clientes;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.DTOs.Recepciones;
using Talleres.Aplicacion.DTOs.TecnicosTaller;
using Talleres.Aplicacion.DTOs.Vehiculos;
using Talleres.Aplicacion.Servicios;
using Talleres.Dominio.Enumeraciones;
using Talleres.Dominio.Excepciones;
using Talleres.Infraestructura.Persistencia;
using Talleres.Pruebas.Soporte;

namespace Talleres.Pruebas.Servicios;

public sealed class OrdenServicioPruebas
{
    [Theory]
    [InlineData(EstadoOrdenServicio.Recepcion)]
    [InlineData(EstadoOrdenServicio.Diagnostico)]
    [InlineData(EstadoOrdenServicio.Cotizacion)]
    [InlineData(EstadoOrdenServicio.PendienteAprobacion)]
    [InlineData(EstadoOrdenServicio.PreparacionReparacion)]
    [InlineData(EstadoOrdenServicio.Reparacion)]
    [InlineData(EstadoOrdenServicio.ControlCalidad)]
    [InlineData(EstadoOrdenServicio.Cancelada)]
    public async Task EliminarOrden_AntesDeEntrega_ConservaClienteYVehiculo(EstadoOrdenServicio estado)
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearContextoEliminacion(empresa);
        var orden = await PrepararOrdenEliminableAsync(contexto, empresa, estado);
        var servicio = CrearServicioEliminacion(contexto, empresa, new IntegracionesEliminacion());

        await servicio.EliminarAsync(orden.Id, 1, "usuario");

        Assert.Empty(await contexto.OrdenesServicio.ToListAsync());
        Assert.Empty(await contexto.HistorialOrdenesServicio.ToListAsync());
        Assert.Single(await contexto.Clientes.ToListAsync());
        Assert.Single(await contexto.Vehiculos.ToListAsync());
    }

    [Theory]
    [InlineData(EstadoOrdenServicio.ListaParaEntrega)]
    [InlineData(EstadoOrdenServicio.Entregada)]
    [InlineData(EstadoOrdenServicio.Cerrada)]
    [InlineData(EstadoOrdenServicio.Cancelada)]
    public async Task EliminarOrden_AlcanzoEntrega_RechazaInclusoCancelada(EstadoOrdenServicio estado)
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearContextoEliminacion(empresa);
        var orden = await PrepararOrdenEliminableAsync(contexto, empresa, estado);
        if (estado == EstadoOrdenServicio.Cancelada)
        {
            orden.Historial.Add(new Talleres.Dominio.Entidades.HistorialOrdenServicio
            {
                EmpresaId = 1, EstadoNuevo = EstadoOrdenServicio.ListaParaEntrega,
                Descripcion = "Lista antes de cancelar", Fecha = DateTime.UtcNow
            });
            await contexto.SaveChangesAsync();
        }
        var integraciones = new IntegracionesEliminacion();
        var servicio = CrearServicioEliminacion(contexto, empresa, integraciones);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.EliminarAsync(orden.Id, 1, "usuario"));

        Assert.Single(await contexto.OrdenesServicio.ToListAsync());
        Assert.Empty(integraciones.SalidasDevueltas);
    }

    [Fact]
    public async Task EliminarOrden_DevuelveConsumosYLimpiaRecepcionCargosYEvidencias()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearContextoEliminacion(empresa);
        var orden = await PrepararOrdenEliminableAsync(contexto, empresa, EstadoOrdenServicio.Reparacion);
        orden.Recepcion = new Talleres.Dominio.Entidades.RecepcionVehiculo
        {
            EmpresaId = 1, DescripcionEstado = "Inspección", FechaRecepcion = DateTime.UtcNow
        };
        orden.Recepcion.Evidencias.Add(new Talleres.Dominio.Entidades.EvidenciaInspeccion
        {
            EmpresaId = 1, ClaveObjeto = "inspeccion", NombreArchivo = "foto.jpg", TipoContenido = "image/jpeg"
        });
        orden.EvidenciasDiagnostico.Add(new Talleres.Dominio.Entidades.EvidenciaDiagnostico
        {
            EmpresaId = 1, ClaveObjeto = "diagnostico", NombreArchivo = "foto.jpg", TipoContenido = "image/jpeg"
        });
        orden.Detalles.Add(CrearConsumoEliminacion(31));
        orden.Detalles.Add(CrearConsumoEliminacion(32));
        await contexto.SaveChangesAsync();
        var integraciones = new IntegracionesEliminacion();

        await CrearServicioEliminacion(contexto, empresa, integraciones).EliminarAsync(orden.Id, 1, "usuario");

        Assert.Equal(new[] { 31, 32 }, integraciones.SalidasDevueltas);
        Assert.Equal(new[] { "diagnostico", "inspeccion" }, integraciones.FotografiasEliminadas.Order().ToArray());
        Assert.Empty(await contexto.OrdenesServicio.ToListAsync());
        Assert.Empty(await contexto.DetallesOrdenesServicio.ToListAsync());
        Assert.Empty(await contexto.RecepcionesVehiculo.ToListAsync());
        Assert.Empty(await contexto.EvidenciasInspeccion.ToListAsync());
        Assert.Empty(await contexto.EvidenciasDiagnostico.ToListAsync());
    }

    [Fact]
    public async Task EliminarOrden_SegundaDevolucionFalla_ConservaOrdenYPermiteReintentar()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearContextoEliminacion(empresa);
        var orden = await PrepararOrdenEliminableAsync(contexto, empresa, EstadoOrdenServicio.Reparacion);
        orden.Detalles.Add(CrearConsumoEliminacion(31));
        orden.Detalles.Add(CrearConsumoEliminacion(32));
        await contexto.SaveChangesAsync();
        var integraciones = new IntegracionesEliminacion { SalidaQueFalla = 32 };
        var servicio = CrearServicioEliminacion(contexto, empresa, integraciones);

        await Assert.ThrowsAsync<IntegracionNoDisponibleException>(() => servicio.EliminarAsync(orden.Id, 1, "usuario"));

        Assert.Single(await contexto.OrdenesServicio.ToListAsync());
        Assert.Equal(2, await contexto.DetallesOrdenesServicio.CountAsync());
        Assert.False((await contexto.DetallesOrdenesServicio.SingleAsync(item => item.SalidaInventarioId == 31)).ExistenciaDescontada);
        Assert.True((await contexto.DetallesOrdenesServicio.SingleAsync(item => item.SalidaInventarioId == 32)).ExistenciaDescontada);
        integraciones.SalidaQueFalla = null;
        await servicio.EliminarAsync(orden.Id, 1, "usuario");
        Assert.Equal(new[] { 31, 32 }, integraciones.SalidasDevueltas);
        Assert.Empty(await contexto.OrdenesServicio.ToListAsync());
    }

    [Fact]
    public async Task EliminarOrden_OtraEmpresaONovaDiferente_NoEliminaNiDevuelveConsumos()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearContextoEliminacion(empresa);
        var orden = await PrepararOrdenEliminableAsync(contexto, empresa, EstadoOrdenServicio.Diagnostico);
        var integraciones = new IntegracionesEliminacion();
        var servicio = CrearServicioEliminacion(contexto, empresa, integraciones);
        await Assert.ThrowsAsync<AccesoDenegadoException>(() => servicio.EliminarAsync(orden.Id, 2, "usuario"));
        var servicioOtraEmpresa = CrearServicioEliminacion(contexto, new ContextoEmpresaPrueba(2), integraciones);
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => servicioOtraEmpresa.EliminarAsync(orden.Id, 2, "usuario"));
        Assert.Single(await contexto.OrdenesServicio.ToListAsync());
        Assert.Empty(integraciones.SalidasDevueltas);
    }

    [Fact]
    public async Task EliminarCatalogos_RespetaRelacionesYPermiteEliminarEnOrden()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearContextoEliminacion(empresa);
        var orden = await PrepararOrdenEliminableAsync(contexto, empresa, EstadoOrdenServicio.Cancelada);
        var clientes = new ClienteServicio(contexto, empresa);
        var vehiculos = new VehiculoServicio(contexto, empresa);
        await Assert.ThrowsAsync<ReglaNegocioException>(() => clientes.EliminarAsync(orden.ClienteId));
        await Assert.ThrowsAsync<ReglaNegocioException>(() => vehiculos.EliminarAsync(orden.VehiculoId));
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => new ClienteServicio(contexto, new ContextoEmpresaPrueba(2)).EliminarAsync(orden.ClienteId));
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => new VehiculoServicio(contexto, new ContextoEmpresaPrueba(2)).EliminarAsync(orden.VehiculoId));

        await CrearServicioEliminacion(contexto, empresa, new IntegracionesEliminacion()).EliminarAsync(orden.Id, 1, "usuario");
        await Assert.ThrowsAsync<ReglaNegocioException>(() => clientes.EliminarAsync(orden.ClienteId));
        await vehiculos.EliminarAsync(orden.VehiculoId);
        await clientes.EliminarAsync(orden.ClienteId);
        Assert.Empty(await contexto.Clientes.ToListAsync());
        Assert.Empty(await contexto.Vehiculos.ToListAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EliminarCliente_ConPagosInclusoAnulados_ConservaHistorial(bool anulado)
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearContextoEliminacion(empresa);
        var cliente = await new ClienteServicio(contexto, empresa).CrearAsync(CrearCliente("PAGO"));
        contexto.PagosClientes.Add(new Talleres.Dominio.Entidades.PagoCliente
        {
            EmpresaId = 1, ClienteId = cliente.Id, Monto = 200,
            FechaRegistroUtc = DateTime.UtcNow, FechaAnulacionUtc = anulado ? DateTime.UtcNow : null
        });
        await contexto.SaveChangesAsync();
        await Assert.ThrowsAsync<ReglaNegocioException>(() => new ClienteServicio(contexto, empresa).EliminarAsync(cliente.Id));
        Assert.Single(await contexto.Clientes.ToListAsync());
        Assert.Single(await contexto.PagosClientes.ToListAsync());
    }

    private static TallerDbContext CrearContextoEliminacion(ContextoEmpresaPrueba empresa) => new(
        new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(aviso => aviso.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options, empresa);

    [Fact]
    public async Task EliminarOrden_HistorialConEntregaComoEstadoAnterior_Rechaza()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearContextoEliminacion(empresa);
        var orden = await PrepararOrdenEliminableAsync(contexto, empresa, EstadoOrdenServicio.Cancelada);
        orden.Historial.Add(new Talleres.Dominio.Entidades.HistorialOrdenServicio
        {
            EmpresaId = 1, EstadoAnterior = EstadoOrdenServicio.ListaParaEntrega,
            EstadoNuevo = EstadoOrdenServicio.Cancelada, Descripcion = "Cancelada después de estar lista", Fecha = DateTime.UtcNow
        });
        await contexto.SaveChangesAsync();
        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicioEliminacion(contexto, empresa, new IntegracionesEliminacion()).EliminarAsync(orden.Id, 1, "usuario"));
        Assert.Single(await contexto.OrdenesServicio.ToListAsync());
    }

    [Fact]
    public async Task EliminarOrden_ConsumoSinSalida_RechazaAntesDeDevolverCualquierProducto()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearContextoEliminacion(empresa);
        var orden = await PrepararOrdenEliminableAsync(contexto, empresa, EstadoOrdenServicio.Reparacion);
        orden.Detalles.Add(CrearConsumoEliminacion(31));
        var consumoIncompleto = CrearConsumoEliminacion(32);
        consumoIncompleto.SalidaInventarioId = null;
        orden.Detalles.Add(consumoIncompleto);
        await contexto.SaveChangesAsync();
        var integraciones = new IntegracionesEliminacion();
        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicioEliminacion(contexto, empresa, integraciones).EliminarAsync(orden.Id, 1, "usuario"));
        Assert.Empty(integraciones.SalidasDevueltas);
        Assert.Single(await contexto.OrdenesServicio.ToListAsync());
    }

    [Fact]
    public async Task EliminarCliente_SinVehiculoPropioPeroConOrdenHistorica_Rechaza()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearContextoEliminacion(empresa);
        var orden = await PrepararOrdenEliminableAsync(contexto, empresa, EstadoOrdenServicio.Cancelada);
        var clientes = new ClienteServicio(contexto, empresa);
        var nuevoPropietario = await clientes.CrearAsync(CrearCliente("NUEVO", "8888-1111"));
        var vehiculo = await contexto.Vehiculos.SingleAsync();
        vehiculo.ClienteId = nuevoPropietario.Id;
        await contexto.SaveChangesAsync();
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => clientes.EliminarAsync(orden.ClienteId));
        Assert.Contains("órdenes asociadas", excepcion.Message);
        Assert.Equal(2, await contexto.Clientes.CountAsync());
    }

    private static async Task<Talleres.Dominio.Entidades.OrdenServicio> PrepararOrdenEliminableAsync(
        TallerDbContext contexto, ContextoEmpresaPrueba empresa, EstadoOrdenServicio estado)
    {
        var modeloId = await CrearCatalogoAsync(contexto, empresa.EmpresaId);
        var cliente = await new ClienteServicio(contexto, empresa).CrearAsync(CrearCliente("ELIMINACION"));
        var vehiculo = await new VehiculoServicio(contexto, empresa).CrearAsync(CrearVehiculo(cliente.Id, modeloId, "M123456"));
        var dto = await new OrdenServicioServicio(contexto, empresa).CrearAsync(new CrearOrdenServicioSolicitud
        {
            ClienteId = cliente.Id, VehiculoId = vehiculo.Id
        });
        var orden = await contexto.OrdenesServicio.SingleAsync(item => item.Id == dto.Id);
        orden.Estado = estado;
        await contexto.SaveChangesAsync();
        return orden;
    }

    private static Talleres.Dominio.Entidades.DetalleOrdenServicio CrearConsumoEliminacion(int salidaId) => new()
    {
        EmpresaId = 1, Tipo = TipoDetalleOrdenServicio.Inventario, Descripcion = "Repuesto",
        Cantidad = 1, PrecioUnitario = 100, ExistenciaDescontada = true, SalidaInventarioId = salidaId
    };

    private static EliminacionOrdenServicio CrearServicioEliminacion(
        TallerDbContext contexto, ContextoEmpresaPrueba empresa, IntegracionesEliminacion integraciones) =>
        new(contexto, empresa, integraciones, integraciones,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<EliminacionOrdenServicio>.Instance);

    private sealed class IntegracionesEliminacion :
        Talleres.Aplicacion.Abstracciones.Integraciones.IInventarioSmartNova,
        Talleres.Aplicacion.Abstracciones.Integraciones.IAlmacenamientoEvidencias
    {
        public List<int> SalidasDevueltas { get; } = [];
        public List<string> FotografiasEliminadas { get; } = [];
        public int? SalidaQueFalla { get; set; }
        public Task AnularSalidaAsync(int empresaNovaId, int salidaId, string usuarioId, string motivo, CancellationToken cancellationToken)
        {
            if (salidaId == SalidaQueFalla) throw new IntegracionNoDisponibleException("Inventario no disponible");
            SalidasDevueltas.Add(salidaId);
            return Task.CompletedTask;
        }
        public Task EliminarAsync(string claveObjeto, CancellationToken cancellationToken = default)
        {
            FotografiasEliminadas.Add(claveObjeto);
            return Task.CompletedTask;
        }
        public Task<IReadOnlyList<Talleres.Aplicacion.DTOs.Inventario.BodegaInventarioDto>> ObtenerBodegasAsync(int empresaNovaId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<Talleres.Aplicacion.DTOs.Inventario.ArticuloInventarioDto>> ObtenerExistenciasAsync(int empresaNovaId, int bodegaId, string? criterio, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Talleres.Aplicacion.DTOs.Inventario.ArticuloInventarioDto?> ObtenerArticuloAsync(int empresaNovaId, int bodegaId, int productoId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<int> RegistrarSalidaAsync(Talleres.Aplicacion.DTOs.Inventario.RegistrarSalidaInventarioSolicitud solicitud, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<string> GuardarAsync(long empresaId, long recepcionVehiculoId, string nombreArchivo, string tipoContenido, Stream contenido, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Uri> CrearDireccionLecturaAsync(string claveObjeto, string nombreArchivo, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    [Fact]
    public async Task RegistrarTecnicoComoPredeterminado_ReemplazaAlAnteriorParaOrdenesNuevas()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(aviso => aviso.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        await using var dbContext = new TallerDbContext(opciones, empresa);
        var tecnicoServicio = new TecnicoTallerServicio(dbContext, empresa);
        var primero = await tecnicoServicio.CrearAsync(new CrearTecnicoTallerSolicitud { Nombre = "Ana Ruiz" });
        var segundo = await tecnicoServicio.CrearAsync(new CrearTecnicoTallerSolicitud
        {
            Nombre = "Luis Mora",
            EsPredeterminado = true
        });

        var tecnicos = await tecnicoServicio.ListarAsync();
        Assert.False(tecnicos.Single(tecnico => tecnico.Id == primero.Id).EsPredeterminado);
        Assert.True(tecnicos.Single(tecnico => tecnico.Id == segundo.Id).EsPredeterminado);

        var modeloId = await CrearCatalogoAsync(dbContext, empresa.EmpresaId);
        var cliente = await new ClienteServicio(dbContext, empresa).CrearAsync(CrearCliente("PREFERIDO"));
        var vehiculo = await new VehiculoServicio(dbContext, empresa).CrearAsync(
            CrearVehiculo(cliente.Id, modeloId, "M009991"));
        var orden = await new OrdenServicioServicio(dbContext, empresa).CrearAsync(
            new CrearOrdenServicioSolicitud { ClienteId = cliente.Id, VehiculoId = vehiculo.Id });
        Assert.Equal(segundo.Id, orden.TecnicoTallerId);
    }

    [Fact]
    public async Task CrearYReasignarOrden_RespetaPredeterminadoYTecnicoActivo()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var tecnicoServicio = new TecnicoTallerServicio(dbContext, empresa);
        var primerTecnico = await tecnicoServicio.CrearAsync(new CrearTecnicoTallerSolicitud { Nombre = "Ana Ruiz" });
        var segundoTecnico = await tecnicoServicio.CrearAsync(new CrearTecnicoTallerSolicitud { Nombre = "Luis Mora" });
        Assert.True(primerTecnico.EsPredeterminado);
        Assert.False(segundoTecnico.EsPredeterminado);

        var modeloId = await CrearCatalogoAsync(dbContext, empresa.EmpresaId);
        var cliente = await new ClienteServicio(dbContext, empresa).CrearAsync(CrearCliente("TECNICO"));
        var vehiculo = await new VehiculoServicio(dbContext, empresa).CrearAsync(
            CrearVehiculo(cliente.Id, modeloId, "M009990"));
        var ordenServicio = new OrdenServicioServicio(dbContext, empresa);
        var orden = await ordenServicio.CrearAsync(new CrearOrdenServicioSolicitud
        {
            ClienteId = cliente.Id,
            VehiculoId = vehiculo.Id
        });
        Assert.Equal(primerTecnico.Id, orden.TecnicoTallerId);
        Assert.Equal("Ana Ruiz", orden.NombreTecnico);

        var reasignada = await ordenServicio.AsignarTecnicoAsync(
            orden.Id, new AsignarTecnicoOrdenSolicitud { TecnicoTallerId = segundoTecnico.Id });
        Assert.Equal(segundoTecnico.Id, reasignada.TecnicoTallerId);
        Assert.Equal("Luis Mora", (await ordenServicio.ObtenerPorIdAsync(orden.Id)).NombreTecnico);

        await tecnicoServicio.ActualizarAsync(segundoTecnico.Id,
            new GuardarTecnicoTallerSolicitud { Nombre = "Luis Mora", Activo = false });
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => ordenServicio.CrearAsync(
            new CrearOrdenServicioSolicitud
            {
                ClienteId = cliente.Id,
                VehiculoId = vehiculo.Id,
                TecnicoTallerId = segundoTecnico.Id
            }));
    }

    [Fact]
    public async Task RegistrarRecepcionAsync_OrdenEnRecepcion_AvanzaADiagnostico()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var clienteServicio = new ClienteServicio(dbContext, empresa);
        var vehiculoServicio = new VehiculoServicio(dbContext, empresa);
        var ordenServicio = new OrdenServicioServicio(dbContext, empresa);
        var recepcionServicio = new RecepcionVehiculoServicio(dbContext, empresa);
        var modeloId = await CrearCatalogoAsync(dbContext, empresa.EmpresaId);

        var cliente = await clienteServicio.CrearAsync(
            CrearCliente("CLIENTE-1"),
            CancellationToken.None);
        var vehiculo = await vehiculoServicio.CrearAsync(
            CrearVehiculo(cliente.Id, modeloId, "M123456"),
            CancellationToken.None);
        var orden = await ordenServicio.CrearAsync(
            new CrearOrdenServicioSolicitud
            {
                ClienteId = cliente.Id,
                VehiculoId = vehiculo.Id,
                Observaciones = "RevisiÃ³n general"
            },
            CancellationToken.None);

        await recepcionServicio.RegistrarAsync(
            orden.Id,
            new RegistrarRecepcionVehiculoSolicitud
            {
                Kilometraje = 85_000,
                PorcentajeCombustible = 50,
                DescripcionEstado = "RayÃ³n leve en puerta derecha",
                DejaLlaves = true,
                DejaLlantaRepuesto = true,
                DejaTrianguloSeguridad = true,
                DejaGata = true,
                DejaLlaveRuedas = true,
                DejaGanchoRemolque = true,
                DejaExtintor = true,
                DejaBotiquin = true,
                DejaChalecoReflectante = true,
                DejaPasacorrientes = true,
                DejaCopaRueda = true,
                DejaAlfombras = true,
                RevisionOperativa = Enum.GetValues<ActividadRevisionOperativa>()
                    .Select(actividad => new ResultadoRevisionOperativaDto
                    {
                        Actividad = actividad,
                        Correcto = actividad != ActividadRevisionOperativa.Radio
                    }).ToArray(),
                OtrosElementos = "  Llave de seguridad de ruedas  ",
                AceptaPruebaRuta = true,
                Danios =
                [
                    new RegistrarDanioVehiculoSolicitud
                    {
                        Zona = ZonaVehiculo.LateralDerecho,
                        Tipo = TipoDanioVehiculo.Rayon,
                        Severidad = SeveridadDanioVehiculo.Leve,
                        Observacion = "Rayón superficial en la puerta trasera."
                    }
                ]
            },
            CancellationToken.None);

        var ordenActualizada = await ordenServicio.ObtenerPorIdAsync(
            orden.Id,
            CancellationToken.None);
        Assert.Equal(EstadoOrdenServicio.Diagnostico, ordenActualizada.Estado);
        Assert.True(ordenActualizada.TieneRecepcion);
        Assert.Null(orden.Kilometraje);
        Assert.Equal(85_000, ordenActualizada.Kilometraje);
        Assert.Equal(2, await dbContext.HistorialOrdenesServicio.CountAsync(
            CancellationToken.None));
        Assert.Single(await dbContext.DaniosVehiculo.ToArrayAsync(
            CancellationToken.None));

        var ordenes = await ordenServicio.ListarAsync(CancellationToken.None);
        Assert.Equal(orden.Id, Assert.Single(ordenes).Id);
        Assert.Equal(85_000, Assert.Single(ordenes).Kilometraje);

        dbContext.ChangeTracker.Clear();
        var recepcionConsultada = await recepcionServicio.ObtenerPorOrdenAsync(orden.Id);
        Assert.True(recepcionConsultada.DejaLlantaRepuesto);
        Assert.True(recepcionConsultada.DejaTrianguloSeguridad);
        Assert.True(recepcionConsultada.DejaGata);
        Assert.True(recepcionConsultada.DejaLlaveRuedas);
        Assert.True(recepcionConsultada.DejaGanchoRemolque);
        Assert.True(recepcionConsultada.DejaExtintor);
        Assert.True(recepcionConsultada.DejaBotiquin);
        Assert.True(recepcionConsultada.DejaChalecoReflectante);
        Assert.True(recepcionConsultada.DejaPasacorrientes);
        Assert.True(recepcionConsultada.DejaCopaRueda);
        Assert.True(recepcionConsultada.DejaAlfombras);
        Assert.Equal(17, recepcionConsultada.RevisionOperativa.Count);
        Assert.True(recepcionConsultada.RevisionOperativa.Single(item => item.Actividad == ActividadRevisionOperativa.AireAcondicionado).Correcto);
        Assert.False(recepcionConsultada.RevisionOperativa.Single(item => item.Actividad == ActividadRevisionOperativa.Radio).Correcto);
        Assert.True(recepcionConsultada.RevisionOperativa.Single(item => item.Actividad == ActividadRevisionOperativa.Retrovisores).Correcto);
        Assert.True(recepcionConsultada.AceptaPruebaRuta);
        Assert.Equal("Llave de seguridad de ruedas", recepcionConsultada.OtrosElementos);

        var recepcionActualizada = await recepcionServicio.ActualizarInspeccionAsync(
            orden.Id,
            new ActualizarRecepcionVehiculoSolicitud
            {
                Kilometraje = 85_010,
                PorcentajeCombustible = 50,
                DescripcionEstado = "Observación general corregida después de revisar con el cliente.",
                DejaCopaRueda = true,
                RevisionOperativa =
                [
                    new() { Actividad = ActividadRevisionOperativa.AireAcondicionado, Correcto = false },
                    new() { Actividad = ActividadRevisionOperativa.Radio, Correcto = true }
                ],
                DejaLlaves = true,
                DejaLlantaRepuesto = true,
                DejaLlaveRuedas = true,
                OtrosElementos = "  ",
                Danios =
                [
                    new RegistrarDanioVehiculoSolicitud
                    {
                        Zona = ZonaVehiculo.LateralDerecho,
                        Tipo = TipoDanioVehiculo.Rayon,
                        Severidad = SeveridadDanioVehiculo.Moderado,
                        Observacion = "El rayón también alcanza el borde de la puerta."
                    }
                ]
            },
            CancellationToken.None);

        Assert.Equal(85_010, recepcionActualizada.Kilometraje);
        Assert.Equal(
            "El rayón también alcanza el borde de la puerta.",
            Assert.Single(recepcionActualizada.Danios).Observacion);

        dbContext.ChangeTracker.Clear();
        recepcionConsultada = await recepcionServicio.ObtenerPorOrdenAsync(orden.Id);
        Assert.True(recepcionConsultada.DejaLlantaRepuesto);
        Assert.True(recepcionConsultada.DejaLlaveRuedas);
        Assert.False(recepcionConsultada.DejaTrianguloSeguridad);
        Assert.False(recepcionConsultada.DejaGata);
        Assert.False(recepcionConsultada.DejaGanchoRemolque);
        Assert.False(recepcionConsultada.DejaExtintor);
        Assert.False(recepcionConsultada.DejaBotiquin);
        Assert.False(recepcionConsultada.DejaChalecoReflectante);
        Assert.False(recepcionConsultada.DejaPasacorrientes);
        Assert.True(recepcionConsultada.DejaCopaRueda);
        Assert.False(recepcionConsultada.DejaAlfombras);
        Assert.Collection(recepcionConsultada.RevisionOperativa,
            revision =>
            {
                Assert.Equal(ActividadRevisionOperativa.AireAcondicionado, revision.Actividad);
                Assert.False(revision.Correcto);
            },
            revision =>
            {
                Assert.Equal(ActividadRevisionOperativa.Radio, revision.Actividad);
                Assert.True(revision.Correcto);
            });
        Assert.False(recepcionConsultada.AceptaPruebaRuta);
        Assert.Null(recepcionConsultada.OtrosElementos);
        Assert.Single(recepcionConsultada.Danios);

        var siguienteOrden = await ordenServicio.CrearAsync(new CrearOrdenServicioSolicitud
        {
            ClienteId = cliente.Id,
            VehiculoId = vehiculo.Id
        });
        ordenes = await ordenServicio.ListarAsync();
        Assert.Equal(85_010, ordenes.Single(item => item.Id == orden.Id).Kilometraje);
        Assert.Null(ordenes.Single(item => item.Id == siguienteOrden.Id).Kilometraje);

        await recepcionServicio.RegistrarAsync(siguienteOrden.Id, new RegistrarRecepcionVehiculoSolicitud
        {
            Kilometraje = 0,
            PorcentajeCombustible = 50,
            DescripcionEstado = "Vehículo recibido para otra orden."
        });
        dbContext.ChangeTracker.Clear();
        ordenes = await ordenServicio.ListarAsync();
        Assert.Equal(85_010, ordenes.Single(item => item.Id == orden.Id).Kilometraje);
        Assert.Equal(0, ordenes.Single(item => item.Id == siguienteOrden.Id).Kilometraje);
    }

    [Fact]
    public async Task CrearAsync_VehiculoDeOtroCliente_LanzaReglaNegocio()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var clienteServicio = new ClienteServicio(dbContext, empresa);
        var vehiculoServicio = new VehiculoServicio(dbContext, empresa);
        var ordenServicio = new OrdenServicioServicio(dbContext, empresa);
        var modeloId = await CrearCatalogoAsync(dbContext, empresa.EmpresaId);

        var propietario = await clienteServicio.CrearAsync(
            CrearCliente("PROPIETARIO"),
            CancellationToken.None);
        var otroCliente = await clienteServicio.CrearAsync(
            CrearCliente("OTRO-CLIENTE", "7777-0000"),
            CancellationToken.None);
        var vehiculo = await vehiculoServicio.CrearAsync(
            CrearVehiculo(propietario.Id, modeloId, "M654321"),
            CancellationToken.None);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            ordenServicio.CrearAsync(
                new CrearOrdenServicioSolicitud
                {
                    ClienteId = otroCliente.Id,
                    VehiculoId = vehiculo.Id
                },
                CancellationToken.None));

        Assert.Contains("no pertenece", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CambiarEstadoAsync_TransicionNoPermitida_LanzaReglaNegocio()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var clienteServicio = new ClienteServicio(dbContext, empresa);
        var vehiculoServicio = new VehiculoServicio(dbContext, empresa);
        var ordenServicio = new OrdenServicioServicio(dbContext, empresa);
        var modeloId = await CrearCatalogoAsync(dbContext, empresa.EmpresaId);

        var cliente = await clienteServicio.CrearAsync(
            CrearCliente("CLIENTE-ESTADO"),
            CancellationToken.None);
        var vehiculo = await vehiculoServicio.CrearAsync(
            CrearVehiculo(cliente.Id, modeloId, "M000001"),
            CancellationToken.None);
        var orden = await ordenServicio.CrearAsync(
            new CrearOrdenServicioSolicitud
            {
                ClienteId = cliente.Id,
                VehiculoId = vehiculo.Id
            },
            CancellationToken.None);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => ordenServicio.CambiarEstadoAsync(
            orden.Id,
            new CambiarEstadoOrdenServicioSolicitud
            {
                Estado = EstadoOrdenServicio.Entregada,
                Descripcion = "TransiciÃ³n invÃ¡lida para la prueba"
            },
                CancellationToken.None));
    }

    [Theory]
    [InlineData(EstadoOrdenServicio.Diagnostico, false)]
    [InlineData(EstadoOrdenServicio.Cotizacion, false)]
    [InlineData(EstadoOrdenServicio.PendienteAprobacion, false)]
    [InlineData(EstadoOrdenServicio.PreparacionReparacion, false)]
    [InlineData(EstadoOrdenServicio.PendienteAprobacion, true)]
    public async Task CambiarEstadoAsync_FlujoOperativo_AvanzaHastaListaParaEntrega(
        EstadoOrdenServicio estadoInicial,
        bool conAutorizacionDigital)
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var clienteServicio = new ClienteServicio(dbContext, empresa);
        var vehiculoServicio = new VehiculoServicio(dbContext, empresa);
        var ordenServicio = new OrdenServicioServicio(dbContext, empresa);
        var modeloId = await CrearCatalogoAsync(dbContext, empresa.EmpresaId);
        var cliente = await clienteServicio.CrearAsync(
            CrearCliente("CLIENTE-FLUJO"),
            CancellationToken.None);
        var vehiculo = await vehiculoServicio.CrearAsync(
            CrearVehiculo(cliente.Id, modeloId, "M000002"),
            CancellationToken.None);
        var orden = await ordenServicio.CrearAsync(
            new CrearOrdenServicioSolicitud
            {
                ClienteId = cliente.Id,
                VehiculoId = vehiculo.Id
            },
            CancellationToken.None);
        var entidad = await dbContext.OrdenesServicio.SingleAsync();
        entidad.Estado = estadoInicial;
        entidad.Diagnostico = "Se detectó desgaste en el sistema de frenos delantero.";
        entidad.FechaDiagnosticoUtc = DateTime.UtcNow;
        var fechaAutorizacion = conAutorizacionDigital ? DateTime.UtcNow : (DateTime?)null;
        entidad.FechaAutorizacionClienteUtc = fechaAutorizacion;
        await dbContext.SaveChangesAsync();

        var ordenConsultada = await ordenServicio.ObtenerPorIdAsync(orden.Id);
        Assert.Equal(fechaAutorizacion, ordenConsultada.FechaAutorizacionClienteUtc);
        Assert.Equal(estadoInicial, ordenConsultada.Estado);
        Assert.Equal(fechaAutorizacion, Assert.Single(await ordenServicio.ListarAsync()).FechaAutorizacionClienteUtc);
        var cuenta = await new EstadoCuentaClienteServicio(dbContext, empresa).ObtenerAsync(cliente.Id);
        Assert.Equal(fechaAutorizacion, Assert.Single(cuenta.Ordenes).FechaAutorizacionClienteUtc);

        foreach (var estado in new[]
                 {
                     EstadoOrdenServicio.Reparacion,
                     EstadoOrdenServicio.ListaParaEntrega
                 })
        {
            orden = await ordenServicio.CambiarEstadoAsync(
                orden.Id,
                new CambiarEstadoOrdenServicioSolicitud
                {
                    Estado = estado,
                    Descripcion = $"Avance de prueba a {estado}."
                },
                CancellationToken.None);
        }

        Assert.Equal(EstadoOrdenServicio.ListaParaEntrega, orden.Estado);
        Assert.Equal(fechaAutorizacion, orden.FechaAutorizacionClienteUtc);
        Assert.True(orden.VisibleEnInicio);
        Assert.True(orden.FechaUltimoCambioEstado > orden.FechaIngreso);
        Assert.Equal(fechaAutorizacion, entidad.FechaAutorizacionClienteUtc);
        var inicioReparacion = Assert.Single(entidad.Historial,
            item => item.EstadoNuevo == EstadoOrdenServicio.Reparacion);
        Assert.Equal(estadoInicial, inicioReparacion.EstadoAnterior);
        Assert.Equal(conAutorizacionDigital
            ? "Reparación iniciada desde el taller con autorización digital del cliente registrada."
            : "Reparación iniciada desde el taller sin autorización digital del cliente.", inicioReparacion.Descripcion);
    }

    [Fact]
    public async Task ListarAsync_ListaParaEntregaConMasDe36Horas_NoEsVisibleEnInicio()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var ordenServicio = new OrdenServicioServicio(dbContext, empresa);
        var modeloId = await CrearCatalogoAsync(dbContext, empresa.EmpresaId);
        var clienteServicio = new ClienteServicio(dbContext, empresa);
        var vehiculoServicio = new VehiculoServicio(dbContext, empresa);
        var cliente = await clienteServicio.CrearAsync(CrearCliente("CLIENTE-ANTIGUO"));
        var vehiculo = await vehiculoServicio.CrearAsync(CrearVehiculo(cliente.Id, modeloId, "M000004"));
        var ordenCreada = await ordenServicio.CrearAsync(new CrearOrdenServicioSolicitud
        {
            ClienteId = cliente.Id,
            VehiculoId = vehiculo.Id
        });
        var entidad = await dbContext.OrdenesServicio
            .Include(orden => orden.Historial)
            .SingleAsync();
        entidad.Estado = EstadoOrdenServicio.ListaParaEntrega;
        Assert.Single(entidad.Historial).Fecha = DateTime.UtcNow.AddHours(-48);
        entidad.Historial.Add(new Talleres.Dominio.Entidades.HistorialOrdenServicio
        {
            EmpresaId = empresa.EmpresaId,
            EstadoAnterior = EstadoOrdenServicio.Reparacion,
            EstadoNuevo = EstadoOrdenServicio.ListaParaEntrega,
            Descripcion = "Lista para entregar desde hace más de 36 horas.",
            Fecha = DateTime.UtcNow.AddHours(-37)
        });
        await dbContext.SaveChangesAsync();

        var orden = Assert.Single(await ordenServicio.ListarAsync());

        Assert.Equal(ordenCreada.Id, orden.Id);
        Assert.False(orden.VisibleEnInicio);
    }

    [Theory]
    [InlineData(EstadoOrdenServicio.Diagnostico, EstadoOrdenServicio.PendienteAprobacion)]
    [InlineData(EstadoOrdenServicio.Cotizacion, EstadoOrdenServicio.PendienteAprobacion)]
    [InlineData(EstadoOrdenServicio.Diagnostico, EstadoOrdenServicio.Reparacion)]
    [InlineData(EstadoOrdenServicio.Cotizacion, EstadoOrdenServicio.Reparacion)]
    [InlineData(EstadoOrdenServicio.PendienteAprobacion, EstadoOrdenServicio.Reparacion)]
    [InlineData(EstadoOrdenServicio.PreparacionReparacion, EstadoOrdenServicio.Reparacion)]
    public async Task CambiarEstadoAsync_SinDiagnostico_NoPermiteEnviarAprobacionNiIniciarReparacion(
        EstadoOrdenServicio estadoInicial,
        EstadoOrdenServicio estadoSolicitado)
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var ordenServicio = new OrdenServicioServicio(dbContext, empresa);
        var modeloId = await CrearCatalogoAsync(dbContext, empresa.EmpresaId);
        var clienteServicio = new ClienteServicio(dbContext, empresa);
        var vehiculoServicio = new VehiculoServicio(dbContext, empresa);
        var cliente = await clienteServicio.CrearAsync(CrearCliente("CLIENTE-DIAGNOSTICO"));
        var vehiculo = await vehiculoServicio.CrearAsync(CrearVehiculo(cliente.Id, modeloId, "M000003"));
        var orden = await ordenServicio.CrearAsync(new CrearOrdenServicioSolicitud
        {
            ClienteId = cliente.Id,
            VehiculoId = vehiculo.Id
        });
        var entidad = await dbContext.OrdenesServicio.SingleAsync();
        entidad.Estado = estadoInicial;
        await dbContext.SaveChangesAsync();

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            ordenServicio.CambiarEstadoAsync(orden.Id, new CambiarEstadoOrdenServicioSolicitud
            {
                Estado = estadoSolicitado,
                Descripcion = "Enviar diagnóstico para autorización."
            }));

        Assert.Contains("diagnóstico", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(estadoInicial, entidad.Estado);
        Assert.Null(entidad.FechaAutorizacionClienteUtc);
    }

    private static TallerDbContext CrearDbContext(ContextoEmpresaPrueba empresa)
    {
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TallerDbContext(opciones, empresa);
    }

    private static CrearClienteSolicitud CrearCliente(string documento, string telefono = "8888-0000") => new()
    {
        Nombre = $"Cliente {documento}",
        Telefono = telefono
    };

    private static CrearVehiculoSolicitud CrearVehiculo(
        long clienteId,
        long modeloVehiculoId,
        string placa) => new()
    {
        ClienteId = clienteId,
        Placa = placa,
        ModeloVehiculoId = modeloVehiculoId,
        Anio = 2020
    };

    private static async Task<long> CrearCatalogoAsync(
        TallerDbContext contexto,
        long empresaId)
    {
        var marca = new Talleres.Dominio.Entidades.MarcaVehiculo
        {
            EmpresaId = empresaId,
            Nombre = "Toyota",
            FechaCreacion = DateTime.UtcNow
        };
        var modelo = new Talleres.Dominio.Entidades.ModeloVehiculo
        {
            EmpresaId = empresaId,
            Marca = marca,
            Nombre = "Corolla",
            FechaCreacion = DateTime.UtcNow
        };
        contexto.ModelosVehiculo.Add(modelo);
        await contexto.SaveChangesAsync();
        return modelo.Id;
    }
}

