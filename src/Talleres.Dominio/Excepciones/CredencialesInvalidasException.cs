namespace Talleres.Dominio.Excepciones;

public sealed class CredencialesInvalidasException() :
    Exception("El usuario o la contraseña no son correctos.");
