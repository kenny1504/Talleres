"use client";

import { use, useEffect, useState } from "react";
import { Building2, CalendarDays, Camera, CarFront, Check, CircleCheck, ClipboardCheck, ClipboardList, FileText, Fuel, KeyRound, MapPin, Minus, Phone, Wrench, X, ZoomIn } from "lucide-react";
import { useCargadorPantalla } from "../../componentes/ProveedorCargadorPantalla";
import { VisorFotografias, type EstadoVisor, type ImagenVisor } from "../../componentes/VisorFotografias";
import { MapaInspeccion } from "../../componentes/MapaInspeccion";
import { etiquetaZona, zonaDesdeApi, type SeveridadDanio } from "../../inspeccionVehiculo";
import { formatearFechaHoraOrden } from "../../fechaHoraOrden";
import { estaAprobadaPorCliente } from "../../estadoOrden";
import { elementosRecepcion, type ElementosRecepcion } from "../../elementosRecepcion";
import { actividadesRevisionOperativa, type ResultadoRevisionOperativa } from "../../revisionOperativa";
import { formatearCantidad, formatearImporte, type ConfiguracionMonetaria } from "../../formatoMonetario";

interface EvidenciaPublica { id: number; nombreArchivo: string; }
interface DanioInspeccionPublico { id: number; zona: string; tipo: string; severidad: SeveridadDanio; observacion: string | null; }
interface DetallePublico { id: number; descripcion: string; cantidad: number; unidadMedida: string | null; precioUnitario: number; subtotal: number; }
interface TallerPublico { nombre: string; direccion: string | null; telefono: string | null; logo: string | null; }
interface OrdenPublica extends ElementosRecepcion {
  taller: TallerPublico;
  numero: string; estado: string; fechaIngreso: string; nombreCliente: string; placa: string;
  marca: string; modelo: string; anio: number; color: string | null; numeroVin: string | null;
  motivoIngreso: string | null; diagnostico: string; fechaAutorizacionClienteUtc: string | null;
  kilometraje: number | null; porcentajeCombustible: number | null; descripcionEstado: string | null;
  revisionOperativa: ResultadoRevisionOperativa[]; observacionRevisionOperativa: string | null;
  daniosInspeccion: DanioInspeccionPublico[];
  evidenciasInspeccion: EvidenciaPublica[]; evidenciasDiagnostico: EvidenciaPublica[];
  detalles: DetallePublico[]; total: number;
  configuracionMonetaria: ConfiguracionMonetaria;
}

