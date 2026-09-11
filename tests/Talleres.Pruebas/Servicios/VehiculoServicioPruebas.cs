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
            var modeloId = await CrearCatalogoAsync(contextoUno, empresaUno.EmpresaId);
            var cliente = await new ClienteServicio(contextoUno, empresaUno).CrearAsync(
                CrearCliente("DOC-EMPRESA-UNO"),
                CancellationToken.None);
            await new VehiculoServicio(contextoUno, empresaUno).CrearAsync(
                CrearVehiculo(cliente.Id, modeloId, "M 100-001"),
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
        var modeloToyotaId = await CrearCatalogoAsync(contexto, empresa.EmpresaId);
        var propietarioOriginal = await clienteServicio.CrearAsync(
            CrearCliente("PROPIETARIO-ORIGINAL"),
            CancellationToken.None);
        var nuevoPropietario = await clienteServicio.CrearAsync(
            CrearCliente("NUEVO-PROPIETARIO"),
            CancellationToken.None);
        var vehiculo = await vehiculoServicio.CrearAsync(
            CrearVehiculo(propietarioOriginal.Id, modeloToyotaId, "M 100-001"),
            CancellationToken.None);
        var modeloHondaId = await CrearCatalogoAsync(
            contexto,
            empresa.EmpresaId,
            "Honda",
            "Civic");

        var actualizado = await vehiculoServicio.ActualizarAsync(
            vehiculo.Id,
            new ActualizarVehiculoSolicitud
            {
                ClienteId = nuevoPropietario.Id,
                Placa = " m 200-002 ",
                ModeloVehiculoId = modeloHondaId,
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
            var modeloId = await CrearCatalogoAsync(contextoUno, empresaUno.EmpresaId);
            var cliente = await new ClienteServicio(contextoUno, empresaUno).CrearAsync(
                CrearCliente("CLIENTE-EMPRESA-UNO"),
                CancellationToken.None);
            var vehiculo = await new VehiculoServicio(contextoUno, empresaUno).CrearAsync(
                CrearVehiculo(cliente.Id, modeloId, "M 300-003"),
                CancellationToken.None);
            vehiculoEmpresaUnoId = vehiculo.Id;
        }

        await using var contextoDos = CrearDbContext(empresaDos, nombreBaseDatos, raiz);
        var clienteEmpresaDos = await new ClienteServicio(contextoDos, empresaDos).CrearAsync(
            CrearCliente("CLIENTE-EMPRESA-DOS"),
            CancellationToken.None);
        var servicioEmpresaDos = new VehiculoServicio(contextoDos, empresaDos);
        var modeloEmpresaDosId = await CrearCatalogoAsync(contextoDos, empresaDos.EmpresaId);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() =>
            servicioEmpresaDos.ActualizarAsync(
                vehiculoEmpresaUnoId,
                new ActualizarVehiculoSolicitud
                {
                    ClienteId = clienteEmpresaDos.Id,
                    Placa = "M 999-999",
                    ModeloVehiculoId = modeloEmpresaDosId,
                    Anio = 2024,
                    Activo = true
                },
                CancellationToken.None));
    }

    private static CrearClienteSolicitud CrearCliente(string _) => new()
    {
        Nombre = "Cliente de prueba",
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
        Anio = 2024
    };

    private static async Task<long> CrearCatalogoAsync(
        TallerDbContext contexto,
        long empresaId,
        string marcaNombre = "Toyota",
        string modeloNombre = "Corolla")
    {
        var marca = new Talleres.Dominio.Entidades.MarcaVehiculo
        {
            EmpresaId = empresaId,
            Nombre = marcaNombre,
            FechaCreacion = DateTime.UtcNow
        };
        var modelo = new Talleres.Dominio.Entidades.ModeloVehiculo
        {
            EmpresaId = empresaId,
            Marca = marca,
            Nombre = modeloNombre,
            FechaCreacion = DateTime.UtcNow
        };
        contexto.ModelosVehiculo.Add(modelo);
        await contexto.SaveChangesAsync();
        return modelo.Id;
    }

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
