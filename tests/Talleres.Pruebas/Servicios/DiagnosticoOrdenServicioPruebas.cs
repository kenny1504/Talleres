using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.DTOs.Autenticacion;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.Servicios;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Enumeraciones;
using Talleres.Infraestructura.Persistencia;
using Talleres.Pruebas.Soporte;

namespace Talleres.Pruebas.Servicios;

public sealed class DiagnosticoOrdenServicioPruebas
{
    [Fact]
    public async Task GuardarAsync_CreaTokenPublicoYPersisteDiagnostico()
    {
        var empresa = new ContextoEmpresaPrueba(7);
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexto = new TallerDbContext(opciones, empresa);
        var orden = new OrdenServicio
        {
            EmpresaId = empresa.EmpresaId,
            Numero = "OS-DIAGNOSTICO-1",
            ClienteId = 1,
            VehiculoId = 1,
            Estado = EstadoOrdenServicio.Diagnostico,
            FechaIngreso = DateTime.UtcNow
        };
        contexto.OrdenesServicio.Add(orden);
        await contexto.SaveChangesAsync();
        var servicio = new DiagnosticoOrdenServicioServicio(contexto, empresa, new AlmacenamientoEvidenciasPrueba(), new IdentidadNovaPrueba());

        var resultado = await servicio.GuardarAsync(orden.Id, new GuardarDiagnosticoOrdenServicioSolicitud
        {
            Diagnostico = "Se detectó desgaste en las pastillas de freno delanteras."
        });

        Assert.Equal("Se detectó desgaste en las pastillas de freno delanteras.", resultado.Diagnostico);
        Assert.NotNull(resultado.FechaDiagnosticoUtc);
        Assert.Equal(48, resultado.TokenPublico?.Length);
        Assert.Null(resultado.FechaAutorizacionClienteUtc);
    }

    [Fact]
    public async Task GuardarAsync_ListaParaEntregaAutorizada_PermiteCorregirDiagnostico()
    {
        var empresa = new ContextoEmpresaPrueba(7);
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexto = new TallerDbContext(opciones, empresa);
        var orden = new OrdenServicio
        {
            EmpresaId = empresa.EmpresaId,
            Numero = "OS-LISTA-1",
            ClienteId = 1,
            VehiculoId = 1,
            Estado = EstadoOrdenServicio.ListaParaEntrega,
            FechaIngreso = DateTime.UtcNow,
            Diagnostico = "Diagnóstico inicial",
            FechaAutorizacionClienteUtc = DateTime.UtcNow,
            TokenPublico = new string('b', 48)
        };
        contexto.OrdenesServicio.Add(orden);
        await contexto.SaveChangesAsync();
        var servicio = new DiagnosticoOrdenServicioServicio(
            contexto, empresa, new AlmacenamientoEvidenciasPrueba(), new IdentidadNovaPrueba());

        var resultado = await servicio.GuardarAsync(orden.Id, new GuardarDiagnosticoOrdenServicioSolicitud
        {
            Diagnostico = "Diagnóstico corregido antes de la entrega"
        });

        Assert.Equal("Diagnóstico corregido antes de la entrega", resultado.Diagnostico);
        Assert.NotNull(resultado.FechaAutorizacionClienteUtc);
    }