export default function OrdenPublicaPagina({ params }: { params: Promise<{ token: string }> }) {
  const { token } = use(params);
  const { ejecutarConCargadorPantalla } = useCargadorPantalla();
  const [orden, setOrden] = useState<OrdenPublica | null>(null);
  const [error, setError] = useState("");
  const [visor, setVisor] = useState<EstadoVisor | null>(null);
  const [imagenesInvalidas, setImagenesInvalidas] = useState<Set<string>>(() => new Set());

  useEffect(() => {
    const controlador = new AbortController();
    ejecutarConCargadorPantalla("Consultando la orden de servicio…", () => obtenerOrden(token, controlador.signal))
      .then(setOrden)
      .catch((motivo) => { if (!controlador.signal.aborted) setError(motivo instanceof Error ? motivo.message : "No fue posible consultar la orden."); });
    return () => controlador.abort();
  }, [ejecutarConCargadorPantalla, token]);

  async function autorizar() {
    setError("");
    try {
      setOrden(await ejecutarConCargadorPantalla("Registrando tu autorización…", () => autorizarOrden(token)));
    } catch (motivo) {
      setError(motivo instanceof Error ? motivo.message : "No fue posible registrar la autorización.");
    }
  }

  function abrirVisor(imagenes: ImagenVisor[], indice: number) {
    setVisor({ imagenes, indice });
  }

  if (error) return <main className="pagina-publica-orden"><section className="estado-publico-error"><ClipboardList size={34} /><h1>No pudimos mostrar esta orden</h1><p>{error}</p></section></main>;
  if (!orden) return null;
  const autorizada = Boolean(orden.fechaAutorizacionClienteUtc);
  const daniosInspeccion = orden.daniosInspeccion ?? [];
  const daniosMapa = daniosInspeccion.map((danio) => ({
    zona: zonaDesdeApi(danio.zona),
    severidad: danio.severidad,
  }));
  const evidenciasInspeccion = orden.evidenciasInspeccion ?? [];
  const elementosEntregados = elementosRecepcion.filter(({ campo }) => orden[campo]);
  const otrosElementos = orden.otrosElementos?.trim();
  const revisionOperativa = orden.revisionOperativa ?? [];
  const observacionRevisionOperativa = orden.observacionRevisionOperativa?.trim();
  const taller = orden.taller ?? { nombre: "Taller", direccion: null, telefono: null, logo: null };
  const formatoMonetario = orden.configuracionMonetaria;
  const imagenesInspeccion = evidenciasInspeccion
    .map((evidencia, indice) => ({ direccion: direccionEvidenciaInspeccion(token, evidencia.id), descripcion: `Fotografía ${indice + 1} de la inspección` }))
    .filter((imagen) => !imagenesInvalidas.has(imagen.direccion));
  const imagenesDiagnostico = (orden.evidenciasDiagnostico ?? [])
    .map((evidencia, indice) => ({ direccion: direccionEvidencia(token, evidencia.id), descripcion: `Fotografía ${indice + 1} del diagnóstico` }))
    .filter((imagen) => !imagenesInvalidas.has(imagen.direccion));

  return (
    <main className="pagina-publica-orden">
      <section className="identidad-taller-publica">
        <div className="logo-taller-publico">
          <Building2 size={30} />
          {taller.logo && (
            // eslint-disable-next-line @next/next/no-img-element
            <img src={taller.logo} alt={`Logo de ${taller.nombre}`} onError={(evento) => { evento.currentTarget.hidden = true; }} />
          )}
        </div>
        <div className="nombre-taller-publico"><small>Atendido por</small><strong>{taller.nombre}</strong></div>
        <div className="contacto-taller-publico">
          {taller.direccion && <span><MapPin size={17} />{taller.direccion}</span>}
          {taller.telefono && <a href={`tel:${taller.telefono.replace(/[^\d+]/g, "")}`}><Phone size={17} />{taller.telefono}</a>}
        </div>
      </section>
      <header className="cabecera-publica">
        <div><span className="sobrelinea">Orden de servicio</span><h1>{orden.numero}</h1><p>Información compartida por tu taller</p></div>
        <span className="estado-publico">{estaAprobadaPorCliente(orden.estado, orden.fechaAutorizacionClienteUtc) ? "Aprobada" : etiquetaEstado(orden.estado)}</span>
      </header>
      <section className="vehiculo-publico">
        <span><CarFront size={30} /></span>
        <div><small>Vehículo de {orden.nombreCliente}</small><h2>{orden.marca} {orden.modelo}</h2><p>{orden.anio} · Placa {orden.placa}{orden.color ? ` · ${orden.color}` : ""}</p></div>
      </section>
      <section className="tarjeta-publica inspeccion-publica">
        <div className="cabecera-seccion-publica">
          <div><span className="sobrelinea">Recepción documentada</span><h2>Inspección del vehículo</h2></div>
          <span className="resumen-hallazgos">{daniosInspeccion.length === 0 ? "Sin daños marcados" : `${daniosInspeccion.length} hallazgo${daniosInspeccion.length === 1 ? "" : "s"}`}</span>
        </div>
        <div className="datos-inspeccion-publica">
          {orden.kilometraje !== null && <article><CarFront size={20} /><span><small>Kilometraje</small><strong>{orden.kilometraje.toLocaleString("es-NI")} km</strong></span></article>}
          {orden.porcentajeCombustible !== null && <article><Fuel size={20} /><span><small>Combustible</small><strong>{etiquetaCombustible(orden.porcentajeCombustible)}</strong></span></article>}
          <article className="elementos-entregados-publicos" aria-labelledby="titulo-elementos-entregados">
            <div className="cabecera-elementos-publicos"><KeyRound size={18} aria-hidden="true" /><h3 id="titulo-elementos-entregados">Elementos entregados</h3></div>
            {elementosEntregados.length > 0 && (
              <ul className="lista-elementos-publicos">
                {elementosEntregados.map(({ campo, etiqueta }) => <li key={campo}><Check size={16} aria-hidden="true" />{etiqueta}</li>)}
              </ul>
            )}
            {otrosElementos && <div className="otros-elementos-publicos"><small>Otros elementos</small><p>{otrosElementos}</p></div>}
            {elementosEntregados.length === 0 && !otrosElementos && <p className="elementos-publicos-vacios">Ninguno registrado</p>}
          </article>
          <article className="revision-operativa-publica" aria-labelledby="titulo-revision-operativa">
            <div className="cabecera-elementos-publicos"><ClipboardCheck size={18} aria-hidden="true" /><h3 id="titulo-revision-operativa">Revisión operativa</h3></div>
            <ul className="lista-elementos-publicos">
              {actividadesRevisionOperativa.map(({ actividad, etiqueta }) => {
                const resultado = revisionOperativa.find((item) => item.actividad === actividad);
                const estado = resultado ? (resultado.correcto ? "Correcto" : "Con problema") : "Sin revisar";
                return (
                  <li key={actividad} className={resultado ? (resultado.correcto ? "revision-correcta" : "revision-con-problema") : "revision-sin-revisar"}>
                    {resultado ? (resultado.correcto ? <Check size={16} aria-hidden="true" /> : <X size={16} aria-hidden="true" />) : <Minus size={16} aria-hidden="true" />}
                    <span>{etiqueta}: {estado}</span>
                  </li>
                );
              })}
            </ul>
            <div className="otros-elementos-publicos"><small>Observación de la revisión operativa</small><p>{observacionRevisionOperativa || "Sin observaciones de la revisión operativa."}</p></div>
          </article>
        </div>
        {orden.descripcionEstado && <div className="observacion-publica"><FileText size={20} /><span><small>Observaciones generales</small><p>{orden.descripcionEstado}</p></span></div>}
        <div className="mapa-hallazgos-publicos">
          <MapaInspeccion danios={daniosMapa} />
          <div className="hallazgos-publicos">
            <h3>Daños y hallazgos visuales</h3>
            {daniosInspeccion.length > 0 ? daniosInspeccion.map((danio) => (
              <article key={danio.id}>
                <span className={"severidad-publica severidad-" + danio.severidad.toLocaleLowerCase("es")}>{danio.severidad}</span>
                <div><strong>{etiquetaTipoDanio(danio.tipo)} · {etiquetaZona(zonaDesdeApi(danio.zona))}</strong><p>{danio.observacion || "Sin observación adicional."}</p></div>
              </article>
            )) : <p>No se registraron daños visibles durante la inspección.</p>}
          </div>
        </div>
        {imagenesInspeccion.length > 0 && (
          <div className="evidencias-publicas">
            <h3><Camera size={20} /> Fotografías de la inspección</h3>
            <div className="galeria-publica">{imagenesInspeccion.map((imagen, indice) => (
              <button type="button" key={imagen.direccion} className="miniatura-publica" aria-label={`Ampliar ${imagen.descripcion.toLocaleLowerCase("es")}`} onClick={() => abrirVisor(imagenesInspeccion, indice)}>
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img src={imagen.direccion} alt={imagen.descripcion} loading="lazy" onError={() => setImagenesInvalidas((actuales) => new Set(actuales).add(imagen.direccion))} />
                <span><ZoomIn size={19} />Ver imagen</span>
              </button>
            ))}</div>
          </div>
        )}
      </section>
      <div className="rejilla-publica">
        <section className="tarjeta-publica diagnostico-publico">
          <span className="sobrelinea">Diagnóstico del taller</span>
          <h2>Hallazgos encontrados</h2>
          <p>{orden.diagnostico}</p>
          {imagenesDiagnostico.length > 0 && <div className="galeria-publica">{imagenesDiagnostico.map((imagen, indice) => (
            <button type="button" key={imagen.direccion} className="miniatura-publica" aria-label={`Ampliar ${imagen.descripcion.toLocaleLowerCase("es")}`} onClick={() => abrirVisor(imagenesDiagnostico, indice)}>
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={imagen.direccion} alt={imagen.descripcion} loading="lazy" onError={() => setImagenesInvalidas((actuales) => new Set(actuales).add(imagen.direccion))} />
              <span><ZoomIn size={19} />Ver imagen</span>
            </button>
          ))}</div>}
        </section>
        <aside className="tarjeta-publica datos-publicos">
          <div><CalendarDays size={19} /><span><small>Fecha y hora de ingreso</small><strong>{formatearFechaHoraOrden(orden.fechaIngreso)}</strong></span></div>
          {orden.motivoIngreso && <div className="dato-publico-ancho"><span><small>Motivo de ingreso</small><strong>{orden.motivoIngreso}</strong></span></div>}
        </aside>
      </div>
      {orden.detalles.length > 0 && (
        <section className="tarjeta-publica cargos-publicos">
          <span className="sobrelinea">Productos y servicios registrados</span>
          <h2>Detalle de la orden</h2>
          {orden.detalles.map((detalle) => (
            <article key={detalle.id}>
              <span>
                <strong>{detalle.descripcion}</strong>
                <small translate="no">{formatearCantidad(detalle.cantidad, formatoMonetario)} × {formatearImporte(detalle.precioUnitario, formatoMonetario)}</small>
              </span>
              <strong className="importe-cargo-publico" translate="no" aria-label={`Subtotal: ${formatearImporte(detalle.subtotal, formatoMonetario)}`}>{formatearImporte(detalle.subtotal, formatoMonetario)}</strong>
            </article>
          ))}
          <div><span>Total</span><strong translate="no">{formatearImporte(orden.total, formatoMonetario)}</strong></div>
        </section>
      )}
      <section className={`autorizacion-publica ${autorizada ? "autorizada" : ""}`}>
        <span>{autorizada ? <CircleCheck size={30} /> : <Wrench size={30} />}</span>
        <div><span className="sobrelinea">Autorización</span><h2>{autorizada ? "Autorización registrada" : "¿Autorizas continuar?"}</h2><p>{autorizada ? "El taller ya puede continuar con la reparación." : "Esta autorización permite al taller continuar. Los productos, servicios y el total pueden agregarse después durante la reparación."}</p>{autorizada && <p className="estado-autorizacion-publica">Estado actual: <strong>{estaAprobadaPorCliente(orden.estado, orden.fechaAutorizacionClienteUtc) ? "Aprobada" : etiquetaEstado(orden.estado)}</strong></p>}</div>
        {!autorizada && orden.estado === "PendienteAprobacion" && <button type="button" className="boton-primario" onClick={autorizar}><Check size={20} />Autorizar continuar</button>}
      </section>
      <footer className="pie-publico">
        <span>La información de este enlace corresponde únicamente a esta orden de servicio.</span>
        <span>Sistema desarrollado por <a href="https://www.ksoftech.com/" target="_blank" rel="noreferrer">www.ksoftech.com</a></span>
      </footer>
      {visor && (
        <VisorFotografias
          seleccion={visor}
          alCerrar={() => setVisor(null)}
          alImagenInvalida={(direccion) => setImagenesInvalidas((actuales) => new Set(actuales).add(direccion))}
        />
      )}
    </main>
  );
}

