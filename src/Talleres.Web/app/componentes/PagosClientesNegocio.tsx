"use client";

import { useEffect, useState, type FormEvent } from "react";
import { ChevronLeft, ChevronRight, RefreshCw, TriangleAlert } from "lucide-react";
import { useCargadorPantalla } from "./ProveedorCargadorPantalla";

type SituacionPago = "Todos" | "Vigentes" | "Anulados";
type AlcancePagos = "historial" | "periodo";

interface FiltrosPagos {
  alcance: AlcancePagos;
  situacion: SituacionPago;
  busqueda: string;
}

interface PagoClienteNegocio {
  id: number;
  clienteId: number;
  nombreCliente: string;
  monto: number;
  fechaRegistroUtc: string;
  formaPago: string | null;
  referencia: string | null;
  fechaAnulacionUtc: string | null;
}

interface PaginaPagos {
  pagos: PagoClienteNegocio[];
  totalRegistros: number;
  pagina: number;
  tamanoPagina: number;
  totalPaginas: number;
}

const filtrosIniciales: FiltrosPagos = { alcance: "historial", situacion: "Todos", busqueda: "" };

export function PagosClientesNegocio({ direccionApi, desde, hasta, alVerCuenta }: {
  direccionApi: string;
  desde: string;
  hasta: string;
  alVerCuenta: (cliente: { clienteId: number; nombre: string }) => void;
}) {
  const { iniciarCargaPantalla, finalizarCargaPantalla } = useCargadorPantalla();
  const [borrador, setBorrador] = useState(filtrosIniciales);
  const [consulta, setConsulta] = useState({ ...filtrosIniciales, pagina: 1, revision: 0 });
  const [resultado, setResultado] = useState<{ clave: string; datos: PaginaPagos } | null>(null);
  const [fallo, setFallo] = useState<{ clave: string; mensaje: string } | null>(null);
  const parametros = new URLSearchParams({
    situacion: consulta.situacion,
    busqueda: consulta.busqueda,
    pagina: String(consulta.pagina),
    tamanoPagina: "6",
    desfaseHorarioMinutos: String(new Date().getTimezoneOffset()),
  });
  if (consulta.alcance === "periodo") {
    parametros.set("desde", desde);
    parametros.set("hasta", hasta);
  }
  const direccion = `${direccionApi}/api/estado-negocio/pagos?${parametros}`;
  const clave = `${direccion}:${consulta.revision}`;
  const datos = resultado?.clave === clave ? resultado.datos : null;
  const error = fallo?.clave === clave ? fallo.mensaje : "";
  const cargando = !datos && !error;

  useEffect(() => {
    const controlador = new AbortController();
    const cargaId = iniciarCargaPantalla("Consultando los pagos de clientes…");
    fetch(direccion, { credentials: "include", cache: "no-store", signal: controlador.signal })
      .then(async (respuesta) => {
        if (!respuesta.ok) {
          const problema = await respuesta.json().catch(() => null) as { detail?: string; title?: string } | null;
          throw new Error(respuesta.status === 401
            ? "La sesión terminó. Vuelve a iniciar sesión para consultar los pagos."
            : problema?.detail || problema?.title || "No fue posible consultar los pagos de clientes.");
        }
        return await respuesta.json() as PaginaPagos;
      })
      .then((pagina) => {
        if (!controlador.signal.aborted) setResultado({ clave, datos: pagina });
      })
      .catch((motivo: unknown) => {
        if (!controlador.signal.aborted) setFallo({ clave, mensaje: motivo instanceof Error ? motivo.message : "No fue posible consultar los pagos de clientes." });
      })
      .finally(() => finalizarCargaPantalla(cargaId));
    return () => {
      controlador.abort();
      finalizarCargaPantalla(cargaId);
    };
  }, [clave, direccion, finalizarCargaPantalla, iniciarCargaPantalla]);

  function aplicarFiltros(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    setConsulta((actual) => ({ ...borrador, busqueda: borrador.busqueda.trim(), pagina: 1, revision: actual.revision + 1 }));
  }

  return <section className="panel negocio-panel negocio-pagos" aria-labelledby="titulo-pagos-negocio">
    <header className="negocio-pagos-encabezado">
      <div><h2 id="titulo-pagos-negocio">Pagos de clientes</h2><p>Todos los pagos registrados, del más reciente al más antiguo.</p></div>
      <button type="button" className="boton-secundario" disabled={cargando} onClick={() => setConsulta((actual) => ({ ...actual, revision: actual.revision + 1 }))}>
        <RefreshCw size={17} aria-hidden="true" />Actualizar pagos
      </button>
    </header>
    <form className="negocio-filtros-cuentas negocio-filtros-pagos" onSubmit={aplicarFiltros} aria-label="Filtrar pagos de clientes">
      <label htmlFor="alcance-pagos-negocio">Fechas<select id="alcance-pagos-negocio" value={borrador.alcance} onChange={(evento) => setBorrador({ ...borrador, alcance: evento.target.value as AlcancePagos })}>
        <option value="historial">Todo el historial</option><option value="periodo">Período del dashboard</option>
      </select></label>
      <label htmlFor="situacion-pagos-negocio">Estado<select id="situacion-pagos-negocio" value={borrador.situacion} onChange={(evento) => setBorrador({ ...borrador, situacion: evento.target.value as SituacionPago })}>
        <option value="Todos">Todos los pagos</option><option value="Vigentes">Solo vigentes</option><option value="Anulados">Solo anulados</option>
      </select></label>
      <label htmlFor="busqueda-pagos-negocio">Buscar<input id="busqueda-pagos-negocio" type="search" maxLength={120} placeholder="Cliente, forma de pago o referencia" value={borrador.busqueda} onChange={(evento) => setBorrador({ ...borrador, busqueda: evento.target.value })} /></label>
      <button type="submit" className="boton-primario" disabled={cargando}>Buscar pagos</button>
    </form>
    <p className="negocio-nota">{consulta.alcance === "historial" ? "Mostrando todo el historial del taller." : "Mostrando pagos del período seleccionado en el dashboard."} Los pagos anulados se conservan como historial y no suman a los cobros.</p>
    {error && <div className="estado-datos estado-datos-error" role="alert"><TriangleAlert size={20} aria-hidden="true" /><span>{error} Usa Actualizar pagos para reintentar.</span></div>}
    {datos && <>
      {datos.pagos.length === 0 ? <p className="negocio-vacio" role="status">No hay pagos que coincidan con estos filtros.</p> :
        <table className="negocio-tabla-pagos">
          <caption className="solo-lectores">Pagos de clientes, fecha de registro, monto, forma de pago, referencia y estado</caption>
          <thead><tr><th scope="col">Fecha y hora</th><th scope="col">Cliente</th><th scope="col">Monto</th><th scope="col">Forma de pago</th><th scope="col">Referencia</th><th scope="col">Estado</th></tr></thead>
          <tbody>{datos.pagos.map((pago) => <tr key={pago.id}>
            <td data-etiqueta="Fecha y hora"><time dateTime={pago.fechaRegistroUtc}>{fechaHora(pago.fechaRegistroUtc)}</time></td>
            <td data-etiqueta="Cliente"><button type="button" className="negocio-pago-cliente" onClick={() => alVerCuenta({ clienteId: pago.clienteId, nombre: pago.nombreCliente })} aria-label={`Ver cuenta de ${pago.nombreCliente}`}>{pago.nombreCliente}</button></td>
            <td data-etiqueta="Monto" className="negocio-pago-monto">{new Intl.NumberFormat("es-NI", { style: "currency", currency: "NIO" }).format(pago.monto)}</td>
            <td data-etiqueta="Forma de pago">{pago.formaPago || "Sin indicar"}</td>
            <td data-etiqueta="Referencia">{pago.referencia || "Sin referencia"}</td>
            <td data-etiqueta="Estado"><span className="negocio-pago-estado">{pago.fechaAnulacionUtc ? "Anulado" : "Vigente"}</span>{pago.fechaAnulacionUtc && <small>Anulado el {fechaHora(pago.fechaAnulacionUtc)}</small>}</td>
          </tr>)}</tbody>
        </table>}
      <div className="negocio-paginacion" aria-live="polite">
        <span>{datos.totalRegistros} {datos.totalRegistros === 1 ? "pago" : "pagos"} · Página {datos.pagina} de {datos.totalPaginas}</span>
        <div><button type="button" className="boton-secundario" disabled={cargando || datos.pagina <= 1} aria-label="Página anterior de pagos" onClick={() => setConsulta((actual) => ({ ...actual, pagina: datos.pagina - 1 }))}><ChevronLeft size={19} /></button><button type="button" className="boton-secundario" disabled={cargando || datos.pagina >= datos.totalPaginas} aria-label="Página siguiente de pagos" onClick={() => setConsulta((actual) => ({ ...actual, pagina: datos.pagina + 1 }))}><ChevronRight size={19} /></button></div>
      </div>
    </>}
  </section>;
}

function fechaHora(valor: string) {
  return new Intl.DateTimeFormat("es-NI", { dateStyle: "medium", timeStyle: "short" }).format(new Date(valor));
}
