namespace Talleres.Dominio.Entidades;

/// <summary>
/// Registra un abono a la cuenta de un cliente, sin asignarlo a una orden.
/// </summary>
public sealed class PagoCliente : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public long ClienteId { get; set; }

    public decimal Monto { get; set; }

    public DateTime FechaRegistroUtc { get; set; }

    public string? FormaPago { get; set; }

    public string? Referencia { get; set; }

    public DateTime? FechaAnulacionUtc { get; set; }

    public Cliente Cliente { get; set; } = null!;
}
