"use client";

import type { FormEvent, ReactNode } from "react";
import { useEffect, useMemo, useState } from "react";
import {
  ArrowLeft,
  Bell,
  Boxes,
  CalendarDays,
  Camera,
  CarFront,
  Check,
  ChevronRight,
  CircleCheck,
  ClipboardCheck,
  ClipboardList,
  Clock3,
  Fuel,
  LayoutDashboard,
  LogOut,
  MessageCircleMore,
  MoreHorizontal,
  ImagePlus,
  Pencil,
  Plus,
  RotateCcw,
  Search,
  Settings2,
  Sparkles,
  Trash2,
  TriangleAlert,
  UserRound,
  Users,
  Wrench,
  X,
  type LucideIcon,
} from "lucide-react";

type Vista = "inicio" | "ordenes" | "clientes" | "vehiculos" | "inventario";
type EstadoOrden =
  | "Recepción"
  | "Diagnóstico"
  | "Cotización"
  | "Por aprobar"
  | "Reparación"
  | "Control de calidad"
  | "Lista para entregar";

interface OrdenTaller {
  id: number;
  clienteId: number;
  vehiculoId: number;
  numero: string;
  cliente: string;
  vehiculo: string;
  placa: string;
  estado: EstadoOrden;
  motivo: string;
  tecnico: string;
  hora: string;
  progreso: number;
  color: string;
  prioridad?: boolean;
}

type ZonaVehiculo =
  | "frente"
  | "capo"
  | "parabrisas"
  | "techo"
  | "lateral-izquierdo"
  | "lateral-derecho"
  | "maletero"
  | "posterior";

type TipoDanio = "Rayón" | "Abolladura" | "Golpe" | "Vidrio" | "Luz";
type SeveridadDanio = "Leve" | "Moderado" | "Severo";

interface DanioVisual {
  id: string;
  zona: ZonaVehiculo;
  tipo: TipoDanio;
  severidad: SeveridadDanio;
  observacion: string;
}

interface InspeccionVisual {
  kilometraje: number;
  porcentajeCombustible: number;
  descripcionEstado: string;
  dejaLlaves: boolean;
  dejaDocumentos: boolean;
  danios: DanioVisual[];
}

interface OrdenServicioApi {
  id: number;
  numero: string;
  clienteId: number;
  nombreCliente: string;
  vehiculoId: number;
  placaVehiculo: string;
  estado: string;
  fechaIngreso: string;
  observaciones: string | null;
}

interface ClienteApi {
  id: number;
  nombre: string;
  documentoIdentidad: string;
  telefono: string;
  correo: string | null;
  direccion: string | null;
  activo: boolean;
  fechaCreacion: string;
}

interface VehiculoApi {
  id: number;
  clienteId: number;
  nombreCliente: string;
  placa: string;
  marca: string;
  modelo: string;
  anio: number;
  color: string | null;
  numeroVin: string | null;
  activo: boolean;
  fechaCreacion: string;
}

interface ClienteTaller {
  id: number;
  iniciales: string;
  nombre: string;
  documentoIdentidad: string;
  telefono: string;
  correo: string | null;
  direccion: string | null;
  activo: boolean;
  cantidadVehiculos: number;
  ordenActiva: string | null;
}

interface VehiculoTaller {
  id: number;
  clienteId: number;
  placa: string;
  marca: string;
  modelo: string;
  anio: number;
  color: string | null;
  numeroVin: string | null;
  nombre: string;
  detalle: string;
  cliente: string;
  activo: boolean;
}

interface DanioVehiculoApi {
  zona: string;
  tipo: string;
  severidad: string;
  observacion: string | null;
}

interface RecepcionVehiculoApi {
  kilometraje: number;
  porcentajeCombustible: number;
  descripcionEstado: string;
  dejaLlaves: boolean;
  dejaDocumentos: boolean;
  danios: DanioVehiculoApi[];
}

interface NavegacionItem {
  id: Vista;
  etiqueta: string;
  icono: LucideIcon;
}

interface TallerSesionApi {
  id: number;
  nombreLegal: string;
  nombreComercial: string | null;
  prefijoTelefono: number;
  telefono: string;
  ruc: string | null;
  correo: string | null;
  direccion: string | null;
  ciudad: string | null;
  barrio: string | null;
  calle: string | null;
  logo: string | null;
  horaApertura: string;
  horaCierre: string;
}
interface BodegaInventarioApi { id: number; nombre: string; esPrincipal: boolean; }
interface ArticuloInventarioApi { productoId: number; codigo: string; nombre: string; unidadMedida: string; existencia: number; costoUnitario?: number; }

interface SesionTallerApi {
  usuarioId: string;
  usuario: string;
  nombreUsuario: string;
  esSuperUsuario: boolean;
  taller: TallerSesionApi;
  talleresDisponibles: TallerSesionApi[];
}

interface TallerSincronizadoApi {
  empresaNovaId: number;
  nombreLegal: string;
  nombreComercial: string | null;
  activo: boolean;
}

const navegacion: NavegacionItem[] = [
  { id: "inicio", etiqueta: "Inicio", icono: LayoutDashboard },
  { id: "ordenes", etiqueta: "Órdenes", icono: ClipboardList },
  { id: "clientes", etiqueta: "Clientes", icono: Users },
  { id: "vehiculos", etiqueta: "Vehículos", icono: CarFront },
  { id: "inventario", etiqueta: "Inventario", icono: Boxes },
];

const zonasVehiculo: Array<{ id: ZonaVehiculo; etiqueta: string }> = [
  { id: "frente", etiqueta: "Frente" },
  { id: "capo", etiqueta: "Capó" },
  { id: "parabrisas", etiqueta: "Parabrisas" },
  { id: "techo", etiqueta: "Techo" },
  { id: "lateral-izquierdo", etiqueta: "Lateral izquierdo" },
  { id: "lateral-derecho", etiqueta: "Lateral derecho" },
  { id: "maletero", etiqueta: "Maletero" },
  { id: "posterior", etiqueta: "Posterior" },
];

