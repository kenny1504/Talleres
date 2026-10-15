"use client";

import { useEffect, useState, type FormEvent, type ReactNode } from "react";
import { ArrowRight, ChevronLeft, ChevronRight, RefreshCw, TriangleAlert } from "lucide-react";
import { useCargadorPantalla } from "./ProveedorCargadorPantalla";

type AgrupacionCobros = "Dia" | "Semana" | "Mes";
type SituacionCuenta = "Pendiente" | "Liquidada" | "SaldoAFavor";
type PeriodoRapido = "hoy" | "semana" | "mes" | "personalizado";

export interface FiltroEstadoNegocio {
  desde: string;
  hasta: string;
  agrupacion: AgrupacionCobros;
}

interface CuentaNegocio {
  clienteId: number;
  nombre: string;
  importeTrabajos: number;
  abonos: number;
  saldo: number;
  situacion: SituacionCuenta;
}

interface ClientePrincipal {
  clienteId: number;
  nombre: string;
  ordenes: number;
  importe: number;
}

interface VehiculoFrecuente {
  vehiculoId: number;
  placa: string;
  marca: string;
  modelo: string;
  anio: number;
  visitas: number;
}

interface CobroNegocio {
  desde: string;
  hasta: string;
  monto: number;
  abonos: number;
}

interface ResumenNegocio {
  desde: string;
  hasta: string;
  fechaConsultaUtc: string;
  cobrosPeriodo: number;
  abonosPeriodo: number;
  importeTrabajosPeriodo: number;
  ordenesPeriodo: number;
  saldoPendienteActual: number;
  saldoFavorActual: number;
  clientesPendientes: number;
  clientesLiquidados: number;
  clientesConSaldoFavor: number;
  evolucionCobros: CobroNegocio[];
  cuentas: CuentaNegocio[];
  clientesPrincipales: ClientePrincipal[];
  vehiculosFrecuentes: VehiculoFrecuente[];
  modelosFrecuentes: Array<{ marca: string; modelo: string; visitas: number }>;
}

export function crearFiltroEstadoNegocio(): FiltroEstadoNegocio {
  return { ...obtenerPeriodo("mes"), agrupacion: "Dia" };
}

