using Amazon.S3;
using Talleres.Infraestructura.Integraciones.AmazonS3;

namespace Talleres.Pruebas.Integraciones;

public sealed class AlmacenamientoEvidenciasS3Pruebas
{
    [Fact]
    public async Task CrearDireccionLecturaAsync_ObjetoPublico_DevuelveUrlDirectaDelBucket()
    {
        var almacenamiento = new AlmacenamientoEvidenciasS3(
            () => throw new InvalidOperationException("No debe crear el cliente para una URL pública."),
            "biossoft-mereb-crm");

        var direccion = await almacenamiento.CrearDireccionLecturaAsync(
            "empresas/17/recepciones/81/foto frente.png",
            "foto frente.png",
            CancellationToken.None);

        Assert.Equal(
            "https://biossoft-mereb-crm.s3.amazonaws.com/empresas/17/recepciones/81/foto%20frente.png",
            direccion.AbsoluteUri);
    }
}
