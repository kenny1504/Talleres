using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.DTOs.Autenticacion;
using Talleres.Aplicacion.DTOs.Configuracion;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.Servicios;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Enumeraciones;
using Talleres.Dominio.Excepciones;
using Talleres.Infraestructura.Persistencia;
using Talleres.Pruebas.Soporte;

namespace Talleres.Pruebas.Servicios;

public sealed class DiagnosticoOrdenServicioPruebas
{
    [Fact]
    public async Task GuardarAsync_CreaTokenPublicoYPersisteDiagnostico()
    {
        var empresa = new ContextoEmpresaPrueba(7);
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexto = new TallerDbContext(opciones, empresa);
        var orden = new OrdenServicio
        {
            EmpresaId = empresa.EmpresaId,
            Numero = "OS-DIAGNOSTICO-1",
            ClienteId = 1,
            VehiculoId = 1,
            Estado = EstadoOrdenServicio.Diagnostico,
            FechaIngreso = DateTime.UtcNow
        };
        contexto.OrdenesServicio.Add(orden);
        await contexto.SaveChangesAsync();
        var servicio = new DiagnosticoOrdenServicioServicio(contexto, empresa, new AlmacenamientoEvidenciasPrueba(), new IdentidadNovaPrueba(), new ConfiguracionMonetariaNovaPrueba());

        var resultado = await servicio.GuardarAsync(orden.Id, new GuardarDiagnosticoOrdenServicioSolicitud
        {
            Diagnostico = "Se detectó desgaste en las pastillas de freno delanteras."
        });

        Assert.Equal("Se detectó desgaste en las pastillas de freno delanteras.", resultado.Diagnostico);
        Assert.NotNull(resultado.FechaDiagnosticoUtc);
        Assert.Equal(48, resultado.TokenPublico?.Length);
        Assert.Null(resultado.FechaAutorizacionClienteUtc);
    }

    [Fact]
    public async Task GuardarAsync_ListaParaEntregaAutorizada_PermiteCorregirDiagnostico()
    {
        var empresa = new ContextoEmpresaPrueba(7);
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexto = new TallerDbContext(opciones, empresa);
        var orden = new OrdenServicio
        {
            EmpresaId = empresa.EmpresaId,
            Numero = "OS-LISTA-1",
            ClienteId = 1,
            VehiculoId = 1,
            Estado = EstadoOrdenServicio.ListaParaEntrega,
            FechaIngreso = DateTime.UtcNow,
            Diagnostico = "Diagnóstico inicial",
            FechaAutorizacionClienteUtc = DateTime.UtcNow,
            TokenPublico = new string('b', 48)
        };
        contexto.OrdenesServicio.Add(orden);
        await contexto.SaveChangesAsync();
        var servicio = new DiagnosticoOrdenServicioServicio(
            contexto, empresa, new AlmacenamientoEvidenciasPrueba(), new IdentidadNovaPrueba(), new ConfiguracionMonetariaNovaPrueba());

        var resultado = await servicio.GuardarAsync(orden.Id, new GuardarDiagnosticoOrdenServicioSolicitud
        {
            Diagnostico = "Diagnóstico corregido antes de la entrega"
        });

        Assert.Equal("Diagnóstico corregido antes de la entrega", resultado.Diagnostico);
        Assert.NotNull(resultado.FechaAutorizacionClienteUtc);
    }

