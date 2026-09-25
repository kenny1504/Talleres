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
        Assert.Equal(2, await dbContext.HistorialOrdenesServicio.CountAsync(
            CancellationToken.None));
        Assert.Single(await dbContext.DaniosVehiculo.ToArrayAsync(
            CancellationToken.None));

        var ordenes = await ordenServicio.ListarAsync(CancellationToken.None);
        Assert.Equal(orden.Id, Assert.Single(ordenes).Id);

        var recepcionActualizada = await recepcionServicio.ActualizarInspeccionAsync(
            orden.Id,
            new ActualizarRecepcionVehiculoSolicitud
            {
                Kilometraje = 85_010,
                PorcentajeCombustible = 50,
                DescripcionEstado = "Observación general corregida después de revisar con el cliente.",
                DejaLlaves = true,
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
            CrearCliente("OTRO-CLIENTE"),
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

    [Fact]
    public async Task CambiarEstadoAsync_FlujoOperativo_AvanzaHastaListaParaEntrega()
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
        entidad.Estado = EstadoOrdenServicio.Diagnostico;
        entidad.Diagnostico = "Se detectó desgaste en el sistema de frenos delantero.";
        entidad.FechaDiagnosticoUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        foreach (var estado in new[]
                 {
                     EstadoOrdenServicio.PendienteAprobacion,
                     EstadoOrdenServicio.Reparacion,
                     EstadoOrdenServicio.ListaParaEntrega
                 })
        {
            if (estado == EstadoOrdenServicio.Reparacion)
            {
                entidad.FechaAutorizacionClienteUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }
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
        Assert.True(orden.VisibleEnInicio);
        Assert.True(orden.FechaUltimoCambioEstado > orden.FechaIngreso);
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

    [Fact]
    public async Task CambiarEstadoAsync_SinDiagnostico_NoPermiteEnviarAprobacion()
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
        entidad.Estado = EstadoOrdenServicio.Diagnostico;
        await dbContext.SaveChangesAsync();

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            ordenServicio.CambiarEstadoAsync(orden.Id, new CambiarEstadoOrdenServicioSolicitud
            {
                Estado = EstadoOrdenServicio.PendienteAprobacion,
                Descripcion = "Enviar diagnóstico para autorización."
            }));

        Assert.Contains("diagnóstico", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static TallerDbContext CrearDbContext(ContextoEmpresaPrueba empresa)
    {
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TallerDbContext(opciones, empresa);
    }

    private static CrearClienteSolicitud CrearCliente(string documento) => new()
    {
        Nombre = $"Cliente {documento}",
        Telefono = "8888-0000"
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