function direccionApi() { return (process.env.NEXT_PUBLIC_API_URL || "/backend").replace(/\/$/, ""); }
function direccionEvidencia(token: string, evidenciaId: number) { return `${direccionApi()}/api/publico/ordenes-servicio/${token}/evidencias-diagnostico/${evidenciaId}/contenido`; }
function direccionEvidenciaInspeccion(token: string, evidenciaId: number) { return `${direccionApi()}/api/publico/ordenes-servicio/${token}/evidencias-inspeccion/${evidenciaId}/contenido`; }
async function obtenerOrden(token: string, signal?: AbortSignal): Promise<OrdenPublica> {
  const respuesta = await fetch(`${direccionApi()}/api/publico/ordenes-servicio/${token}`, { signal });
  if (!respuesta.ok) throw new Error(respuesta.status === 404 ? "El enlace no existe o dejó de estar disponible." : "No fue posible consultar la orden.");
  const datos = await respuesta.json() as OrdenPublica;
  if (!datos.configuracionMonetaria) throw new Error("No fue posible obtener la configuración monetaria del taller.");
  return datos;
}
async function autorizarOrden(token: string): Promise<OrdenPublica> {
  const respuesta = await fetch(`${direccionApi()}/api/publico/ordenes-servicio/${token}/autorizar`, { method: "POST" });
  if (!respuesta.ok) throw new Error("No fue posible registrar la autorización. Inténtalo nuevamente.");
  return await respuesta.json() as OrdenPublica;
}
function etiquetaEstado(estado: string) { return ({ Recepcion: "Recepción", Diagnostico: "Diagnóstico", Cotizacion: "Cotización", PendienteAprobacion: "Por autorizar", PreparacionReparacion: "Preparación de reparación", Reparacion: "En reparación", ControlCalidad: "Control de calidad", ListaParaEntrega: "Lista para entregar", Entregada: "Entregada", Cerrada: "Cerrada", Cancelada: "Cancelada" } as Record<string, string>)[estado] ?? estado; }
function etiquetaCombustible(porcentaje: number) { return porcentaje >= 88 ? "Tanque lleno" : porcentaje >= 63 ? "3/4 de tanque" : porcentaje >= 38 ? "Medio tanque" : porcentaje >= 13 ? "1/4 de tanque" : "Reserva"; }
function etiquetaTipoDanio(tipo: string) { return tipo === "Rayon" ? "Rayón" : tipo; }