    [Theory]
    [InlineData("C$", 0, ",", ".")]
    [InlineData("$", 3, ".", ",")]
    public async Task ObtenerPublicaAsync_IncluyeLaInspeccionCompleta(string simbolo, int decimales, string separadorDecimal, string separadorMiles)
    {
        var empresa = new ContextoEmpresaPrueba(7);
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexto = new TallerDbContext(opciones, empresa);
        var cliente = new Cliente { EmpresaId = 7, Nombre = "Cliente", Telefono = "88880000" };
        var marca = new MarcaVehiculo { EmpresaId = 7, Nombre = "Toyota" };
        var modelo = new ModeloVehiculo { EmpresaId = 7, Nombre = "Hilux", Marca = marca };
        var vehiculo = new Vehiculo { EmpresaId = 7, Cliente = cliente, Modelo = modelo, Placa = "M123456", Anio = 2024 };
        var orden = new OrdenServicio
        {
            EmpresaId = 7,
            Numero = "OS-PUBLICA-1",
            Cliente = cliente,
            Vehiculo = vehiculo,
            Estado = EstadoOrdenServicio.PendienteAprobacion,
            FechaIngreso = DateTime.UtcNow,
            Diagnostico = "Diagnóstico público",
            TokenPublico = new string('a', 48)
        };
        var recepcion = new RecepcionVehiculo
        {
            EmpresaId = 7,
            OrdenServicio = orden,
            Kilometraje = 14500,
            PorcentajeCombustible = 100,
            DescripcionEstado = "Estado documentado",
            DejaLlaves = true,
            DejaDocumentos = true,
            DejaLlantaRepuesto = true,
            DejaTrianguloSeguridad = true,
            DejaGata = true,
            DejaLlaveRuedas = true,
            DejaGanchoRemolque = true,
            DejaExtintor = true,
            DejaBotiquin = true,
            DejaChalecoReflectante = true,
            DejaPasacorrientes = true,
            DejaCopaRueda = true,
            DejaAlfombras = true,
            OtrosElementos = "Cables de arranque",
            AceptaPruebaRuta = true,
            ObservacionRevisionOperativa = "La bocina no funciona.\nLuces delanteras verificadas."
        };
        recepcion.RevisionOperativa.Add(new ResultadoRevisionOperativa
        {
            EmpresaId = 7,
            Actividad = ActividadRevisionOperativa.LucesDelanteras,
            Correcto = true
        });
        recepcion.RevisionOperativa.Add(new ResultadoRevisionOperativa
        {
            EmpresaId = 7,
            Actividad = ActividadRevisionOperativa.Bocina,
            Correcto = false
        });
        recepcion.Danios.Add(new DanioVehiculo
        {
            EmpresaId = 7,
            Zona = ZonaVehiculo.Frente,
            Tipo = TipoDanioVehiculo.Golpe,
            Severidad = SeveridadDanioVehiculo.Moderado,
            Observacion = "Golpe en defensa"
        });
        foreach (var actividad in new[]
        {
            ActividadRevisionOperativa.AireAcondicionado,
            ActividadRevisionOperativa.Radio,
            ActividadRevisionOperativa.Retrovisores
        })
        {
            recepcion.RevisionOperativa.Add(new ResultadoRevisionOperativa
            {
                EmpresaId = 7,
                Actividad = actividad,
                Correcto = actividad != ActividadRevisionOperativa.Radio
            });
        }
        recepcion.Evidencias.Add(new EvidenciaInspeccion
        {
            EmpresaId = 7,
            ClaveObjeto = "inspeccion/1.jpg",
            NombreArchivo = "frente.jpg",
            TipoContenido = "image/jpeg",
            Longitud = 100,
            FechaCargaUtc = DateTime.UtcNow
        });
        contexto.RecepcionesVehiculo.Add(recepcion);
        orden.Detalles.Add(new Talleres.Dominio.Entidades.DetalleOrdenServicio
        {
            EmpresaId = 7,
            Tipo = TipoDetalleOrdenServicio.Inventario,
            Descripcion = "Aceite de motor",
            UnidadMedida = "1",
            Cantidad = 2,
            PrecioUnitario = 1200
        });
        await contexto.SaveChangesAsync();
        var empresaPublica = new ContextoEmpresaPrueba(0);
        await using var contextoPublico = new TallerDbContext(opciones, empresaPublica);
        var configuracion = new ConfiguracionMonetariaDto(simbolo, separadorDecimal, decimales, separadorMiles);
        var servicio = new DiagnosticoOrdenServicioServicio(contextoPublico, empresaPublica, new AlmacenamientoEvidenciasPrueba(), new IdentidadNovaPrueba(), new ConfiguracionMonetariaNovaPrueba(configuracion));

        var resultado = await servicio.ObtenerPublicaAsync(orden.TokenPublico);

        Assert.Equal(configuracion, resultado.ConfiguracionMonetaria);
        Assert.Equal(2400m, resultado.Total);
        var cargo = Assert.Single(resultado.Detalles);
        Assert.Equal(2m, cargo.Cantidad);
        Assert.Equal(1200m, cargo.PrecioUnitario);
        Assert.Equal(2400m, cargo.Subtotal);
        Assert.Equal((byte)100, resultado.PorcentajeCombustible);
        Assert.True(resultado.DejaLlaves);
        Assert.True(resultado.DejaDocumentos);
        Assert.True(resultado.DejaLlantaRepuesto);
        Assert.True(resultado.DejaTrianguloSeguridad);
        Assert.True(resultado.DejaGata);
        Assert.True(resultado.DejaLlaveRuedas);
        Assert.True(resultado.DejaGanchoRemolque);
        Assert.True(resultado.DejaExtintor);
        Assert.True(resultado.DejaBotiquin);
        Assert.True(resultado.DejaChalecoReflectante);
        Assert.True(resultado.DejaPasacorrientes);
        Assert.True(resultado.DejaCopaRueda);
        Assert.True(resultado.DejaAlfombras);
        Assert.True(resultado.AceptaPruebaRuta);
        Assert.Equal("Cables de arranque", resultado.OtrosElementos);
        Assert.Equal("La bocina no funciona.\nLuces delanteras verificadas.", resultado.ObservacionRevisionOperativa);
        Assert.Collection(resultado.RevisionOperativa,
            revision =>
            {
                Assert.Equal(ActividadRevisionOperativa.LucesDelanteras, revision.Actividad);
                Assert.True(revision.Correcto);
            },
            revision =>
            {
                Assert.Equal(ActividadRevisionOperativa.Bocina, revision.Actividad);
                Assert.False(revision.Correcto);
            },
            revision =>
            {
                Assert.Equal(ActividadRevisionOperativa.AireAcondicionado, revision.Actividad);
                Assert.True(revision.Correcto);
            },
            revision =>
            {
                Assert.Equal(ActividadRevisionOperativa.Radio, revision.Actividad);
                Assert.False(revision.Correcto);
            },
            revision =>
            {
                Assert.Equal(ActividadRevisionOperativa.Retrovisores, revision.Actividad);
                Assert.True(revision.Correcto);
            });
        Assert.Single(resultado.DaniosInspeccion);
        Assert.Equal("Golpe en defensa", resultado.DaniosInspeccion.Single().Observacion);
        Assert.Single(resultado.EvidenciasInspeccion);
        Assert.Equal("frente.jpg", resultado.EvidenciasInspeccion.Single().NombreArchivo);
        Assert.Equal("Taller de Prueba", resultado.Taller.Nombre);
        Assert.Equal("Carretera Norte, Managua", resultado.Taller.Direccion);
        Assert.Equal("+505 88887777", resultado.Taller.Telefono);
        Assert.Equal("https://example.test/logo.png", resultado.Taller.Logo);
    }