    [Fact]
    public async Task ObtenerPublicaAsync_IncluyeLaInspeccionCompleta()
    {
        var empresa = new ContextoEmpresaPrueba(7);
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexto = new TallerDbContext(opciones, empresa);
        var cliente = new Cliente { EmpresaId = 7, Nombre = "Cliente", Telefono = "88880000" };
        var marca = new MarcaVehiculo { EmpresaId = 7, Nombre = "Toyota" };
        var modelo = new ModeloVehiculo { EmpresaId = 7, Nombre = "Hilux", Marca = marca };
        var vehiculo = new Vehiculo { EmpresaId = 7, Cliente = cliente, Modelo = modelo, Placa = "M123456", Anio = 2024 };
        var orden = new OrdenServicio
        {
            EmpresaId = 7,
            Numero = "OS-PUBLICA-1",
            Cliente = cliente,
            Vehiculo = vehiculo,
            Estado = EstadoOrdenServicio.PendienteAprobacion,
            FechaIngreso = DateTime.UtcNow,
            Diagnostico = "Diagnóstico público",
            TokenPublico = new string('a', 48)
        };
        var recepcion = new RecepcionVehiculo
        {
            EmpresaId = 7,
            OrdenServicio = orden,
            Kilometraje = 14500,
            PorcentajeCombustible = 100,
            DescripcionEstado = "Estado documentado",
            DejaLlaves = true,
            DejaDocumentos = true
        };
        recepcion.Danios.Add(new DanioVehiculo
        {
            EmpresaId = 7,
            Zona = ZonaVehiculo.Frente,
            Tipo = TipoDanioVehiculo.Golpe,
            Severidad = SeveridadDanioVehiculo.Moderado,
            Observacion = "Golpe en defensa"
        });
        recepcion.Evidencias.Add(new EvidenciaInspeccion
        {
            EmpresaId = 7,
            ClaveObjeto = "inspeccion/1.jpg",
            NombreArchivo = "frente.jpg",
            TipoContenido = "image/jpeg",
            Longitud = 100,
            FechaCargaUtc = DateTime.UtcNow
        });
        contexto.RecepcionesVehiculo.Add(recepcion);
        await contexto.SaveChangesAsync();
        var servicio = new DiagnosticoOrdenServicioServicio(contexto, empresa, new AlmacenamientoEvidenciasPrueba(), new IdentidadNovaPrueba());

        var resultado = await servicio.ObtenerPublicaAsync(orden.TokenPublico);

        Assert.Equal((byte)100, resultado.PorcentajeCombustible);
        Assert.True(resultado.DejaLlaves);
        Assert.True(resultado.DejaDocumentos);
        Assert.Single(resultado.DaniosInspeccion);
        Assert.Equal("Golpe en defensa", resultado.DaniosInspeccion.Single().Observacion);
        Assert.Single(resultado.EvidenciasInspeccion);
        Assert.Equal("frente.jpg", resultado.EvidenciasInspeccion.Single().NombreArchivo);
        Assert.Equal("Taller de Prueba", resultado.Taller.Nombre);
        Assert.Equal("Carretera Norte, Managua", resultado.Taller.Direccion);
        Assert.Equal("+505 88887777", resultado.Taller.Telefono);
        Assert.Equal("https://example.test/logo.png", resultado.Taller.Logo);
    }

    private sealed class IdentidadNovaPrueba : IIdentidadSmartNova
    {
        private static readonly UsuarioSmartNovaDto Usuario = new("usuario", "usuario", "Usuario", false, 7);
        private static readonly TallerSmartNovaDto Taller = new(
            7, "Taller Legal", "Taller de Prueba", 505, "88887777", null, null,
            "Carretera Norte", "Managua", null, null, "https://example.test/logo.png",
            TimeSpan.FromHours(8), TimeSpan.FromHours(17));

        public Task<UsuarioSmartNovaDto?> AutenticarAsync(string usuario, string contrasena, CancellationToken cancellationToken) => Task.FromResult<UsuarioSmartNovaDto?>(Usuario);
        public Task<UsuarioSmartNovaDto?> AutenticarProveedorAsync(string proveedor, string claveProveedor, string? correo, CancellationToken cancellationToken) => Task.FromResult<UsuarioSmartNovaDto?>(Usuario);
        public Task<UsuarioSmartNovaDto?> ObtenerUsuarioActivoAsync(string usuarioId, CancellationToken cancellationToken) => Task.FromResult<UsuarioSmartNovaDto?>(Usuario);
        public Task<IReadOnlyList<TallerSmartNovaDto>> ObtenerTalleresAsync(IReadOnlyCollection<int> empresaIds, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TallerSmartNovaDto>>(empresaIds.Contains(Taller.Id) ? [Taller] : []);
    }

    private sealed class AlmacenamientoEvidenciasPrueba : IAlmacenamientoEvidencias
    {
        public Task<string> GuardarAsync(long empresaId, long recepcionVehiculoId, string nombreArchivo, string tipoContenido, Stream contenido, CancellationToken cancellationToken = default) => Task.FromResult("evidencia");
        public Task<Uri> CrearDireccionLecturaAsync(string claveObjeto, string nombreArchivo, CancellationToken cancellationToken = default) => Task.FromResult(new Uri("https://example.test/evidencia"));
        public Task EliminarAsync(string claveObjeto, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
