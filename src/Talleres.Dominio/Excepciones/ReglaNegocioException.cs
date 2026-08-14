namespace Talleres.Dominio.Excepciones;

/// <summary>
/// Representa el incumplimiento de una regla del negocio.
/// </summary>
public sealed class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje)
        : base(mensaje)
    {
    }
}
