using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Talleres.Aplicacion.DTOs.CatalogosVehiculos;
using Talleres.Aplicacion.Servicios;
using Talleres.Dominio.Excepciones;
using Talleres.Infraestructura.Persistencia;
using Talleres.Pruebas.Soporte;

namespace Talleres.Pruebas.Servicios;

public sealed class CatalogoVehiculoServicioPruebas
{
    [Fact]
    public async Task CrearMarcas_MismoNombreEnEmpresasDistintas_LasMantieneAisladas()
    {
        var raiz = new InMemoryDatabaseRoot();
        var nombreBaseDatos = Guid.NewGuid().ToString();
        var empresaUno = new ContextoEmpresaPrueba(1);
        var empresaDos = new ContextoEmpresaPrueba(2);

        await using (var contextoUno = CrearDbContext(empresaUno, nombreBaseDatos, raiz))
        {
            await new CatalogoVehiculoServicio(contextoUno, empresaUno).CrearMarcaAsync(
                new GuardarMarcaVehiculoSolicitud { Nombre = "Toyota" });
        }

        await using var contextoDos = CrearDbContext(empresaDos, nombreBaseDatos, raiz);
        var servicioDos = new CatalogoVehiculoServicio(contextoDos, empresaDos);
        Assert.Empty(await servicioDos.ListarMarcasAsync());
        var marca = await servicioDos.CrearMarcaAsync(
            new GuardarMarcaVehiculoSolicitud { Nombre = "Toyota" });
        Assert.Equal("Toyota", marca.Nombre);
    }

    [Fact]
    public async Task CrearModelo_MismoNombreEnLaMismaMarca_LanzaReglaNegocio()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearDbContext(
            empresa,
            Guid.NewGuid().ToString(),
            new InMemoryDatabaseRoot());
        var servicio = new CatalogoVehiculoServicio(contexto, empresa);
        var marca = await servicio.CrearMarcaAsync(
            new GuardarMarcaVehiculoSolicitud { Nombre = "Toyota" });
        var solicitud = new GuardarModeloVehiculoSolicitud
        {
            MarcaVehiculoId = marca.Id,
            Nombre = "Corolla"
        };

        await servicio.CrearModeloAsync(solicitud);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            servicio.CrearModeloAsync(solicitud));
    }

    [Fact]
    public async Task CrearModelo_MarcaDeOtraEmpresa_NoPermiteRelacionarla()
    {
        var raiz = new InMemoryDatabaseRoot();
        var nombreBaseDatos = Guid.NewGuid().ToString();
        var empresaUno = new ContextoEmpresaPrueba(1);
        var empresaDos = new ContextoEmpresaPrueba(2);
        long marcaEmpresaUnoId;

        await using (var contextoUno = CrearDbContext(empresaUno, nombreBaseDatos, raiz))
        {
            var marca = await new CatalogoVehiculoServicio(contextoUno, empresaUno)
                .CrearMarcaAsync(new GuardarMarcaVehiculoSolicitud { Nombre = "Toyota" });
            marcaEmpresaUnoId = marca.Id;
        }

        await using var contextoDos = CrearDbContext(empresaDos, nombreBaseDatos, raiz);
        var servicioDos = new CatalogoVehiculoServicio(contextoDos, empresaDos);
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() =>
            servicioDos.CrearModeloAsync(new GuardarModeloVehiculoSolicitud
            {
                MarcaVehiculoId = marcaEmpresaUnoId,
                Nombre = "Corolla"
            }));
    }

    private static TallerDbContext CrearDbContext(
        ContextoEmpresaPrueba empresa,
        string nombreBaseDatos,
        InMemoryDatabaseRoot raiz)
    {
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(nombreBaseDatos, raiz)
            .Options;
        return new TallerDbContext(opciones, empresa);
    }
}
