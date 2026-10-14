"use client";

import { useEffect, useRef, useState, type KeyboardEvent } from "react";
import { ChevronLeft, ChevronRight, RotateCcw, X, ZoomIn, ZoomOut } from "lucide-react";

export interface ImagenVisor { direccion: string; descripcion: string; }
export interface EstadoVisor { imagenes: ImagenVisor[]; indice: number; }

export function VisorFotografias({
  seleccion,
  alCerrar,
  alImagenInvalida,
}: {
  seleccion: EstadoVisor;
  alCerrar: () => void;
  alImagenInvalida?: (direccion: string) => void;
}) {
  const dialogo = useRef<HTMLDialogElement | null>(null);
  const botonCerrar = useRef<HTMLButtonElement | null>(null);
  const [indice, setIndice] = useState(seleccion.indice);
  const [zoom, setZoom] = useState(1);
  const imagen = seleccion.imagenes[indice];

  useEffect(() => {
    const controlAnterior = document.activeElement;
    const desplazamientoAnterior = document.body.style.overflow;
    const visor = dialogo.current;
    document.body.style.overflow = "hidden";
    visor?.showModal();
    botonCerrar.current?.focus();
    return () => {
      visor?.close();
      document.body.style.overflow = desplazamientoAnterior;
      if (controlAnterior instanceof HTMLElement && controlAnterior.isConnected) controlAnterior.focus();
    };
  }, []);

  function cambiarImagen(desplazamiento: number) {
    setZoom(1);
    setIndice((actual) => (actual + desplazamiento + seleccion.imagenes.length) % seleccion.imagenes.length);
  }

  function controlarTeclado(evento: KeyboardEvent<HTMLDialogElement>) {
    if (evento.key === "ArrowLeft" || evento.key === "ArrowRight") {
      evento.preventDefault();
      evento.stopPropagation();
      cambiarImagen(evento.key === "ArrowLeft" ? -1 : 1);
    }
  }

  return (
    <dialog
      ref={dialogo}
      className="fondo-visor-imagen"
      aria-label="Visor de fotografías"
      onKeyDown={controlarTeclado}
      onCancel={(evento) => { evento.preventDefault(); evento.stopPropagation(); alCerrar(); }}
    >
      <button type="button" className="cerrador-modal-fondo" tabIndex={-1} aria-label="Cerrar visor desde el fondo" onClick={alCerrar} />
      <section className="visor-imagen-publica">
        <header>
          <strong>{imagen.descripcion}</strong>
          <span>{indice + 1} de {seleccion.imagenes.length}</span>
          <button ref={botonCerrar} type="button" aria-label="Cerrar visor" onClick={alCerrar}><X size={24} /></button>
        </header>
        <div className="contenido-visor-imagen">
          <button type="button" className="navegacion-visor anterior" aria-label="Fotografía anterior" disabled={seleccion.imagenes.length < 2} onClick={() => cambiarImagen(-1)}><ChevronLeft size={30} /></button>
          <div className="lienzo-visor-imagen">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={imagen.direccion} alt={imagen.descripcion} style={{ width: `${zoom * 100}%` }} onError={() => { alImagenInvalida?.(imagen.direccion); alCerrar(); }} />
          </div>
          <button type="button" className="navegacion-visor siguiente" aria-label="Fotografía siguiente" disabled={seleccion.imagenes.length < 2} onClick={() => cambiarImagen(1)}><ChevronRight size={30} /></button>
        </div>
        <footer>
          <button type="button" aria-label="Alejar imagen" disabled={zoom <= 1} onClick={() => setZoom((actual) => Math.max(1, actual - .5))}><ZoomOut size={21} /></button>
          <button type="button" className="nivel-zoom" onClick={() => setZoom(1)}><RotateCcw size={18} />{Math.round(zoom * 100)}%</button>
          <button type="button" aria-label="Acercar imagen" disabled={zoom >= 3} onClick={() => setZoom((actual) => Math.min(3, actual + .5))}><ZoomIn size={21} /></button>
        </footer>
      </section>
    </dialog>
  );
}