    [Fact]
    public async Task ExpedientePublico_LimitaVehiculoEmpresaYRelacionadosSinSesion()
    {
        var opciones = new DbContextOptionsBuilder<TallerDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var compartidaId = await PrepararExpedienteAsync(opciones);
        await using var publico = new TallerDbContext(opciones, new ContextoEmpresaPrueba(0));
        var servicio = new ExpedienteVehiculoPublicoServicio(publico, new IdentidadNovaPrueba(), new ConfiguracionMonetariaNovaPrueba());

        var resultado = await servicio.ObtenerAsync("  " + new string('A', 48) + "  ", new());

        Assert.Equal("M123456", resultado.Vehiculo.Placa);
        Assert.Equal("Taller de Prueba", resultado.NombreTaller);
        Assert.Equal(12, resultado.TotalOrdenes);
        Assert.Equal(10, resultado.TamanoPagina);
        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(10, resultado.Ordenes.Count);
        var compartida = Assert.Single(resultado.Ordenes, orden => orden.EsOrdenCompartida);
        Assert.Equal("OS-01", compartida.Numero);
        Assert.Null(compartida.Kilometraje); // La recepción ajena insertada deliberadamente no se expone.
        Assert.Equal(250m, compartida.Total);
        Assert.Equal("Servicio propio", Assert.Single(compartida.Cargos).Descripcion);
        Assert.Contains(resultado.Ordenes, orden => orden.Estado == EstadoOrdenServicio.Cancelada);
        Assert.Contains(resultado.Ordenes, orden => orden.Estado == EstadoOrdenServicio.Cerrada);
        Assert.Contains(resultado.Ordenes, orden => orden.Diagnostico == null);
        Assert.DoesNotContain(resultado.Ordenes, orden => orden.Numero.Contains("AJENA") || orden.Numero.Contains("OTRO"));
        Assert.Empty(publico.ChangeTracker.Entries());
        var ordenPersistida = await publico.OrdenesServicio.IgnoreQueryFilters().AsNoTracking().SingleAsync(orden => orden.Id == compartidaId);
        Assert.Null(ordenPersistida.FechaAutorizacionClienteUtc);
        Assert.Equal(EstadoOrdenServicio.PendienteAprobacion, ordenPersistida.Estado);
    }

