using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Talleres.Aplicacion.DTOs.Clientes;
using Talleres.Aplicacion.Servicios;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Enumeraciones;
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

    [Theory]
    [InlineData("8888-0000", true)]
    [InlineData("88880000", true)]
    [InlineData("(8888) 0000", true)]
    [InlineData("+8888.0000", true)]
    [InlineData("8888\u00a00000", true)]
    [InlineData("8888 0000", false)]
    public async Task CrearAsync_TelefonoRegistrado_RechazaDuplicadoAunqueCambieElFormato(
        string telefono,
        bool activo)
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearDbContext(empresa);
        var servicio = new ClienteServicio(contexto, empresa);
        var original = await servicio.CrearAsync(CrearSolicitudCliente());
        if (!activo)
        {
            await servicio.ActualizarAsync(original.Id, new ActualizarClienteSolicitud
            {
                Nombre = original.Nombre,
                Telefono = original.Telefono,
                Activo = false
            });
        }

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            servicio.CrearAsync(new CrearClienteSolicitud
            {
                Nombre = "Otro nombre",
                Telefono = telefono
            }));

        Assert.Equal("El cliente ya está registrado con este número telefónico.", excepcion.Message);
        Assert.Equal(original.Id, Assert.Single(await servicio.ListarAsync()).Id);
    }

    [Fact]
    public async Task ActualizarAsync_TelefonoPropio_PermiteCambiarElFormato()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearDbContext(empresa);
        var servicio = new ClienteServicio(contexto, empresa);
        var original = await servicio.CrearAsync(CrearSolicitudCliente());

        var actualizado = await servicio.ActualizarAsync(original.Id, new ActualizarClienteSolicitud
        {
            Nombre = "Nombre corregido",
            Telefono = "8888 0000"
        });

        contexto.ChangeTracker.Clear();
        var guardado = await servicio.ObtenerPorIdAsync(original.Id);
        Assert.Equal(actualizado, guardado);
        Assert.Equal("Nombre corregido", guardado.Nombre);
        Assert.Equal("8888 0000", guardado.Telefono);
    }

    [Fact]
    public async Task ActualizarAsync_TelefonoDeOtroCliente_RechazaSinModificarLosDatos()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearDbContext(empresa);
        var servicio = new ClienteServicio(contexto, empresa);
        await servicio.CrearAsync(CrearSolicitudCliente());
        var otro = await servicio.CrearAsync(new CrearClienteSolicitud
        {
            Nombre = "Segundo cliente",
            Telefono = "7777-0000"
        });

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            servicio.ActualizarAsync(otro.Id, new ActualizarClienteSolicitud
            {
                Nombre = "No debe guardarse",
                Telefono = "8888 0000"
            }));

        Assert.Equal("El cliente ya está registrado con este número telefónico.", excepcion.Message);
        // Un guardado posterior tampoco debe persistir cambios del intento rechazado.
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var guardado = await servicio.ObtenerPorIdAsync(otro.Id);
        Assert.Equal("Segundo cliente", guardado.Nombre);
        Assert.Equal("7777-0000", guardado.Telefono);
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

    [Fact]
    public async Task EstadoCuenta_SumaOrdenesVigentesYAbonosGlobales_SinExigirDatosOpcionales()
    {
        var empresa = new ContextoEmpresaPrueba(1);
        await using var contexto = CrearDbContext(empresa);
        var cliente = await CrearClienteConOrdenesAsync(contexto);
        var servicio = new EstadoCuentaClienteServicio(contexto, empresa);

        var cuenta = await servicio.ObtenerAsync(cliente.Id, CancellationToken.None);
        Assert.Equal(1500m, cuenta.TotalOrdenes);
        Assert.Equal(0m, cuenta.TotalPagos);
        Assert.Equal(1500m, cuenta.Saldo);
        Assert.Equal(2, cuenta.Ordenes.Count);

        cuenta = await servicio.RegistrarPagoAsync(cliente.Id,
            new RegistrarPagoClienteSolicitud { Monto = 500m }, CancellationToken.None);
        Assert.Equal(500m, cuenta.TotalPagos);
        Assert.Equal(1000m, cuenta.Saldo);
        var pago = Assert.Single(cuenta.Pagos);
        Assert.Null(pago.FormaPago);
        Assert.Null(pago.Referencia);

        cuenta = await servicio.AnularPagoAsync(cliente.Id, pago.Id, CancellationToken.None);
        Assert.Equal(0m, cuenta.TotalPagos);
        Assert.Equal(1500m, cuenta.Saldo);
        Assert.NotNull(Assert.Single(cuenta.Pagos).FechaAnulacionUtc);

        cuenta = await servicio.RegistrarPagoAsync(cliente.Id,
            new RegistrarPagoClienteSolicitud { Monto = 1700m }, CancellationToken.None);
        Assert.Equal(-200m, cuenta.Saldo);
    }

    [Fact]
    public async Task EstadoCuenta_EmpresaAjena_NoExponeCuentaNiPermiteRegistrarPago()
    {
        var raiz = new InMemoryDatabaseRoot();
        var nombreBaseDatos = Guid.NewGuid().ToString();
        var empresaUno = new ContextoEmpresaPrueba(1);
        long clienteId;
        await using (var contextoUno = CrearDbContext(empresaUno, nombreBaseDatos, raiz))
        {
            clienteId = (await CrearClienteConOrdenesAsync(contextoUno)).Id;
        }

        var empresaDos = new ContextoEmpresaPrueba(2);
        await using var contextoDos = CrearDbContext(empresaDos, nombreBaseDatos, raiz);
        var servicio = new EstadoCuentaClienteServicio(contextoDos, empresaDos);
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() =>
            servicio.ObtenerAsync(clienteId, CancellationToken.None));
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() =>
            servicio.RegistrarPagoAsync(clienteId,
                new RegistrarPagoClienteSolicitud { Monto = 10m }, CancellationToken.None));
    }

    private static async Task<Cliente> CrearClienteConOrdenesAsync(TallerDbContext contexto)
    {
        var cliente = new Cliente
        {
            EmpresaId = 1,
            Nombre = "Cliente con cuenta",
            Telefono = "8888-0000",
            FechaCreacion = DateTime.UtcNow
        };
        var marca = new MarcaVehiculo
        {
            EmpresaId = 1,
            Nombre = "Marca prueba",
            FechaCreacion = DateTime.UtcNow
        };
        var modelo = new ModeloVehiculo
        {
            EmpresaId = 1,
            Marca = marca,
            Nombre = "Modelo prueba",
            FechaCreacion = DateTime.UtcNow
        };
        var vehiculo = new Vehiculo
        {
            EmpresaId = 1,
            Cliente = cliente,
            Modelo = modelo,
            Placa = "M 12345",
            Anio = 2024,
            FechaCreacion = DateTime.UtcNow
        };
        foreach (var (numero, estado, monto) in new[]
        {
            ("OS-1", EstadoOrdenServicio.Reparacion, 1000m),
            ("OS-2", EstadoOrdenServicio.Cerrada, 500m),
            ("OS-3", EstadoOrdenServicio.Cancelada, 900m)
        })
        {
            var orden = new OrdenServicio
            {
                EmpresaId = 1,
                Numero = numero,
                Cliente = cliente,
                Vehiculo = vehiculo,
                Estado = estado,
                FechaIngreso = DateTime.UtcNow
            };
            orden.Detalles.Add(new Talleres.Dominio.Entidades.DetalleOrdenServicio
            {
                EmpresaId = 1,
                Tipo = TipoDetalleOrdenServicio.Manual,
                Descripcion = "Servicio",
                Cantidad = 1,
                PrecioUnitario = monto,
                FechaCreacion = DateTime.UtcNow
            });
            contexto.OrdenesServicio.Add(orden);
        }

        await contexto.SaveChangesAsync(CancellationToken.None);
        return cliente;
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