export default function PaginaPrincipal() {
  const [sesion, setSesion] = useState<SesionTallerApi | null>();
  const [cargandoSesion, setCargandoSesion] = useState(true);
  const [cambiandoTaller, setCambiandoTaller] = useState(false);
  const [mostrarAdministracion, setMostrarAdministracion] = useState(false);
  const [vista, setVista] = useState<Vista>("inicio");
  const [ordenes, setOrdenes] = useState<OrdenTaller[]>([]);
  const [clientes, setClientes] = useState<ClienteTaller[]>([]);
  const [vehiculos, setVehiculos] = useState<VehiculoTaller[]>([]);
  const [cargandoDatos, setCargandoDatos] = useState(true);
  const [errorDatos, setErrorDatos] = useState("");
  const [guardandoOrden, setGuardandoOrden] = useState(false);
  const [guardandoCliente, setGuardandoCliente] = useState(false);
  const [guardandoVehiculo, setGuardandoVehiculo] = useState(false);
  const [busqueda, setBusqueda] = useState("");
  const [filtroEstado, setFiltroEstado] = useState("Todas");
  const [mostrarNuevaOrden, setMostrarNuevaOrden] = useState(false);
  const [mostrarNuevoCliente, setMostrarNuevoCliente] = useState(false);
  const [clienteEditando, setClienteEditando] = useState<ClienteTaller | null>(null);
  const [mostrarFormularioVehiculo, setMostrarFormularioVehiculo] = useState(false);
  const [vehiculoEditando, setVehiculoEditando] = useState<VehiculoTaller | null>(null);
  const [clienteInicialNuevaOrdenId, setClienteInicialNuevaOrdenId] = useState<number | null>(null);
  const [ordenDetalle, setOrdenDetalle] = useState<OrdenTaller | null>(null);
  const [mostrarRecepcion, setMostrarRecepcion] = useState(false);
  const [aviso, setAviso] = useState("");
  const [inspecciones, setInspecciones] = useState<Record<number, InspeccionVisual>>({});
  const procesoActivo =
    mostrarNuevaOrden || mostrarNuevoCliente || mostrarFormularioVehiculo || ordenDetalle !== null;

  useEffect(() => {
    const controlador = new AbortController();
    obtenerSesionApi(controlador.signal)
      .then(setSesion)
      .catch(() => {
        if (!controlador.signal.aborted) setSesion(null);
      })
      .finally(() => {
        if (!controlador.signal.aborted) setCargandoSesion(false);
      });
    return () => controlador.abort();
  }, []);

  useEffect(() => {
    if (!sesion) {
      return;
    }
    const controlador = new AbortController();
    cargarDatosApi(controlador.signal)
      .then((datos) => {
        setOrdenes(datos.ordenes);
        setClientes(datos.clientes);
        setVehiculos(datos.vehiculos);
        setErrorDatos("");
      })
      .catch(() => {
        if (controlador.signal.aborted) return;
        setOrdenes([]);
        setClientes([]);
        setVehiculos([]);
        setErrorDatos("No fue posible comunicarse con la API configurada.");
      })
      .finally(() => {
        if (!controlador.signal.aborted) setCargandoDatos(false);
      });
    return () => controlador.abort();
  }, [sesion]);

  useEffect(() => {
    if (!ordenDetalle || ordenDetalle.estado === "Recepción") return;
    const controlador = new AbortController();
    cargarInspeccionApi(ordenDetalle.id, controlador.signal)
      .then((inspeccion) => {
        if (inspeccion) {
          setInspecciones((actuales) => ({ ...actuales, [ordenDetalle.id]: inspeccion }));
        }
      })
      .catch(() => {
        mostrarAviso("No fue posible consultar la inspección en la API");
      });
    return () => controlador.abort();
  }, [ordenDetalle]);

  useEffect(() => {
    if (procesoActivo) window.scrollTo({ top: 0, behavior: "smooth" });
  }, [procesoActivo, mostrarRecepcion]);

  const ordenesFiltradas = useMemo(() => {
    const termino = busqueda.trim().toLocaleLowerCase("es");
    return ordenes.filter((orden) => {
      const coincideEstado = filtroEstado === "Todas" || orden.estado === filtroEstado;
      const coincideBusqueda =
        !termino ||
        [orden.numero, orden.cliente, orden.vehiculo, orden.placa]
          .join(" ")
          .toLocaleLowerCase("es")
          .includes(termino);
      return coincideEstado && coincideBusqueda;
    });
  }, [busqueda, filtroEstado, ordenes]);

  function navegar(nuevaVista: Vista) {
    setMostrarAdministracion(false);
    setMostrarNuevaOrden(false);
    setMostrarNuevoCliente(false);
    setClienteEditando(null);
    setMostrarFormularioVehiculo(false);
    setVehiculoEditando(null);
    setClienteInicialNuevaOrdenId(null);
    setOrdenDetalle(null);
    setMostrarRecepcion(false);
    setVista(nuevaVista);
    setBusqueda("");
  }

  function buscar(termino: string) {
    setBusqueda(termino);
    if (!termino) return;

    setMostrarNuevaOrden(false);
    setMostrarNuevoCliente(false);
    setClienteEditando(null);
    setMostrarFormularioVehiculo(false);
    setVehiculoEditando(null);
    setClienteInicialNuevaOrdenId(null);
    setOrdenDetalle(null);
    setMostrarRecepcion(false);
    setVista("ordenes");
  }

  function iniciarNuevaOrden(clienteId?: number) {
    setMostrarNuevoCliente(false);
    setClienteEditando(null);
    setMostrarFormularioVehiculo(false);
    setVehiculoEditando(null);
    setOrdenDetalle(null);
    setMostrarRecepcion(false);
    setClienteInicialNuevaOrdenId(clienteId ?? null);
    setMostrarNuevaOrden(true);
  }

  function abrirOrden(orden: OrdenTaller) {
    setMostrarNuevaOrden(false);
    setMostrarNuevoCliente(false);
    setClienteEditando(null);
    setMostrarFormularioVehiculo(false);
    setVehiculoEditando(null);
    setClienteInicialNuevaOrdenId(null);
    setMostrarRecepcion(false);
    setOrdenDetalle(orden);
  }

  function regresarDesdeProceso() {
    if (mostrarRecepcion) {
      setMostrarRecepcion(false);
      return;
    }

    setMostrarNuevaOrden(false);
    setMostrarNuevoCliente(false);
    setClienteEditando(null);
    setMostrarFormularioVehiculo(false);
    setVehiculoEditando(null);
    setClienteInicialNuevaOrdenId(null);
    setOrdenDetalle(null);
  }

  const etiquetaRegresoProceso = mostrarRecepcion
    ? "Volver a la orden"
    : vista === "inicio"
      ? "Volver al tablero"
      : `Volver a ${navegacion.find((item) => item.id === vista)?.etiqueta.toLocaleLowerCase("es") ?? "la sección"}`;

  function iniciarNuevoCliente() {
    setMostrarNuevaOrden(false);
    setMostrarFormularioVehiculo(false);
    setVehiculoEditando(null);
    setClienteInicialNuevaOrdenId(null);
    setOrdenDetalle(null);
    setMostrarRecepcion(false);
    setVista("clientes");
    setClienteEditando(null);
    setMostrarNuevoCliente(true);
  }

  function editarCliente(cliente: ClienteTaller) {
    iniciarNuevoCliente();
    setClienteEditando(cliente);
  }

  function abrirFormularioVehiculo(vehiculo?: VehiculoTaller) {
    setMostrarNuevaOrden(false);
    setMostrarNuevoCliente(false);
    setClienteEditando(null);
    setClienteInicialNuevaOrdenId(null);
    setOrdenDetalle(null);
    setMostrarRecepcion(false);
    setVista("vehiculos");
    setVehiculoEditando(vehiculo ?? null);
    setMostrarFormularioVehiculo(true);
  }

  async function crearOrden(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    setGuardandoOrden(true);
    try {
      const nuevaOrden = await crearOrdenApi({
        clienteId: Number(datos.get("clienteId")),
        vehiculoId: Number(datos.get("vehiculoId")),
        observaciones: String(datos.get("motivo")),
      });
      setOrdenes((actuales) => [nuevaOrden, ...actuales]);
      setMostrarNuevaOrden(false);
      setVista("ordenes");
      mostrarAviso(`Orden ${nuevaOrden.numero} creada y lista para recepción`);
    } catch (error) {
      mostrarAviso(
        error instanceof Error ? error.message : "No fue posible crear la orden en la API",
      );
    } finally {
      setGuardandoOrden(false);
    }
  }

  async function guardarCliente(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    setGuardandoCliente(true);

    try {
      const clienteApi = await guardarClienteApi(
        {
          nombre: String(datos.get("nombre")),
          documentoIdentidad: String(datos.get("documentoIdentidad")),
          telefono: String(datos.get("telefono")),
          correo: valorOpcionalFormulario(datos.get("correo")),
          direccion: valorOpcionalFormulario(datos.get("direccion")),
          activo: clienteEditando ? datos.has("activo") : true,
        },
        clienteEditando?.id,
      );
      const clienteGuardado = convertirClienteTaller(
        clienteApi,
        clienteEditando?.cantidadVehiculos ?? 0,
        clienteEditando?.ordenActiva ?? null,
      );

      setClientes((actuales) => {
        const siguientes = clienteEditando
          ? actuales.map((cliente) =>
              cliente.id === clienteGuardado.id ? clienteGuardado : cliente)
          : [...actuales, clienteGuardado];
        return siguientes.sort((a, b) => a.nombre.localeCompare(b.nombre, "es"));
      });
      setVehiculos((actuales) =>
        actuales.map((vehiculo) =>
          vehiculo.clienteId === clienteGuardado.id
            ? { ...vehiculo, cliente: clienteGuardado.nombre }
            : vehiculo,
        ),
      );
      setOrdenes((actuales) =>
        actuales.map((orden) =>
          orden.clienteId === clienteGuardado.id
            ? { ...orden, cliente: clienteGuardado.nombre }
            : orden,
        ),
      );
      setMostrarNuevoCliente(false);
      setClienteEditando(null);
      setVista("clientes");
      mostrarAviso(
        clienteEditando
          ? `Cliente ${clienteGuardado.nombre} actualizado correctamente`
          : `Cliente ${clienteGuardado.nombre} registrado correctamente`,
      );
    } catch (error) {
      mostrarAviso(
        error instanceof Error ? error.message : "No fue posible guardar el cliente",
      );
    } finally {
      setGuardandoCliente(false);
    }
  }

  async function guardarVehiculo(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    const propietarioAnteriorId = vehiculoEditando?.clienteId;
    setGuardandoVehiculo(true);

    try {
      const vehiculoApi = await guardarVehiculoApi(
        {
          clienteId: Number(datos.get("clienteId")),
          placa: String(datos.get("placa")),
          marca: String(datos.get("marca")),
          modelo: String(datos.get("modelo")),
          anio: Number(datos.get("anio")),
          color: valorOpcionalFormulario(datos.get("color")),
          numeroVin: valorOpcionalFormulario(datos.get("numeroVin")),
          activo: vehiculoEditando ? datos.has("activo") : true,
        },
        vehiculoEditando?.id,
      );
      const vehiculoGuardado = convertirVehiculoTaller(vehiculoApi);

      setVehiculos((actuales) => {
        const siguientes = vehiculoEditando
          ? actuales.map((vehiculo) =>
              vehiculo.id === vehiculoGuardado.id ? vehiculoGuardado : vehiculo)
          : [...actuales, vehiculoGuardado];
        return siguientes.sort((a, b) => a.placa.localeCompare(b.placa, "es"));
      });
      setClientes((actuales) =>
        actuales.map((cliente) => {
          if (!vehiculoEditando && cliente.id === vehiculoGuardado.clienteId) {
            return { ...cliente, cantidadVehiculos: cliente.cantidadVehiculos + 1 };
          }
          if (propietarioAnteriorId !== vehiculoGuardado.clienteId) {
            if (cliente.id === propietarioAnteriorId) {
              return { ...cliente, cantidadVehiculos: Math.max(0, cliente.cantidadVehiculos - 1) };
            }
            if (cliente.id === vehiculoGuardado.clienteId) {
              return { ...cliente, cantidadVehiculos: cliente.cantidadVehiculos + 1 };
            }
          }
          return cliente;
        }),
      );
      setOrdenes((actuales) =>
        actuales.map((orden) =>
          orden.vehiculoId === vehiculoGuardado.id
            ? {
                ...orden,
                vehiculo: `Vehículo · ${vehiculoGuardado.placa}`,
                placa: vehiculoGuardado.placa,
              }
            : orden,
        ),
      );
      setMostrarFormularioVehiculo(false);
      setVehiculoEditando(null);
      mostrarAviso(
        vehiculoEditando
          ? `Vehículo ${vehiculoGuardado.placa} actualizado correctamente`
          : `Vehículo ${vehiculoGuardado.placa} registrado correctamente`,
      );
    } catch (error) {
      mostrarAviso(
        error instanceof Error ? error.message : "No fue posible guardar el vehículo",
      );
    } finally {
      setGuardandoVehiculo(false);
    }
  }

  async function registrarRecepcion(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    if (!ordenDetalle) return;
    const datos = new FormData(evento.currentTarget);
    const danios = JSON.parse(String(datos.get("danios") || "[]")) as DanioVisual[];
    const esActualizacion = ordenDetalle.estado !== "Recepción";
    const inspeccion: InspeccionVisual = {
      kilometraje: Number(datos.get("kilometraje")),
      porcentajeCombustible: Number(datos.get("combustible")),
      descripcionEstado: String(datos.get("estado")),
      dejaLlaves: datos.has("dejaLlaves"),
      dejaDocumentos: datos.has("dejaDocumentos"),
      danios,
    };

    try {
      await guardarRecepcionApi(ordenDetalle.id, inspeccion, esActualizacion);
    } catch {
      mostrarAviso("No fue posible guardar la recepción en la API");
      return;
    }
    setOrdenes((actuales) =>
      actuales.map((orden) =>
        orden.id === ordenDetalle.id
          ? esActualizacion
            ? orden
            : { ...orden, estado: "Diagnóstico", progreso: 28, tecnico: "Por asignar" }
          : orden,
      ),
    );
    setInspecciones((actuales) => ({ ...actuales, [ordenDetalle.id]: inspeccion }));
    setMostrarRecepcion(false);
    setOrdenDetalle(null);
    mostrarAviso(
      esActualizacion
        ? "Inspección actualizada correctamente"
        : "Recepción guardada; la orden pasó a diagnóstico",
    );
  }

  function mostrarAviso(mensaje: string) {
    setAviso(mensaje);
    window.setTimeout(() => setAviso(""), 3600);
  }

  async function cambiarTaller(empresaNovaId: number) {
    if (!sesion || empresaNovaId === sesion.taller.id) return;
    setCambiandoTaller(true);
    try {
      const nuevaSesion = await seleccionarTallerApi(empresaNovaId);
      navegar("inicio");
      setOrdenes([]);
      setClientes([]);
      setVehiculos([]);
      setCargandoDatos(true);
      setSesion(nuevaSesion);
    } catch (error) {
      mostrarAviso(
        error instanceof Error ? error.message : "No fue posible cambiar de taller",
      );
    } finally {
      setCambiandoTaller(false);
    }
  }

  async function cerrarSesion() {
    await cerrarSesionApi();
    setSesion(null);
    setOrdenes([]);
    setClientes([]);
    setVehiculos([]);
    setMostrarAdministracion(false);
  }

  if (cargandoSesion) {
    return (
      <main className="pantalla-acceso pantalla-cargando" role="status">
        <span className="marca-simbolo">T</span>
        <Cargador mensaje="Preparando Talleres…" />
      </main>
    );
  }

  if (!sesion) {
    return <PantallaInicioSesion alIngresar={setSesion} />;
  }

  return (
    <div className="aplicacion">
      <BarraLateral
        vista={vista}
        nombreTaller={sesion.taller.nombreComercial || sesion.taller.nombreLegal}
        alNavegar={navegar}
      />

      <div className={`superficie ${procesoActivo ? "proceso-activo" : ""}`}>
        <header className="barra-superior">
          <div className="marca-compacta">
            <span className="marca-simbolo">T</span>
            <span>{sesion.taller.nombreComercial || sesion.taller.nombreLegal}</span>
          </div>

          <label className="buscador-global">
            <Search aria-hidden="true" size={21} />
            <span className="solo-lectores">Buscar</span>
            <input
              value={busqueda}
              onChange={(evento) => buscar(evento.target.value)}
              placeholder="Buscar orden, cliente o placa"
            />
            <kbd>⌘ K</kbd>
          </label>

          <div className="acciones-superiores">
            <button className="boton-icono" aria-label="Ver notificaciones">
              <Bell size={22} />
              <span className="punto-notificacion" />
            </button>
            {sesion.esSuperUsuario && (
              <button
                className="boton-icono"
                onClick={() => setMostrarAdministracion((actual) => !actual)}
                aria-label="Administrar talleres sincronizados"
                aria-pressed={mostrarAdministracion}
              >
                <Settings2 size={21} />
              </button>
            )}
            {sesion.esSuperUsuario && (
              <label className="selector-taller">
                <span className="solo-lectores">Taller activo</span>
                <select
                  value={sesion.taller.id}
                  disabled={cambiandoTaller}
                  onChange={(evento) => cambiarTaller(Number(evento.target.value))}
                >
                  {sesion.talleresDisponibles.map((taller) => (
                    <option key={taller.id} value={taller.id}>{taller.nombreLegal}</option>
                  ))}
                </select>
              </label>
            )}
            <div className="usuario">
              <span className="avatar">{obtenerIniciales(sesion.nombreUsuario)}</span>
              <span className="usuario-texto">
                <strong>{sesion.nombreUsuario}</strong>
                <small>{sesion.esSuperUsuario ? "Superusuario" : "Equipo del taller"}</small>
              </span>
            </div>
            <button className="boton-icono" onClick={cerrarSesion} aria-label="Cerrar sesión">
              <LogOut size={21} />
            </button>
          </div>
        </header>

        <main className={`contenido-principal ${procesoActivo ? "contenido-proceso" : ""}`}>
          {!procesoActivo && cargandoDatos && (
            <div className="estado-datos" role="status">
              <Cargador mensaje="Consultando datos de la API…" icono={<Clock3 size={20} />} />
            </div>
          )}
          {!procesoActivo && errorDatos && (
            <div className="estado-datos estado-datos-error" role="alert">
              <TriangleAlert size={20} />
              <span><strong>Datos no disponibles.</strong> {errorDatos}</span>
            </div>
          )}
          {mostrarAdministracion ? (
            <VistaAdministracion
              sesion={sesion}
              alActualizarSesion={setSesion}
              alCerrar={() => setMostrarAdministracion(false)}
            />
          ) : mostrarNuevaOrden ? (
            <PaginaProceso
              titulo="Nueva orden de servicio"
              descripcion="Registra el vehículo y el motivo de ingreso sin salir del área de trabajo."
              alRegresar={regresarDesdeProceso}
              etiquetaRegreso={etiquetaRegresoProceso}
            >
              <div className="contenedor-formulario-orden">
                <FormularioNuevaOrden
                  clientes={clientes}
                  vehiculos={vehiculos}
                  clienteInicialId={clienteInicialNuevaOrdenId}
                  guardando={guardandoOrden}
                  alEnviar={crearOrden}
                />
              </div>
            </PaginaProceso>
          ) : mostrarNuevoCliente ? (
            <PaginaProceso
              titulo={clienteEditando ? "Actualizar cliente" : "Nuevo cliente"}
              descripcion={clienteEditando
                ? "Corrige los datos de contacto y el estado del cliente."
                : "Registra los datos de contacto para asociar vehículos y órdenes de servicio."}
              alRegresar={regresarDesdeProceso}
              etiquetaRegreso="Volver a clientes"
            >
              <div className="contenedor-formulario-orden">
                <FormularioCliente
                  cliente={clienteEditando}
                  guardando={guardandoCliente}
                  alEnviar={guardarCliente}
                />
              </div>
            </PaginaProceso>
          ) : mostrarFormularioVehiculo ? (
            <PaginaProceso
              titulo={vehiculoEditando ? "Actualizar vehículo" : "Nuevo vehículo"}
              descripcion="Asocia el vehículo con su propietario y registra sus datos de identificación."
              alRegresar={regresarDesdeProceso}
              etiquetaRegreso="Volver a vehículos"
            >
              <div className="contenedor-formulario-orden">
                <FormularioVehiculo
                  clientes={clientes}
                  vehiculo={vehiculoEditando}
                  guardando={guardandoVehiculo}
                  alEnviar={guardarVehiculo}
                />
              </div>
            </PaginaProceso>
          ) : ordenDetalle ? (
            <PaginaProceso
              titulo={mostrarRecepcion
                ? ordenDetalle.estado === "Recepción"
                  ? "Recepción e inspección"
                  : "Editar inspección"
                : ordenDetalle.numero}
              descripcion={mostrarRecepcion
                ? `${ordenDetalle.vehiculo} · ${ordenDetalle.placa}`
                : `${ordenDetalle.cliente} · ${ordenDetalle.vehiculo}`}
              alRegresar={regresarDesdeProceso}
              etiquetaRegreso={etiquetaRegresoProceso}
            >
              {mostrarRecepcion ? (
                <FormularioRecepcion
                  orden={ordenDetalle}
                  inspeccionInicial={inspecciones[ordenDetalle.id]}
                  alEnviar={registrarRecepcion}
                />
              ) : (
                <DetalleOrden
                  orden={ordenDetalle}
                  inspeccion={inspecciones[ordenDetalle.id]}
                  alRecibir={() => setMostrarRecepcion(true)}
                  alNotificar={() => mostrarAviso("Actualización enviada a WhatsApp")}
                />
              )}
            </PaginaProceso>
          ) : vista === "inicio" ? (
            <VistaInicio
              ordenes={ordenes}
              nombreUsuario={sesion.nombreUsuario}
              taller={sesion.taller}
              cargando={cargandoDatos}
              datosDisponibles={!errorDatos}
              alCrearOrden={() => iniciarNuevaOrden()}
              alAbrirOrden={abrirOrden}
              alVerOrdenes={() => navegar("ordenes")}
            />
          ) : vista === "ordenes" ? (
            <VistaOrdenes
              ordenes={ordenesFiltradas}
              cargando={cargandoDatos}
              datosDisponibles={!errorDatos}
              filtro={filtroEstado}
              alFiltrar={setFiltroEstado}
              alCrearOrden={() => iniciarNuevaOrden()}
              alAbrirOrden={abrirOrden}
            />
          ) : vista === "clientes" ? (
            <VistaClientes
              clientes={clientes}
              cargando={cargandoDatos}
              datosDisponibles={!errorDatos}
              alCrearCliente={iniciarNuevoCliente}
              alEditarCliente={editarCliente}
              alCrearOrden={iniciarNuevaOrden}
            />
          ) : vista === "vehiculos" ? (
            <VistaVehiculos
              vehiculos={vehiculos}
              cargando={cargandoDatos}
              datosDisponibles={!errorDatos}
              alCrear={() => abrirFormularioVehiculo()}
              alEditar={abrirFormularioVehiculo}
            />
          ) : (
            <VistaInventario taller={sesion.taller} />
          )}
        </main>

        <NavegacionInferior vista={vista} alNavegar={navegar} />
      </div>

      {aviso && (
        <div className="aviso" role="status">
          <CircleCheck size={21} />
          {aviso}
        </div>
      )}
    </div>
  );
}

