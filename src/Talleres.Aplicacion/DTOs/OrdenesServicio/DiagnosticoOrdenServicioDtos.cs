using System.ComponentModel.DataAnnotations;
using Talleres.Aplicacion.DTOs.Configuracion;
using Talleres.Aplicacion.DTOs.Recepciones;
using Talleres.Dominio.Enumeraciones;

namespace Talleres.Aplicacion.DTOs.OrdenesServicio;

public sealed class GuardarDiagnosticoOrdenServicioSolicitud
{
    [Required, StringLength(4000, MinimumLength = 5)]
    public required string Diagnostico { get; init; }
}

public sealed record EvidenciaDiagnosticoDto(
    long Id,
    string NombreArchivo,
    string TipoContenido,
    long Longitud,
    DateTime FechaCargaUtc);

public sealed record DiagnosticoOrdenServicioDto(
    string? Diagnostico,
    DateTime? FechaDiagnosticoUtc,
    string? TokenPublico,
    DateTime? FechaAutorizacionClienteUtc,
    IReadOnlyCollection<EvidenciaDiagnosticoDto> Evidencias);

public sealed record RegistrarEvidenciaDiagnosticoSolicitud(
    string NombreArchivo,
    string TipoContenido,
    long Longitud,
    Stream Contenido);

public sealed record TallerPublicoDto(
    string Nombre,
    string? Direccion,
    string? Telefono,
    string? Logo);

public sealed record OrdenServicioPublicaDto(
    TallerPublicoDto Taller,
    string Numero,
    EstadoOrdenServicio Estado,
    DateTime FechaIngreso,
    string NombreCliente,
    string Placa,
    string Marca,
    string Modelo,
    int Anio,
    string? Color,
    string? NumeroVin,
    string? MotivoIngreso,
    string Diagnostico,
    DateTime? FechaAutorizacionClienteUtc,
    int? Kilometraje,
    byte? PorcentajeCombustible,
    string? DescripcionEstado,
    bool DejaLlaves,
    bool DejaDocumentos,
    IReadOnlyCollection<DanioVehiculoDto> DaniosInspeccion,
    IReadOnlyCollection<EvidenciaInspeccionDto> EvidenciasInspeccion,
    IReadOnlyCollection<EvidenciaDiagnosticoDto> EvidenciasDiagnostico,
    IReadOnlyCollection<DetalleOrdenServicioDto> Detalles,
    decimal Total)
{
    public required ConfiguracionMonetariaDto ConfiguracionMonetaria { get; init; }

    public IReadOnlyCollection<ResultadoRevisionOperativaDto> RevisionOperativa { get; init; } = [];
    public string? ObservacionRevisionOperativa { get; init; }
    public bool DejaLlantaRepuesto { get; init; }
    public bool DejaTrianguloSeguridad { get; init; }
    public bool DejaGata { get; init; }
    public bool DejaLlaveRuedas { get; init; }
    public bool DejaGanchoRemolque { get; init; }
    public bool DejaExtintor { get; init; }
    public bool DejaBotiquin { get; init; }
    public bool DejaChalecoReflectante { get; init; }
    public bool DejaPasacorrientes { get; init; }
    public bool DejaCopaRueda { get; init; }
    public bool DejaAlfombras { get; init; }
    public string? OtrosElementos { get; init; }
    public bool AceptaPruebaRuta { get; init; }
}
