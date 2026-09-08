using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.DTOs.Recepciones;
using Talleres.Aplicacion.Servicios;
using Talleres.Dominio.Entidades;
using Talleres.Infraestructura.Persistencia;
using Talleres.Pruebas.Soporte;

namespace Talleres.Pruebas.Servicios;

public sealed class EvidenciaInspeccionServicioPruebas
{
    [Fact]
    public async Task RegistrarAsync_RecepcionDeEmpresa_RegistraMetadatosYClavePrivada()
    {
        var empresa = new ContextoEmpresaPrueba(17);
        await using var dbContext = CrearDbContext(empresa);
        var recepcion = new RecepcionVehiculo
        {
            EmpresaId = empresa.EmpresaId,
            OrdenServicioId = 81,
            Kilometraje = 10,
            PorcentajeCombustible = 50,
            DescripcionEstado = "Sin novedades",
            FechaRecepcion = DateTime.UtcNow
        };
        dbContext.RecepcionesVehiculo.Add(recepcion);
        await dbContext.SaveChangesAsync();
        var almacenamiento = new AlmacenamientoEvidenciasPrueba();
        var servicio = new EvidenciaInspeccionServicio(dbContext, empresa, almacenamiento);

        await using var contenido = new MemoryStream([0xFF, 0xD8, 0xFF, 0xD9]);
        var resultado = await servicio.RegistrarAsync(
            recepcion.OrdenServicioId,
            [new RegistrarEvidenciaInspeccionSolicitud(
                "frente.jpg",
                "image/jpeg",
                contenido.Length,
                contenido)],
            CancellationToken.None);

        var evidencia = Assert.Single(resultado);
        Assert.Equal("frente.jpg", evidencia.NombreArchivo);
        Assert.Equal(
            $"empresas/{empresa.EmpresaId}/recepciones/{recepcion.Id}/frente.jpg",
            almacenamiento.ClaveGuardada);
        Assert.Equal(
            almacenamiento.ClaveGuardada,
            (await dbContext.EvidenciasInspeccion.SingleAsync()).ClaveObjeto);
    }

    private static TallerDbContext CrearDbContext(ContextoEmpresaPrueba empresa)
    {
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TallerDbContext(opciones, empresa);
    }

    private sealed class AlmacenamientoEvidenciasPrueba : IAlmacenamientoEvidencias
    {
        public string? ClaveGuardada { get; private set; }

        public Task<string> GuardarAsync(
            long empresaId,
            long recepcionVehiculoId,
            string nombreArchivo,
            string tipoContenido,
            Stream contenido,
            CancellationToken cancellationToken = default)
        {
            ClaveGuardada = $"empresas/{empresaId}/recepciones/{recepcionVehiculoId}/{nombreArchivo}";
            return Task.FromResult(ClaveGuardada);
        }

        public Task<Uri> CrearDireccionLecturaAsync(
            string claveObjeto,
            string nombreArchivo,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new Uri("https://ejemplo.invalid/evidencia"));

        public Task EliminarAsync(
            string claveObjeto,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