export function EstadoNegocio({ direccionApi, filtro, alConsultar, alVerCuenta, alVerVehiculo }: {
  direccionApi: string;
  filtro: FiltroEstadoNegocio;
  alConsultar: (filtro: FiltroEstadoNegocio) => void;
  alVerCuenta: (cliente: { clienteId: number; nombre: string }) => void;
  alVerVehiculo: (vehiculo: VehiculoFrecuente) => void;
}) {
  const { iniciarCargaPantalla, finalizarCargaPantalla } = useCargadorPantalla();
  const [desde, setDesde] = useState(filtro.desde);
  const [hasta, setHasta] = useState(filtro.hasta);
  const [agrupacion, setAgrupacion] = useState(filtro.agrupacion);
  const [periodo, setPeriodo] = useState<PeriodoRapido>(() => {
    for (const opcion of ["mes", "semana", "hoy"] as const) {
      const fechas = obtenerPeriodo(opcion);
      if (fechas.desde === filtro.desde && fechas.hasta === filtro.hasta) return opcion;
    }
    return "personalizado";
  });
  const [recarga, setRecarga] = useState(0);
  const [resultado, setResultado] = useState<{ clave: string; datos: ResumenNegocio } | null>(null);
  const [fallo, setFallo] = useState<{ clave: string; mensaje: string } | null>(null);
  const [validacion, setValidacion] = useState("");
  const [situacion, setSituacion] = useState<SituacionCuenta | "Todas">("Pendiente");
  const [busqueda, setBusqueda] = useState("");
  const [pagina, setPagina] = useState(0);
  const parametros = new URLSearchParams({
    desde: filtro.desde,
    hasta: filtro.hasta,
    agrupacion: filtro.agrupacion,
    desfaseHorarioMinutos: String(new Date().getTimezoneOffset()),
  }).toString();
  const direccion = `${direccionApi}/api/estado-negocio?${parametros}`;
  const clave = `${direccion}:${recarga}`;
  const datos = resultado?.clave === clave ? resultado.datos : null;
  const error = fallo?.clave === clave ? fallo.mensaje : "";
  const cargando = !datos && !error;

  useEffect(() => {
    const controlador = new AbortController();
    const cargaId = iniciarCargaPantalla("Consultando el estado del negocio…");
    fetch(direccion, { credentials: "include", cache: "no-store", signal: controlador.signal })
      .then(async (respuesta) => {
        if (!respuesta.ok) {
          const problema = await respuesta.json().catch(() => null) as { detail?: string; title?: string } | null;
          throw new Error(respuesta.status === 401
            ? "La sesión terminó. Vuelve a iniciar sesión para consultar el negocio."
            : problema?.detail || problema?.title || "No fue posible consultar el estado del negocio.");
        }
        return await respuesta.json() as ResumenNegocio;
      })
      .then((resumen) => {
        if (!controlador.signal.aborted) setResultado({ clave, datos: resumen });
      })
      .catch((motivo: unknown) => {
        if (!controlador.signal.aborted) setFallo({ clave, mensaje: motivo instanceof Error ? motivo.message : "No fue posible consultar el estado del negocio." });
      })
      .finally(() => finalizarCargaPantalla(cargaId));
    return () => {
      controlador.abort();
      finalizarCargaPantalla(cargaId);
    };
  }, [clave, direccion, finalizarCargaPantalla, iniciarCargaPantalla]);

  function elegirPeriodo(valor: PeriodoRapido) {
    setPeriodo(valor);
    if (valor === "personalizado") return;
    const fechas = obtenerPeriodo(valor);
    setDesde(fechas.desde);
    setHasta(fechas.hasta);
  }

  function consultar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const dias = (Date.parse(`${hasta}T00:00:00Z`) - Date.parse(`${desde}T00:00:00Z`)) / 86_400_000;
    if (!desde || !hasta || !Number.isFinite(dias) || dias < 0 || dias >= 366) {
      setValidacion("Selecciona un período de hasta 366 días, con la fecha inicial anterior o igual a la final.");
      return;
    }
    setValidacion("");
    setPagina(0);
    alConsultar({ desde, hasta, agrupacion });
    setRecarga((actual) => actual + 1);
  }

  const cuentas = datos?.cuentas.filter((cuenta) =>
    (situacion === "Todas" || cuenta.situacion === situacion) &&
    cuenta.nombre.toLocaleLowerCase("es").includes(busqueda.trim().toLocaleLowerCase("es"))) ?? [];
  const paginas = Math.max(1, Math.ceil(cuentas.length / 6));
  const paginaVisible = Math.min(pagina, paginas - 1);

  return <div className="estado-negocio">
    <header className="encabezado-pagina negocio-encabezado">
      <div><h1>Estado del negocio</h1><p>Cobros, cuentas de clientes y actividad de tu taller.</p></div>
      <button type="button" className="boton-secundario" disabled={cargando} onClick={() => setRecarga((actual) => actual + 1)}>
        <RefreshCw size={18} aria-hidden="true" />Actualizar
      </button>
    </header>

    <form className="negocio-filtros" onSubmit={consultar} aria-label="Período del negocio">
      <label>Período<select value={periodo} onChange={(evento) => elegirPeriodo(evento.target.value as PeriodoRapido)}>
        <option value="hoy">Hoy</option><option value="semana">Esta semana</option><option value="mes">Este mes</option><option value="personalizado">Personalizado</option>
      </select></label>
      <label>Desde<input type="date" required min="2000-01-01" max={hasta || undefined} value={desde} onChange={(evento) => { setDesde(evento.target.value); setPeriodo("personalizado"); }} /></label>
      <label>Hasta<input type="date" required min={desde || "2000-01-01"} value={hasta} onChange={(evento) => { setHasta(evento.target.value); setPeriodo("personalizado"); }} /></label>
      <label>Agrupar cobros<select value={agrupacion} onChange={(evento) => setAgrupacion(evento.target.value as AgrupacionCobros)}>
        <option value="Dia">Por día</option><option value="Semana">Por semana</option><option value="Mes">Por mes</option>
      </select></label>
      <button type="submit" className="boton-primario" disabled={cargando}>Aplicar</button>
    </form>
    {validacion && <p className="estado-datos estado-datos-error" role="alert">{validacion}</p>}
    {error && <div className="estado-datos estado-datos-error" role="alert"><TriangleAlert size={20} aria-hidden="true" /><span>{error} Usa Actualizar para volver a intentarlo.</span></div>}

    {datos && <>
      <p className="negocio-contexto">Período: <strong>{fecha(datos.desde)} — {fecha(datos.hasta)}</strong><span>Consultado a las {new Intl.DateTimeFormat("es-NI", { timeStyle: "short" }).format(new Date(datos.fechaConsultaUtc))}</span></p>
      <section className="negocio-metricas" aria-label="Indicadores del negocio">
        <MetricaNegocio etiqueta="Cobros del período" valor={moneda(datos.cobrosPeriodo)} detalle={`${datos.abonosPeriodo} abonos vigentes`} />
        <MetricaNegocio etiqueta="Importe de trabajos" valor={moneda(datos.importeTrabajosPeriodo)} detalle={`${datos.ordenesPeriodo} órdenes ingresadas en el período`} />
        <MetricaNegocio etiqueta="Por cobrar · saldo actual" valor={moneda(datos.saldoPendienteActual)} detalle={`${datos.clientesPendientes} clientes con saldo pendiente`} />
        <MetricaNegocio etiqueta="Clientes al día · actual" valor={String(datos.clientesLiquidados)} detalle="Con cargos y saldo liquidado" />
      </section>
      <p className="negocio-nota">Los trabajos incluyen órdenes en proceso y pueden cambiar. Los cobros corresponden a abonos al cliente, incluso de trabajos anteriores. Se excluyen órdenes canceladas y abonos anulados.</p>

      <div className="negocio-columnas">
        <PanelNegocio titulo="Evolución de cobros" detalle="Abonos vigentes según su fecha de registro.">
          <GraficoCobros cobros={datos.evolucionCobros} />
        </PanelNegocio>
        <PanelNegocio titulo="Clientes que más compran" detalle="Por importe actual de órdenes ingresadas en el período.">
          {datos.clientesPrincipales.length === 0 ? <VacioNegocio texto="No hay trabajos con importe en este período." /> :
            <ol className="negocio-clasificacion">{datos.clientesPrincipales.map((cliente) => <li key={cliente.clienteId}>
              <button type="button" className="negocio-fila" onClick={() => alVerCuenta(cliente)} aria-label={`Ver estado de cuenta de ${cliente.nombre}`}>
                <span><strong>{cliente.nombre}</strong><small>{cliente.ordenes} órdenes</small></span>
                <strong>{moneda(cliente.importe)}</strong><ArrowRight size={17} aria-hidden="true" />
              </button>
            </li>)}</ol>}
        </PanelNegocio>
      </div>

      <PanelNegocio titulo="Cuentas de clientes" detalle="Saldo actual de todo el historial registrado, independiente del período seleccionado.">
        <div className="negocio-cartera-resumen">
          <span><strong>{datos.clientesPendientes}</strong> pendientes</span>
          <span><strong>{datos.clientesLiquidados}</strong> liquidados</span>
          <span><strong>{datos.clientesConSaldoFavor}</strong> con saldo a favor · {moneda(datos.saldoFavorActual)}</span>
        </div>
        <div className="negocio-filtros-cuentas">
          <label>Mostrar<select value={situacion} onChange={(evento) => { setSituacion(evento.target.value as SituacionCuenta | "Todas"); setPagina(0); }}>
            <option value="Pendiente">Con saldo pendiente</option><option value="Liquidada">Con saldo liquidado</option><option value="SaldoAFavor">Con saldo a favor</option><option value="Todas">Todas las cuentas</option>
          </select></label>
          <label>Buscar cliente<input type="search" value={busqueda} onChange={(evento) => { setBusqueda(evento.target.value); setPagina(0); }} placeholder="Nombre del cliente" /></label>
        </div>
        <div className="negocio-cuentas" aria-live="polite">
          {cuentas.length === 0 ? <VacioNegocio texto="No hay cuentas que coincidan con estos filtros." /> : cuentas.slice(paginaVisible * 6, paginaVisible * 6 + 6).map((cuenta) =>
            <button type="button" className="negocio-fila negocio-cuenta" key={cuenta.clienteId} onClick={() => alVerCuenta(cuenta)} aria-label={`Ver estado de cuenta de ${cuenta.nombre}`}>
              <span><strong>{cuenta.nombre}</strong><small>{etiquetaSituacion(cuenta.situacion)}</small></span>
              <span className="negocio-cuenta-importes"><small>Trabajos <strong>{moneda(cuenta.importeTrabajos)}</strong></small><small>Abonos <strong>{moneda(cuenta.abonos)}</strong></small></span>
              <span className="negocio-cuenta-saldo"><small>{cuenta.situacion === "SaldoAFavor" ? "A favor" : "Saldo"}</small><strong>{moneda(Math.abs(cuenta.saldo))}</strong></span>
              <ArrowRight size={18} aria-hidden="true" />
            </button>)}
        </div>
        <div className="negocio-paginacion">
          <span>{cuentas.length} cuentas · Página {paginaVisible + 1} de {paginas}</span>
          <div><button type="button" className="boton-secundario" disabled={paginaVisible === 0} aria-label="Página anterior de cuentas" onClick={() => setPagina(paginaVisible - 1)}><ChevronLeft size={19} /></button><button type="button" className="boton-secundario" disabled={paginaVisible >= paginas - 1} aria-label="Página siguiente de cuentas" onClick={() => setPagina(paginaVisible + 1)}><ChevronRight size={19} /></button></div>
        </div>
        <p className="negocio-nota">Los saldos a favor no reducen la deuda de otros clientes. Abre una cuenta para consultar sus cargos y registrar abonos.</p>
      </PanelNegocio>

      <div className="negocio-columnas">
        <PanelNegocio titulo="Vehículos con más visitas" detalle="Órdenes ingresadas en el período, agrupadas por vehículo.">
          {datos.vehiculosFrecuentes.length === 0 ? <VacioNegocio texto="No hay visitas de vehículos en este período." /> :
            <ol className="negocio-clasificacion">{datos.vehiculosFrecuentes.map((vehiculo) => <li key={vehiculo.vehiculoId}>
              <button type="button" className="negocio-fila" onClick={() => alVerVehiculo(vehiculo)} aria-label={`Ver historial completo de ${vehiculo.placa}`}>
                <span><strong>{vehiculo.placa}</strong><small>{vehiculo.marca} {vehiculo.modelo} · {vehiculo.anio}</small></span>
                <strong>{vehiculo.visitas} visitas</strong><ArrowRight size={17} aria-hidden="true" />
              </button>
            </li>)}</ol>}
        </PanelNegocio>
        <PanelNegocio titulo="Marcas y modelos más atendidos" detalle="Cantidad de visitas durante el período.">
          {datos.modelosFrecuentes.length === 0 ? <VacioNegocio texto="No hay marcas y modelos para mostrar." /> :
            <ol className="negocio-clasificacion">{datos.modelosFrecuentes.map((modelo) => <li key={`${modelo.marca}:${modelo.modelo}`}>
              <div className="negocio-fila"><span><strong>{modelo.marca}</strong><small>{modelo.modelo}</small></span><strong>{modelo.visitas} visitas</strong></div>
            </li>)}</ol>}
        </PanelNegocio>
      </div>
    </>}
  </div>;
}