function Cargador({ mensaje, icono }: { mensaje: string; icono?: ReactNode }) {
  return <span className="estado-carga"><span className="spinner-carga" aria-hidden="true" />{icono}{mensaje}</span>;
}

function PantallaInicioSesion({
  alIngresar,
}: {
  alIngresar: (sesion: SesionTallerApi) => void;
}) {
  const [enviando, setEnviando] = useState(false);
  const [proveedorEnviando, setProveedorEnviando] = useState<string | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    const mensaje = new URLSearchParams(window.location.search).get("error");
    if (!mensaje) return;
    const identificador = window.setTimeout(
      () => setError("No fue posible completar el inicio de sesión externo."),
      0,
    );
    return () => window.clearTimeout(identificador);
  }, []);

  async function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    setEnviando(true);
    setError("");
    try {
      const sesion = await iniciarSesionApi({
        usuario: String(datos.get("usuario")),
        contrasena: String(datos.get("contrasena")),
        recordarme: datos.has("recordarme"),
      });
      alIngresar(sesion);
    } catch (excepcion) {
      setError(
        excepcion instanceof Error
          ? excepcion.message
          : "No fue posible iniciar sesión.",
      );
    } finally {
      setEnviando(false);
    }
  }

  return (
    <main className="pantalla-acceso">
      <section className="presentacion-acceso" aria-label="Bienvenida a Talleres">
        <div className="marca marca-acceso">
          <span className="marca-simbolo">T</span>
          <span>Talleres</span>
        </div>
        <div>
          <span className="sobrelinea">Integrado con SMART TPV NOVA</span>
          <h1>La operación del taller, en un solo lugar.</h1>
          <p>Ingresa con tu cuenta de SMART TPV NOVA. El sistema abrirá únicamente el taller que tienes autorizado.</p>
        </div>
        <small>Acceso seguro · Información aislada por empresa</small>
      </section>

      <section className="contenedor-login">
        <form className="formulario-login" onSubmit={enviar}>
          <div>
            <span className="sobrelinea">Bienvenido</span>
            <h2>Iniciar sesión</h2>
            <p>Utiliza las mismas credenciales de SMART TPV NOVA.</p>
          </div>

          {error && <div className="mensaje-acceso-error" role="alert"><TriangleAlert size={20} />{error}</div>}

          <label>
            Usuario
            <input
              name="usuario"
              type="text"
              autoComplete="username"
              required
              maxLength={256}
            />
          </label>
          <label>
            Contraseña
            <input
              name="contrasena"
              type="password"
              autoComplete="current-password"
              required
              maxLength={256}
            />
          </label>
          <label className="opcion-recordarme">
            <input name="recordarme" type="checkbox" />
            Mantener mi sesión iniciada
          </label>
          <button className="boton-primario boton-ancho" disabled={enviando}>
            {enviando ? <Cargador mensaje="Validando acceso…" /> : "Ingresar al taller"}
          </button>
          <div className="separador-login"><span>o continúa con</span></div>
          <div className="botones-proveedor-login">
            <button
              type="button"
              className={`boton-proveedor-login ${proveedorEnviando ? "boton-cargando" : ""}`}
              disabled={Boolean(proveedorEnviando)}
              onClick={() => { setProveedorEnviando("google"); window.location.assign(`${obtenerDireccionApi()}/api/autenticacion/externo/google`); }}
            >
              {proveedorEnviando === "google" ? <Cargador mensaje="Conectando…" /> : "Continuar con Google"}
            </button>
            <button
              type="button"
              className={`boton-proveedor-login ${proveedorEnviando ? "boton-cargando" : ""}`}
              disabled={Boolean(proveedorEnviando)}
              onClick={() => { setProveedorEnviando("microsoft"); window.location.assign(`${obtenerDireccionApi()}/api/autenticacion/externo/microsoft`); }}
            >
              {proveedorEnviando === "microsoft" ? <Cargador mensaje="Conectando…" /> : "Continuar con Microsoft"}
            </button>
          </div>
        </form>
      </section>
    </main>
  );
}

function VistaAdministracion({
  sesion,
  alActualizarSesion,
  alCerrar,
}: {
  sesion: SesionTallerApi;
  alActualizarSesion: (sesion: SesionTallerApi) => void;
  alCerrar: () => void;
}) {
  const [talleres, setTalleres] = useState<TallerSincronizadoApi[]>([]);
  const [empresaNovaId, setEmpresaNovaId] = useState("");
  const [cargando, setCargando] = useState(true);
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState("");

  async function cargar() {
    setCargando(true);
    try {
      setTalleres(await listarTalleresSincronizadosApi());
      setError("");
    } catch (excepcion) {
      setError(excepcion instanceof Error ? excepcion.message : "No fue posible consultar los talleres.");
    } finally {
      setCargando(false);
    }
  }

  useEffect(() => {
    const identificador = window.setTimeout(() => { void cargar(); }, 0);
    return () => window.clearTimeout(identificador);
  }, []);

  async function agregar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    setGuardando(true);
    try {
      await agregarTallerSincronizadoApi(Number(empresaNovaId));
      setEmpresaNovaId("");
      await cargar();
      alActualizarSesion(await obtenerSesionApi(new AbortController().signal));
    } catch (excepcion) {
      setError(excepcion instanceof Error ? excepcion.message : "No fue posible agregar el taller.");
    } finally {
      setGuardando(false);
    }
  }

  async function retirar(id: number) {
    if (!window.confirm("¿Retirar este taller de la sincronización?")) return;
    setGuardando(true);
    try {
      await retirarTallerSincronizadoApi(id);
      await cargar();
      alActualizarSesion(await obtenerSesionApi(new AbortController().signal));
    } catch (excepcion) {
      setError(excepcion instanceof Error ? excepcion.message : "No fue posible retirar el taller.");
    } finally {
      setGuardando(false);
    }
  }

  return (
    <section className="pagina-administracion">
      <div className="encabezado-pagina">
        <div>
          <span className="sobrelinea">Administración · Superusuario</span>
          <h1>Talleres sincronizados</h1>
          <p>Define qué empresas de SMART TPV NOVA pueden operar en Talleres.</p>
        </div>
        <button className="boton-secundario" onClick={alCerrar}>Volver al tablero</button>
      </div>

      <div className="panel panel-administracion">
        <form className="formulario-agregar-taller" onSubmit={agregar}>
          <label>
            Id de empresa en SMART TPV NOVA
            <input
              type="number"
              min="1"
              required
              value={empresaNovaId}
              onChange={(evento) => setEmpresaNovaId(evento.target.value)}
              placeholder="Ej. 3071"
            />
          </label>
          <button className="boton-primario" disabled={guardando}>{guardando ? <Cargador mensaje="Agregando…" /> : "Agregar taller"}</button>
        </form>
        {error && <div className="estado-datos estado-datos-error" role="alert"><TriangleAlert size={20} />{error}</div>}
        {cargando ? <div className="estado-datos">Consultando empresas configuradas…</div> : (
          <div className="lista-talleres-admin">
            {talleres.map((taller) => (
              <article key={taller.empresaNovaId} className={!taller.activo ? "taller-retirado" : ""}>
                <div>
                  <span className="sobrelinea">Empresa {taller.empresaNovaId}</span>
                  <strong>{taller.nombreComercial || taller.nombreLegal}</strong>
                  <small>{taller.activo ? "Disponible para iniciar sesión" : "Retirado de la sincronización"}</small>
                </div>
                {taller.activo && <button className="boton-secundario" disabled={guardando} onClick={() => retirar(taller.empresaNovaId)}>{guardando ? <Cargador mensaje="Retirando…" /> : "Retirar"}</button>}
              </article>
            ))}
          </div>
        )}
      </div>
      <p className="nota-administracion">El usuario actual: {sesion.nombreUsuario}. La autorización se vuelve a comprobar en SMART TPV NOVA en cada operación.</p>
    </section>
  );
}