    [Fact]
    public async Task ExpedientePublico_PaginaTodoElHistorialSinDuplicarOrdenes()
    {
        var opciones = new DbContextOptionsBuilder<TallerDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await PrepararExpedienteAsync(opciones);
        await using var publico = new TallerDbContext(opciones, new ContextoEmpresaPrueba(0));
        var servicio = new ExpedienteVehiculoPublicoServicio(publico, new IdentidadNovaPrueba(), new ConfiguracionMonetariaNovaPrueba());
        var token = new string('a', 48);

        var primera = await servicio.ObtenerAsync(token, new());
        var segunda = await servicio.ObtenerAsync(token, new() { Pagina = 2 });
        var ultima = await servicio.ObtenerAsync(token, new() { Pagina = int.MaxValue });

        Assert.Equal(2, segunda.Pagina);
        Assert.Equal(2, segunda.Ordenes.Count);
        Assert.Equal(12, primera.Ordenes.Concat(segunda.Ordenes).Select(orden => orden.Numero).Distinct().Count());
        Assert.Equal(segunda.Ordenes.Select(orden => orden.Numero), ultima.Ordenes.Select(orden => orden.Numero));
        Assert.Equal(primera.Ordenes.Select(orden => orden.Numero), (await servicio.ObtenerAsync(token, new())).Ordenes.Select(orden => orden.Numero));
    }

    [Fact]
    public async Task ExpedientePublico_RechazaTokensInvalidosInexistentesYRelacionesDeOtraEmpresa()
    {
        var opciones = new DbContextOptionsBuilder<TallerDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var compartidaId = await PrepararExpedienteAsync(opciones);
        await using var publico = new TallerDbContext(opciones, new ContextoEmpresaPrueba(0));
        var servicio = new ExpedienteVehiculoPublicoServicio(publico, new IdentidadNovaPrueba(), new ConfiguracionMonetariaNovaPrueba());
        foreach (var token in new[] { "", "invalido", new string('z', 48), new string('b', 48), new string('c', 48) })
            await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => servicio.ObtenerAsync(token, new()));

