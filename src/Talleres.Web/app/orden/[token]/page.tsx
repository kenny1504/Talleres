"use client";

import { use, useEffect, useState } from "react";
import { Building2, CalendarDays, Camera, CarFront, Check, ChevronLeft, ChevronRight, CircleCheck, ClipboardList, FileText, Fuel, KeyRound, MapPin, Phone, RotateCcw, Wrench, X, ZoomIn, ZoomOut } from "lucide-react";
import { useCargadorPantalla } from "../../componentes/ProveedorCargadorPantalla";

interface EvidenciaPublica { id: number; nombreArchivo: string; }
interface DanioInspeccionPublico { id: number; zona: string; tipo: string; severidad: string; observacion: string | null; }
interface DetallePublico { id: number; descripcion: string; cantidad: number; unidadMedida: string | null; precioUnitario: number; subtotal: number; }
interface TallerPublico { nombre: string; direccion: string | null; telefono: string | null; logo: string | null; }
interface ImagenVisor { direccion: string; descripcion: string; }
interface EstadoVisor { imagenes: ImagenVisor[]; indice: number; }
interface OrdenPublica {
  taller: TallerPublico;
  numero: string; estado: string; fechaIngreso: string; nombreCliente: string; placa: string;
  marca: string; modelo: string; anio: number; color: string | null; numeroVin: string | null;
  motivoIngreso: string | null; diagnostico: string; fechaAutorizacionClienteUtc: string | null;
  kilometraje: number | null; porcentajeCombustible: number | null; descripcionEstado: string | null;
  dejaLlaves: boolean; dejaDocumentos: boolean; daniosInspeccion: DanioInspeccionPublico[];
  evidenciasInspeccion: EvidenciaPublica[]; evidenciasDiagnostico: EvidenciaPublica[];
  detalles: DetallePublico[]; total: number;
}

