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
    public async Task CrearAsync_DocumentoDuplicadoEnMismaEmpresa_LanzaReglaNegocio()
    {
        var contextoEmpresa = new ContextoEmpresaPrueba(1);
        await using var dbContext = CrearDbContext(contextoEmpresa);
        var servicio = new ClienteServicio(dbContext, contextoEmpresa);
        var solicitud = CrearSolicitudCliente("001-010190-0001A");

        await servicio.CrearAsync(solicitud, CancellationToken.None);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            servicio.CrearAsync(solicitud, CancellationToken.None));
        Assert.Contains("documento", excepcion.Message, StringComparison.OrdinalIgnoreCase);
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
                CrearSolicitudCliente("DOC-COMPARTIDO"),
                CancellationToken.None);
        }

        await using var contextoDos = CrearDbContext(empresaDos, nombreBaseDatos, raiz);
        var servicioDos = new ClienteServicio(contextoDos, empresaDos);
        var clientesEmpresaDos = await servicioDos.ListarAsync(
            CancellationToken.None);

        Assert.Empty(clientesEmpresaDos);

        var clienteEmpresaDos = await servicioDos.CrearAsync(
            CrearSolicitudCliente("DOC-COMPARTIDO"),
            CancellationToken.None);
        Assert.Equal("DOC-COMPARTIDO", clienteEmpresaDos.DocumentoIdentidad);
    }

    private static CrearClienteSolicitud CrearSolicitudCliente(string documento) => new()
    {
        Nombre = "Cliente de prueba",
        DocumentoIdentidad = documento,
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

