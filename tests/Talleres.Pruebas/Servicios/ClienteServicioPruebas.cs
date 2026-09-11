using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Talleres.Aplicacion.DTOs.Clientes;
using Talleres.Aplicacion.Servicios;
using Talleres.Dominio.Excepciones;
using Talleres.Infraestructura.Persistencia;
using Talleres.Pruebas.Soporte;

namespace Talleres.Pruebas.Servicios;

public sealed class ClienteServicioPruebas
{
    [Fact]
    public async Task CrearAsync_RegistraCliente()
    {
        var contextoEmpresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(contextoEmpresa);
        var servicio = new ClienteServicio(dbContext, contextoEmpresa);

        var cliente = await servicio.CrearAsync(new CrearClienteSolicitud
        {
            Nombre = "María López",
            Telefono = "8888-0000"
        }, CancellationToken.None);

        Assert.Equal("María López", cliente.Nombre);
        Assert.Equal("8888-0000", cliente.Telefono);
    }

    [Fact]
    public async Task ListarAsync_EmpresasDistintas_AislaLosClientes()
    {
        var raiz = new InMemoryDatabaseRoot();
        var nombreBaseDatos = Guid.NewGuid().ToString();
        var empresaUno = new ContextoEmpresaPrueba(1);
        var empresaDos = new ContextoEmpresaPrueba(2);

        await using (var contextoUno = CrearDbContext(empresaUno, nombreBaseDatos, raiz))
        {
            var servicioUno = new ClienteServicio(contextoUno, empresaUno);
            await servicioUno.CrearAsync(
                CrearSolicitudCliente(),
                CancellationToken.None);
        }

        await using var contextoDos = CrearDbContext(empresaDos, nombreBaseDatos, raiz);
        var servicioDos = new ClienteServicio(contextoDos, empresaDos);
        var clientesEmpresaDos = await servicioDos.ListarAsync(
            CancellationToken.None);

        Assert.Empty(clientesEmpresaDos);

        var clienteEmpresaDos = await servicioDos.CrearAsync(
            CrearSolicitudCliente(),
            CancellationToken.None);
        Assert.Equal("Cliente de prueba", clienteEmpresaDos.Nombre);
    }

    private static CrearClienteSolicitud CrearSolicitudCliente() => new()
    {
        Nombre = "Cliente de prueba",
        Telefono = "8888-0000"
    };

    private static TallerDbContext CrearDbContext(
        ContextoEmpresaPrueba contextoEmpresa,
        string? nombreBaseDatos = null,
        InMemoryDatabaseRoot? raiz = null)
    {
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(
                nombreBaseDatos ?? Guid.NewGuid().ToString(),
                raiz ?? new InMemoryDatabaseRoot())
            .Options;

        return new TallerDbContext(opciones, contextoEmpresa);
    }
}

