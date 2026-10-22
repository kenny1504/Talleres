using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class ExpedienteVehiculoPublicoServicio(
    ITallerDbContext dbContext,
    IIdentidadSmartNova identidadSmartNova,
    IConfiguracionMonetariaSmartNova configuracionMonetaria) : IExpedienteVehiculoPublicoServicio
{
    private const int TamanoPagina = 10;

    public async Task<ExpedienteVehiculoPublicoDto> ObtenerAsync(
        string token, ConsultarExpedienteVehiculoPublicoSolicitud solicitud, CancellationToken cancellationToken = default)
    {
        token = token.Trim().ToLowerInvariant();
        if (token.Length != 48 || token.Any(caracter => !Uri.IsHexDigit(caracter)))
            throw new RecursoNoEncontradoException("El enlace público no es válido.");

        // El acceso público no tiene empresa de sesión: su alcance procede exclusivamente de la orden del token.
        var acceso = await dbContext.OrdenesServicio.IgnoreQueryFilters().AsNoTracking()
            .Where(orden => orden.TokenPublico == token && orden.Diagnostico != null
                && orden.Vehiculo.EmpresaId == orden.EmpresaId
                && orden.Cliente.EmpresaId == orden.EmpresaId
                && orden.Vehiculo.Modelo.EmpresaId == orden.EmpresaId
                && orden.Vehiculo.Modelo.Marca.EmpresaId == orden.EmpresaId)
            .Select(orden => new
            {
                orden.Id, orden.EmpresaId, orden.VehiculoId, orden.Vehiculo.Placa,
                Marca = orden.Vehiculo.Modelo.Marca.Nombre,
                Modelo = orden.Vehiculo.Modelo.Nombre, orden.Vehiculo.Anio, orden.Vehiculo.Color
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new RecursoNoEncontradoException("El enlace público no existe o dejó de estar disponible.");

        var consulta = dbContext.OrdenesServicio.IgnoreQueryFilters().AsNoTracking()
            .Where(orden => orden.EmpresaId == acceso.EmpresaId && orden.VehiculoId == acceso.VehiculoId
                && orden.Vehiculo.EmpresaId == acceso.EmpresaId && orden.Cliente.EmpresaId == acceso.EmpresaId);
        var totalOrdenes = await consulta.CountAsync(cancellationToken);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalOrdenes / (double)TamanoPagina));
        var pagina = Math.Clamp(solicitud.Pagina, 1, totalPaginas);
        var datos = await consulta.OrderByDescending(orden => orden.FechaIngreso).ThenByDescending(orden => orden.Id)
            .Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina)
            .Select(orden => new
            {
                orden.Numero, orden.Estado, orden.FechaIngreso, orden.FechaAutorizacionClienteUtc,
                Kilometraje = orden.Recepcion != null && orden.Recepcion.EmpresaId == acceso.EmpresaId
                    ? (int?)orden.Recepcion.Kilometraje : null,
                MotivoIngreso = orden.Observaciones,
                orden.Diagnostico,
                EsOrdenCompartida = orden.Id == acceso.Id,
                Cargos = orden.Detalles.Where(cargo => cargo.EmpresaId == acceso.EmpresaId)
                    .OrderBy(cargo => cargo.Id)
                    .Select(cargo => new CargoExpedientePublicoDto(
                        cargo.Descripcion, cargo.Cantidad, cargo.PrecioUnitario, cargo.Cantidad * cargo.PrecioUnitario))
                    .ToArray()
            }).ToListAsync(cancellationToken);

        var empresaNovaId = checked((int)acceso.EmpresaId);
        var taller = (await identidadSmartNova.ObtenerTalleresAsync([empresaNovaId], cancellationToken))
            .SingleOrDefault(taller => taller.Id == empresaNovaId)
            ?? throw new RecursoNoEncontradoException("No fue posible encontrar la información pública del taller.");
        var formatoMonetario = await configuracionMonetaria.ObtenerAsync(empresaNovaId, cancellationToken);
        var ordenes = datos.Select(orden => new OrdenExpedientePublicoDto(
            orden.Numero, orden.Estado, orden.FechaIngreso, orden.FechaAutorizacionClienteUtc,
            orden.Kilometraje, orden.MotivoIngreso, orden.Diagnostico, orden.EsOrdenCompartida,
            orden.Cargos, orden.Cargos.Sum(cargo => cargo.Subtotal))).ToArray();

        return new ExpedienteVehiculoPublicoDto(
            taller.NombreComercial ?? taller.NombreLegal,
            new VehiculoExpedientePublicoDto(acceso.Placa, acceso.Marca, acceso.Modelo, acceso.Anio, acceso.Color),
            ordenes, pagina, TamanoPagina, totalOrdenes, formatoMonetario);
    }
}