function BarraLateral({
  vista,
  nombreTaller,
  alNavegar,
}: {
  vista: Vista;
  nombreTaller: string;
  alNavegar: (vista: Vista) => void;
}) {
  return (
    <aside className="barra-lateral">
      <button className="marca" onClick={() => alNavegar("inicio")} aria-label="Ir al inicio">
        <span className="marca-simbolo">T</span>
        <span className="marca-nombre">{nombreTaller}</span>
      </button>

      <nav className="navegacion-principal" aria-label="Navegación principal">
        {navegacion.map((item) => {
          const Icono = item.icono;
          return (
            <button
              key={item.id}
              className={vista === item.id ? "navegacion-activa" : ""}
              onClick={() => alNavegar(item.id)}
            >
              <Icono size={23} strokeWidth={1.9} />
              <span>{item.etiqueta}</span>
            </button>
          );
        })}
      </nav>

      <div className="estado-sede">
        <span className="estado-en-linea" />
        <div>
          <strong>{nombreTaller}</strong>
          <small>Operación en línea</small>
        </div>
        <ChevronRight size={18} />
      </div>
    </aside>
  );
}

function NavegacionInferior({ vista, alNavegar }: { vista: Vista; alNavegar: (vista: Vista) => void }) {
  return (
    <nav className="navegacion-inferior" aria-label="Navegación para tablet vertical">
      {navegacion.slice(0, 4).map((item) => {
        const Icono = item.icono;
        return (
          <button
            key={item.id}
            className={vista === item.id ? "navegacion-activa" : ""}
            onClick={() => alNavegar(item.id)}
          >
            <Icono size={22} />
            <span>{item.etiqueta}</span>
          </button>
        );
      })}
      <button
        className={vista === "inventario" ? "navegacion-activa" : ""}
        onClick={() => alNavegar("inventario")}
      >
        <MoreHorizontal size={22} />
        <span>Más</span>
      </button>
    </nav>
  );
}

function VistaInicio({
  ordenes,
  nombreUsuario,
  taller,
  cargando,
  datosDisponibles,
  alCrearOrden,
  alAbrirOrden,
  alVerOrdenes,
}: {
  ordenes: OrdenTaller[];
  nombreUsuario: string;
  taller: TallerSesionApi;
  cargando: boolean;
  datosDisponibles: boolean;
  alCrearOrden: () => void;
  alAbrirOrden: (orden: OrdenTaller) => void;
  alVerOrdenes: () => void;
}) {
  const ordenesEnTaller = ordenes.filter((orden) => orden.estado !== "Lista para entregar");
  const ordenesPorAprobar = ordenes.filter((orden) => orden.estado === "Por aprobar");
  const ordenesListas = ordenes.filter((orden) => orden.estado === "Lista para entregar");
  const ordenesEnRecepcion = ordenes.filter((orden) => orden.estado === "Recepción");
  const fechaActual = new Intl.DateTimeFormat("es-NI", {
    weekday: "long",
    day: "numeric",
    month: "long",
  }).format(new Date());
  const valorMetrica = (cantidad: number) =>
    cargando ? "…" : datosDisponibles ? String(cantidad) : "—";

  return (
    <>
      <section className="encabezado-pagina encabezado-inicio">
        <div>
          <span className="sobrelinea">{fechaActual}</span>
          <h1>Buen día, {nombreUsuario.split(" ")[0]}</h1>
          <p>
            {cargando
              ? "Consultando la operación actual del taller."
              : datosDisponibles
                ? `${ordenesPorAprobar.length} órdenes necesitan aprobación.`
                : "La operación no puede consultarse en este momento."}
          </p>
        </div>
        <button className="boton-primario" onClick={alCrearOrden}>
          <Plus size={21} />
          Nueva orden
        </button>
      </section>

      <section className="panel informacion-taller" aria-label="Información del taller activo">
        <div className="titulo-panel">
          <div>
            <span className="sobrelinea">Taller activo · Empresa {taller.id}</span>
            <h2>{taller.nombreComercial || taller.nombreLegal}</h2>
          </div>
          <span className="estado-operativo">Operación en línea</span>
        </div>
        <div className="datos-taller">
          <span><strong>Razón social</strong><em>{taller.nombreLegal}</em></span>
          <span><strong>RUC</strong><em>{taller.ruc || "No registrado"}</em></span>
          <span><strong>Teléfono</strong><em>+{taller.prefijoTelefono} {taller.telefono}</em></span>
          <span><strong>Correo</strong><em>{taller.correo || "No registrado"}</em></span>
          <span className="dato-taller-ancho"><strong>Dirección</strong><em>{[taller.direccion, taller.ciudad, taller.barrio, taller.calle].filter(Boolean).join(", ") || "No registrada"}</em></span>
          <span><strong>Horario</strong><em>{formatearHora(taller.horaApertura)} – {formatearHora(taller.horaCierre)}</em></span>
          {taller.logo && <span><strong>Identidad visual</strong><a href={taller.logo} target="_blank" rel="noreferrer">Abrir logo registrado</a></span>}
        </div>
      </section>

      <section className="metricas" aria-label="Resumen del taller">
        <TarjetaMetrica etiqueta="En taller" valor={valorMetrica(ordenesEnTaller.length)} detalle={datosDisponibles ? "Órdenes activas" : "Sin conexión"} icono={Wrench} tono="verde" />
        <TarjetaMetrica etiqueta="Por aprobar" valor={valorMetrica(ordenesPorAprobar.length)} detalle={datosDisponibles ? "Pendientes del cliente" : "Sin conexión"} icono={Clock3} tono="ambar" />
        <TarjetaMetrica etiqueta="Listas" valor={valorMetrica(ordenesListas.length)} detalle={datosDisponibles ? "Para entregar" : "Sin conexión"} icono={CircleCheck} tono="azul" />
        <TarjetaMetrica etiqueta="Recepción" valor={valorMetrica(ordenesEnRecepcion.length)} detalle={datosDisponibles ? "Por inspeccionar" : "Sin conexión"} icono={ClipboardCheck} tono="rojo" />
      </section>

      <div className="tablero-principal">
        <section className="panel panel-operacion">
          <div className="titulo-panel">
            <div>
              <span className="sobrelinea">Operación en vivo</span>
              <h2>Bahías del taller</h2>
            </div>
            <button className="boton-texto" onClick={alVerOrdenes}>
              Ver todas <ChevronRight size={18} />
            </button>
          </div>

          <div className="lista-bahias">
            {ordenes.slice(0, 4).map((orden, indice) => (
              <button className="tarjeta-bahia" key={orden.id} onClick={() => alAbrirOrden(orden)}>
                <span className="numero-bahia">B{indice + 1}</span>
                <span className="datos-bahia">
                  <span className="fila-orden">
                    <strong>{orden.vehiculo.split(" · ")[0]}</strong>
                    <small>{orden.numero}</small>
                  </span>
                  <span className="detalle-orden">{orden.placa} · {orden.tecnico}</span>
                  <span className="progreso" aria-label={`${orden.progreso}% completado`}>
                    <span style={{ width: `${orden.progreso}%`, background: orden.color }} />
                  </span>
                </span>
                <span className={`etiqueta-estado estado-${normalizarClase(orden.estado)}`}>
                  {orden.estado}
                </span>
                <ChevronRight className="flecha-tarjeta" size={20} />
              </button>
            ))}
            {!cargando && ordenes.length === 0 && (
              <EstadoVacio
                icono={ClipboardList}
                titulo={datosDisponibles ? "No hay órdenes registradas" : "Órdenes no disponibles"}
                detalle={datosDisponibles
                  ? "La API respondió sin órdenes para la empresa activa."
                  : "No fue posible consultar las órdenes en la API."}
              />
            )}
          </div>
        </section>

        <aside className="panel panel-agenda">
          <div className="titulo-panel">
            <div>
              <span className="sobrelinea">Próximamente</span>
              <h2>Agenda de hoy</h2>
            </div>
            <CalendarDays size={22} />
          </div>

          <EstadoVacio
            icono={CalendarDays}
            titulo="Agenda sin datos"
            detalle="La agenda todavía no dispone de un contrato HTTP persistido."
          />
        </aside>
      </div>

      <section className="panel decisiones">
        <div className="titulo-panel">
          <div>
            <span className="sobrelinea">Requieren acción</span>
            <h2>Decisiones pendientes</h2>
          </div>
          <span className="contador">{ordenesPorAprobar.length}</span>
        </div>
        <div className="rejilla-decisiones">
          {ordenesPorAprobar.map((orden) => (
            <button key={orden.id} onClick={() => alAbrirOrden(orden)}>
              <span className="icono-decision ambar"><ClipboardCheck size={22} /></span>
              <span><strong>{orden.numero}</strong><small>Esperando aprobación de {orden.cliente}</small></span>
              <ChevronRight size={19} />
            </button>
          ))}
          {!cargando && ordenesPorAprobar.length === 0 && (
            <EstadoVacio
              icono={CircleCheck}
              titulo="Sin decisiones pendientes"
              detalle="No hay órdenes esperando aprobación."
            />
          )}
        </div>
      </section>
    </>
  );
}

function TarjetaMetrica({
  etiqueta,
  valor,
  detalle,
  icono: Icono,
  tono,
}: {
  etiqueta: string;
  valor: string;
  detalle: string;
  icono: LucideIcon;
  tono: string;
}) {
  return (
    <article className="tarjeta-metrica">
      <span className={`icono-metrica ${tono}`}><Icono size={22} /></span>
      <div><span>{etiqueta}</span><strong>{valor}</strong><small>{detalle}</small></div>
    </article>
  );
}

