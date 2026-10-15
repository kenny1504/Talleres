"use client";

import { useEffect, useId, useMemo, useRef, useState } from "react";
import { Search } from "lucide-react";
import estilos from "./SelectorCliente.module.css";

interface ClienteSeleccionable {
  id: number;
  nombre: string;
  telefono: string;
}

function normalizarNombre(nombre: string) {
  return nombre.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLocaleLowerCase("es");
}

export function SelectorCliente({
  clientes,
  clienteId,
  alSeleccionar,
}: {
  clientes: ClienteSeleccionable[];
  clienteId: number;
  alSeleccionar: (clienteId: number) => void;
}) {
  const identificador = useId();
  const entrada = useRef<HTMLInputElement>(null);
  const [busqueda, setBusqueda] = useState<string | null>(null);
  const [abierto, setAbierto] = useState(false);
  const [indiceActivo, setIndiceActivo] = useState(-1);
  const clienteSeleccionado = clientes.find((cliente) => cliente.id === clienteId);
  const nombresClientes = useMemo(
    () => clientes.map((cliente) => ({ cliente, nombre: normalizarNombre(cliente.nombre) })),
    [clientes],
  );
  const coincidencias = useMemo(() => {
    const termino = normalizarNombre(clienteId ? "" : busqueda ?? "").trim();
    return nombresClientes.filter(({ nombre }) => nombre.includes(termino));
  }, [busqueda, clienteId, nombresClientes]);
  const opciones = coincidencias.slice(0, 50);
  const opcionActiva = abierto ? opciones[indiceActivo]?.cliente : undefined;
  const idOpcionActiva = opcionActiva ? `${identificador}-cliente-${opcionActiva.id}` : undefined;

  useEffect(() => {
    entrada.current?.setCustomValidity(clienteId ? "" : "Selecciona un cliente de la lista.");
  }, [clienteId]);

  useEffect(() => {
    if (idOpcionActiva) document.getElementById(idOpcionActiva)?.scrollIntoView({ block: "nearest" });
  }, [idOpcionActiva]);

  function seleccionarCliente(cliente: ClienteSeleccionable) {
    alSeleccionar(cliente.id);
    setBusqueda(null);
    setIndiceActivo(-1);
    entrada.current?.focus();
    setAbierto(false);
  }

  return (
    <div className={estilos.campo} onBlur={(evento) => {
      if (!evento.currentTarget.contains(evento.relatedTarget)) setAbierto(false);
    }}>
      <label htmlFor={identificador}>Cliente</label>
      <input type="hidden" name="clienteId" value={clienteId || ""} />
      <div className={estilos.control}>
        <Search size={18} aria-hidden="true" />
        <input
          ref={entrada}
          id={identificador}
          type="text"
          role="combobox"
          required
          autoComplete="off"
          aria-autocomplete="list"
          aria-expanded={abierto}
          aria-controls={`${identificador}-opciones`}
          aria-activedescendant={idOpcionActiva}
          value={clienteSeleccionado?.nombre ?? busqueda ?? ""}
          placeholder={clientes.length ? "Busca por nombre o selecciona un cliente" : "No hay clientes registrados"}
          onFocus={(evento) => {
            setAbierto(true);
            evento.currentTarget.select();
          }}
          onClick={() => setAbierto(true)}
          onChange={(evento) => {
            setBusqueda(evento.target.value);
            alSeleccionar(0);
            setIndiceActivo(-1);
            setAbierto(true);
          }}
          onKeyDown={(evento) => {
            if (evento.nativeEvent.isComposing) return;
            if (evento.key === "ArrowDown" || evento.key === "ArrowUp") {
              evento.preventDefault();
              setAbierto(true);
              setIndiceActivo((actual) => evento.key === "ArrowDown"
                ? Math.min(actual + 1, opciones.length - 1)
                : actual <= 0 ? opciones.length - 1 : actual - 1);
            } else if (evento.key === "Enter" && abierto) {
              evento.preventDefault();
              const cliente = opcionActiva ?? opciones[0]?.cliente;
              if (cliente) seleccionarCliente(cliente);
            } else if (evento.key === "Escape" && abierto) {
              evento.preventDefault();
              evento.stopPropagation();
              setAbierto(false);
            }
          }}
        />
      </div>
      {abierto && (
        <div className={estilos.desplegable}>
          <ul id={`${identificador}-opciones`} role="listbox" aria-label="Clientes" className={estilos.opciones}>
            {opciones.map(({ cliente }, indice) => (
              <li key={cliente.id} role="presentation">
                <button
                  id={`${identificador}-cliente-${cliente.id}`}
                  type="button"
                  role="option"
                  aria-selected={cliente.id === clienteId}
                  tabIndex={-1}
                  className={indice === indiceActivo ? estilos.opcionActiva : undefined}
                  onMouseDown={(evento) => evento.preventDefault()}
                  onClick={() => seleccionarCliente(cliente)}
                >
                  <span>{cliente.nombre}</span>
                  <small>{cliente.telefono}</small>
                </button>
              </li>
            ))}
          </ul>
          {(!coincidencias.length || coincidencias.length > opciones.length) && (
            <p className={estilos.mensaje} role="status">
              {!coincidencias.length
                ? "No se encontraron clientes."
                : "Mostrando los primeros 50 clientes. Escribe para buscar por nombre."}
            </p>
          )}
        </div>
      )}
    </div>
  );
}
