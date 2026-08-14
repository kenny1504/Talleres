using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Talleres.Aplicacion.DTOs.Clientes;
using Talleres.Aplicacion.DTOs.Vehiculos;
using Talleres.Aplicacion.Servicios;
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
