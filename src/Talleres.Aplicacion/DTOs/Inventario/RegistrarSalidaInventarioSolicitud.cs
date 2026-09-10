namespace Talleres.Aplicacion.DTOs.Inventario;

public sealed record RegistrarSalidaInventarioSolicitud(
    int EmpresaNovaId,
    int BodegaId,
    int ProductoId,
    decimal Cantidad,
    string UnidadMedida,
    string UsuarioId,
    string NumeroOrden);