        await using var internoTaller = new TallerDbContext(opciones, new ContextoEmpresaPrueba(7));
        var compartida = await internoTaller.OrdenesServicio.SingleAsync(orden => orden.Id == compartidaId);
        compartida.Diagnostico = null;
        await internoTaller.SaveChangesAsync();
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => servicio.ObtenerAsync(new string('a', 48), new()));
    }

    private static async Task<long> PrepararExpedienteAsync(DbContextOptions<TallerDbContext> opciones)
    {
        long compartidaId;
        long vehiculoId;
        await using (var contexto = new TallerDbContext(opciones, new ContextoEmpresaPrueba(7)))
        {
            var cliente = new Cliente { EmpresaId = 7, Nombre = "Cliente", Telefono = "88880000" };
            var marca = new MarcaVehiculo { EmpresaId = 7, Nombre = "Toyota" };
            var modelo = new ModeloVehiculo { EmpresaId = 7, Nombre = "Hilux", Marca = marca };
            var vehiculo = new Vehiculo { EmpresaId = 7, Cliente = cliente, Modelo = modelo, Placa = "M123456", Anio = 2024 };
            OrdenServicio? compartida = null;
            for (var indice = 0; indice < 12; indice++)
            {
                var orden = new OrdenServicio
                {
                    EmpresaId = 7, Cliente = cliente, Vehiculo = vehiculo, Numero = $"OS-{indice + 1:00}",
                    Estado = indice == 0 ? EstadoOrdenServicio.PendienteAprobacion : indice == 1 ? EstadoOrdenServicio.Cancelada : EstadoOrdenServicio.Cerrada,
                    FechaIngreso = new DateTime(2026, 10, 21, 12, 0, 0, DateTimeKind.Utc).AddDays(-indice),
                    Diagnostico = indice == 0 ? "Diagnóstico compartido" : null,
                    TokenPublico = indice == 0 ? new string('a', 48) : null
                };
                contexto.OrdenesServicio.Add(orden);
                if (indice == 0) compartida = orden;
            }
            compartida!.Detalles.Add(new Talleres.Dominio.Entidades.DetalleOrdenServicio
            {
                EmpresaId = 7, Descripcion = "Servicio propio", Cantidad = 2, PrecioUnitario = 125, Tipo = TipoDetalleOrdenServicio.Manual
            });
            var otroVehiculo = new Vehiculo { EmpresaId = 7, Cliente = cliente, Modelo = modelo, Placa = "OTRO-VEHICULO", Anio = 2024 };
            contexto.OrdenesServicio.Add(new OrdenServicio
            {
                EmpresaId = 7, Cliente = cliente, Vehiculo = otroVehiculo, Numero = "OS-OTRO-VEHICULO", FechaIngreso = DateTime.UtcNow
            });
            await contexto.SaveChangesAsync();
            compartidaId = compartida.Id;
            vehiculoId = vehiculo.Id;
        }
        await using (var contextoAjeno = new TallerDbContext(opciones, new ContextoEmpresaPrueba(8)))
        {
            var cliente = new Cliente { EmpresaId = 8, Nombre = "Cliente ajeno", Telefono = "88889999" };
            var marca = new MarcaVehiculo { EmpresaId = 8, Nombre = "Otra marca" };
            var modelo = new ModeloVehiculo { EmpresaId = 8, Nombre = "Otro modelo", Marca = marca };
            var vehiculo = new Vehiculo { EmpresaId = 8, Cliente = cliente, Modelo = modelo, Placa = "M123456", Anio = 2024 };
            contextoAjeno.OrdenesServicio.Add(new OrdenServicio
            {
                EmpresaId = 8, Cliente = cliente, Vehiculo = vehiculo, Numero = "OS-AJENA-MISMA-PLACA", FechaIngreso = DateTime.UtcNow
            });
            contextoAjeno.OrdenesServicio.Add(new OrdenServicio
            {
                EmpresaId = 8, Cliente = cliente, VehiculoId = vehiculoId, Numero = "OS-AJENA-RELACION", FechaIngreso = DateTime.UtcNow,
                Diagnostico = "Ajeno", TokenPublico = new string('c', 48)
            });
            contextoAjeno.DetallesOrdenesServicio.Add(new Talleres.Dominio.Entidades.DetalleOrdenServicio
            {
                EmpresaId = 8, OrdenServicioId = compartidaId, Descripcion = "Cargo ajeno", Cantidad = 1, PrecioUnitario = 999, Tipo = TipoDetalleOrdenServicio.Manual
            });
            contextoAjeno.RecepcionesVehiculo.Add(new RecepcionVehiculo
            {
                EmpresaId = 8, OrdenServicioId = compartidaId, Kilometraje = 999999, DescripcionEstado = "Recepción ajena"
            });
            await contextoAjeno.SaveChangesAsync();
        }
        return compartidaId;
    }
    private sealed class ConfiguracionMonetariaNovaPrueba(ConfiguracionMonetariaDto? configuracion = null) : IConfiguracionMonetariaSmartNova
    {
        public Task<ConfiguracionMonetariaDto> ObtenerAsync(int empresaNovaId, CancellationToken cancellationToken)
        {
            Assert.Equal(7, empresaNovaId);
            return Task.FromResult(configuracion ?? new ConfiguracionMonetariaDto("C$", ".", 2, ","));
        }
    }

    private sealed class IdentidadNovaPrueba : IIdentidadSmartNova
    {
        private static readonly UsuarioSmartNovaDto Usuario = new("usuario", "usuario", "Usuario", false, 7);
        private static readonly TallerSmartNovaDto Taller = new(
            7, "Taller Legal", "Taller de Prueba", 505, "88887777", null, null,
            "Carretera Norte", "Managua", null, null, "https://example.test/logo.png",
            TimeSpan.FromHours(8), TimeSpan.FromHours(17));

        public Task<UsuarioSmartNovaDto?> AutenticarAsync(string usuario, string contrasena, CancellationToken cancellationToken) => Task.FromResult<UsuarioSmartNovaDto?>(Usuario);
        public Task<UsuarioSmartNovaDto?> AutenticarProveedorAsync(string proveedor, string claveProveedor, string? correo, CancellationToken cancellationToken) => Task.FromResult<UsuarioSmartNovaDto?>(Usuario);
        public Task<UsuarioSmartNovaDto?> ObtenerUsuarioActivoAsync(string usuarioId, CancellationToken cancellationToken) => Task.FromResult<UsuarioSmartNovaDto?>(Usuario);
        public Task<IReadOnlyList<TallerSmartNovaDto>> ObtenerTalleresAsync(IReadOnlyCollection<int> empresaIds, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TallerSmartNovaDto>>(empresaIds.Contains(Taller.Id) ? [Taller] : []);
    }

    private sealed class AlmacenamientoEvidenciasPrueba : IAlmacenamientoEvidencias
    {
        public Task<string> GuardarAsync(long empresaId, long recepcionVehiculoId, string nombreArchivo, string tipoContenido, Stream contenido, CancellationToken cancellationToken = default) => Task.FromResult("evidencia");
        public Task<Uri> CrearDireccionLecturaAsync(string claveObjeto, string nombreArchivo, CancellationToken cancellationToken = default) => Task.FromResult(new Uri("https://example.test/evidencia"));
        public Task EliminarAsync(string claveObjeto, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