function VistaOrdenes({
  ordenes,
  cargando,
  datosDisponibles,
  filtro,
  alFiltrar,
  alCrearOrden,
  alAbrirOrden,
}: {
  ordenes: OrdenTaller[];
  cargando: boolean;
  datosDisponibles: boolean;
  filtro: string;
  alFiltrar: (filtro: string) => void;
  alCrearOrden: () => void;
  alAbrirOrden: (orden: OrdenTaller) => void;
}) {
  const filtros = ["Todas", "Recepción", "Diagnóstico", "Por aprobar", "Reparación", "Lista para entregar"];
  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Operación</span><h1>Órdenes de servicio</h1><p>Sigue cada vehículo desde la recepción hasta la entrega.</p></div>
        <button className="boton-primario" onClick={alCrearOrden}><Plus size={21} />Nueva orden</button>
      </section>
      <div className="filtros" role="group" aria-label="Filtrar órdenes por estado">
        {filtros.map((item) => <button key={item} className={filtro === item ? "activo" : ""} onClick={() => alFiltrar(item)}>{item}</button>)}
      </div>
      <section className="panel tabla-ordenes">
        <div className="cabecera-tabla"><span>Orden y vehículo</span><span>Cliente</span><span>Estado</span><span>Responsable</span><span /></div>
        {ordenes.map((orden) => (
          <button className="fila-tabla" key={orden.id} onClick={() => alAbrirOrden(orden)}>
            <span className="celda-vehiculo"><span className="mini-auto"><CarFront size={21} /></span><span><strong>{orden.vehiculo}</strong><small>{orden.numero} · {orden.placa}</small></span></span>
            <span className="cliente-tabla"><strong>{orden.cliente}</strong><small>{orden.motivo}</small></span>
            <span><span className={`etiqueta-estado estado-${normalizarClase(orden.estado)}`}>{orden.estado}</span></span>
            <span><strong>{orden.tecnico}</strong><small>Desde {orden.hora}</small></span>
            <ChevronRight size={20} />
          </button>
        ))}
        {!cargando && ordenes.length === 0 && (
          <EstadoVacio
            icono={Search}
            titulo={datosDisponibles ? "No hay órdenes para mostrar" : "Órdenes no disponibles"}
            detalle={datosDisponibles
              ? "La API no devolvió registros o ningún registro coincide con el filtro."
              : "No fue posible consultar las órdenes en la API."}
          />
        )}
      </section>
    </>
  );
}

function VistaClientes({
  clientes,
  cargando,
  datosDisponibles,
  alCrearCliente,
  alEditarCliente,
  alCrearOrden,
}: {
  clientes: ClienteTaller[];
  cargando: boolean;
  datosDisponibles: boolean;
  alCrearCliente: () => void;
  alEditarCliente: (cliente: ClienteTaller) => void;
  alCrearOrden: (clienteId: number) => void;
}) {
  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Relaciones</span><h1>Clientes</h1><p>Información de contacto y vehículos en un solo lugar.</p></div>
        <button className="boton-primario" onClick={alCrearCliente}><Plus size={21} />Nuevo cliente</button>
      </section>
      <section className="rejilla-clientes">
        {clientes.map((cliente) => (
          <article className={`tarjeta-cliente ${cliente.activo ? "" : "cliente-inactivo"}`} key={cliente.id}>
            <div className="cliente-superior">
              <span className="avatar avatar-grande">{cliente.iniciales}</span>
              <button className="boton-icono" type="button" onClick={() => alEditarCliente(cliente)} aria-label={`Editar cliente ${cliente.nombre}`}><Pencil size={19} /></button>
            </div>
            <h3>{cliente.nombre}</h3><p>{cliente.telefono}</p>
            {!cliente.activo && <span className="estado-cliente-inactivo">Inactivo</span>}
            <div className="datos-cliente"><span><CarFront size={17} />{cliente.cantidadVehiculos} vehículos</span><span><ClipboardList size={17} />{cliente.ordenActiva || "Sin orden activa"}</span></div>
            <div className="acciones-cliente">
              <button className="boton-secundario" type="button" onClick={() => alEditarCliente(cliente)}><Pencil size={17} />Editar</button>
              <button className="boton-primario" type="button" disabled={!cliente.activo} onClick={() => alCrearOrden(cliente.id)}>Crear orden</button>
            </div>
          </article>
        ))}
        {!cargando && clientes.length === 0 && (
          <EstadoVacio
            icono={Users}
            titulo={datosDisponibles ? "No hay clientes registrados" : "Clientes no disponibles"}
            detalle={datosDisponibles
              ? "La API respondió sin clientes para la empresa activa."
              : "No fue posible consultar los clientes en la API."}
          />
        )}
      </section>
    </>
  );
}

function VistaVehiculos({
  vehiculos,
  cargando,
  datosDisponibles,
  alCrear,
  alEditar,
}: {
  vehiculos: VehiculoTaller[];
  cargando: boolean;
  datosDisponibles: boolean;
  alCrear: () => void;
  alEditar: (vehiculo: VehiculoTaller) => void;
}) {
  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Parque vehicular</span><h1>Vehículos</h1><p>Historial y situación actual de cada unidad.</p></div>
        <button className="boton-primario" onClick={alCrear}><Plus size={21} />Nuevo vehículo</button>
      </section>
      <section className="panel lista-vehiculos">
        {vehiculos.map((vehiculo) => (
          <button key={vehiculo.id} onClick={() => alEditar(vehiculo)} aria-label={`Editar vehículo ${vehiculo.placa}`}>
            <span className="icono-vehiculo"><CarFront size={25} /></span>
            <span><strong>{vehiculo.nombre}</strong><small>{vehiculo.detalle}</small></span>
            <span><strong>{vehiculo.placa}</strong><small>{vehiculo.cliente}</small></span>
            <span className={`estado-vehiculo ${vehiculo.activo ? "listo" : ""}`}>{vehiculo.activo ? "Registrado" : "Inactivo"}</span>
            <ChevronRight size={20} />
          </button>
        ))}
        {!cargando && vehiculos.length === 0 && (
          <EstadoVacio
            icono={CarFront}
            titulo={datosDisponibles ? "No hay vehículos registrados" : "Vehículos no disponibles"}
            detalle={datosDisponibles
              ? "La API respondió sin vehículos para la empresa activa."
              : "No fue posible consultar los vehículos en la API."}
          />
        )}
      </section>
    </>
  );
}

function VistaInventario({ taller }: { taller: TallerSesionApi }) {
  const [bodegas, setBodegas] = useState<BodegaInventarioApi[]>([]);
  const [bodegaId, setBodegaId] = useState<number | null>(null);
  const [articulos, setArticulos] = useState<ArticuloInventarioApi[]>([]);
  const [cargando, setCargando] = useState(true);
  const [cargandoExistencias, setCargandoExistencias] = useState(false);
  const [error, setError] = useState("");
  const [busquedaInventario, setBusquedaInventario] = useState("");
  const [paginaInventario, setPaginaInventario] = useState(1);
  const registrosPorPagina = 10;
  const articulosFiltrados = articulos.filter((articulo) => `${articulo.nombre} ${articulo.codigo}`.toLowerCase().includes(busquedaInventario.toLowerCase().trim()));
  const totalPaginas = Math.max(1, Math.ceil(articulosFiltrados.length / registrosPorPagina));
  const articulosPagina = articulosFiltrados.slice((paginaInventario - 1) * registrosPorPagina, paginaInventario * registrosPorPagina);

  useEffect(() => {
    listarBodegasApi().then((resultado) => { setBodegas(resultado); setBodegaId(resultado.find((item) => item.esPrincipal)?.id ?? resultado[0]?.id ?? null); }).catch((excepcion) => setError(excepcion instanceof Error ? excepcion.message : "No fue posible consultar las bodegas.")).finally(() => setCargando(false));
  }, [taller.id]);
  useEffect(() => {
    if (bodegaId === null) return;
    const identificador = window.setTimeout(() => {
      setCargandoExistencias(true); setError("");
      listarExistenciasApi(bodegaId).then(setArticulos).catch((excepcion) => { setArticulos([]); setError(excepcion instanceof Error ? excepcion.message : "No fue posible consultar las existencias."); }).finally(() => setCargandoExistencias(false));
    }, 0);
    return () => window.clearTimeout(identificador);
  }, [bodegaId]);

  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Repuestos y consumibles</span><h1>Inventario</h1><p>Disponibilidad rápida para decidir sin salir de la orden.</p></div>
        <button className="boton-primario"><Plus size={21} />Registrar entrada</button>
      </section>
      <section className="panel inventario">
        <div className="controles-inventario"><div className="selector-inventario"><label>Bodega<select value={bodegaId ?? ""} onChange={(evento) => { setBodegaId(Number(evento.target.value)); setPaginaInventario(1); }}>{bodegas.map((bodega) => <option key={bodega.id} value={bodega.id}>{bodega.nombre}{bodega.esPrincipal ? " · Principal" : ""}</option>)}</select></label></div><label className="busqueda-inventario">Buscar<input value={busquedaInventario} onChange={(evento) => { setBusquedaInventario(evento.target.value); setPaginaInventario(1); }} placeholder="Nombre o código" /></label></div>
        {cargando || cargandoExistencias ? <Cargador mensaje="Consultando existencias…" /> : error ? <div className="estado-datos estado-datos-error" role="alert">{error}</div> : articulos.length === 0 ? <EstadoVacio icono={Boxes} titulo="Sin existencias" detalle="La bodega seleccionada no tiene artículos disponibles." /> : articulosFiltrados.length === 0 ? <EstadoVacio icono={Boxes} titulo="Sin coincidencias" detalle="No hay artículos que coincidan con la búsqueda." /> : <><div className="lista-inventario">{articulosPagina.map((articulo) => <article key={articulo.productoId}><div><strong>{articulo.nombre}</strong><small>{articulo.codigo} · {articulo.unidadMedida}</small></div><b>{articulo.existencia}</b></article>)}</div><div className="paginacion-inventario"><button type="button" disabled={paginaInventario <= 1} onClick={() => setPaginaInventario((pagina) => pagina - 1)}>Anterior</button><span>Página {paginaInventario} de {totalPaginas}</span><button type="button" disabled={paginaInventario >= totalPaginas} onClick={() => setPaginaInventario((pagina) => pagina + 1)}>Siguiente</button></div></>}
      </section>
    </>
  );
}

function EstadoVacio({
  icono: Icono,
  titulo,
  detalle,
}: {
  icono: LucideIcon;
  titulo: string;
  detalle: string;
}) {
  return (
    <div className="estado-vacio">
      <Icono size={32} />
      <h3>{titulo}</h3>
      <p>{detalle}</p>
    </div>
  );
}

function PaginaProceso({
  titulo,
  descripcion,
  etiquetaRegreso = "Volver",
  alRegresar,
  children,
}: {
  titulo: string;
  descripcion: string;
  etiquetaRegreso?: string;
  alRegresar: () => void;
  children: React.ReactNode;
}) {
  return (
    <section className="pagina-proceso" aria-labelledby="titulo-pagina-proceso">
      <header className="encabezado-proceso">
        <button className="boton-regresar" type="button" onClick={alRegresar}>
          <ArrowLeft size={21} />
          <span>{etiquetaRegreso}</span>
        </button>
        <div>
          <span className="sobrelinea">Área de trabajo</span>
          <h1 id="titulo-pagina-proceso">{titulo}</h1>
          <p>{descripcion}</p>
        </div>
        <span className="indicador-guardado"><CircleCheck size={17} />Guardado automático</span>
      </header>
      <div className="cuerpo-proceso">{children}</div>
    </section>
  );
}

function FormularioNuevaOrden({
  clientes,
  vehiculos,
  clienteInicialId,
  guardando,
  alEnviar,
}: {
  clientes: ClienteTaller[];
  vehiculos: VehiculoTaller[];
  clienteInicialId: number | null;
  guardando: boolean;
  alEnviar: (evento: FormEvent<HTMLFormElement>) => void;
}) {
  const [clienteId, setClienteId] = useState(
    clienteInicialId && clientes.some((cliente) => cliente.id === clienteInicialId)
      ? clienteInicialId
      : clientes[0]?.id ?? 0,
  );
  const vehiculosCliente = vehiculos.filter((vehiculo) => vehiculo.clienteId === clienteId);
  const formularioDisponible = clientes.length > 0 && vehiculosCliente.length > 0;

  return (
    <form className="formulario" onSubmit={alEnviar}>
      <div className="paso-formulario"><span>1</span><div><strong>Cliente y vehículo</strong><small>Selecciona a quién vamos a atender</small></div></div>
      <label>
        Cliente
        <select
          name="clienteId"
          required
          value={clienteId || ""}
          onChange={(evento) => setClienteId(Number(evento.target.value))}
        >
          {clientes.length === 0 && <option value="">No hay clientes registrados</option>}
          {clientes.map((cliente) => <option key={cliente.id} value={cliente.id}>{cliente.nombre}</option>)}
        </select>
      </label>
      <label>
        Vehículo
        <select name="vehiculoId" required disabled={!formularioDisponible}>
          {vehiculosCliente.length === 0 && <option value="">El cliente no tiene vehículos</option>}
          {vehiculosCliente.map((vehiculo) => (
            <option key={vehiculo.id} value={vehiculo.id}>{vehiculo.nombre} · {vehiculo.placa}</option>
          ))}
        </select>
      </label>
      <div className="separador-formulario" />
      <div className="paso-formulario"><span>2</span><div><strong>Motivo de visita</strong><small>Describe lo que reporta el cliente</small></div></div>
      <label>Descripción<textarea name="motivo" rows={4} required placeholder="Ej. Se escucha un ruido al frenar..." /></label>
      <div className="opciones-prioridad"><label><input type="radio" name="prioridad" defaultChecked />Normal</label><label><input type="radio" name="prioridad" />Prioritaria</label></div>
      <button className="boton-primario boton-ancho" type="submit" disabled={!formularioDisponible || guardando}>
        {guardando ? <Cargador mensaje="Creando orden…" /> : <><Check size={20} />Crear orden de servicio</>}
      </button>
    </form>
  );
}

