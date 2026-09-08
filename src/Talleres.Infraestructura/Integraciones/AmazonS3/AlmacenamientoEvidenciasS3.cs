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
                    AutoCloseStream = false,
                    CannedACL = S3CannedACL.PublicRead,
                    Headers =
                    {
                        Expires = new DateTime(2040, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    }
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

    public Task<Uri> CrearDireccionLecturaAsync(
        string claveObjeto,
        string nombreArchivo,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = nombreArchivo;
        var claveEscapada = string.Join(
            "/",
            claveObjeto.Split('/').Select(Uri.EscapeDataString));
        var direccion = new Uri(
            $"https://{ObtenerBucket()}.s3.amazonaws.com/{claveEscapada}");
        return Task.FromResult(direccion);
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
                "Amazon S3 no está configurado. Defina el bucket y las credenciales del entorno.");
}
