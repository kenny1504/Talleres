using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Dominio.Excepciones;

namespace Talleres.Infraestructura.Integraciones.AmazonS3;

public sealed class AlmacenamientoEvidenciasS3(
    Func<IAmazonS3> crearCliente,
    string? nombreBucket) : IAlmacenamientoEvidencias
{
    private readonly Lazy<IAmazonS3> cliente = new(crearCliente);

    public async Task<string> GuardarAsync(
        long empresaId,
        long recepcionVehiculoId,
        string nombreArchivo,
        string tipoContenido,
        Stream contenido,
        CancellationToken cancellationToken = default)
    {
        var bucket = ObtenerBucket();
        var extension = tipoContenido.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new IntegracionNoDisponibleException(
                "El formato de la evidencia no está permitido para Amazon S3.")
        };
        var clave = $"empresas/{empresaId}/recepciones/{recepcionVehiculoId}/{Guid.NewGuid():N}{extension}";

        try
        {
            await cliente.Value.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = bucket,
                    Key = clave,
                    InputStream = contenido,
                    ContentType = tipoContenido,
                    AutoCloseStream = false
                },
                cancellationToken);
            return clave;
        }
        catch (AmazonS3Exception excepcion)
        {
            throw new IntegracionNoDisponibleException(
                "No fue posible guardar las fotografías de la inspección en Amazon S3.",
                excepcion);
        }
        catch (AmazonClientException excepcion)
        {
            throw new IntegracionNoDisponibleException(
                "No fue posible autenticar la carga de fotografías en Amazon S3.",
                excepcion);
        }
    }

    public async Task<Uri> CrearDireccionLecturaAsync(
        string claveObjeto,
        string nombreArchivo,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var direccion = await cliente.Value.GetPreSignedURLAsync(
                new GetPreSignedUrlRequest
                {
                    BucketName = ObtenerBucket(),
                    Key = claveObjeto,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.AddMinutes(10)
                });
            return new Uri(direccion);
        }
        catch (AmazonS3Exception excepcion)
        {
            throw new IntegracionNoDisponibleException(
                $"No fue posible abrir la fotografía '{nombreArchivo}' desde Amazon S3.",
                excepcion);
        }
        catch (AmazonClientException excepcion)
        {
            throw new IntegracionNoDisponibleException(
                $"No fue posible autenticar la lectura de la fotografía '{nombreArchivo}' en Amazon S3.",
                excepcion);
        }
    }

    public async Task EliminarAsync(
        string claveObjeto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await cliente.Value.DeleteObjectAsync(
                ObtenerBucket(),
                claveObjeto,
                cancellationToken);
        }
        catch (AmazonS3Exception excepcion)
        {
            throw new IntegracionNoDisponibleException(
                "No fue posible limpiar una fotografía incompleta en Amazon S3.",
                excepcion);
        }
        catch (AmazonClientException excepcion)
        {
            throw new IntegracionNoDisponibleException(
                "No fue posible autenticar la limpieza de fotografías en Amazon S3.",
                excepcion);
        }
    }

    private string ObtenerBucket() =>
        !string.IsNullOrWhiteSpace(nombreBucket)
            ? nombreBucket
            : throw new IntegracionNoDisponibleException(
                "Amazon S3 no está configurado. Defina AWS_S3_BUCKET, AWS_REGION y las credenciales del entorno.");
}
