namespace Talleres.Dominio.Entidades;

public sealed class TallerSincronizado
{
    public int EmpresaNovaId { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaConfiguracionUtc { get; set; }
}