export default function OrdenPublicaPagina({ params }: { params: Promise<{ token: string }> }) {
  const { token } = use(params);
  const { ejecutarConCargadorPantalla } = useCargadorPantalla();
  const [orden, setOrden] = useState<OrdenPublica | null>(null);
  const [error, setError] = useState("");
  const [visor, setVisor] = useState<EstadoVisor | null>(null);
  const [zoom, setZoom] = useState(1);
  const [imagenesInvalidas, setImagenesInvalidas] = useState<Set<string>>(() => new Set());

  useEffect(() => {
    const controlador = new AbortController();
    ejecutarConCargadorPantalla("Consultando la orden de servicio…", () => obtenerOrden(token, controlador.signal))
      .then(setOrden)
      .catch((motivo) => { if (!controlador.signal.aborted) setError(motivo instanceof Error ? motivo.message : "No fue posible consultar la orden."); });
    return () => controlador.abort();
  }, [ejecutarConCargadorPantalla, token]);

  useEffect(() => {
    if (!visor) return;
    const desplazamientoAnterior = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    const alPresionarTecla = (evento: KeyboardEvent) => {
      if (evento.key === "Escape") setVisor(null);
      if (evento.key === "ArrowLeft") {
        setZoom(1);
        setVisor((actual) => actual ? { ...actual, indice: (actual.indice - 1 + actual.imagenes.length) % actual.imagenes.length } : null);
      }
      if (evento.key === "ArrowRight") {
        setZoom(1);
        setVisor((actual) => actual ? { ...actual, indice: (actual.indice + 1) % actual.imagenes.length } : null);
      }
    };
    window.addEventListener("keydown", alPresionarTecla);
    return () => {
      document.body.style.overflow = desplazamientoAnterior;
      window.removeEventListener("keydown", alPresionarTecla);
    };
  }, [visor]);

  async function autorizar() {
    setError("");
    try {
      setOrden(await ejecutarConCargadorPantalla("Registrando tu autorización…", () => autorizarOrden(token)));
    } catch (motivo) {
      setError(motivo instanceof Error ? motivo.message : "No fue posible registrar la autorización.");
    }
  }

  function abrirVisor(imagenes: ImagenVisor[], indice: number) {
    setZoom(1);
    setVisor({ imagenes, indice });
  }

  function cambiarImagen(desplazamiento: number) {
    setZoom(1);
    setVisor((actual) => actual ? { ...actual, indice: (actual.indice + desplazamiento + actual.imagenes.length) % actual.imagenes.length } : null);
  }

  if (error) return <main className="pagina-publica-orden"><section className="estado-publico-error"><ClipboardList size={34} /><h1>No pudimos mostrar esta orden</h1><p>{error}</p></section></main>;
  if (!orden) return null;
  const autorizada = Boolean(orden.fechaAutorizacionClienteUtc);
  const daniosInspeccion = orden.daniosInspeccion ?? [];
  const evidenciasInspeccion = orden.evidenciasInspeccion ?? [];
  const taller = orden.taller ?? { nombre: "Taller", direccion: null, telefono: null, logo: null };
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
        <span className="estado-publico">{etiquetaEstado(orden.estado)}</span>
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
          <article><KeyRound size={20} /><span><small>Elementos entregados</small><strong>{elementosEntregados(orden)}</strong></span></article>
        </div>
        {orden.descripcionEstado && <div className="observacion-publica"><FileText size={20} /><span><small>Observaciones generales</small><p>{orden.descripcionEstado}</p></span></div>}
        {daniosInspeccion.length > 0 && (
          <div className="hallazgos-publicos">
            <h3>Daños y hallazgos visuales</h3>
            {daniosInspeccion.map((danio) => (
              <article key={danio.id}>
                <span className={`severidad-publica severidad-${danio.severidad.toLocaleLowerCase("es")}`}>{danio.severidad}</span>
                <div><strong>{etiquetaTipoDanio(danio.tipo)} · {etiquetaZona(danio.zona)}</strong><p>{danio.observacion || "Sin observación adicional."}</p></div>
              </article>
            ))}
          </div>
        )}
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
          <div><CalendarDays size={19} /><span><small>Ingreso</small><strong>{new Intl.DateTimeFormat("es-NI", { dateStyle: "medium" }).format(new Date(orden.fechaIngreso))}</strong></span></div>
          {orden.motivoIngreso && <div className="dato-publico-ancho"><span><small>Motivo de ingreso</small><strong>{orden.motivoIngreso}</strong></span></div>}
        </aside>
      </div>
      {orden.detalles.length > 0 && <section className="tarjeta-publica cargos-publicos"><span className="sobrelinea">Productos y servicios registrados</span><h2>Detalle de la orden</h2>{orden.detalles.map((detalle) => <article key={detalle.id}><span><strong>{detalle.descripcion}</strong><small>{detalle.cantidad} {detalle.unidadMedida || "unidad"} × {moneda(detalle.precioUnitario)}</small></span><strong>{moneda(detalle.subtotal)}</strong></article>)}<div><span>Total</span><strong>{moneda(orden.total)}</strong></div></section>}
      <section className={`autorizacion-publica ${autorizada ? "autorizada" : ""}`}>
        <span>{autorizada ? <CircleCheck size={30} /> : <Wrench size={30} />}</span>
        <div><span className="sobrelinea">Autorización</span><h2>{autorizada ? "Autorización registrada" : "¿Autorizas continuar?"}</h2><p>{autorizada ? "El taller ya puede continuar con la reparación." : "Esta autorización permite al taller continuar. Los productos, servicios y el total pueden agregarse después durante la reparación."}</p></div>
        {!autorizada && orden.estado === "PendienteAprobacion" && <button type="button" className="boton-primario" onClick={autorizar}><Check size={20} />Autorizar continuar</button>}
      </section>
      <footer className="pie-publico">
        <span>La información de este enlace corresponde únicamente a esta orden de servicio.</span>
        <span>Sistema desarrollado por <a href="https://www.ksoftech.com/" target="_blank" rel="noreferrer">www.ksoftech.com</a></span>
      </footer>
      {visor && (
        <div className="fondo-visor-imagen" role="dialog" aria-modal="true" aria-label="Visor de fotografías" onClick={() => setVisor(null)}>
          <section className="visor-imagen-publica" onClick={(evento) => evento.stopPropagation()}>
            <header>
              <strong>{visor.imagenes[visor.indice].descripcion}</strong>
              <span>{visor.indice + 1} de {visor.imagenes.length}</span>
              <button type="button" aria-label="Cerrar visor" onClick={() => setVisor(null)}><X size={24} /></button>
            </header>
            <div className="contenido-visor-imagen">
              <button type="button" className="navegacion-visor anterior" aria-label="Fotografía anterior" disabled={visor.imagenes.length < 2} onClick={() => cambiarImagen(-1)}><ChevronLeft size={30} /></button>
              <div className="lienzo-visor-imagen">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img src={visor.imagenes[visor.indice].direccion} alt={visor.imagenes[visor.indice].descripcion} style={{ width: `${zoom * 100}%` }} onError={() => { setImagenesInvalidas((actuales) => new Set(actuales).add(visor.imagenes[visor.indice].direccion)); setVisor(null); }} />
              </div>
              <button type="button" className="navegacion-visor siguiente" aria-label="Fotografía siguiente" disabled={visor.imagenes.length < 2} onClick={() => cambiarImagen(1)}><ChevronRight size={30} /></button>
            </div>
            <footer>
              <button type="button" aria-label="Alejar imagen" disabled={zoom <= 1} onClick={() => setZoom((actual) => Math.max(1, actual - .5))}><ZoomOut size={21} /></button>
              <button type="button" className="nivel-zoom" onClick={() => setZoom(1)}><RotateCcw size={18} />{Math.round(zoom * 100)}%</button>
              <button type="button" aria-label="Acercar imagen" disabled={zoom >= 3} onClick={() => setZoom((actual) => Math.min(3, actual + .5))}><ZoomIn size={21} /></button>
            </footer>
          </section>
        </div>
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
  return await respuesta.json() as OrdenPublica;
}
async function autorizarOrden(token: string): Promise<OrdenPublica> {
  const respuesta = await fetch(`${direccionApi()}/api/publico/ordenes-servicio/${token}/autorizar`, { method: "POST" });
  if (!respuesta.ok) throw new Error("No fue posible registrar la autorización. Inténtalo nuevamente.");
  return await respuesta.json() as OrdenPublica;
}
function moneda(valor: number) { return new Intl.NumberFormat("es-NI", { style: "currency", currency: "NIO" }).format(valor); }
function etiquetaEstado(estado: string) { return ({ Diagnostico: "Diagnóstico", PendienteAprobacion: "Por autorizar", Reparacion: "En reparación", ListaParaEntrega: "Lista para entregar" } as Record<string, string>)[estado] ?? estado; }
function etiquetaCombustible(porcentaje: number) { return porcentaje >= 88 ? "Tanque lleno" : porcentaje >= 63 ? "3/4 de tanque" : porcentaje >= 38 ? "Medio tanque" : porcentaje >= 13 ? "1/4 de tanque" : "Reserva"; }
function elementosEntregados(orden: OrdenPublica) { const elementos = [orden.dejaLlaves ? "Llaves" : "", orden.dejaDocumentos ? "Documentos" : ""].filter(Boolean); return elementos.length > 0 ? elementos.join(" y ") : "Ninguno registrado"; }
function etiquetaTipoDanio(tipo: string) { return tipo === "Rayon" ? "Rayón" : tipo; }
function etiquetaZona(zona: string) { return zona.replace(/([a-záéíóúñ])([A-ZÁÉÍÓÚÑ])/g, "$1 $2").toLocaleLowerCase("es").replace(/^./, (letra) => letra.toLocaleUpperCase("es")); }
