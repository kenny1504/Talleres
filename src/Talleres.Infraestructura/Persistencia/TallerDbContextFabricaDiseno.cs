using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Talleres.Aplicacion.Abstracciones.Multitenencia;

namespace Talleres.Infraestructura.Persistencia;

public sealed class TallerDbContextFabricaDiseno : IDesignTimeDbContextFactory<TallerDbContext>
{
    public TallerDbContext CreateDbContext(string[] args)
    {
        var cadenaConexion = Environment.GetEnvironmentVariable("ConnectionStrings__TallerDb")
            ?? Environment.GetEnvironmentVariable("TALLERES_CONNECTION_STRING");
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseSqlServer(ConfiguracionConexionSql.ValidarRemota(cadenaConexion))
            .Options;

        return new TallerDbContext(opciones, new ContextoEmpresaDiseno());
    }

    private sealed class ContextoEmpresaDiseno : IContextoEmpresa
    {
        public long EmpresaId => 1;

        public bool EstaDisponible => true;
    }
}