function MetricaNegocio({ etiqueta, valor, detalle }: { etiqueta: string; valor: string; detalle: string }) {
  return <article className="negocio-metrica"><span>{etiqueta}</span><strong>{valor}</strong><small>{detalle}</small></article>;
}

function PanelNegocio({ titulo, detalle, children }: { titulo: string; detalle: string; children: ReactNode }) {
  return <section className="panel negocio-panel"><header><h2>{titulo}</h2><p>{detalle}</p></header>{children}</section>;
}

function VacioNegocio({ texto }: { texto: string }) {
  return <p className="negocio-vacio">{texto}</p>;
}

function GraficoCobros({ cobros }: { cobros: CobroNegocio[] }) {
  const maximo = Math.max(...cobros.map((cobro) => cobro.monto), 1);
  const puntos = cobros.map((cobro, indice) => ({
    x: cobros.length === 1 ? 320 : 12 + indice * 616 / (cobros.length - 1),
    y: 160 - cobro.monto / maximo * 140,
  }));
  return <>
    <figure className="negocio-grafico">
      <div className="negocio-escala"><span>Máximo {moneda(Math.max(...cobros.map((cobro) => cobro.monto), 0))}</span><span>C$ 0</span></div>
      <svg viewBox="0 0 640 180" role="img" aria-label="Evolución de los cobros. Los importes están disponibles debajo del gráfico.">
        <title>Cobros del período seleccionado</title>
        <line x1="12" y1="20" x2="628" y2="20" className="negocio-guia" />
        <line x1="12" y1="90" x2="628" y2="90" className="negocio-guia" />
        <line x1="12" y1="160" x2="628" y2="160" className="negocio-guia" />
        {puntos.length > 1 && <polyline points={puntos.map((punto) => `${punto.x},${punto.y}`).join(" ")} className="negocio-linea" />}
        {puntos.length <= 31 && puntos.map((punto, indice) => <circle key={cobros[indice].desde} cx={punto.x} cy={punto.y} r="3.5" className="negocio-punto" />)}
      </svg>
      <figcaption><span>{cobros[0] ? fecha(cobros[0].desde) : ""}</span><span>{cobros.length ? fecha(cobros[cobros.length - 1].hasta) : ""}</span></figcaption>
    </figure>
    <details className="negocio-detalle-cobros"><summary>Ver importes del gráfico</summary><div>
      <table><caption className="solo-lectores">Cobros y cantidad de abonos por intervalo</caption><thead><tr><th scope="col">Período</th><th scope="col">Abonos</th><th scope="col">Cobrado</th></tr></thead><tbody>
        {cobros.map((cobro) => <tr key={cobro.desde}><th scope="row">{fecha(cobro.desde)}{cobro.hasta !== cobro.desde ? ` — ${fecha(cobro.hasta)}` : ""}</th><td>{cobro.abonos}</td><td>{moneda(cobro.monto)}</td></tr>)}
      </tbody></table>
    </div></details>
  </>;
}

function obtenerPeriodo(periodo: Exclude<PeriodoRapido, "personalizado">) {
  const hoy = new Date();
  const inicio = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
  if (periodo === "mes") inicio.setDate(1);
  if (periodo === "semana") inicio.setDate(inicio.getDate() - (inicio.getDay() + 6) % 7);
  return { desde: fechaEntrada(inicio), hasta: fechaEntrada(hoy) };
}

function fechaEntrada(valor: Date) {
  return `${valor.getFullYear()}-${String(valor.getMonth() + 1).padStart(2, "0")}-${String(valor.getDate()).padStart(2, "0")}`;
}

function fecha(valor: string) {
  return new Intl.DateTimeFormat("es-NI", { day: "numeric", month: "short", year: "numeric" }).format(new Date(`${valor}T12:00:00`));
}

function moneda(valor: number) {
  return new Intl.NumberFormat("es-NI", { style: "currency", currency: "NIO" }).format(valor);
}

function etiquetaSituacion(situacion: SituacionCuenta) {
  return ({ Pendiente: "Saldo pendiente", Liquidada: "Saldo liquidado", SaldoAFavor: "Saldo a favor" })[situacion];
}