function FormularioCliente({
  cliente,
  guardando,
  alEnviar,
}: {
  cliente: ClienteTaller | null;
  guardando: boolean;
  alEnviar: (evento: FormEvent<HTMLFormElement>) => void;
}) {
  return (
    <form className="formulario" onSubmit={alEnviar}>
      <div className="paso-formulario">
        <span>1</span>
        <div><strong>Identificación</strong><small>Datos necesarios para reconocer al cliente</small></div>
      </div>
      <label>
        Nombre completo
        <input name="nombre" type="text" required minLength={2} maxLength={150} defaultValue={cliente?.nombre ?? ""} autoComplete="name" placeholder="Ej. María Fernández López" />
      </label>
      <label>
        Documento de identidad
        <input name="documentoIdentidad" type="text" required minLength={3} maxLength={30} defaultValue={cliente?.documentoIdentidad ?? ""} autoComplete="off" placeholder="Cédula, RUC o pasaporte" />
      </label>
      <div className="separador-formulario" />
      <div className="paso-formulario">
        <span>2</span>
        <div><strong>Contacto</strong><small>Información para avisos y seguimiento de órdenes</small></div>
      </div>
      <label>
        Teléfono
        <input name="telefono" type="tel" required minLength={7} maxLength={30} defaultValue={cliente?.telefono ?? ""} autoComplete="tel" inputMode="tel" placeholder="Ej. 8888-0000" />
      </label>
      <label>
        Correo electrónico <small>(opcional)</small>
        <input name="correo" type="email" maxLength={150} defaultValue={cliente?.correo ?? ""} autoComplete="email" placeholder="cliente@correo.com" />
      </label>
      <label>
        Dirección <small>(opcional)</small>
        <textarea name="direccion" rows={3} maxLength={300} defaultValue={cliente?.direccion ?? ""} autoComplete="street-address" placeholder="Barrio, referencia o dirección de contacto" />
      </label>
      {cliente && (
        <label className="opcion-estado-vehiculo">
          <input name="activo" type="checkbox" defaultChecked={cliente.activo} />
          Cliente activo y disponible para nuevas órdenes
        </label>
      )}
      <button className="boton-primario boton-ancho" type="submit" disabled={guardando}>
        {guardando ? <Cargador mensaje="Guardando cliente…" /> : <><Check size={20} />{cliente ? "Guardar cambios" : "Registrar cliente"}</>}
      </button>
    </form>
  );
}

function FormularioVehiculo({
  clientes,
  vehiculo,
  guardando,
  alEnviar,
}: {
  clientes: ClienteTaller[];
  vehiculo: VehiculoTaller | null;
  guardando: boolean;
  alEnviar: (evento: FormEvent<HTMLFormElement>) => void;
}) {
  const formularioDisponible = clientes.length > 0;
  const anioActual = new Date().getFullYear();

  return (
    <form className="formulario" onSubmit={alEnviar}>
      <div className="paso-formulario">
        <span>1</span>
        <div><strong>Propietario e identificación</strong><small>Selecciona el cliente y registra la placa</small></div>
      </div>
      <label>
        Propietario
        <select name="clienteId" required defaultValue={vehiculo?.clienteId ?? clientes[0]?.id ?? ""} disabled={!formularioDisponible}>
          {!formularioDisponible && <option value="">Primero registra un cliente</option>}
          {clientes.map((cliente) => <option key={cliente.id} value={cliente.id}>{cliente.nombre}</option>)}
        </select>
      </label>
      <label>
        Placa
        <input name="placa" type="text" required minLength={2} maxLength={15} defaultValue={vehiculo?.placa ?? ""} autoComplete="off" placeholder="Ej. M 245-781" />
      </label>
      <label>
        Número VIN <small>(opcional)</small>
        <input name="numeroVin" type="text" maxLength={50} defaultValue={vehiculo?.numeroVin ?? ""} autoComplete="off" placeholder="Número de identificación vehicular" />
      </label>
      <div className="separador-formulario" />
      <div className="paso-formulario">
        <span>2</span>
        <div><strong>Características</strong><small>Datos básicos para reconocer la unidad</small></div>
      </div>
      <label>
        Marca
        <input name="marca" type="text" required minLength={2} maxLength={80} defaultValue={vehiculo?.marca ?? ""} autoComplete="off" placeholder="Ej. Toyota" />
      </label>
      <label>
        Modelo
        <input name="modelo" type="text" required minLength={1} maxLength={80} defaultValue={vehiculo?.modelo ?? ""} autoComplete="off" placeholder="Ej. Hilux" />
      </label>
      <label>
        Año
        <input name="anio" type="number" required min={1900} max={2100} defaultValue={vehiculo?.anio ?? anioActual} inputMode="numeric" />
      </label>
      <label>
        Color <small>(opcional)</small>
        <input name="color" type="text" maxLength={40} defaultValue={vehiculo?.color ?? ""} autoComplete="off" placeholder="Ej. Blanco" />
      </label>
      {vehiculo && (
        <label className="opcion-estado-vehiculo">
          <input name="activo" type="checkbox" defaultChecked={vehiculo.activo} />
          Vehículo activo y disponible para nuevas órdenes
        </label>
      )}
      <button className="boton-primario boton-ancho" type="submit" disabled={!formularioDisponible || guardando}>
        {guardando ? <Cargador mensaje="Guardando vehículo…" /> : <><Check size={20} />{vehiculo ? "Guardar cambios" : "Registrar vehículo"}</>}
      </button>
    </form>
  );
}

function DetalleOrden({
  orden,
  inspeccion,
  alRecibir,
  alNotificar,
}: {
  orden: OrdenTaller;
  inspeccion?: InspeccionVisual;
  alRecibir: () => void;
  alNotificar: () => void;
}) {
  return (
    <div className="detalle-panel detalle-orden-pagina">
      <div className="columna-orden">
        <div className="vehiculo-destacado"><span className="icono-auto-grande"><CarFront size={31} /></span><div><span className={`etiqueta-estado estado-${normalizarClase(orden.estado)}`}>{orden.estado}</span><h3>{orden.vehiculo}</h3><p>{orden.placa} · {orden.cliente}</p></div></div>
        <div className="bloque-detalle"><span className="sobrelinea">Motivo de ingreso</span><p>{orden.motivo}</p></div>
        <div className="datos-rapidos"><div><UserRound size={20} /><span><small>Responsable</small><strong>{orden.tecnico}</strong></span></div><div><Clock3 size={20} /><span><small>Ingreso</small><strong>{orden.hora} a. m.</strong></span></div></div>
        <div className="bloque-detalle"><span className="sobrelinea">Avance de la orden</span><div className="barra-avance"><span style={{ width: `${orden.progreso}%` }} /></div><strong>{orden.progreso}% completado</strong></div>
        <div className="linea-tiempo"><div className="completo"><span><Check size={14} /></span><div><strong>Orden creada</strong><small>Datos del cliente y vehículo confirmados</small></div></div><div className={orden.estado !== "Recepción" ? "completo" : "actual"}><span>{orden.estado !== "Recepción" ? <Check size={14} /> : ""}</span><div><strong>Recepción del vehículo</strong><small>Inspección visual y evidencias</small></div></div><div><span /><div><strong>Diagnóstico técnico</strong><small>Pendiente de resultados</small></div></div></div>
        <button className="boton-secundario boton-ancho" onClick={alNotificar}><MessageCircleMore size={20} />Enviar actualización al cliente</button>
      </div>

      <div className="columna-inspeccion-orden">
        {orden.estado !== "Recepción" ? (
          <ResumenInspeccion inspeccion={inspeccion} alEditar={alRecibir} />
        ) : (
          <section className="llamada-recepcion">
            <span className="icono-llamada"><ClipboardCheck size={28} /></span>
            <span className="sobrelinea">Siguiente paso</span>
            <h2>Recibe e inspecciona el vehículo</h2>
            <p>Registra kilometraje, combustible, fotografías y daños exteriores usando toda la pantalla de la tablet.</p>
            <button className="boton-primario boton-ancho" onClick={alRecibir}><ClipboardCheck size={20} />Iniciar recepción</button>
          </section>
        )}
      </div>
    </div>
  );
}

