namespace Talleres.Aplicacion.DTOs.Configuracion;

public sealed record ConfiguracionMonetariaDto(
    string Simbolo,
    string SeparadorDecimal,
    int CantidadDecimales,
    string? SeparadorMiles);
