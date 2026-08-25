using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Talleres.Aplicacion.DTOs.Clientes;
using Talleres.Aplicacion.DTOs.Vehiculos;
using Talleres.Aplicacion.Servicios;
using Talleres.Dominio.Excepciones;
using Talleres.Infraestructura.Persistencia;
using Talleres.Pruebas.Soporte;

namespace Talleres.Pruebas.Servicios;

public sealed class VehiculoServicioPruebas
{
    [Fact]
    public async Task ListarAsync_EmpresasDistintas_AislaLosVehiculos()
    {
        var raiz = new InMemoryDatabaseRoot();
        var nombreBaseDatos = Guid.NewGuid().ToString();
        var empresaUno = new ContextoEmpresaPrueba(1);
        var empresaDos = new ContextoEmpresaPrueba(2);

        await using (var contextoUno = CrearDbContext(empresaUno, nombreBaseDatos, raiz))
        {
            var cliente = await new ClienteServicio(contextoUno, empresaUno).CrearAsync(
                CrearCliente("DOC-EMPRESA-UNO"),
                CancellationToken.None);
            await new VehiculoServicio(contextoUno, empresaUno).CrearAsync(
                CrearVehiculo(cliente.Id, "M 100-001"),
                CancellationToken.None);
        }

        await using var contextoDos = CrearDbContext(empresaDos, nombreBaseDatos, raiz);
        var vehiculos = await new VehiculoServicio(contextoDos, empresaDos).ListarAsync(
            CancellationToken.None);

        Assert.Empty(vehiculos);
    }

    [Fact]
    public async Task ActualizarAsync_DatosValidos_ActualizaTodosLosCamposEditables()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearDbContext(
            empresa,
            Guid.NewGuid().ToString(),
            new InMemoryDatabaseRoot());
        var clienteServicio = new ClienteServicio(contexto, empresa);
        var vehiculoServicio = new VehiculoServicio(contexto, empresa);
        var propietarioOriginal = await clienteServicio.CrearAsync(
            CrearCliente("PROPIETARIO-ORIGINAL"),
            CancellationToken.None);
        var nuevoPropietario = await clienteServicio.CrearAsync(
            CrearCliente("NUEVO-PROPIETARIO"),
            CancellationToken.None);
        var vehiculo = await vehiculoServicio.CrearAsync(
            CrearVehiculo(propietarioOriginal.Id, "M 100-001"),
            CancellationToken.None);

        var actualizado = await vehiculoServicio.ActualizarAsync(
            vehiculo.Id,
            new ActualizarVehiculoSolicitud
            {
                ClienteId = nuevoPropietario.Id,
                Placa = " m 200-002 ",
                Marca = " Honda ",
                Modelo = " Civic ",
                Anio = 2025,
                Color = " Azul ",
                NumeroVin = " vin-actualizado ",
                Activo = false
            },
            CancellationToken.None);

        Assert.Equal(nuevoPropietario.Id, actualizado.ClienteId);
        Assert.Equal("M 200-002", actualizado.Placa);
        Assert.Equal("Honda", actualizado.Marca);
        Assert.Equal("Civic", actualizado.Modelo);
        Assert.Equal(2025, actualizado.Anio);
        Assert.Equal("Azul", actualizado.Color);
        Assert.Equal("VIN-ACTUALIZADO", actualizado.NumeroVin);
        Assert.False(actualizado.Activo);
    }

    [Fact]
    public async Task ActualizarAsync_VehiculoDeOtraEmpresa_NoPermiteModificarlo()
    {
        var raiz = new InMemoryDatabaseRoot();
        var nombreBaseDatos = Guid.NewGuid().ToString();
        var empresaUno = new ContextoEmpresaPrueba(1);
        var empresaDos = new ContextoEmpresaPrueba(2);
        long vehiculoEmpresaUnoId;

        await using (var contextoUno = CrearDbContext(empresaUno, nombreBaseDatos, raiz))
        {
            var cliente = await new ClienteServicio(contextoUno, empresaUno).CrearAsync(
                CrearCliente("CLIENTE-EMPRESA-UNO"),
                CancellationToken.None);
            var vehiculo = await new VehiculoServicio(contextoUno, empresaUno).CrearAsync(
                CrearVehiculo(cliente.Id, "M 300-003"),
                CancellationToken.None);
            vehiculoEmpresaUnoId = vehiculo.Id;
        }

        await using var contextoDos = CrearDbContext(empresaDos, nombreBaseDatos, raiz);
        var clienteEmpresaDos = await new ClienteServicio(contextoDos, empresaDos).CrearAsync(
            CrearCliente("CLIENTE-EMPRESA-DOS"),
            CancellationToken.None);
        var servicioEmpresaDos = new VehiculoServicio(contextoDos, empresaDos);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() =>
            servicioEmpresaDos.ActualizarAsync(
                vehiculoEmpresaUnoId,
                new ActualizarVehiculoSolicitud
                {
                    ClienteId = clienteEmpresaDos.Id,
                    Placa = "M 999-999",
                    Marca = "Toyota",
                    Modelo = "Corolla",
                    Anio = 2024,
                    Activo = true
                },
                CancellationToken.None));
    }

    private static CrearClienteSolicitud CrearCliente(string documento) => new()
    {
        Nombre = "Cliente de prueba",
        DocumentoIdentidad = documento,
        Telefono = "8888-0000"
    };

    private static CrearVehiculoSolicitud CrearVehiculo(long clienteId, string placa) => new()
    {
        ClienteId = clienteId,
        Placa = placa,
        Marca = "Toyota",
        Modelo = "Corolla",
        Anio = 2024
    };

    private static TallerDbContext CrearDbContext(
        ContextoEmpresaPrueba contextoEmpresa,
        string nombreBaseDatos,
        InMemoryDatabaseRoot raiz)
    {
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(nombreBaseDatos, raiz)
            .Options;

        return new TallerDbContext(opciones, contextoEmpresa);
    }
}
