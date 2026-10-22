"use client";

import { use, useEffect, useState } from "react";
import { useCargadorPantalla } from "../../../componentes/ProveedorCargadorPantalla";
import { estaAprobadaPorCliente, etiquetaEstado } from "../../../estadoOrden";
import { formatearFechaHoraOrden } from "../../../fechaHoraOrden";
import { formatearCantidad, formatearImporte, type ConfiguracionMonetaria } from "../../../formatoMonetario";

interface CargoExpediente {
  descripcion: string;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
}

interface OrdenExpediente {
  numero: string;
  estado: string;
  fechaIngreso: string;
  fechaAutorizacionClienteUtc: string | null;
  kilometraje: number | null;
  motivoIngreso: string | null;
  diagnostico: string | null;
  esOrdenCompartida: boolean;
  cargos: CargoExpediente[];
  total: number;
}

interface ExpedienteVehiculo {
  nombreTaller: string;
  vehiculo: { placa: string; marca: string; modelo: string; anio: number; color: string | null };
  ordenes: OrdenExpediente[];
  pagina: number;
  tamanoPagina: number;
  totalOrdenes: number;
  configuracionMonetaria: ConfiguracionMonetaria;
}

export default function ExpedienteVehiculoPagina({ params }: { params: Promise<{ token: string }> }) {
  const { token } = use(params);
  const { ejecutarConCargadorPantalla } = useCargadorPantalla();
  const [respuesta, setRespuesta] = useState<{ token: string; datos: ExpedienteVehiculo } | null>(null);
  const [pagina, setPagina] = useState(1);
  const [intento, setIntento] = useState(0);
  const [error, setError] = useState("");
  const expediente = respuesta?.token === token ? respuesta.datos : null;
  const regreso = `/orden/${encodeURIComponent(token)}`;

  useEffect(() => {
    const controlador = new AbortController();
    ejecutarConCargadorPantalla("Consultando el expediente del vehículo…", () => obtenerExpediente(token, pagina, controlador.signal))
      .then((datos) => { if (!controlador.signal.aborted) setRespuesta({ token, datos }); })
      .catch((motivo: unknown) => {
        if (!controlador.signal.aborted) setError(motivo instanceof Error ? motivo.message : "No fue posible consultar el expediente.");
      });
    return () => controlador.abort();
  }, [ejecutarConCargadorPantalla, token, pagina, intento]);

  function cambiarPagina(siguiente: number) {
    setError("");
    setPagina(siguiente);
    setIntento((actual) => actual + 1);
  }

  if (!expediente) {
    return (
      <main className="pagina-publica-orden expediente-publico">
        <a className="boton-secundario regreso-expediente" href={regreso}>Volver a la orden compartida</a>
        {error && <section className="tarjeta-publica" role="alert"><h1>No pudimos mostrar el expediente</h1><p>{error}</p><button className="boton-secundario" type="button" onClick={() => cambiarPagina(pagina)}>Reintentar</button></section>}
      </main>
    );
  }

  const totalPaginas = Math.max(1, Math.ceil(expediente.totalOrdenes / expediente.tamanoPagina));
  const formatoMonetario = expediente.configuracionMonetaria;
  const vehiculo = expediente.vehiculo;

  return (
    <main className="pagina-publica-orden expediente-publico">
      <a className="boton-secundario regreso-expediente" href={regreso}>Volver a la orden compartida</a>
      <header className="cabecera-publica">
        <div><span className="sobrelinea">{expediente.nombreTaller}</span><h1>Expediente del vehículo</h1><p>Todas las órdenes de este vehículo en el taller, de más reciente a más antigua.</p></div>
      </header>
      <section className="vehiculo-publico">
        <div><small>Placa {vehiculo.placa}</small><h2>{vehiculo.marca} {vehiculo.modelo}</h2><p>{vehiculo.anio}{vehiculo.color ? ` · ${vehiculo.color}` : ""}</p></div>
      </section>
      {error && <section className="tarjeta-publica" role="alert"><p>{error}</p><button type="button" className="boton-secundario" onClick={() => cambiarPagina(pagina)}>Reintentar</button></section>}
      <section className="historial-expediente-publico" aria-labelledby="titulo-historial-vehiculo">
        <div className="cabecera-historial-expediente"><h2 id="titulo-historial-vehiculo">Historial de órdenes</h2><span>{expediente.totalOrdenes} {expediente.totalOrdenes === 1 ? "orden" : "órdenes"}</span></div>
        {expediente.ordenes.length === 0 && <p>No hay órdenes registradas para este vehículo.</p>}
        {expediente.ordenes.map((orden) => (
          <details key={`${expediente.pagina}-${orden.numero}`} className="orden-expediente-publico">
            <summary>
              <span className="identificacion-orden-expediente"><strong>{orden.numero}</strong><time dateTime={orden.fechaIngreso}>{formatearFechaHoraOrden(orden.fechaIngreso)}</time>{orden.esOrdenCompartida && <small>Orden del enlace compartido</small>}</span>
              <span className="resumen-orden-expediente"><span className="estado-publico">{estaAprobadaPorCliente(orden.estado, orden.fechaAutorizacionClienteUtc) ? "Aprobada" : etiquetaEstado(orden.estado)}</span><strong translate="no">{formatearImporte(orden.total, formatoMonetario)}</strong><span className="indicio-detalle-expediente"><span className="texto-abrir-expediente">Ver detalle</span><span className="texto-cerrar-expediente">Ocultar detalle</span><span aria-hidden="true">⌄</span></span></span>
            </summary>
            <div className="contenido-orden-expediente">
              <dl className="datos-orden-expediente">
                <div><dt>Kilometraje de ingreso</dt><dd>{orden.kilometraje === null ? "Sin registrar" : `${orden.kilometraje.toLocaleString("es-NI")} km`}</dd></div>
                <div><dt>Motivo de ingreso</dt><dd>{orden.motivoIngreso || "Sin motivo registrado."}</dd></div>
                <div><dt>Diagnóstico</dt><dd>{orden.diagnostico || "Sin diagnóstico registrado."}</dd></div>
              </dl>
              <div className="cargos-publicos cargos-expediente-publico">
                <h3>Productos y servicios</h3>
                {orden.cargos.length === 0 && <p>Sin productos ni servicios registrados.</p>}
                {orden.cargos.map((cargo, indice) => (
                  <article key={indice}>
                    <span><strong>{cargo.descripcion}</strong><small translate="no">{formatearCantidad(cargo.cantidad, formatoMonetario)} × {formatearImporte(cargo.precioUnitario, formatoMonetario)}</small></span>
                    <strong className="importe-cargo-publico" translate="no">{formatearImporte(cargo.subtotal, formatoMonetario)}</strong>
                  </article>
                ))}
                <div><span>Total de la orden</span><strong translate="no">{formatearImporte(orden.total, formatoMonetario)}</strong></div>
              </div>
            </div>
          </details>
        ))}
      </section>
      {totalPaginas > 1 && (
        <nav className="paginacion-expediente-publico" aria-label="Páginas del expediente">
          <button type="button" className="boton-secundario" disabled={expediente.pagina <= 1} onClick={() => cambiarPagina(expediente.pagina - 1)}>Anterior</button>
          <span aria-live="polite">Página {expediente.pagina} de {totalPaginas}</span>
          <button type="button" className="boton-secundario" disabled={expediente.pagina >= totalPaginas} onClick={() => cambiarPagina(expediente.pagina + 1)}>Siguiente</button>
        </nav>
      )}
      <footer className="pie-publico"><span>Este expediente corresponde únicamente al vehículo del enlace compartido.</span></footer>
    </main>
  );
}

async function obtenerExpediente(token: string, pagina: number, signal: AbortSignal): Promise<ExpedienteVehiculo> {
  const direccionApi = (process.env.NEXT_PUBLIC_API_URL || "/backend").replace(/\/$/, "");
  const respuesta = await fetch(`${direccionApi}/api/publico/ordenes-servicio/${encodeURIComponent(token)}/expediente?pagina=${pagina}`, { signal, cache: "no-store" });
  if (!respuesta.ok) throw new Error(respuesta.status === 404 ? "El enlace no existe o dejó de estar disponible." : "No fue posible consultar el expediente. Inténtalo nuevamente.");
  const datos = await respuesta.json() as ExpedienteVehiculo;
  if (!datos.configuracionMonetaria) throw new Error("No fue posible obtener la configuración monetaria del taller.");
  return datos;
}