function FormularioRecepcion({
  orden,
  inspeccionInicial,
  alEnviar,
}: {
  orden: OrdenTaller;
  inspeccionInicial?: InspeccionVisual;
  alEnviar: (evento: FormEvent<HTMLFormElement>) => void;
}) {
  const [tipoDanio, setTipoDanio] = useState<TipoDanio>("Rayón");
  const [severidad, setSeveridad] = useState<SeveridadDanio>("Leve");
  const [danios, setDanios] = useState<DanioVisual[]>(inspeccionInicial?.danios || []);
  const [danioEditandoId, setDanioEditandoId] = useState<string | null>(null);
  const [cantidadFotos, setCantidadFotos] = useState(0);
  const danioEditando = danios.find((danio) => danio.id === danioEditandoId);

  function marcarZona(zona: ZonaVehiculo) {
    const id = `${zona}-${tipoDanio}-${Date.now()}`;
    setDanios((actuales) => [
      ...actuales,
      {
        id,
        zona,
        tipo: tipoDanio,
        severidad,
        observacion: "",
      },
    ]);
    setDanioEditandoId(id);
  }

  function actualizarDanio(cambios: Partial<DanioVisual>) {
    if (!danioEditandoId) return;
    setDanios((actuales) =>
      actuales.map((danio) =>
        danio.id === danioEditandoId ? { ...danio, ...cambios } : danio,
      ),
    );
  }

  return (
    <form className="formulario formulario-recepcion-pagina" onSubmit={alEnviar}>
      <div className="resumen-recepcion"><span className="icono-auto-grande"><CarFront size={28} /></span><div><strong>{orden.vehiculo}</strong><small>{orden.placa} · {orden.cliente}</small></div></div>
      <div className="fila-formulario"><label>Kilometraje<input type="number" name="kilometraje" min="0" placeholder="85,240" defaultValue={inspeccionInicial?.kilometraje || ""} required /></label><label><span className="etiqueta-con-icono"><Fuel size={17} />Combustible</span><select name="combustible" defaultValue={String(inspeccionInicial?.porcentajeCombustible || 50)}><option value="25">¼ de tanque</option><option value="50">½ tanque</option><option value="75">¾ de tanque</option><option value="100">Tanque lleno</option></select></label></div>
      <section className="inspeccion-visual">
        <div className="encabezado-inspeccion">
          <div>
            <span className="sobrelinea">Inspección visual</span>
            <h3>Marca los daños sobre el vehículo</h3>
            <p>Elige el tipo, la severidad y toca la zona afectada.</p>
          </div>
          <span className="contador-danios">{danios.length}</span>
        </div>

        <div className="selector-inspeccion">
          <span>Tipo de daño</span>
          <div className="chips-inspeccion">
            {(["Rayón", "Abolladura", "Golpe", "Vidrio", "Luz"] as TipoDanio[]).map(
              (tipo) => (
                <button
                  key={tipo}
                  type="button"
                  className={tipoDanio === tipo ? "activo" : ""}
                  onClick={() => setTipoDanio(tipo)}
                >
                  {tipo}
                </button>
              ),
            )}
          </div>
        </div>

        <div className="selector-inspeccion selector-severidad">
          <span>Severidad</span>
          <div className="chips-inspeccion">
            {(["Leve", "Moderado", "Severo"] as SeveridadDanio[]).map((nivel) => (
              <button
                key={nivel}
                type="button"
                className={`${severidad === nivel ? "activo" : ""} nivel-${normalizarClase(nivel)}`}
                onClick={() => setSeveridad(nivel)}
              >
                {nivel}
              </button>
            ))}
          </div>
        </div>

        <MapaInspeccion danios={danios} alMarcar={marcarZona} />

        {danioEditando && (
          <div className="editor-danio">
            <div className="encabezado-editor-danio">
              <div>
                <span className="sobrelinea">Editar hallazgo</span>
                <strong>{etiquetaZona(danioEditando.zona)}</strong>
              </div>
              <button type="button" onClick={() => setDanioEditandoId(null)} aria-label="Cerrar editor">
                <X size={18} />
              </button>
            </div>
            <div className="fila-formulario">
              <label>
                Tipo de daño
                <select
                  value={danioEditando.tipo}
                  onChange={(evento) => actualizarDanio({ tipo: evento.target.value as TipoDanio })}
                >
                  {(["Rayón", "Abolladura", "Golpe", "Vidrio", "Luz"] as TipoDanio[]).map((tipo) => (
                    <option key={tipo}>{tipo}</option>
                  ))}
                </select>
              </label>
              <label>
                Severidad
                <select
                  value={danioEditando.severidad}
                  onChange={(evento) => actualizarDanio({ severidad: evento.target.value as SeveridadDanio })}
                >
                  {(["Leve", "Moderado", "Severo"] as SeveridadDanio[]).map((nivel) => (
                    <option key={nivel}>{nivel}</option>
                  ))}
                </select>
              </label>
            </div>
            <label>
              Descripción u observación
              <textarea
                rows={3}
                maxLength={500}
                value={danioEditando.observacion}
                onChange={(evento) => actualizarDanio({ observacion: evento.target.value })}
                placeholder="Ej. Rayón superficial de 12 cm en la puerta trasera..."
              />
              <small>{danioEditando.observacion.length}/500 caracteres</small>
            </label>
            <button className="boton-secundario boton-ancho" type="button" onClick={() => setDanioEditandoId(null)}>
              <Check size={18} /> Listo
            </button>
          </div>
        )}

        {danios.length > 0 ? (
          <div className="danios-registrados">
            <div className="titulo-danios">
              <strong>Daños registrados</strong>
              <button type="button" onClick={() => { setDanios([]); setDanioEditandoId(null); }}>
                <RotateCcw size={15} /> Limpiar
              </button>
            </div>
            {danios.map((danio, indice) => (
              <div className="fila-danio" key={danio.id}>
                <span className={`numero-danio nivel-${normalizarClase(danio.severidad)}`}>
                  {indice + 1}
                </span>
                <span>
                  <strong>{danio.tipo} · {etiquetaZona(danio.zona)}</strong>
                  <small>{danio.observacion || `Severidad ${danio.severidad.toLocaleLowerCase("es")} · Sin observación`}</small>
                </span>
                <span className="acciones-danio">
                  <button
                    type="button"
                    aria-label={`Editar ${danio.tipo} en ${etiquetaZona(danio.zona)}`}
                    onClick={() => setDanioEditandoId(danio.id)}
                  >
                    <Pencil size={16} />
                  </button>
                  <button
                    type="button"
                    aria-label={`Eliminar ${danio.tipo} en ${etiquetaZona(danio.zona)}`}
                    onClick={() => {
                      setDanios((actuales) => actuales.filter((item) => item.id !== danio.id));
                      if (danioEditandoId === danio.id) setDanioEditandoId(null);
                    }}
                  >
                    <Trash2 size={16} />
                  </button>
                </span>
              </div>
            ))}
          </div>
        ) : (
          <div className="sin-danios"><Check size={18} />Aún no se marcaron daños visibles</div>
        )}
      </section>

      <input type="hidden" name="danios" value={JSON.stringify(danios)} />
      <label>Observaciones generales<textarea name="estado" rows={4} required defaultValue={inspeccionInicial?.descripcionEstado || ""} placeholder="Describe el estado interior, accesorios o cualquier detalle adicional..." /></label>
      <label className="zona-fotos">
        <input
          type="file"
          accept="image/*"
          capture="environment"
          multiple
          onChange={(evento) => setCantidadFotos(evento.target.files?.length || 0)}
        />
        {cantidadFotos > 0 ? <ImagePlus size={25} /> : <Camera size={25} />}
        <span>
          <strong>{cantidadFotos > 0 ? `${cantidadFotos} fotografías seleccionadas` : "Agregar fotografías"}</strong>
          <small>Frente, laterales y evidencia de cada daño</small>
        </span>
        <Plus size={20} />
      </label>
      <div className="lista-comprobacion"><label><input type="checkbox" name="dejaLlaves" defaultChecked={inspeccionInicial?.dejaLlaves} />Deja llaves</label><label><input type="checkbox" name="dejaDocumentos" defaultChecked={inspeccionInicial?.dejaDocumentos} />Deja documentos</label><label><input type="checkbox" name="aceptaPruebaRuta" />Acepta prueba de ruta</label></div>
      <button className="boton-primario boton-ancho" type="submit"><Sparkles size={20} />{inspeccionInicial ? "Guardar cambios de inspección" : "Guardar y pasar a diagnóstico"}</button>
    </form>
  );
}

function MapaInspeccion({
  danios,
  alMarcar,
  compacto = false,
}: {
  danios: DanioVisual[];
  alMarcar?: (zona: ZonaVehiculo) => void;
  compacto?: boolean;
}) {
  return (
    <div className={`mapa-inspeccion ${compacto ? "mapa-compacto" : ""}`}>
      <div className="orientacion-vehiculo"><span />Frente del vehículo<span /></div>
      <div className="silueta-vehiculo" aria-label="Esquema superior del vehículo">
        <span className="rueda rueda-1" />
        <span className="rueda rueda-2" />
        <span className="rueda rueda-3" />
        <span className="rueda rueda-4" />
        <span className="carroceria">
          <span className="parabrisas-esquema" />
          <span className="techo-esquema" />
          <span className="vidrio-trasero-esquema" />
        </span>
        {zonasVehiculo.map((zona) => {
          const daniosZona = danios.filter((danio) => danio.zona === zona.id);
          const severidadMayor = obtenerSeveridadMayor(daniosZona);
          return (
            <button
              key={zona.id}
              type="button"
              disabled={!alMarcar}
              className={`marca-zona zona-${zona.id} ${daniosZona.length ? "con-danio" : ""} ${severidadMayor ? `nivel-${normalizarClase(severidadMayor)}` : ""}`}
              aria-label={`${zona.etiqueta}${daniosZona.length ? `, ${daniosZona.length} daños` : ""}`}
              onClick={() => alMarcar?.(zona.id)}
            >
              {daniosZona.length > 0 ? daniosZona.length : <Plus size={14} />}
              <span>{zona.etiqueta}</span>
            </button>
          );
        })}
      </div>
      {!compacto && <p className="ayuda-mapa">Toca una zona para colocar un marcador</p>}
    </div>
  );
}

function ResumenInspeccion({
  inspeccion,
  alEditar,
}: {
  inspeccion?: InspeccionVisual;
  alEditar: () => void;
}) {
  const danios = inspeccion?.danios || [];
  return (
    <section className="resumen-inspeccion">
      <div className="encabezado-inspeccion">
        <div>
          <span className="sobrelinea">Recepción documentada</span>
          <h3>Inspección visual del vehículo</h3>
          <p>{danios.length > 0 ? `${danios.length} hallazgos registrados al ingresar` : "Sin daños exteriores reportados"}</p>
        </div>
        <span className={`contador-danios ${danios.length === 0 ? "sin-hallazgos" : ""}`}>{danios.length}</span>
      </div>
      {inspeccion && (
        <div className="datos-inspeccion">
          <span><small>Kilometraje</small><strong>{inspeccion.kilometraje.toLocaleString("es-NI")} km</strong></span>
          <span><small>Combustible</small><strong>{inspeccion.porcentajeCombustible}%</strong></span>
          <span><small>Elementos</small><strong>{inspeccion.dejaLlaves ? "Con llaves" : "Sin llaves"}</strong></span>
        </div>
      )}
      <MapaInspeccion danios={danios} compacto />
      {danios.length > 0 && (
        <div className="leyenda-inspeccion">
          {danios.map((danio, indice) => (
            <article key={danio.id}>
              <i className={`nivel-${normalizarClase(danio.severidad)}`}>{indice + 1}</i>
              <span>
                <strong>{danio.tipo} en {etiquetaZona(danio.zona)}</strong>
                <small>{danio.observacion || "Sin observación específica"}</small>
              </span>
            </article>
          ))}
        </div>
      )}
      <div className="observacion-inspeccion">
        <span className="sobrelinea">Observaciones generales</span>
        <p>{inspeccion?.descripcionEstado || "No se registraron observaciones generales durante la recepción."}</p>
      </div>
      <button className="boton-secundario boton-ancho" type="button" onClick={alEditar}>
        <Pencil size={18} /> Editar inspección y observaciones
      </button>
    </section>
  );
}

function etiquetaZona(zona: ZonaVehiculo) {
  return zonasVehiculo.find((item) => item.id === zona)?.etiqueta || zona;
}

function obtenerSeveridadMayor(danios: DanioVisual[]): SeveridadDanio | null {
  if (danios.some((danio) => danio.severidad === "Severo")) return "Severo";
  if (danios.some((danio) => danio.severidad === "Moderado")) return "Moderado";
  if (danios.some((danio) => danio.severidad === "Leve")) return "Leve";
  return null;
}

function normalizarClase(valor: string) {
  return valor
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLocaleLowerCase("es")
    .replaceAll(" ", "-");
}

function formatearHora(hora: string) {
  const [horas, minutos] = hora.split(":");
  const fecha = new Date(2000, 0, 1, Number(horas), Number(minutos));
  return new Intl.DateTimeFormat("es-NI", {
    hour: "numeric",
    minute: "2-digit",
  }).format(fecha);
}

function obtenerDireccionApi() {
  const direccionApi = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "");
  if (!direccionApi) {
    throw new Error("No se configuró NEXT_PUBLIC_API_URL para consultar la API.");
  }
  return direccionApi;
}

async function iniciarSesionApi(solicitud: {
  usuario: string;
  contrasena: string;
  recordarme: boolean;
}): Promise<SesionTallerApi> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/autenticacion/iniciar`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(solicitud),
  });
  if (!respuesta.ok) {
    throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible iniciar sesión."));
  }
  return (await respuesta.json()) as SesionTallerApi;
}

async function obtenerSesionApi(senal: AbortSignal): Promise<SesionTallerApi> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/autenticacion/sesion`, {
    credentials: "include",
    signal: senal,
  });
  if (!respuesta.ok) throw new Error("No hay una sesión activa.");
  return (await respuesta.json()) as SesionTallerApi;
}

async function seleccionarTallerApi(empresaNovaId: number): Promise<SesionTallerApi> {
  const respuesta = await fetch(
    `${obtenerDireccionApi()}/api/autenticacion/seleccionar-taller`,
    {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ empresaNovaId }),
    },
  );
  if (!respuesta.ok) {
    throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible cambiar de taller."));
  }
  return (await respuesta.json()) as SesionTallerApi;
}

async function cerrarSesionApi() {
  await fetch(`${obtenerDireccionApi()}/api/autenticacion/cerrar`, {
    method: "POST",
    credentials: "include",
  });
}

async function listarTalleresSincronizadosApi(): Promise<TallerSincronizadoApi[]> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/talleres-sincronizados`, {
    credentials: "include",
  });
  if (!respuesta.ok) throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible consultar los talleres."));
  return (await respuesta.json()) as TallerSincronizadoApi[];
}

async function agregarTallerSincronizadoApi(empresaNovaId: number) {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/talleres-sincronizados`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ empresaNovaId }),
  });
  if (!respuesta.ok) throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible agregar el taller."));
}

