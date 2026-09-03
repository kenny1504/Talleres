namespace Talleres.Dominio.Excepciones;

/// <summary>
/// Representa una integración externa ausente, mal configurada o temporalmente inaccesible.
/// </summary>
public sealed class IntegracionNoDisponibleException : Exception
{
    public IntegracionNoDisponibleException(string mensaje)
        : base(mensaje)
    {
    }

    public IntegracionNoDisponibleException(string mensaje, Exception excepcion)
        : base(mensaje, excepcion)
    {
    }
}
