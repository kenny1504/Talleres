using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.DTOs.Clientes;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.DTOs.Recepciones;
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
    public async Task RegistrarRecepcionAsync_OrdenEnRecepcion_AvanzaADiagnostico()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(empresa);
        var clienteServicio = new ClienteServicio(dbContext, empresa);
        var vehiculoServicio = new VehiculoServicio(dbContext, empresa);
        var ordenServicio = new OrdenServicioServicio(dbContext, empresa);
        var recepcionServicio = new RecepcionVehiculoServicio(dbContext, empresa);

        var cliente = await clienteServicio.CrearAsync(
            CrearCliente("CLIENTE-1"),
            CancellationToken.None);
        var vehiculo = await vehiculoServicio.CrearAsync(
            CrearVehiculo(cliente.Id, "M123456"),
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

        var propietario = await clienteServicio.CrearAsync(
            CrearCliente("PROPIETARIO"),
            CancellationToken.None);
        var otroCliente = await clienteServicio.CrearAsync(
            CrearCliente("OTRO-CLIENTE"),
            CancellationToken.None);
        var vehiculo = await vehiculoServicio.CrearAsync(
            CrearVehiculo(propietario.Id, "M654321"),
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

        var cliente = await clienteServicio.CrearAsync(
            CrearCliente("CLIENTE-ESTADO"),
            CancellationToken.None);
        var vehiculo = await vehiculoServicio.CrearAsync(
            CrearVehiculo(cliente.Id, "M000001"),
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
        DocumentoIdentidad = documento,
        Telefono = "8888-0000"
    };

    private static CrearVehiculoSolicitud CrearVehiculo(long clienteId, string placa) => new()
    {
        ClienteId = clienteId,
        Placa = placa,
        Marca = "Toyota",
        Modelo = "Corolla",
        Anio = 2020
    };
}