async function retirarTallerSincronizadoApi(empresaNovaId: number) {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/talleres-sincronizados/${empresaNovaId}`, {
    method: "DELETE",
    credentials: "include",
  });
  if (!respuesta.ok) throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible retirar el taller."));
}

async function listarBodegasApi(): Promise<BodegaInventarioApi[]> { const respuesta = await fetch(`${obtenerDireccionApi()}/api/inventario/bodegas`, { credentials: "include" }); if (!respuesta.ok) throw new Error("No fue posible consultar las bodegas."); return await respuesta.json() as BodegaInventarioApi[]; }
async function listarExistenciasApi(bodegaId: number): Promise<ArticuloInventarioApi[]> { const respuesta = await fetch(`${obtenerDireccionApi()}/api/inventario/existencias?bodegaId=${bodegaId}`, { credentials: "include" }); if (!respuesta.ok) throw new Error("No fue posible consultar las existencias."); return await respuesta.json() as ArticuloInventarioApi[]; }

async function cargarDatosApi(senal: AbortSignal): Promise<{
  ordenes: OrdenTaller[];
  clientes: ClienteTaller[];
  vehiculos: VehiculoTaller[];
}> {
  const [ordenes, clientesApi, vehiculosApi] = await Promise.all([
    cargarOrdenesApi(senal),
    cargarClientesApi(senal),
    cargarVehiculosApi(senal),
  ]);

  const clientes = clientesApi.map((cliente) =>
    convertirClienteTaller(
      cliente,
      vehiculosApi.filter((vehiculo) => vehiculo.clienteId === cliente.id).length,
      ordenes.find(
        (orden) =>
          orden.clienteId === cliente.id && orden.estado !== "Lista para entregar",
      )?.numero ?? null,
    ),
  );
  const vehiculos = vehiculosApi.map(convertirVehiculoTaller);

  return { ordenes, clientes, vehiculos };
}

async function cargarOrdenesApi(senal: AbortSignal): Promise<OrdenTaller[]> {
  const direccionApi = obtenerDireccionApi();

  const respuesta = await fetch(`${direccionApi}/api/ordenes-servicio`, {
    credentials: "include",
    signal: senal,
  });
  if (!respuesta.ok) throw new Error("No fue posible consultar las órdenes.");

  const ordenesApi = (await respuesta.json()) as OrdenServicioApi[];
  return ordenesApi.map(convertirOrdenApi);
}

async function cargarClientesApi(senal: AbortSignal): Promise<ClienteApi[]> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/clientes`, {
    credentials: "include",
    signal: senal,
  });
  if (!respuesta.ok) throw new Error("No fue posible consultar los clientes.");
  return (await respuesta.json()) as ClienteApi[];
}

async function guardarClienteApi(solicitud: {
  nombre: string;
  documentoIdentidad: string;
  telefono: string;
  correo: string | null;
  direccion: string | null;
  activo: boolean;
}, clienteId?: number): Promise<ClienteApi> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/clientes${clienteId ? `/${clienteId}` : ""}`, {
    method: clienteId ? "PUT" : "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(solicitud),
  });
  if (!respuesta.ok) {
    throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible guardar el cliente."));
  }
  return (await respuesta.json()) as ClienteApi;
}

async function cargarVehiculosApi(senal: AbortSignal): Promise<VehiculoApi[]> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/vehiculos`, {
    credentials: "include",
    signal: senal,
  });
  if (!respuesta.ok) throw new Error("No fue posible consultar los vehículos.");
  return (await respuesta.json()) as VehiculoApi[];
}

async function guardarVehiculoApi(
  solicitud: {
    clienteId: number;
    placa: string;
    marca: string;
    modelo: string;
    anio: number;
    color: string | null;
    numeroVin: string | null;
    activo: boolean;
  },
  vehiculoId?: number,
): Promise<VehiculoApi> {
  const respuesta = await fetch(
    `${obtenerDireccionApi()}/api/vehiculos${vehiculoId ? `/${vehiculoId}` : ""}`,
    {
      method: vehiculoId ? "PUT" : "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(solicitud),
    },
  );
  if (!respuesta.ok) {
    throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible guardar el vehículo."));
  }
  return (await respuesta.json()) as VehiculoApi;
}

async function crearOrdenApi(solicitud: {
  clienteId: number;
  vehiculoId: number;
  observaciones: string;
}): Promise<OrdenTaller> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/ordenes-servicio`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(solicitud),
  });
  if (!respuesta.ok) throw new Error("No fue posible crear la orden en la API.");
  return convertirOrdenApi((await respuesta.json()) as OrdenServicioApi);
}

function convertirOrdenApi(orden: OrdenServicioApi): OrdenTaller {
  const estado = convertirEstadoApi(orden.estado);
  return {
    id: orden.id,
    clienteId: orden.clienteId,
    vehiculoId: orden.vehiculoId,
    numero: orden.numero,
    cliente: orden.nombreCliente,
    vehiculo: `Vehículo · ${orden.placaVehiculo}`,
    placa: orden.placaVehiculo,
    estado,
    motivo: orden.observaciones || "Sin observaciones registradas",
    tecnico: "Por asignar",
    hora: new Date(orden.fechaIngreso).toLocaleTimeString("es-NI", {
      hour: "numeric",
      minute: "2-digit",
    }),
    progreso: progresoPorEstado(estado),
    color: colorPorEstado(estado),
  };
}

function obtenerIniciales(nombre: string) {
  return nombre
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((parte) => parte[0]?.toLocaleUpperCase("es") ?? "")
    .join("");
}

function convertirClienteTaller(
  cliente: ClienteApi,
  cantidadVehiculos: number,
  ordenActiva: string | null,
): ClienteTaller {
  return {
    id: cliente.id,
    iniciales: obtenerIniciales(cliente.nombre),
    nombre: cliente.nombre,
    documentoIdentidad: cliente.documentoIdentidad,
    telefono: cliente.telefono,
    correo: cliente.correo,
    direccion: cliente.direccion,
    activo: cliente.activo,
    cantidadVehiculos,
    ordenActiva,
  };
}

function convertirVehiculoTaller(vehiculo: VehiculoApi): VehiculoTaller {
  return {
    id: vehiculo.id,
    clienteId: vehiculo.clienteId,
    placa: vehiculo.placa,
    marca: vehiculo.marca,
    modelo: vehiculo.modelo,
    anio: vehiculo.anio,
    color: vehiculo.color,
    numeroVin: vehiculo.numeroVin,
    nombre: `${vehiculo.marca} ${vehiculo.modelo}`,
    detalle: `${vehiculo.anio} · ${vehiculo.color || "Color no registrado"}`,
    cliente: vehiculo.nombreCliente,
    activo: vehiculo.activo,
  };
}

function valorOpcionalFormulario(valor: FormDataEntryValue | null) {
  const texto = String(valor ?? "").trim();
  return texto || null;
}

async function obtenerMensajeErrorApi(respuesta: Response, mensajePredeterminado: string) {
  try {
    const problema = (await respuesta.json()) as {
      detail?: string;
      title?: string;
      errors?: Record<string, string[]>;
    };
    const primerErrorValidacion = Object.values(problema.errors ?? {}).flat()[0];
    return problema.detail || primerErrorValidacion || problema.title || mensajePredeterminado;
  } catch {
    return mensajePredeterminado;
  }
}

async function guardarRecepcionApi(
  ordenServicioId: number,
  inspeccion: InspeccionVisual,
  esActualizacion: boolean,
) {
  const direccionApi = obtenerDireccionApi();

  const respuesta = await fetch(
    `${direccionApi}/api/ordenes-servicio/${ordenServicioId}/recepcion`,
    {
      method: esActualizacion ? "PUT" : "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        kilometraje: inspeccion.kilometraje,
        porcentajeCombustible: inspeccion.porcentajeCombustible,
        descripcionEstado: inspeccion.descripcionEstado,
        dejaLlaves: inspeccion.dejaLlaves,
        dejaDocumentos: inspeccion.dejaDocumentos,
        danios: inspeccion.danios.map((danio) => ({
          zona: zonaParaApi(danio.zona),
          tipo: danio.tipo === "Rayón" ? "Rayon" : danio.tipo,
          severidad: danio.severidad,
          observacion: danio.observacion.trim() || null,
        })),
      }),
    },
  );

  if (!respuesta.ok) throw new Error("No fue posible registrar la recepción.");
}

async function cargarInspeccionApi(
  ordenServicioId: number,
  senal: AbortSignal,
): Promise<InspeccionVisual | null> {
  const direccionApi = obtenerDireccionApi();

  const respuesta = await fetch(
    `${direccionApi}/api/ordenes-servicio/${ordenServicioId}/recepcion`,
    {
      credentials: "include",
      signal: senal,
    },
  );
  if (!respuesta.ok) throw new Error("No fue posible consultar la inspección.");

  const recepcion = (await respuesta.json()) as RecepcionVehiculoApi;
  return {
    kilometraje: recepcion.kilometraje,
    porcentajeCombustible: recepcion.porcentajeCombustible,
    descripcionEstado: recepcion.descripcionEstado,
    dejaLlaves: recepcion.dejaLlaves,
    dejaDocumentos: recepcion.dejaDocumentos,
    danios: recepcion.danios.map((danio, indice) => ({
      id: `api-${ordenServicioId}-${indice}`,
      zona: zonaDesdeApi(danio.zona),
      tipo: danio.tipo === "Rayon" ? "Rayón" : danio.tipo as TipoDanio,
      severidad: danio.severidad as SeveridadDanio,
      observacion: danio.observacion || "",
    })),
  };
}

function zonaParaApi(zona: ZonaVehiculo) {
  const zonas: Record<ZonaVehiculo, string> = {
    frente: "Frente",
    capo: "Capo",
    parabrisas: "Parabrisas",
    techo: "Techo",
    "lateral-izquierdo": "LateralIzquierdo",
    "lateral-derecho": "LateralDerecho",
    maletero: "Maletero",
    posterior: "Posterior",
  };
  return zonas[zona];
}

function zonaDesdeApi(zona: string): ZonaVehiculo {
  const zonas: Record<string, ZonaVehiculo> = {
    Frente: "frente",
    Capo: "capo",
    Parabrisas: "parabrisas",
    Techo: "techo",
    LateralIzquierdo: "lateral-izquierdo",
    LateralDerecho: "lateral-derecho",
    Maletero: "maletero",
    Posterior: "posterior",
  };
  return zonas[zona] || "frente";
}

function convertirEstadoApi(estado: string): EstadoOrden {
  const equivalencias: Record<string, EstadoOrden> = {
    Recepcion: "Recepción",
    Diagnostico: "Diagnóstico",
    Cotizacion: "Cotización",
    PendienteAprobacion: "Por aprobar",
    PreparacionReparacion: "Reparación",
    Reparacion: "Reparación",
    ControlCalidad: "Control de calidad",
    ListaParaEntrega: "Lista para entregar",
    Entregada: "Lista para entregar",
    Cerrada: "Lista para entregar",
  };
  return equivalencias[estado] || "Recepción";
}

function progresoPorEstado(estado: EstadoOrden) {
  const progresos: Record<EstadoOrden, number> = {
    "Recepción": 10,
    "Diagnóstico": 28,
    "Cotización": 42,
    "Por aprobar": 48,
    "Reparación": 68,
    "Control de calidad": 88,
    "Lista para entregar": 100,
  };
  return progresos[estado];
}

function colorPorEstado(estado: EstadoOrden) {
  const colores: Record<EstadoOrden, string> = {
    "Recepción": "#c66c3d",
    "Diagnóstico": "#1f7a5b",
    "Cotización": "#d78b31",
    "Por aprobar": "#d78b31",
    "Reparación": "#315e9d",
    "Control de calidad": "#7b5aa6",
    "Lista para entregar": "#238a70",
  };
  return colores[estado];
}
