"use client";

import { useEffect, useId, useRef, useState, type FormEvent, type RefObject } from "react";
import { createPortal } from "react-dom";
import { Check, X } from "lucide-react";

export function ModalCatalogoVehiculo({
  tipo,
  nombreMarca,
  botonApertura,
  alGuardar,
  alCerrar,
}: {
  tipo: "marca" | "modelo";
  nombreMarca?: string;
  botonApertura: RefObject<HTMLButtonElement | null>;
  alGuardar: (nombre: string) => Promise<void>;
  alCerrar: () => void;
}) {
  const id = useId();
  const modal = useRef<HTMLElement | null>(null);
  const entradaNombre = useRef<HTMLInputElement | null>(null);
  const guardandoEnCurso = useRef(false);
  const [nombre, setNombre] = useState("");
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState("");
  const esMarca = tipo === "marca";
  const longitudMinima = esMarca ? 2 : 1;

  useEffect(() => {
    const controlAnterior = botonApertura.current ?? document.activeElement;
    const desbordamientoAnterior = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    entradaNombre.current?.focus();
    return () => {
      document.body.style.overflow = desbordamientoAnterior;
      if (controlAnterior instanceof HTMLElement && controlAnterior.isConnected) {
        controlAnterior.focus();
      }
    };
  }, [botonApertura]);

  useEffect(() => {
    if (error && !guardando) entradaNombre.current?.focus();
  }, [error, guardando]);

  useEffect(() => {
    function controlarTeclado(evento: KeyboardEvent) {
      if (evento.key === "Escape") {
        // El Escape de este modal no debe cerrar también el alta del vehículo.
        evento.preventDefault();
        evento.stopPropagation();
        if (!guardandoEnCurso.current) alCerrar();
      }
      if (evento.key !== "Tab") return;
      const controles = modal.current?.querySelectorAll<HTMLElement>(
        "button:not([disabled]), input:not([disabled])",
      );
      if (!controles?.length) {
        evento.preventDefault();
        return;
      }
      const primero = controles[0];
      const ultimo = controles[controles.length - 1];
      if (!modal.current?.contains(document.activeElement)) {
        evento.preventDefault();
        primero.focus();
      } else if (evento.shiftKey && document.activeElement === primero) {
        evento.preventDefault();
        ultimo.focus();
      } else if (!evento.shiftKey && document.activeElement === ultimo) {
        evento.preventDefault();
        primero.focus();
      }
    }
    window.addEventListener("keydown", controlarTeclado, true);
    return () => window.removeEventListener("keydown", controlarTeclado, true);
  }, [alCerrar]);

  async function guardar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    if (guardandoEnCurso.current || nombre.trim().length < longitudMinima) return;
    guardandoEnCurso.current = true;
    setGuardando(true);
    setError("");
    try {
      await alGuardar(nombre.trim());
      alCerrar();
    } catch (causa) {
      setError(causa instanceof Error ? causa.message : `No fue posible guardar ${esMarca ? "la marca" : "el modelo"}.`);
    } finally {
      guardandoEnCurso.current = false;
      setGuardando(false);
    }
  }

  return createPortal(
    <div className="fondo-modal-alta fondo-modal-catalogo">
      <button
        className="cerrador-modal-fondo"
        type="button"
        tabIndex={-1}
        disabled={guardando}
        aria-label="Cancelar registro"
        onClick={alCerrar}
      />
      <section
        ref={modal}
        className="modal-alta modal-catalogo-vehiculo"
        role="dialog"
        aria-modal="true"
        aria-labelledby={`${id}-titulo`}
        aria-describedby={`${id}-descripcion`}
      >
        <div className="cabecera-modal-alta">
          <div>
            <h2 id={`${id}-titulo`}>{esMarca ? "Nueva marca" : "Nuevo modelo"}</h2>
            <p id={`${id}-descripcion`}>Al guardar quedará {esMarca ? "seleccionada" : "seleccionado"} en el vehículo.</p>
          </div>
          <button className="boton-icono" type="button" disabled={guardando} onClick={alCerrar} aria-label="Cerrar registro">
            <X size={20} />
          </button>
        </div>
        <div className="contenido-modal-alta">
          <form className="formulario" onSubmit={guardar}>
            {!esMarca && <p className="marca-modelo-catalogo">Marca: <strong>{nombreMarca}</strong></p>}
            <label htmlFor={`${id}-nombre`}>
              {esMarca ? "Nombre de la marca" : "Nombre del modelo"}
              <input
                ref={entradaNombre}
                id={`${id}-nombre`}
                name="nombre"
                type="text"
                required
                minLength={longitudMinima}
                maxLength={80}
                autoComplete="off"
                placeholder={esMarca ? "Ej. Toyota" : "Ej. Hilux"}
                value={nombre}
                disabled={guardando}
                onChange={(evento) => {
                  setNombre(evento.target.value);
                  setError("");
                }}
                />
            </label>
            {error && <div className="estado-datos estado-datos-error" role="alert">{error}</div>}
            <div className="acciones-modal-catalogo">
              <button className="boton-secundario" type="button" disabled={guardando} onClick={alCerrar}>Cancelar</button>
              <button className="boton-primario" type="submit" disabled={guardando || nombre.trim().length < longitudMinima}>
                <Check size={18} />{esMarca ? "Guardar marca" : "Guardar modelo"}
              </button>
            </div>
          </form>
        </div>
      </section>
    </div>,
    document.body,
  );
}
