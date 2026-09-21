"use client";

import type { FormEvent } from "react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  CargadorPantalla,
  useCargadorPantalla,
} from "./componentes/ProveedorCargadorPantalla";
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
  Copy,
  Eye,
  EyeOff,
  Fuel,
  LayoutDashboard,
  LockKeyhole,
  LogOut,
  MessageCircleMore,
  Mic,
  MoreHorizontal,
  ImagePlus,
  Pencil,
  Plus,
  RotateCcw,
  Search,
  Share2,
  Settings2,
  Square,
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
  evidencias: EvidenciaInspeccion[];
}

interface EvidenciaInspeccion {
  id: number;
  nombreArchivo: string;
  tipoContenido: string;
  longitud: number;
  fechaCargaUtc: string;
}

interface AlternativaReconocimientoVoz {
  transcript: string;
}

interface ResultadoReconocimientoVoz {
  readonly isFinal: boolean;
  readonly 0: AlternativaReconocimientoVoz;
}

interface EventoReconocimientoVoz {
  readonly resultIndex: number;
  readonly results: ArrayLike<ResultadoReconocimientoVoz>;
}

interface EventoErrorReconocimientoVoz {
  readonly error: string;
}

interface ReconocimientoVoz {
  lang: string;
  continuous: boolean;
  interimResults: boolean;
  onresult: ((evento: EventoReconocimientoVoz) => void) | null;
  onerror: ((evento: EventoErrorReconocimientoVoz) => void) | null;
  onend: (() => void) | null;
  start: () => void;
  stop: () => void;
  abort: () => void;
}

type ConstructorReconocimientoVoz = new () => ReconocimientoVoz;

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

interface DetalleOrdenServicioApi {
  id: number;
  tipo: "Inventario" | "Manual";
  productoInventarioId: number | null;
  bodegaInventarioId: number | null;
  codigoProducto: string | null;
  descripcion: string;
  unidadMedida: string | null;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
  existenciaDescontada: boolean;
  fechaCreacion: string;
}

interface ResumenDetallesOrdenServicioApi {
  detalles: DetalleOrdenServicioApi[];
  total: number;
}

interface EvidenciaDiagnosticoApi {
  id: number;
  nombreArchivo: string;
  tipoContenido: string;
  longitud: number;
  fechaCargaUtc: string;
}

interface DiagnosticoOrdenServicioApi {
  diagnostico: string | null;
  fechaDiagnosticoUtc: string | null;
  tokenPublico: string | null;
  fechaAutorizacionClienteUtc: string | null;
  evidencias: EvidenciaDiagnosticoApi[];
}

interface ClienteApi {
  id: number;
  nombre: string;
  telefono: string;
  direccion: string | null;
  activo: boolean;
  fechaCreacion: string;
}

interface VehiculoApi {
  id: number;
  clienteId: number;
  nombreCliente: string;
  placa: string;
  marcaVehiculoId: number;
  marca: string;
  modeloVehiculoId: number;
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
  telefono: string;
  direccion: string | null;
  activo: boolean;
  cantidadVehiculos: number;
  ordenActiva: string | null;
}

interface VehiculoTaller {
  id: number;
  clienteId: number;
  placa: string;
  marcaVehiculoId: number;
  marca: string;
  modeloVehiculoId: number;
  modelo: string;
  anio: number;
  color: string | null;
  numeroVin: string | null;
  nombre: string;
  detalle: string;
  cliente: string;
  activo: boolean;
}

interface MarcaVehiculoApi {
  id: number;
  nombre: string;
  activa: boolean;
  fechaCreacion: string;
}

interface ModeloVehiculoApi {
  id: number;
  marcaVehiculoId: number;
  nombreMarca: string;
  nombre: string;
  activo: boolean;
  fechaCreacion: string;
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
  evidencias: EvidenciaInspeccion[];
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
interface ArticuloInventarioApi { productoId: number; codigo: string; nombre: string; unidadMedida: string; existencia: number; precioUnitario?: number; }

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
  const { iniciarCargaPantalla, finalizarCargaPantalla, ejecutarConCargadorPantalla } = useCargadorPantalla();
  const [sesion, setSesion] = useState<SesionTallerApi | null>();
  const [cargandoSesion, setCargandoSesion] = useState(true);
  const [cambiandoTaller, setCambiandoTaller] = useState(false);
  const [mostrarAdministracion, setMostrarAdministracion] = useState(false);
  const [vista, setVista] = useState<Vista>("inicio");
  const [ordenes, setOrdenes] = useState<OrdenTaller[]>([]);
  const [clientes, setClientes] = useState<ClienteTaller[]>([]);
  const [vehiculos, setVehiculos] = useState<VehiculoTaller[]>([]);
  const [marcasVehiculo, setMarcasVehiculo] = useState<MarcaVehiculoApi[]>([]);
  const [modelosVehiculo, setModelosVehiculo] = useState<ModeloVehiculoApi[]>([]);
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
    const cargaId = iniciarCargaPantalla("Consultando datos del taller…");
    cargarDatosApi(controlador.signal)
      .then((datos) => {
        setOrdenes(datos.ordenes);
        setClientes(datos.clientes);
        setVehiculos(datos.vehiculos);
        setMarcasVehiculo(datos.marcasVehiculo);
        setModelosVehiculo(datos.modelosVehiculo);
        setErrorDatos("");
      })
      .catch(() => {
        if (controlador.signal.aborted) return;
        setOrdenes([]);
        setClientes([]);
        setVehiculos([]);
        setMarcasVehiculo([]);
        setModelosVehiculo([]);
        setErrorDatos("No fue posible comunicarse con la API configurada.");
      })
      .finally(() => {
        finalizarCargaPantalla(cargaId);
        if (!controlador.signal.aborted) setCargandoDatos(false);
      });
    return () => controlador.abort();
  }, [finalizarCargaPantalla, iniciarCargaPantalla, sesion]);

  useEffect(() => {
    if (!ordenDetalle || ordenDetalle.estado === "Recepción") return;
    const controlador = new AbortController();
    const cargaId = iniciarCargaPantalla("Consultando la inspección de la orden…");
    cargarInspeccionApi(ordenDetalle.id, controlador.signal)
      .then((inspeccion) => {
        if (inspeccion) {
          setInspecciones((actuales) => ({ ...actuales, [ordenDetalle.id]: inspeccion }));
        }
      })
      .catch(() => {
        mostrarAviso("No fue posible consultar la inspección en la API");
      })
      .finally(() => finalizarCargaPantalla(cargaId));
    return () => controlador.abort();
  }, [finalizarCargaPantalla, iniciarCargaPantalla, ordenDetalle]);

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
      const nuevaOrden = await ejecutarConCargadorPantalla(
        "Creando la orden de servicio…",
        () => crearOrdenApi({
          clienteId: Number(datos.get("clienteId")),
          vehiculoId: Number(datos.get("vehiculoId")),
          observaciones: String(datos.get("motivo")),
        }),
      );
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

  function incorporarClienteGuardado(
    clienteGuardado: ClienteTaller,
    clienteAnterior: ClienteTaller | null,
  ) {
    setClientes((actuales) => {
      const siguientes = clienteAnterior
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
  }

  async function registrarClienteDesdeOrden(datos: FormData) {
    setGuardandoCliente(true);
    try {
      const clienteApi = await ejecutarConCargadorPantalla(
        "Registrando el cliente…",
        () => guardarClienteApi({
          nombre: String(datos.get("nombre")),
          telefono: String(datos.get("telefono")),
          direccion: valorOpcionalFormulario(datos.get("direccion")),
          activo: true,
        }),
      );
      const clienteGuardado = convertirClienteTaller(clienteApi, 0, null);
      incorporarClienteGuardado(clienteGuardado, null);
      mostrarAviso(`Cliente ${clienteGuardado.nombre} registrado y seleccionado`);
      return clienteGuardado;
    } catch (error) {
      mostrarAviso(
        error instanceof Error ? error.message : "No fue posible guardar el cliente",
      );
      throw error;
    } finally {
      setGuardandoCliente(false);
    }
  }

  async function guardarCliente(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    setGuardandoCliente(true);

    try {
      const clienteApi = await ejecutarConCargadorPantalla(
        clienteEditando ? "Actualizando el cliente…" : "Registrando el cliente…",
        () => guardarClienteApi(
          {
            nombre: String(datos.get("nombre")),
            telefono: String(datos.get("telefono")),
            direccion: valorOpcionalFormulario(datos.get("direccion")),
            activo: clienteEditando ? datos.has("activo") : true,
          },
          clienteEditando?.id,
        ),
      );
      const clienteGuardado = convertirClienteTaller(
        clienteApi,
        clienteEditando?.cantidadVehiculos ?? 0,
        clienteEditando?.ordenActiva ?? null,
      );

      incorporarClienteGuardado(clienteGuardado, clienteEditando);
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

  function incorporarVehiculoGuardado(
    vehiculoGuardado: VehiculoTaller,
    vehiculoAnterior: VehiculoTaller | null,
  ) {
    const propietarioAnteriorId = vehiculoAnterior?.clienteId;
    setVehiculos((actuales) => {
      const siguientes = vehiculoAnterior
        ? actuales.map((vehiculo) =>
            vehiculo.id === vehiculoGuardado.id ? vehiculoGuardado : vehiculo)
        : [...actuales, vehiculoGuardado];
      return siguientes.sort((a, b) => a.placa.localeCompare(b.placa, "es"));
    });
    setClientes((actuales) =>
      actuales.map((cliente) => {
        if (!vehiculoAnterior && cliente.id === vehiculoGuardado.clienteId) {
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
  }

  async function crearMarcaVehiculo(nombre: string) {
    try {
      const marca = await ejecutarConCargadorPantalla(
        "Agregando la marca del vehículo…",
        () => guardarMarcaVehiculoApi(nombre),
      );
      setMarcasVehiculo((actuales) =>
        [...actuales, marca].sort((a, b) => a.nombre.localeCompare(b.nombre, "es")),
      );
      mostrarAviso(`Marca ${marca.nombre} agregada al catálogo`);
      return marca;
    } catch (error) {
      mostrarAviso(error instanceof Error ? error.message : "No fue posible guardar la marca");
      throw error;
    }
  }

  async function crearModeloVehiculo(marcaVehiculoId: number, nombre: string) {
    try {
      const modelo = await ejecutarConCargadorPantalla(
        "Agregando el modelo del vehículo…",
        () => guardarModeloVehiculoApi(marcaVehiculoId, nombre),
      );
      setModelosVehiculo((actuales) =>
        [...actuales, modelo].sort((a, b) => a.nombre.localeCompare(b.nombre, "es")),
      );
      mostrarAviso(`Modelo ${modelo.nombre} agregado al catálogo`);
      return modelo;
    } catch (error) {
      mostrarAviso(error instanceof Error ? error.message : "No fue posible guardar el modelo");
      throw error;
    }
  }

  async function registrarVehiculoDesdeOrden(datos: FormData) {
    setGuardandoVehiculo(true);
    try {
      const vehiculoApi = await ejecutarConCargadorPantalla(
        "Registrando el vehículo…",
        () => guardarVehiculoApi({
          clienteId: Number(datos.get("clienteId")),
          placa: String(datos.get("placa")),
          modeloVehiculoId: Number(datos.get("modeloVehiculoId")),
          anio: Number(datos.get("anio")),
          color: valorOpcionalFormulario(datos.get("color")),
          numeroVin: valorOpcionalFormulario(datos.get("numeroVin")),
          activo: true,
        }),
      );
      const vehiculoGuardado = convertirVehiculoTaller(vehiculoApi);
      incorporarVehiculoGuardado(vehiculoGuardado, null);
      mostrarAviso(`Vehículo ${vehiculoGuardado.placa} registrado y seleccionado`);
      return vehiculoGuardado;
    } catch (error) {
      mostrarAviso(
        error instanceof Error ? error.message : "No fue posible guardar el vehículo",
      );
      throw error;
    } finally {
      setGuardandoVehiculo(false);
    }
  }

  async function guardarVehiculo(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    setGuardandoVehiculo(true);

    try {
      const vehiculoApi = await ejecutarConCargadorPantalla(
        vehiculoEditando ? "Actualizando el vehículo…" : "Registrando el vehículo…",
        () => guardarVehiculoApi(
          {
            clienteId: Number(datos.get("clienteId")),
            placa: String(datos.get("placa")),
            modeloVehiculoId: Number(datos.get("modeloVehiculoId")),
            anio: Number(datos.get("anio")),
            color: valorOpcionalFormulario(datos.get("color")),
            numeroVin: valorOpcionalFormulario(datos.get("numeroVin")),
            activo: vehiculoEditando ? datos.has("activo") : true,
          },
          vehiculoEditando?.id,
        ),
      );
      const vehiculoGuardado = convertirVehiculoTaller(vehiculoApi);
      incorporarVehiculoGuardado(vehiculoGuardado, vehiculoEditando);
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
      evidencias: inspecciones[ordenDetalle.id]?.evidencias || [],
    };

    const fotografias = datos
      .getAll("fotografias")
      .filter((valor): valor is File => valor instanceof File && valor.size > 0);

    let resultadoGuardado: { inspeccion: InspeccionVisual; advertencia?: string };
    try {
      resultadoGuardado = await ejecutarConCargadorPantalla(
        esActualizacion ? "Actualizando la inspección…" : "Guardando la recepción e inspección…",
        () => guardarRecepcionApi(
          ordenDetalle.id,
          inspeccion,
          esActualizacion,
          fotografias,
        ),
      );
    } catch (error) {
      mostrarAviso(error instanceof Error ? error.message : "No fue posible guardar la recepción en la API");
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
    setInspecciones((actuales) => ({
      ...actuales,
      [ordenDetalle.id]: resultadoGuardado.inspeccion,
    }));
    setMostrarRecepcion(false);
    setOrdenDetalle(null);
    mostrarAviso(
      resultadoGuardado.advertencia || (esActualizacion
        ? "Inspección actualizada correctamente"
        : "Recepción guardada; la orden pasó a diagnóstico"),
    );
  }

  async function eliminarEvidenciaOrden(evidenciaId: number) {
    if (!ordenDetalle) return;
    const ordenServicioId = ordenDetalle.id;
    await ejecutarConCargadorPantalla(
      "Eliminando la fotografía de la inspección…",
      () => eliminarEvidenciaApi(ordenServicioId, evidenciaId),
    );
    setInspecciones((actuales) => {
      const inspeccion = actuales[ordenServicioId];
      if (!inspeccion) return actuales;
      return {
        ...actuales,
        [ordenServicioId]: {
          ...inspeccion,
          evidencias: inspeccion.evidencias.filter(
            (evidencia) => evidencia.id !== evidenciaId,
          ),
        },
      };
    });
    mostrarAviso("Fotografía eliminada de la inspección");
  }

  function actualizarOrdenEnPantalla(ordenActualizada: OrdenTaller) {
    setOrdenDetalle(ordenActualizada);
    setOrdenes((actuales) => actuales.map((orden) =>
      orden.id === ordenActualizada.id ? ordenActualizada : orden,
    ));
  }

  function mostrarAviso(mensaje: string) {
    setAviso(mensaje);
    window.setTimeout(() => setAviso(""), 3600);
  }

  async function cambiarTaller(empresaNovaId: number) {
    if (!sesion || empresaNovaId === sesion.taller.id) return;
    setCambiandoTaller(true);
    try {
      const nuevaSesion = await ejecutarConCargadorPantalla(
        "Cambiando el taller activo…",
        () => seleccionarTallerApi(empresaNovaId),
      );
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
    await ejecutarConCargadorPantalla("Cerrando la sesión…", cerrarSesionApi);
    setSesion(null);
    setOrdenes([]);
    setClientes([]);
    setVehiculos([]);
    setMostrarAdministracion(false);
  }

  if (cargandoSesion) {
    return <CargadorPantalla mensaje="Preparando Talleres…" />;
  }

  if (!sesion) {
    return <PantallaInicioSesion alIngresar={setSesion} />;
  }

  return (
    <div className="aplicacion">
      <BarraLateral
        vista={vista}
        nombreTaller={sesion.taller.nombreComercial || sesion.taller.nombreLegal}
        logoTaller={sesion.taller.logo}
        alNavegar={navegar}
      />

      <div className={`superficie ${procesoActivo ? "proceso-activo" : ""}`}>
        <header className="barra-superior">
          <div className="marca-compacta">
            <LogoTaller
              nombreTaller={sesion.taller.nombreComercial || sesion.taller.nombreLegal}
              logo={sesion.taller.logo}
            />
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
                  marcasVehiculo={marcasVehiculo}
                  modelosVehiculo={modelosVehiculo}
                  clienteInicialId={clienteInicialNuevaOrdenId}
                  guardando={guardandoOrden}
                  guardandoCliente={guardandoCliente}
                  guardandoVehiculo={guardandoVehiculo}
                  alEnviar={crearOrden}
                  alRegistrarCliente={registrarClienteDesdeOrden}
                  alRegistrarVehiculo={registrarVehiculoDesdeOrden}
                  alCrearMarca={crearMarcaVehiculo}
                  alCrearModelo={crearModeloVehiculo}
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
                  marcas={marcasVehiculo}
                  modelos={modelosVehiculo}
                  vehiculo={vehiculoEditando}
                  guardando={guardandoVehiculo}
                  alEnviar={guardarVehiculo}
                  alCrearMarca={crearMarcaVehiculo}
                  alCrearModelo={crearModeloVehiculo}
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
                  alEliminarEvidencia={eliminarEvidenciaOrden}
                />
              ) : (
                <DetalleOrden
                  orden={ordenDetalle}
                  cliente={clientes.find((cliente) => cliente.id === ordenDetalle.clienteId)}
                  inspeccion={inspecciones[ordenDetalle.id]}
                  alRecibir={() => setMostrarRecepcion(true)}
                  alActualizarOrden={actualizarOrdenEnPantalla}
                  alMostrarAviso={mostrarAviso}
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

function PantallaInicioSesion({
  alIngresar,
}: {
  alIngresar: (sesion: SesionTallerApi) => void;
}) {
  const { iniciarCargaPantalla, ejecutarConCargadorPantalla } = useCargadorPantalla();
  const [enviando, setEnviando] = useState(false);
  const [proveedorEnviando, setProveedorEnviando] = useState<string | null>(null);
  const [error, setError] = useState("");
  const [mostrarContrasena, setMostrarContrasena] = useState(false);

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
      const sesion = await ejecutarConCargadorPantalla(
        "Validando el acceso al taller…",
        () => iniciarSesionApi({
          usuario: String(datos.get("usuario")),
          contrasena: String(datos.get("contrasena")),
          recordarme: datos.has("recordarme"),
        }),
      );
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
          <span>Taller Smart</span>
        </div>
        <div className="mensaje-presentacion-acceso">
          <span className="sobrelinea">Integrado con SMART TPV NOVA</span>
          <h1>Todo tu taller.<br />Una sola vista.</h1>
          <p>Gestiona órdenes, clientes, vehículos e inventario de forma simple y rápida.</p>
        </div>
      </section>

      <section className="contenedor-login">
        <form className="formulario-login" onSubmit={enviar}>
          <header className="encabezado-login">
            <span className="sobrelinea">Bienvenido de nuevo</span>
            <h2>Inicia sesión</h2>
            <p>Usa tus credenciales de SMART TPV NOVA.</p>
          </header>

          {error && <div className="mensaje-acceso-error" role="alert"><TriangleAlert size={20} />{error}</div>}

          <label className="campo-login">
            <span>Usuario</span>
            <span className="control-login">
              <UserRound size={19} aria-hidden="true" />
              <input
                name="usuario"
                type="text"
                autoComplete="username"
                placeholder="Ingresa tu usuario"
                required
                maxLength={256}
              />
            </span>
          </label>
          <label className="campo-login">
            <span>Contraseña</span>
            <span className="control-login">
              <LockKeyhole size={19} aria-hidden="true" />
              <input
                name="contrasena"
                type={mostrarContrasena ? "text" : "password"}
                autoComplete="current-password"
                placeholder="Ingresa tu contraseña"
                required
                maxLength={256}
              />
              <button
                type="button"
                className="alternar-contrasena"
                aria-label={mostrarContrasena ? "Ocultar contraseña" : "Mostrar contraseña"}
                aria-pressed={mostrarContrasena}
                onClick={() => setMostrarContrasena((visible) => !visible)}
              >
                {mostrarContrasena ? <EyeOff size={19} /> : <Eye size={19} />}
              </button>
            </span>
          </label>
          <label className="opcion-recordarme">
            <input name="recordarme" type="checkbox" />
            <span>Recordarme</span>
          </label>
          <button type="submit" className="boton-primario boton-ancho boton-ingresar" disabled={enviando}>
            <span>Ingresar al taller</span>
            <ChevronRight size={19} aria-hidden="true" />
          </button>
          <div className="separador-login"><span>También puedes ingresar con</span></div>
          <div className="botones-proveedor-login">
            <button
              type="button"
              className="boton-proveedor-login"
              disabled={Boolean(proveedorEnviando)}
              onClick={() => {
                setProveedorEnviando("google");
                iniciarCargaPantalla("Conectando con Google…");
                window.location.assign(`${obtenerDireccionApi()}/api/autenticacion/externo/google`);
              }}
            >
              <span className="distintivo-proveedor" aria-hidden="true">G</span>
              <span>Google</span>
            </button>
            <button
              type="button"
              className="boton-proveedor-login"
              disabled={Boolean(proveedorEnviando)}
              onClick={() => {
                setProveedorEnviando("microsoft");
                iniciarCargaPantalla("Conectando con Microsoft…");
                window.location.assign(`${obtenerDireccionApi()}/api/autenticacion/externo/microsoft`);
              }}
            >
              <span className="distintivo-proveedor distintivo-microsoft" aria-hidden="true"><i /><i /><i /><i /></span>
              <span>Microsoft</span>
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
  const { ejecutarConCargadorPantalla } = useCargadorPantalla();
  const [talleres, setTalleres] = useState<TallerSincronizadoApi[]>([]);
  const [empresaNovaId, setEmpresaNovaId] = useState("");
  const [cargando, setCargando] = useState(true);
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState("");
  const [tallerPorRetirar, setTallerPorRetirar] = useState<TallerSincronizadoApi | null>(null);

  const cargar = useCallback(async (mostrarCargador = true) => {
    setCargando(true);
    try {
      const consulta = () => listarTalleresSincronizadosApi();
      setTalleres(mostrarCargador
        ? await ejecutarConCargadorPantalla("Consultando los talleres sincronizados…", consulta)
        : await consulta());
      setError("");
    } catch (excepcion) {
      setError(excepcion instanceof Error ? excepcion.message : "No fue posible consultar los talleres.");
    } finally {
      setCargando(false);
    }
  }, [ejecutarConCargadorPantalla]);

  useEffect(() => {
    const identificador = window.setTimeout(() => { void cargar(); }, 0);
    return () => window.clearTimeout(identificador);
  }, [cargar]);

  async function agregar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    setGuardando(true);
    try {
      await ejecutarConCargadorPantalla("Agregando el taller a la sincronización…", async () => {
        await agregarTallerSincronizadoApi(Number(empresaNovaId));
        setEmpresaNovaId("");
        await cargar(false);
        alActualizarSesion(await obtenerSesionApi(new AbortController().signal));
      });
    } catch (excepcion) {
      setError(excepcion instanceof Error ? excepcion.message : "No fue posible agregar el taller.");
    } finally {
      setGuardando(false);
    }
  }

  async function retirar() {
    if (!tallerPorRetirar) return;
    setGuardando(true);
    setError("");
    try {
      await ejecutarConCargadorPantalla("Retirando el taller de la sincronización…", async () => {
        await retirarTallerSincronizadoApi(tallerPorRetirar.empresaNovaId);
        await cargar(false);
        alActualizarSesion(await obtenerSesionApi(new AbortController().signal));
      });
      setTallerPorRetirar(null);
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
          <button className="boton-primario" disabled={guardando}>Agregar taller</button>
        </form>
        {error && <div className="estado-datos estado-datos-error" role="alert"><TriangleAlert size={20} />{error}</div>}
        {!cargando && (
          <div className="lista-talleres-admin">
            {talleres.map((taller) => (
              <article key={taller.empresaNovaId} className={!taller.activo ? "taller-retirado" : ""}>
                <div>
                  <span className="sobrelinea">Empresa {taller.empresaNovaId}</span>
                  <strong>{taller.nombreComercial || taller.nombreLegal}</strong>
                  <small>{taller.activo ? "Disponible para iniciar sesión" : "Retirado de la sincronización"}</small>
                </div>
                {taller.activo && <button className="boton-secundario" disabled={guardando} onClick={() => { setError(""); setTallerPorRetirar(taller); }}>Retirar</button>}
              </article>
            ))}
          </div>
        )}
      </div>
      <p className="nota-administracion">El usuario actual: {sesion.nombreUsuario}. La autorización se vuelve a comprobar en SMART TPV NOVA en cada operación.</p>
      {tallerPorRetirar && (
        <ModalConfirmacion
          titulo="Retirar taller"
          descripcion={`¿Deseas retirar ${tallerPorRetirar.nombreComercial || tallerPorRetirar.nombreLegal} de la sincronización?`}
          detalle="El taller dejará de estar disponible para iniciar sesión."
          etiquetaConfirmar="Retirar taller"
          procesando={guardando}
          mensajeError={error}
          alCancelar={() => setTallerPorRetirar(null)}
          alConfirmar={retirar}
        />
      )}
    </section>
  );
}

function BarraLateral({
  vista,
  nombreTaller,
  logoTaller,
  alNavegar,
}: {
  vista: Vista;
  nombreTaller: string;
  logoTaller: string | null;
  alNavegar: (vista: Vista) => void;
}) {
  return (
    <aside className="barra-lateral">
      <button className="marca" onClick={() => alNavegar("inicio")} aria-label="Ir al inicio">
        <LogoTaller nombreTaller={nombreTaller} logo={logoTaller} />
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

function LogoTaller({ nombreTaller, logo }: { nombreTaller: string; logo: string | null }) {
  return (
    <span className="marca-simbolo" aria-hidden="true">
      <span>{nombreTaller.trim().charAt(0).toUpperCase() || "T"}</span>
      {logo && (
        // El origen del logo se configura por empresa y no se conoce durante la compilación.
        // eslint-disable-next-line @next/next/no-img-element
        <img
          src={logo}
          alt=""
          onError={(evento) => { evento.currentTarget.hidden = true; }}
        />
      )}
    </span>
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
          {taller.nombreComercial && taller.nombreLegal !== taller.nombreComercial && (
            <span className="dato-taller-ancho"><strong>Razón social</strong><em>{taller.nombreLegal}</em></span>
          )}
          <span className="dato-taller-ruc"><strong>RUC</strong><em>{taller.ruc || "No registrado"}</em></span>
          <span className="dato-taller-telefono"><strong>Teléfono</strong><em>+{taller.prefijoTelefono} {taller.telefono}</em></span>
          <span className="dato-taller-contacto"><strong>Correo</strong><em>{taller.correo || "No registrado"}</em></span>
          <span className="dato-taller-ancho"><strong>Dirección</strong><em>{[taller.direccion, taller.ciudad, taller.barrio, taller.calle].filter(Boolean).join(", ") || "No registrada"}</em></span>
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
  const [busquedaOrdenes, setBusquedaOrdenes] = useState("");
  const ordenesVisibles = useMemo(() => {
    const termino = busquedaOrdenes.trim().toLocaleLowerCase("es");
    if (!termino) return ordenes;
    return ordenes.filter((orden) =>
      [orden.numero, orden.cliente, orden.vehiculo, orden.placa]
        .join(" ")
        .toLocaleLowerCase("es")
        .includes(termino),
    );
  }, [busquedaOrdenes, ordenes]);
  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Operación</span><h1>Órdenes de servicio</h1><p>Sigue cada vehículo desde la recepción hasta la entrega.</p></div>
        <button className="boton-primario" onClick={alCrearOrden}><Plus size={21} />Nueva orden</button>
      </section>
      <div className="filtros" role="group" aria-label="Filtrar órdenes por estado">
        {filtros.map((item) => <button key={item} className={filtro === item ? "activo" : ""} onClick={() => alFiltrar(item)}>{item}</button>)}
      </div>
      <BuscadorListado valor={busquedaOrdenes} alCambiar={setBusquedaOrdenes} etiqueta="Buscar órdenes" marcador="Número, cliente, vehículo o placa" />
      <section className="panel tabla-ordenes">
        <div className="cabecera-tabla"><span>Orden y vehículo</span><span>Cliente</span><span>Estado</span><span>Responsable</span><span /></div>
        {ordenesVisibles.map((orden) => (
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
        {!cargando && ordenes.length > 0 && ordenesVisibles.length === 0 && (
          <EstadoVacio icono={Search} titulo="Sin coincidencias" detalle="Prueba con otro número, cliente, vehículo o placa." />
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
  const [busquedaClientes, setBusquedaClientes] = useState("");
  const clientesVisibles = useMemo(() => {
    const termino = busquedaClientes.trim().toLocaleLowerCase("es");
    if (!termino) return clientes;
    return clientes.filter((cliente) =>
      [cliente.nombre, cliente.telefono]
        .join(" ")
        .toLocaleLowerCase("es")
        .includes(termino),
    );
  }, [busquedaClientes, clientes]);
  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Relaciones</span><h1>Clientes</h1><p>Información de contacto y vehículos en un solo lugar.</p></div>
        <button className="boton-primario" onClick={alCrearCliente}><Plus size={21} />Nuevo cliente</button>
      </section>
      <BuscadorListado valor={busquedaClientes} alCambiar={setBusquedaClientes} etiqueta="Buscar clientes" marcador="Nombre o teléfono" />
      <section className="panel lista-clientes">
        {clientesVisibles.map((cliente) => (
          <article className={`fila-cliente ${cliente.activo ? "" : "cliente-inactivo"}`} key={cliente.id}>
            <span className="avatar avatar-cliente">{cliente.iniciales}</span>
            <span className="resumen-cliente"><strong>{cliente.nombre}</strong><small>{cliente.telefono}</small></span>
            <span className="datos-cliente"><span><CarFront size={16} />{cliente.cantidadVehiculos} vehículos</span><span><ClipboardList size={16} />{cliente.ordenActiva || "Sin orden activa"}</span></span>
            {!cliente.activo && <span className="estado-cliente-inactivo">Inactivo</span>}
            <span className="acciones-cliente">
              <button className="boton-icono" type="button" onClick={() => alEditarCliente(cliente)} aria-label={`Editar cliente ${cliente.nombre}`}><Pencil size={18} /></button>
              <button className="boton-primario" type="button" disabled={!cliente.activo} onClick={() => alCrearOrden(cliente.id)}>Crear orden</button>
            </span>
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
        {!cargando && clientes.length > 0 && clientesVisibles.length === 0 && (
          <EstadoVacio icono={Users} titulo="Sin coincidencias" detalle="Prueba con otro nombre o teléfono." />
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
  const [busquedaVehiculos, setBusquedaVehiculos] = useState("");
  const vehiculosVisibles = useMemo(() => {
    const termino = busquedaVehiculos.trim().toLocaleLowerCase("es");
    if (!termino) return vehiculos;
    return vehiculos.filter((vehiculo) =>
      [vehiculo.nombre, vehiculo.detalle, vehiculo.placa, vehiculo.cliente]
        .join(" ")
        .toLocaleLowerCase("es")
        .includes(termino),
    );
  }, [busquedaVehiculos, vehiculos]);
  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Parque vehicular</span><h1>Vehículos</h1><p>Historial y situación actual de cada unidad.</p></div>
        <button className="boton-primario" onClick={alCrear}><Plus size={21} />Nuevo vehículo</button>
      </section>
      <BuscadorListado valor={busquedaVehiculos} alCambiar={setBusquedaVehiculos} etiqueta="Buscar vehículos" marcador="Placa, marca, modelo o cliente" />
      <section className="panel lista-vehiculos">
        {vehiculosVisibles.map((vehiculo) => (
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
        {!cargando && vehiculos.length > 0 && vehiculosVisibles.length === 0 && (
          <EstadoVacio icono={CarFront} titulo="Sin coincidencias" detalle="Prueba con otra placa, marca, modelo o cliente." />
        )}
      </section>
    </>
  );
}

function BuscadorListado({
  valor,
  alCambiar,
  etiqueta,
  marcador,
}: {
  valor: string;
  alCambiar: (valor: string) => void;
  etiqueta: string;
  marcador: string;
}) {
  return (
    <label className="buscador-listado">
      <Search size={19} aria-hidden="true" />
      <span className="solo-lectores">{etiqueta}</span>
      <input value={valor} onChange={(evento) => alCambiar(evento.target.value)} placeholder={marcador} />
    </label>
  );
}

function VistaInventario({ taller }: { taller: TallerSesionApi }) {
  const { ejecutarConCargadorPantalla } = useCargadorPantalla();
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
    ejecutarConCargadorPantalla("Consultando las bodegas del taller…", listarBodegasApi)
      .then((resultado) => {
        setBodegas(resultado);
        setBodegaId(resultado.find((item) => item.esPrincipal)?.id ?? resultado[0]?.id ?? null);
      })
      .catch((excepcion) => setError(excepcion instanceof Error ? excepcion.message : "No fue posible consultar las bodegas."))
      .finally(() => setCargando(false));
  }, [ejecutarConCargadorPantalla, taller.id]);
  useEffect(() => {
    if (bodegaId === null) return;
    const identificador = window.setTimeout(() => {
      setCargandoExistencias(true); setError("");
      ejecutarConCargadorPantalla(
        "Consultando las existencias de la bodega…",
        () => listarExistenciasApi(bodegaId),
      ).then(setArticulos).catch((excepcion) => { setArticulos([]); setError(excepcion instanceof Error ? excepcion.message : "No fue posible consultar las existencias."); }).finally(() => setCargandoExistencias(false));
    }, 0);
    return () => window.clearTimeout(identificador);
  }, [bodegaId, ejecutarConCargadorPantalla]);

  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Repuestos y consumibles</span><h1>Inventario</h1><p>Disponibilidad rápida para decidir sin salir de la orden.</p></div>
        <button className="boton-primario"><Plus size={21} />Registrar entrada</button>
      </section>
      <section className="panel inventario">
        <div className="controles-inventario"><div className="selector-inventario"><label>Bodega<select value={bodegaId ?? ""} onChange={(evento) => { setBodegaId(Number(evento.target.value)); setPaginaInventario(1); }}>{bodegas.map((bodega) => <option key={bodega.id} value={bodega.id}>{bodega.nombre}{bodega.esPrincipal ? " · Principal" : ""}</option>)}</select></label></div><label className="busqueda-inventario">Buscar<input value={busquedaInventario} onChange={(evento) => { setBusquedaInventario(evento.target.value); setPaginaInventario(1); }} placeholder="Nombre o código" /></label></div>
        {cargando || cargandoExistencias ? null : error ? <div className="estado-datos estado-datos-error" role="alert">{error}</div> : articulos.length === 0 ? <EstadoVacio icono={Boxes} titulo="Sin existencias" detalle="La bodega seleccionada no tiene artículos disponibles." /> : articulosFiltrados.length === 0 ? <EstadoVacio icono={Boxes} titulo="Sin coincidencias" detalle="No hay artículos que coincidan con la búsqueda." /> : <><div className="lista-inventario">{articulosPagina.map((articulo) => <article key={articulo.productoId}><div><strong>{articulo.nombre}</strong><small>{articulo.codigo} · {articulo.unidadMedida}</small></div><b>{articulo.existencia}</b></article>)}</div><div className="paginacion-inventario"><button type="button" disabled={paginaInventario <= 1} onClick={() => setPaginaInventario((pagina) => pagina - 1)}>Anterior</button><span>Página {paginaInventario} de {totalPaginas}</span><button type="button" disabled={paginaInventario >= totalPaginas} onClick={() => setPaginaInventario((pagina) => pagina + 1)}>Siguiente</button></div></>}
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
  marcasVehiculo,
  modelosVehiculo,
  clienteInicialId,
  guardando,
  guardandoCliente,
  guardandoVehiculo,
  alEnviar,
  alRegistrarCliente,
  alRegistrarVehiculo,
  alCrearMarca,
  alCrearModelo,
}: {
  clientes: ClienteTaller[];
  vehiculos: VehiculoTaller[];
  marcasVehiculo: MarcaVehiculoApi[];
  modelosVehiculo: ModeloVehiculoApi[];
  clienteInicialId: number | null;
  guardando: boolean;
  guardandoCliente: boolean;
  guardandoVehiculo: boolean;
  alEnviar: (evento: FormEvent<HTMLFormElement>) => void;
  alRegistrarCliente: (datos: FormData) => Promise<ClienteTaller>;
  alRegistrarVehiculo: (datos: FormData) => Promise<VehiculoTaller>;
  alCrearMarca: (nombre: string) => Promise<MarcaVehiculoApi>;
  alCrearModelo: (marcaVehiculoId: number, nombre: string) => Promise<ModeloVehiculoApi>;
}) {
  const [clienteId, setClienteId] = useState(
    clienteInicialId && clientes.some((cliente) => cliente.id === clienteInicialId)
      ? clienteInicialId
      : clientes[0]?.id ?? 0,
  );
  const [vehiculoId, setVehiculoId] = useState(
    vehiculos.find((vehiculo) =>
      vehiculo.clienteId === (clienteInicialId ?? clientes[0]?.id))?.id ?? 0,
  );
  const [altaVisible, setAltaVisible] = useState<"cliente" | "vehiculo" | null>(null);
  const [motivo, setMotivo] = useState("");
  const [dictando, setDictando] = useState(false);
  const [dictadoCompatible, setDictadoCompatible] = useState<boolean | null>(null);
  const [mensajeDictado, setMensajeDictado] = useState("");
  const reconocimientoVoz = useRef<ReconocimientoVoz | null>(null);
  const vehiculosCliente = useMemo(
    () => vehiculos.filter((vehiculo) => vehiculo.clienteId === clienteId),
    [clienteId, vehiculos],
  );
  const formularioDisponible = clientes.length > 0 && vehiculosCliente.length > 0;

  useEffect(() => {
    if (!altaVisible) return;

    const desbordamientoAnterior = document.body.style.overflow;
    const cerrarConEscape = (evento: KeyboardEvent) => {
      if (evento.key === "Escape") setAltaVisible(null);
    };
    document.body.style.overflow = "hidden";
    window.addEventListener("keydown", cerrarConEscape);

    return () => {
      document.body.style.overflow = desbordamientoAnterior;
      window.removeEventListener("keydown", cerrarConEscape);
    };
  }, [altaVisible]);

  useEffect(() => {
    return () => reconocimientoVoz.current?.abort();
  }, []);

  function alternarDictado() {
    if (dictando) {
      reconocimientoVoz.current?.stop();
      return;
    }

    const ventanaConReconocimiento = window as typeof window & {
      SpeechRecognition?: ConstructorReconocimientoVoz;
      webkitSpeechRecognition?: ConstructorReconocimientoVoz;
    };
    const Constructor = ventanaConReconocimiento.SpeechRecognition
      ?? ventanaConReconocimiento.webkitSpeechRecognition;
    if (!Constructor) {
      setDictadoCompatible(false);
      setMensajeDictado("El dictado por voz no está disponible en este navegador.");
      return;
    }

    const reconocimiento = new Constructor();
    const motivoAntesDeDictar = motivo.trim();
    let dictadoConError = false;
    setDictadoCompatible(true);
    reconocimiento.lang = "es-NI";
    reconocimiento.continuous = true;
    reconocimiento.interimResults = true;
    reconocimiento.onresult = (evento) => {
      let transcripcion = "";
      for (let indice = 0; indice < evento.results.length; indice += 1) {
        transcripcion += evento.results[indice][0].transcript;
      }
      setMotivo([motivoAntesDeDictar, transcripcion.trim()].filter(Boolean).join(" "));
    };
    reconocimiento.onerror = (evento) => {
      dictadoConError = true;
      const mensajes: Record<string, string> = {
        "not-allowed": "Permite el acceso al micrófono para usar el dictado.",
        "audio-capture": "No se encontró un micrófono disponible.",
        network: "No fue posible procesar la voz. Revisa la conexión e inténtalo de nuevo.",
        "no-speech": "No se detectó voz. Toca el micrófono para volver a intentarlo.",
      };
      setMensajeDictado(mensajes[evento.error] ?? "El dictado se interrumpió. Puedes intentarlo de nuevo.");
    };
    reconocimiento.onend = () => {
      setDictando(false);
      reconocimientoVoz.current = null;
      if (!dictadoConError) {
        setMensajeDictado("Dictado finalizado. Revisa o corrige la descripción si es necesario.");
      }
    };

    reconocimientoVoz.current = reconocimiento;
    setMensajeDictado("Escuchando… habla con claridad.");
    setDictando(true);
    try {
      reconocimiento.start();
    } catch {
      setDictando(false);
      reconocimientoVoz.current = null;
      setMensajeDictado("No fue posible iniciar el dictado. Inténtalo nuevamente.");
    }
  }

  async function registrarCliente(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    try {
      const clienteGuardado = await alRegistrarCliente(new FormData(evento.currentTarget));
      setClienteId(clienteGuardado.id);
      setVehiculoId(0);
      setAltaVisible(null);
    } catch {
      // El aviso global conserva el formulario abierto para que el usuario pueda corregirlo.
    }
  }

  async function registrarVehiculo(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    try {
      const vehiculoGuardado = await alRegistrarVehiculo(new FormData(evento.currentTarget));
      setClienteId(vehiculoGuardado.clienteId);
      setVehiculoId(vehiculoGuardado.id);
      setAltaVisible(null);
    } catch {
      // El aviso global conserva el formulario abierto para que el usuario pueda corregirlo.
    }
  }

  return (
    <div className="flujo-nueva-orden">
      <form className="formulario formulario-nueva-orden" onSubmit={alEnviar}>
        <div className="paso-formulario"><span>1</span><div><strong>Cliente y vehículo</strong><small>Selecciona a quién vamos a atender</small></div></div>
        <div className="campo-con-accion">
          <label>
            Cliente
            <select
              name="clienteId"
              required
              value={clienteId || ""}
              onChange={(evento) => {
                const siguienteClienteId = Number(evento.target.value);
                setClienteId(siguienteClienteId);
                setVehiculoId(
                  vehiculos.find((vehiculo) => vehiculo.clienteId === siguienteClienteId)?.id ?? 0,
                );
              }}
            >
              {clientes.length === 0 && <option value="">No hay clientes registrados</option>}
              {clientes.map((cliente) => <option key={cliente.id} value={cliente.id}>{cliente.nombre}</option>)}
            </select>
          </label>
          <button
            className="boton-alta-rapida"
            type="button"
            aria-label="Registrar nuevo cliente"
            aria-haspopup="dialog"
            onClick={() => setAltaVisible("cliente")}
          >
            <Plus size={19} />
          </button>
        </div>
        <div className="campo-con-accion">
          <label>
            Vehículo
            <select
              name="vehiculoId"
              required
              disabled={!formularioDisponible}
              value={vehiculoId || ""}
              onChange={(evento) => setVehiculoId(Number(evento.target.value))}
            >
              {vehiculosCliente.length === 0 && <option value="">El cliente no tiene vehículos</option>}
              {vehiculosCliente.map((vehiculo) => (
                <option key={vehiculo.id} value={vehiculo.id}>{vehiculo.nombre} · {vehiculo.placa}</option>
              ))}
            </select>
          </label>
          <button
            className="boton-alta-rapida"
            type="button"
            disabled={!clienteId}
            aria-label="Registrar nuevo vehículo"
            aria-haspopup="dialog"
            onClick={() => setAltaVisible("vehiculo")}
          >
            <Plus size={19} />
          </button>
        </div>
        <div className="separador-formulario" />
        <div className="paso-formulario"><span>2</span><div><strong>Motivo de visita</strong><small>Describe lo que reporta el cliente</small></div></div>
        <div className="campo-descripcion-voz">
          <label htmlFor="motivo-orden">Descripción</label>
          <textarea
            id="motivo-orden"
            name="motivo"
            rows={4}
            required
            value={motivo}
            onChange={(evento) => setMotivo(evento.target.value)}
            placeholder="Ej. Se escucha un ruido al frenar..."
          />
          <div className="controles-dictado">
            <button
              className={dictando ? "boton-dictado activo" : "boton-dictado"}
              type="button"
              onClick={alternarDictado}
              disabled={dictadoCompatible === false}
              aria-pressed={dictando}
              aria-describedby="estado-dictado"
            >
              {dictando ? <Square size={17} fill="currentColor" /> : <Mic size={19} />}
              {dictando ? "Detener dictado" : "Dictar descripción"}
            </button>
            <span id="estado-dictado" className={dictando ? "estado-dictado escuchando" : "estado-dictado"} role="status" aria-live="polite">
              {mensajeDictado || (dictadoCompatible === false
                ? "Dictado no disponible en este navegador."
                : "También puedes escribir o corregir el texto.")}
            </span>
          </div>
        </div>
        <div className="opciones-prioridad"><label><input type="radio" name="prioridad" defaultChecked />Normal</label><label><input type="radio" name="prioridad" />Prioritaria</label></div>
        <button className="boton-primario boton-ancho" type="submit" disabled={!formularioDisponible || guardando}>
          <Check size={20} />Crear orden de servicio
        </button>
      </form>

      {altaVisible && (
        <div className="fondo-modal-alta">
          <button
            className="cerrador-modal-fondo"
            type="button"
            aria-label="Cerrar formulario de registro"
            onClick={() => setAltaVisible(null)}
          />
          <section
            className="modal-alta"
            role="dialog"
            aria-modal="true"
            aria-labelledby="titulo-modal-alta"
          >
            <div className="cabecera-modal-alta">
              <div>
                <span className="sobrelinea">Nueva orden</span>
                <h2 id="titulo-modal-alta">{altaVisible === "cliente" ? "Nuevo cliente" : "Nuevo vehículo"}</h2>
                <p>Al guardar quedará seleccionado automáticamente.</p>
              </div>
              <button className="boton-icono" type="button" onClick={() => setAltaVisible(null)} aria-label="Cerrar formulario de registro"><X size={20} /></button>
            </div>
            <div className="contenido-modal-alta">
              {altaVisible === "cliente" ? (
                <FormularioCliente cliente={null} guardando={guardandoCliente} alEnviar={registrarCliente} />
              ) : (
                <FormularioVehiculo
                  key={clienteId}
                  clientes={clientes}
                  marcas={marcasVehiculo}
                  modelos={modelosVehiculo}
                  clienteInicialId={clienteId}
                  vehiculo={null}
                  guardando={guardandoVehiculo}
                  alEnviar={registrarVehiculo}
                  alCrearMarca={alCrearMarca}
                  alCrearModelo={alCrearModelo}
                />
              )}
            </div>
          </section>
        </div>
      )}
    </div>
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
        <div><strong>Datos básicos</strong><small>Los campos con * son obligatorios</small></div>
      </div>
      <label>
        Nombre completo <span aria-hidden="true">*</span>
        <input name="nombre" type="text" required minLength={2} maxLength={150} defaultValue={cliente?.nombre ?? ""} autoComplete="name" placeholder="Ej. María Fernández López" />
      </label>
      <label>
        Teléfono <span aria-hidden="true">*</span>
        <input name="telefono" type="tel" required minLength={7} maxLength={30} defaultValue={cliente?.telefono ?? ""} autoComplete="tel" inputMode="tel" placeholder="Ej. 8888-0000" />
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
        <Check size={20} />{cliente ? "Guardar cambios" : "Registrar cliente"}
      </button>
    </form>
  );
}

function FormularioVehiculo({
  clientes,
  marcas,
  modelos,
  clienteInicialId,
  vehiculo,
  guardando,
  alEnviar,
  alCrearMarca,
  alCrearModelo,
}: {
  clientes: ClienteTaller[];
  marcas: MarcaVehiculoApi[];
  modelos: ModeloVehiculoApi[];
  clienteInicialId?: number;
  vehiculo: VehiculoTaller | null;
  guardando: boolean;
  alEnviar: (evento: FormEvent<HTMLFormElement>) => void;
  alCrearMarca: (nombre: string) => Promise<MarcaVehiculoApi>;
  alCrearModelo: (marcaVehiculoId: number, nombre: string) => Promise<ModeloVehiculoApi>;
}) {
  const marcaInicialId = vehiculo?.marcaVehiculoId ?? marcas.find((marca) => marca.activa)?.id ?? 0;
  const [marcaVehiculoId, setMarcaVehiculoId] = useState(marcaInicialId);
  const [modeloVehiculoId, setModeloVehiculoId] = useState(
    vehiculo?.modeloVehiculoId ??
      modelos.find((modelo) => modelo.marcaVehiculoId === marcaInicialId && modelo.activo)?.id ??
      0,
  );
  const [nombreNuevaMarca, setNombreNuevaMarca] = useState("");
  const [nombreNuevoModelo, setNombreNuevoModelo] = useState("");
  const [mostrandoAltaMarca, setMostrandoAltaMarca] = useState(false);
  const [mostrandoAltaModelo, setMostrandoAltaModelo] = useState(false);
  const [guardandoCatalogo, setGuardandoCatalogo] = useState(false);
  const modelosMarca = modelos.filter(
    (modelo) => modelo.marcaVehiculoId === marcaVehiculoId &&
      (modelo.activo || modelo.id === vehiculo?.modeloVehiculoId),
  );
  const formularioDisponible = clientes.length > 0 && modeloVehiculoId > 0;
  const anioActual = new Date().getFullYear();

  async function crearMarca() {
    if (nombreNuevaMarca.trim().length < 2) return;
    setGuardandoCatalogo(true);
    try {
      const marca = await alCrearMarca(nombreNuevaMarca.trim());
      setMarcaVehiculoId(marca.id);
      setModeloVehiculoId(0);
      setNombreNuevaMarca("");
      setMostrandoAltaMarca(false);
    } finally {
      setGuardandoCatalogo(false);
    }
  }

  async function crearModelo() {
    if (!marcaVehiculoId || !nombreNuevoModelo.trim()) return;
    setGuardandoCatalogo(true);
    try {
      const modelo = await alCrearModelo(marcaVehiculoId, nombreNuevoModelo.trim());
      setModeloVehiculoId(modelo.id);
      setNombreNuevoModelo("");
      setMostrandoAltaModelo(false);
    } finally {
      setGuardandoCatalogo(false);
    }
  }

  return (
    <form className="formulario" onSubmit={alEnviar}>
      <div className="paso-formulario">
        <span>1</span>
        <div><strong>Propietario e identificación</strong><small>Selecciona el cliente y registra la placa</small></div>
      </div>
      <label>
        Propietario
        <select name="clienteId" required defaultValue={vehiculo?.clienteId ?? clienteInicialId ?? clientes[0]?.id ?? ""} disabled={!formularioDisponible}>
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
      <div className="campo-con-accion">
        <label>
          Marca
          <select
            required
            value={marcaVehiculoId || ""}
            onChange={(evento) => {
              const marcaId = Number(evento.target.value);
              setMarcaVehiculoId(marcaId);
              setModeloVehiculoId(
                modelos.find((modelo) => modelo.marcaVehiculoId === marcaId && modelo.activo)?.id ?? 0,
              );
            }}
          >
            <option value="">Selecciona una marca</option>
            {marcas
              .filter((marca) => marca.activa || marca.id === vehiculo?.marcaVehiculoId)
              .map((marca) => <option key={marca.id} value={marca.id}>{marca.nombre}</option>)}
          </select>
        </label>
        <button className="boton-alta-rapida" type="button" aria-label="Agregar una marca al catálogo" aria-controls="alta-marca" aria-expanded={mostrandoAltaMarca} onClick={() => setMostrandoAltaMarca((visible) => !visible)}>
          <Plus size={19} />
        </button>
      </div>
      {mostrandoAltaMarca && (
        <div className="alta-catalogo" id="alta-marca">
        <div className="campo-con-accion">
          <input aria-label="Nombre de la nueva marca" value={nombreNuevaMarca} onChange={(evento) => setNombreNuevaMarca(evento.target.value)} minLength={2} maxLength={80} placeholder="Ej. Toyota" />
          <button type="button" className="boton-secundario" disabled={guardandoCatalogo || nombreNuevaMarca.trim().length < 2} onClick={crearMarca}><Plus size={18} />Agregar</button>
        </div>
        </div>
      )}
      <div className="campo-con-accion">
        <label>
          Modelo
          <select name="modeloVehiculoId" required value={modeloVehiculoId || ""} onChange={(evento) => setModeloVehiculoId(Number(evento.target.value))} disabled={!marcaVehiculoId}>
            <option value="">{marcaVehiculoId ? "Selecciona un modelo" : "Primero selecciona una marca"}</option>
            {modelosMarca.map((modelo) => <option key={modelo.id} value={modelo.id}>{modelo.nombre}</option>)}
          </select>
        </label>
        <button className="boton-alta-rapida" type="button" aria-label="Agregar un modelo a esta marca" aria-controls="alta-modelo" aria-expanded={mostrandoAltaModelo} disabled={!marcaVehiculoId} onClick={() => setMostrandoAltaModelo((visible) => !visible)}>
          <Plus size={19} />
        </button>
      </div>
      {mostrandoAltaModelo && (
        <div className="alta-catalogo" id="alta-modelo">
        <div className="campo-con-accion">
          <input aria-label="Nombre del nuevo modelo" value={nombreNuevoModelo} onChange={(evento) => setNombreNuevoModelo(evento.target.value)} maxLength={80} disabled={!marcaVehiculoId} placeholder="Ej. Hilux" />
          <button type="button" className="boton-secundario" disabled={guardandoCatalogo || !marcaVehiculoId || !nombreNuevoModelo.trim()} onClick={crearModelo}><Plus size={18} />Agregar</button>
        </div>
        </div>
      )}
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
        <Check size={20} />{vehiculo ? "Guardar cambios" : "Registrar vehículo"}
      </button>
    </form>
  );
}

function DetalleOrden({
  orden,
  cliente,
  inspeccion,
  alRecibir,
  alActualizarOrden,
  alMostrarAviso,
  alNotificar,
}: {
  orden: OrdenTaller;
  cliente?: ClienteTaller;
  inspeccion?: InspeccionVisual;
  alRecibir: () => void;
  alActualizarOrden: (orden: OrdenTaller) => void;
  alMostrarAviso: (mensaje: string) => void;
  alNotificar: () => void;
}) {
  const { ejecutarConCargadorPantalla } = useCargadorPantalla();
  const [resumen, setResumen] = useState<ResumenDetallesOrdenServicioApi>({ detalles: [], total: 0 });
  const [diagnostico, setDiagnostico] = useState<DiagnosticoOrdenServicioApi | null>(null);
  const [bodegas, setBodegas] = useState<BodegaInventarioApi[]>([]);
  const [bodegaId, setBodegaId] = useState<number | null>(null);
  const [articulos, setArticulos] = useState<ArticuloInventarioApi[]>([]);
  const [productoId, setProductoId] = useState<number | null>(null);
  const [busquedaProducto, setBusquedaProducto] = useState("");
  const [cantidadProducto, setCantidadProducto] = useState("1");
  const [buscandoProductos, setBuscandoProductos] = useState(false);
  const [mostrarCargoManual, setMostrarCargoManual] = useState(false);
  const [mensajeCargoManual, setMensajeCargoManual] = useState<{ texto: string; esError: boolean } | null>(null);
  const [detallePorEliminar, setDetallePorEliminar] = useState<DetalleOrdenServicioApi | null>(null);
  const [cargandoProceso, setCargandoProceso] = useState(true);
  const [guardandoProceso, setGuardandoProceso] = useState(false);
  const [errorProceso, setErrorProceso] = useState("");
  const etapas: EstadoOrden[] = [
    "Recepción",
    "Diagnóstico",
    "Por aprobar",
    "Reparación",
    "Lista para entregar",
  ];
  const indiceActual = Math.max(0, etapas.indexOf(orden.estado));
  const detallesEditables = orden.estado === "Reparación" || orden.estado === "Lista para entregar";
  const articuloSeleccionado = articulos.find((articulo) => articulo.productoId === productoId);
  const terminoBusquedaProducto = busquedaProducto.trim();
  const productosCoincidentes = articulos.slice(0, 6);

  useEffect(() => {
    if (!mensajeCargoManual || mensajeCargoManual.esError) return;

    const temporizador = window.setTimeout(() => {
      setMensajeCargoManual((mensajeActual) =>
        mensajeActual?.esError ? mensajeActual : null);
    }, 4000);
    return () => window.clearTimeout(temporizador);
  }, [mensajeCargoManual]);

  useEffect(() => {
    const controlador = new AbortController();
    ejecutarConCargadorPantalla("Consultando la información de la orden…", async () => {
      const [cargos, diagnosticoActual] = await Promise.all([
        obtenerDetallesOrdenApi(orden.id, controlador.signal),
        obtenerDiagnosticoOrdenApi(orden.id, controlador.signal),
      ]);
      return { cargos, diagnosticoActual };
    })
      .then((resultado) => {
        setResumen(resultado.cargos);
        setDiagnostico(resultado.diagnosticoActual);
        setErrorProceso("");
      })
      .catch((error) => {
        if (!controlador.signal.aborted) {
          setErrorProceso(error instanceof Error ? error.message : "No fue posible consultar los cargos.");
        }
      })
      .finally(() => {
        if (!controlador.signal.aborted) setCargandoProceso(false);
      });
    return () => controlador.abort();
  }, [ejecutarConCargadorPantalla, orden.id]);

  useEffect(() => {
    if (!detallesEditables) return;
    ejecutarConCargadorPantalla("Consultando las bodegas disponibles…", listarBodegasApi)
      .then((resultado) => {
        setBodegas(resultado);
        setBodegaId(resultado.find((bodega) => bodega.esPrincipal)?.id ?? resultado[0]?.id ?? null);
      })
      .catch((error) => setErrorProceso(error instanceof Error ? error.message : "No fue posible consultar las bodegas."));
  }, [detallesEditables, ejecutarConCargadorPantalla]);

  useEffect(() => {
    if (!detallesEditables || bodegaId === null || terminoBusquedaProducto.length < 2) {
      return;
    }

    const controlador = new AbortController();
    const espera = window.setTimeout(() => {
      setBuscandoProductos(true);
      listarExistenciasApi(bodegaId, terminoBusquedaProducto, controlador.signal)
        .then((resultado) => {
          if (!controlador.signal.aborted) setArticulos(resultado);
        })
        .catch((error) => {
          if (!controlador.signal.aborted) setErrorProceso(error instanceof Error ? error.message : "No fue posible buscar productos.");
        })
        .finally(() => {
          if (!controlador.signal.aborted) setBuscandoProductos(false);
        });
    }, 300);

    return () => {
      window.clearTimeout(espera);
      controlador.abort();
    };
  }, [bodegaId, detallesEditables, terminoBusquedaProducto]);

  async function avanzar(estado: "PendienteAprobacion" | "Reparacion" | "ListaParaEntrega", descripcion: string) {
    setGuardandoProceso(true);
    setErrorProceso("");
    try {
      const actualizada = await ejecutarConCargadorPantalla(
        "Actualizando el estado de la orden…",
        () => cambiarEstadoOrdenApi(orden.id, estado, descripcion),
      );
      alActualizarOrden(actualizada);
      alMostrarAviso(`La orden pasó a ${actualizada.estado.toLocaleLowerCase("es")}`);
      return actualizada;
    } catch (error) {
      setErrorProceso(error instanceof Error ? error.message : "No fue posible avanzar la orden.");
      return null;
    } finally {
      setGuardandoProceso(false);
    }
  }

  async function agregarProducto(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    if (bodegaId === null || productoId === null) return;
    const formulario = evento.currentTarget;
    setGuardandoProceso(true);
    setErrorProceso("");
    try {
      const resultado = await ejecutarConCargadorPantalla(
        "Agregando el producto a la orden…",
        () => agregarDetalleInventarioApi(orden.id, {
          bodegaId,
          productoId,
          cantidad: Number(cantidadProducto),
        }),
      );
      setResumen(resultado);
      const cantidad = Number(cantidadProducto);
      const descontado = articuloSeleccionado && articuloSeleccionado.existencia >= cantidad;
      alMostrarAviso(descontado
        ? "Producto agregado y existencia descontada"
        : "Producto agregado sin descontar inventario por existencia insuficiente");
      formulario.reset();
      setProductoId(null);
      setBusquedaProducto("");
      setCantidadProducto("1");
    } catch (error) {
      setErrorProceso(error instanceof Error ? error.message : "No fue posible agregar el producto.");
    } finally {
      setGuardandoProceso(false);
    }
  }

  async function agregarManual(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const formulario = evento.currentTarget;
    const datos = new FormData(formulario);
    setGuardandoProceso(true);
    setErrorProceso("");
    setMensajeCargoManual(null);
    try {
      const resultado = await ejecutarConCargadorPantalla(
        "Agregando el cargo a la orden…",
        () => agregarDetalleManualApi(orden.id, {
          descripcion: String(datos.get("descripcion")),
          unidadMedida: null,
          cantidad: Number(datos.get("cantidad")),
          precioUnitario: Number(datos.get("precioUnitario")),
        }),
      );
      setResumen(resultado);
      formulario.reset();
      setMensajeCargoManual({ texto: "Cargo manual agregado a la orden.", esError: false });
    } catch (error) {
      setMensajeCargoManual({ texto: error instanceof Error ? error.message : "No fue posible agregar el cargo.", esError: true });
    } finally {
      setGuardandoProceso(false);
    }
  }

  async function eliminarDetalle() {
    if (!detallePorEliminar) return;
    const devuelveExistencia = detallePorEliminar.tipo === "Inventario" && detallePorEliminar.existenciaDescontada;
    setGuardandoProceso(true);
    setErrorProceso("");
    try {
      setResumen(await ejecutarConCargadorPantalla(
        devuelveExistencia ? "Devolviendo el producto al inventario…" : "Eliminando el cargo de la orden…",
        () => eliminarDetalleOrdenApi(orden.id, detallePorEliminar.id),
      ));
      setDetallePorEliminar(null);
      alMostrarAviso(devuelveExistencia
        ? "Producto eliminado y existencia devuelta al inventario"
        : "Cargo eliminado de la orden");
    } catch (error) {
      setErrorProceso(error instanceof Error ? error.message : "No fue posible eliminar el cargo.");
    } finally {
      setGuardandoProceso(false);
    }
  }

  async function guardarDiagnostico(texto: string, fotografias: File[]) {
    setGuardandoProceso(true);
    setErrorProceso("");
    try {
      const resultado = await ejecutarConCargadorPantalla("Guardando el diagnóstico…", async () => {
        let actualizado = await guardarDiagnosticoOrdenApi(orden.id, texto);
        if (fotografias.length > 0) actualizado = await cargarEvidenciasDiagnosticoApi(orden.id, fotografias);
        return actualizado;
      });
      setDiagnostico(resultado);
      alMostrarAviso("Diagnóstico guardado correctamente");
    } catch (error) {
      const mensaje = error instanceof Error ? error.message : "No fue posible guardar el diagnóstico.";
      setErrorProceso(mensaje);
      throw error;
    } finally {
      setGuardandoProceso(false);
    }
  }

  async function eliminarEvidenciaDiagnostico(evidenciaId: number) {
    setGuardandoProceso(true);
    try {
      await ejecutarConCargadorPantalla("Eliminando la evidencia del diagnóstico…", () => eliminarEvidenciaDiagnosticoApi(orden.id, evidenciaId));
      setDiagnostico(await obtenerDiagnosticoOrdenApi(orden.id));
      alMostrarAviso("Evidencia eliminada");
    } finally {
      setGuardandoProceso(false);
    }
  }

  async function actualizarAutorizacion() {
    setDiagnostico(await ejecutarConCargadorPantalla(
      "Verificando la autorización del cliente…",
      () => obtenerDiagnosticoOrdenApi(orden.id),
    ));
  }

  async function solicitarAprobacion() {
    if (!diagnostico?.tokenPublico) {
      setErrorProceso("Guarda el diagnóstico antes de solicitar la aprobación.");
      return;
    }
    if (!cliente?.telefono || !normalizarTelefonoWhatsapp(cliente.telefono)) {
      setErrorProceso("El cliente no tiene un número de teléfono válido para abrir WhatsApp.");
      return;
    }

    const ventanaWhatsapp = window.open("", "_blank");
    const actualizada = await avanzar(
      "PendienteAprobacion",
      "Diagnóstico finalizado y preparado para aprobación del cliente.",
    );
    if (!actualizada) {
      ventanaWhatsapp?.close();
      return;
    }

    const direccionWhatsapp = crearDireccionWhatsappAprobacion({
      telefono: cliente.telefono,
      nombreCliente: cliente.nombre,
      numeroOrden: orden.numero,
      token: diagnostico.tokenPublico,
    });
    if (ventanaWhatsapp) {
      ventanaWhatsapp.opener = null;
      ventanaWhatsapp.location.href = direccionWhatsapp;
    } else {
      setErrorProceso("WhatsApp no pudo abrirse. Permite las ventanas emergentes e inténtalo nuevamente.");
    }
  }

  return (
    <div className="detalle-panel detalle-orden-pagina">
      <div className="columna-orden">
        <div className="vehiculo-destacado"><span className="icono-auto-grande"><CarFront size={31} /></span><div><span className={`etiqueta-estado estado-${normalizarClase(orden.estado)}`}>{orden.estado}</span><h3>{orden.vehiculo}</h3><p>{orden.placa} · {orden.cliente}</p></div></div>
        <div className="bloque-detalle"><span className="sobrelinea">Motivo de ingreso</span><p>{orden.motivo}</p></div>
        <div className="datos-rapidos"><div><UserRound size={20} /><span><small>Responsable</small><strong>{orden.tecnico}</strong></span></div><div><Clock3 size={20} /><span><small>Ingreso</small><strong>{orden.hora} a. m.</strong></span></div></div>
        <div className="bloque-detalle"><span className="sobrelinea">Avance de la orden</span><div className="barra-avance"><span style={{ width: `${orden.progreso}%` }} /></div><strong>{orden.progreso}% completado</strong></div>
        <div className="linea-tiempo linea-tiempo-orden">
          {etapas.map((etapa, indice) => (
            <div key={etapa} className={indice < indiceActual ? "completo" : indice === indiceActual ? "actual" : ""}>
              <span>{indice < indiceActual ? <Check size={14} /> : ""}</span>
              <div><strong>{etapa}</strong><small>{indice < indiceActual ? "Completado" : indice === indiceActual ? "Etapa actual" : "Pendiente"}</small></div>
            </div>
          ))}
        </div>
        <button className="boton-secundario boton-ancho" onClick={alNotificar}><MessageCircleMore size={20} />Enviar actualización al cliente</button>
      </div>

      <div className="columna-inspeccion-orden flujo-orden">
        {orden.estado !== "Recepción" ? (
          <>
            <ResumenInspeccion
              ordenServicioId={orden.id}
              inspeccion={inspeccion}
              alEditar={alRecibir}
            />
            {errorProceso && <div className="estado-datos estado-datos-error" role="alert">{errorProceso}</div>}
            {orden.estado === "Lista para entregar" && (
              <section className="paso-orden orden-lista">
                <span className="icono-llamada"><CircleCheck size={28} /></span>
                <span className="sobrelinea">Trabajo finalizado</span>
                <h2>Lista para entregar</h2>
                <p>El vehículo completó el proceso del taller. Puedes corregir el diagnóstico, las evidencias, los productos y los cargos antes de entregarlo.</p>
                <strong className="total-entrega">{formatearMoneda(resumen.total)}</strong>
              </section>
            )}
            {(orden.estado === "Diagnóstico" || orden.estado === "Reparación" || orden.estado === "Lista para entregar") && (
              <FormularioDiagnostico
                key={diagnostico?.fechaDiagnosticoUtc ?? "diagnostico-nuevo"}
                ordenServicioId={orden.id}
                diagnostico={diagnostico}
                cliente={cliente}
                numeroOrden={orden.numero}
                guardando={guardandoProceso}
                alGuardar={guardarDiagnostico}
                alEliminarEvidencia={eliminarEvidenciaDiagnostico}
                alEnviarAprobacion={orden.estado === "Diagnóstico" ? solicitarAprobacion : undefined}
              />
            )}
            {orden.estado === "Por aprobar" && !diagnostico?.diagnostico && (
              <FormularioDiagnostico
                key="diagnostico-pendiente-legacy"
                ordenServicioId={orden.id}
                diagnostico={diagnostico}
                cliente={cliente}
                numeroOrden={orden.numero}
                guardando={guardandoProceso}
                alGuardar={guardarDiagnostico}
                alEliminarEvidencia={eliminarEvidenciaDiagnostico}
              />
            )}
            {orden.estado === "Por aprobar" && (
              <section className="paso-orden">
                <span className="icono-llamada"><CircleCheck size={28} /></span>
                <span className="sobrelinea">Aprobación del cliente</span>
                <h2>Iniciar reparación</h2>
                <p>{diagnostico?.fechaAutorizacionClienteUtc
                  ? "El cliente autorizó continuar. Ya puedes iniciar la reparación y registrar sus cargos."
                  : "Comparte el diagnóstico y espera la autorización del cliente antes de iniciar la reparación."}</p>
                {diagnostico?.tokenPublico && (
                  <AccionesEnlacePublico
                    token={diagnostico.tokenPublico}
                    telefono={cliente?.telefono ?? ""}
                    nombreCliente={cliente?.nombre ?? orden.cliente}
                    numeroOrden={orden.numero}
                    alMostrarAviso={alMostrarAviso}
                  />
                )}
                {!diagnostico?.fechaAutorizacionClienteUtc && <button className="boton-secundario boton-ancho" type="button" disabled={guardandoProceso} onClick={actualizarAutorizacion}><RotateCcw size={18} />Verificar autorización</button>}
                <button className="boton-primario boton-ancho" disabled={guardandoProceso || !diagnostico?.fechaAutorizacionClienteUtc} onClick={() => avanzar("Reparacion", "Cliente autorizó continuar y la orden inició reparación.")}><Wrench size={20} />Iniciar reparación</button>
              </section>
            )}
            {detallesEditables && (
              <section className="trabajo-reparacion">
                <div className="cabecera-trabajo-reparacion"><div><span className="sobrelinea">Reparación</span><h2>Productos y cargos</h2></div><strong>{formatearMoneda(resumen.total)}</strong></div>
                <div className="formularios-cargos">
                  <form className="formulario-cargo" onSubmit={agregarProducto}>
                    <div><Boxes size={21} /><span><strong>Agregar desde inventario</strong><small>Busca el producto y luego indica la cantidad</small></span></div>
                    <div className="controles-productos-inventario">
                      <label>Bodega<select value={bodegaId ?? ""} required onChange={(evento) => { setBodegaId(Number(evento.target.value)); setProductoId(null); setBusquedaProducto(""); setArticulos([]); setBuscandoProductos(false); }}>{bodegas.map((bodega) => <option key={bodega.id} value={bodega.id}>{bodega.nombre}{bodega.esPrincipal ? " · Principal" : ""}</option>)}</select></label>
                      <label className="campo-busqueda-producto"><Search size={18} aria-hidden="true" /><span className="texto-solo-lectores">Buscar producto</span><input value={busquedaProducto} onChange={(evento) => { const siguienteBusqueda = evento.target.value; setBusquedaProducto(siguienteBusqueda); setProductoId(null); setArticulos([]); setBuscandoProductos(siguienteBusqueda.trim().length >= 2); }} placeholder="Busca por nombre o código" /></label>
                    </div>
                    {!articuloSeleccionado && terminoBusquedaProducto.length < 2 && <p className="ayuda-busqueda-producto">Escribe al menos 2 caracteres para buscar un producto.</p>}
                    {!articuloSeleccionado && terminoBusquedaProducto.length >= 2 && <div className="selector-productos-inventario" role="group" aria-label="Resultados de productos">
                      {buscandoProductos ? <p className="sin-productos-inventario" role="status">Buscando productos…</p> : productosCoincidentes.length === 0 ? (
                        <p className="sin-productos-inventario">No encontramos productos con esa búsqueda.</p>
                      ) : productosCoincidentes.map((articulo) => (
                        <button key={articulo.productoId} className="producto-inventario" type="button" onClick={() => { setProductoId(articulo.productoId); setCantidadProducto("1"); }}>
                          <span><strong>{articulo.nombre}</strong><small>{articulo.codigo}</small></span>
                          <span className="datos-producto-inventario"><strong>{formatearMoneda(articulo.precioUnitario ?? 0)}</strong><small>{articulo.existencia} disponibles</small></span>
                        </button>
                      ))}
                    </div>}
                    {articuloSeleccionado && <div className="producto-seleccionado-inventario">
                      <div><strong>{articuloSeleccionado.nombre}</strong><small>{articuloSeleccionado.codigo} · {formatearMoneda(articuloSeleccionado.precioUnitario ?? 0)}</small></div>
                      <button type="button" className="boton-texto" onClick={() => setProductoId(null)}>Cambiar</button>
                      <div className="accion-producto-inventario">
                        <label>Cantidad para {articuloSeleccionado.nombre}<input name="cantidad" type="number" min="0.0001" step="0.0001" value={cantidadProducto} onChange={(evento) => setCantidadProducto(evento.target.value)} required inputMode="decimal" /></label>
                        <button className="boton-secundario" type="submit" disabled={guardandoProceso}><Plus size={18} />Agregar a la orden</button>
                      </div>
                      <p className={articuloSeleccionado.existencia > 0 ? "disponibilidad-producto" : "disponibilidad-producto sin-existencia"}>{articuloSeleccionado.existencia > 0 ? `${articuloSeleccionado.existencia} disponibles.` : "Sin existencia; se agregará sin descontar inventario."}</p>
                    </div>}
                  </form>
                  <section className="accion-cargo-manual">
                    <span><Wrench size={21} /></span>
                    <div><strong>Cargo manual</strong><small>Mano de obra, compra externa u otro detalle</small></div>
                    <button className="boton-secundario" type="button" disabled={guardandoProceso} onClick={() => { setMensajeCargoManual(null); setMostrarCargoManual(true); }}><Plus size={18} />Añadir cargo manual</button>
                  </section>
                </div>
                <ResumenCargosOrden resumen={resumen} cargando={cargandoProceso} guardando={guardandoProceso} alEliminar={(detalle) => { setErrorProceso(""); setDetallePorEliminar(detalle); }} />
                {orden.estado === "Reparación" && <button className="boton-primario boton-ancho" disabled={guardandoProceso} onClick={() => avanzar("ListaParaEntrega", "Reparación finalizada; vehículo listo para entregar.")}><CircleCheck size={20} />Marcar lista para entregar</button>}
              </section>
            )}
          </>
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
      {mostrarCargoManual && (
        <div className="fondo-modal-alta">
          <section className="modal-alta modal-cargo-manual" role="dialog" aria-modal="true" aria-labelledby="titulo-modal-cargo-manual">
            <div className="cabecera-modal-alta">
              <div><span className="sobrelinea">Orden de servicio</span><h2 id="titulo-modal-cargo-manual">Añadir cargo manual</h2><p>Guarda cada cargo para añadir otro sin cerrar esta ventana.</p></div>
              <button className="boton-icono" type="button" onClick={() => { setMensajeCargoManual(null); setMostrarCargoManual(false); }} aria-label="Cerrar cargos manuales"><X size={20} /></button>
            </div>
            <div className="contenido-modal-alta">
              {mensajeCargoManual && <div className={mensajeCargoManual.esError ? "estado-datos estado-datos-error mensaje-modal-cargo" : "estado-datos mensaje-modal-cargo mensaje-modal-cargo-exito"} role={mensajeCargoManual.esError ? "alert" : "status"} aria-live="polite">{mensajeCargoManual.esError ? <TriangleAlert size={20} aria-hidden="true" /> : <CircleCheck size={20} aria-hidden="true" />}<span>{mensajeCargoManual.texto}</span></div>}
              <form className="formulario-cargo formulario-cargo-modal" onSubmit={agregarManual}>
                <label>Descripción<input name="descripcion" maxLength={300} minLength={2} required placeholder="Ej. Cambio de pastillas de freno" /></label>
                <label>Cantidad<input name="cantidad" type="number" min="0.0001" step="0.0001" defaultValue="1" required inputMode="decimal" /></label>
                <label>Precio unitario<input name="precioUnitario" type="number" min="0" step="0.01" required inputMode="decimal" placeholder="0.00" /></label>
                <div className="acciones-modal-cargo">
                  <button className="boton-secundario" type="button" onClick={() => { setMensajeCargoManual(null); setMostrarCargoManual(false); }}>Cerrar</button>
                  <button className="boton-primario" type="submit" disabled={guardandoProceso}><Plus size={18} />Guardar cargo</button>
                </div>
              </form>
            </div>
          </section>
        </div>
      )}
      {detallePorEliminar && (
        <ModalConfirmacion
          titulo={detallePorEliminar.tipo === "Inventario" ? "Eliminar producto" : "Eliminar cargo"}
          descripcion={`¿Deseas eliminar ${detallePorEliminar.descripcion} de la orden?`}
          detalle={detallePorEliminar.existenciaDescontada
            ? `Se devolverán ${detallePorEliminar.cantidad} ${detallePorEliminar.unidadMedida || "unidad(es)"} al inventario.`
            : "Este concepto se quitará del total de la orden."}
          etiquetaConfirmar={detallePorEliminar.existenciaDescontada ? "Eliminar y devolver" : "Eliminar cargo"}
          procesando={guardandoProceso}
          mensajeError={errorProceso}
          alCancelar={() => setDetallePorEliminar(null)}
          alConfirmar={eliminarDetalle}
        />
      )}
    </div>
  );
}

function ModalConfirmacion({
  titulo,
  descripcion,
  detalle,
  etiquetaConfirmar,
  procesando,
  mensajeError,
  alCancelar,
  alConfirmar,
}: {
  titulo: string;
  descripcion: string;
  detalle: string;
  etiquetaConfirmar: string;
  procesando: boolean;
  mensajeError?: string;
  alCancelar: () => void;
  alConfirmar: () => Promise<void>;
}) {
  const botonCancelarRef = useRef<HTMLButtonElement | null>(null);

  useEffect(() => {
    botonCancelarRef.current?.focus();
  }, []);

  useEffect(() => {
    function cerrarConEscape(evento: KeyboardEvent) {
      if (evento.key === "Escape" && !procesando) alCancelar();
    }

    document.addEventListener("keydown", cerrarConEscape);
    return () => document.removeEventListener("keydown", cerrarConEscape);
  }, [alCancelar, procesando]);

  return (
    <div className="fondo-modal-alta">
      <button className="cerrador-modal-fondo" type="button" disabled={procesando} aria-label="Cancelar confirmación" onClick={alCancelar} />
      <section className="modal-alta modal-confirmacion" role="alertdialog" aria-modal="true" aria-labelledby="titulo-modal-confirmacion" aria-describedby="detalle-modal-confirmacion">
        <div className="cabecera-modal-alta">
          <div><span className="sobrelinea">Confirmación</span><h2 id="titulo-modal-confirmacion">{titulo}</h2></div>
          <button className="boton-icono" type="button" disabled={procesando} onClick={alCancelar} aria-label="Cancelar"><X size={20} /></button>
        </div>
        <div className="contenido-modal-alta contenido-modal-confirmacion">
          <p>{descripcion}</p>
          <div id="detalle-modal-confirmacion" className="detalle-modal-confirmacion"><TriangleAlert size={20} aria-hidden="true" /><span>{detalle}</span></div>
          {mensajeError && <div className="estado-datos estado-datos-error" role="alert">{mensajeError}</div>}
          <div className="acciones-modal-confirmacion">
            <button ref={botonCancelarRef} className="boton-secundario" type="button" disabled={procesando} onClick={alCancelar}>Cancelar</button>
            <button className="boton-eliminar-confirmacion" type="button" disabled={procesando} onClick={() => void alConfirmar()}><Trash2 size={18} />{etiquetaConfirmar}</button>
          </div>
        </div>
      </section>
    </div>
  );
}

function FormularioDiagnostico({
  ordenServicioId,
  diagnostico,
  cliente,
  numeroOrden,
  guardando,
  alGuardar,
  alEliminarEvidencia,
  alEnviarAprobacion,
}: {
  ordenServicioId: number;
  diagnostico: DiagnosticoOrdenServicioApi | null;
  cliente?: ClienteTaller;
  numeroOrden: string;
  guardando: boolean;
  alGuardar: (texto: string, fotografias: File[]) => Promise<void>;
  alEliminarEvidencia: (evidenciaId: number) => Promise<void>;
  alEnviarAprobacion?: () => Promise<void>;
}) {
  const [texto, setTexto] = useState(diagnostico?.diagnostico ?? "");
  const [fotografias, setFotografias] = useState<File[]>([]);
  const [errorFotos, setErrorFotos] = useState("");
  const [dictando, setDictando] = useState(false);
  const [mensajeDictado, setMensajeDictado] = useState("");
  const [dictadoCompatible, setDictadoCompatible] = useState<boolean | null>(null);
  const reconocimiento = useRef<ReconocimientoVoz | null>(null);
  const entradaFotos = useRef<HTMLInputElement | null>(null);
  const textoBaseDictado = useRef("");
  const vistasPrevias = useMemo(
    () => fotografias.map((fotografia) => ({
      nombre: fotografia.name,
      direccion: URL.createObjectURL(fotografia),
    })),
    [fotografias],
  );

  useEffect(() => {
    return () => reconocimiento.current?.abort();
  }, []);
  useEffect(
    () => () => vistasPrevias.forEach((vista) => URL.revokeObjectURL(vista.direccion)),
    [vistasPrevias],
  );

  function alternarDictado() {
    if (dictando) {
      reconocimiento.current?.stop();
      return;
    }
    const ventana = window as typeof window & {
      SpeechRecognition?: ConstructorReconocimientoVoz;
      webkitSpeechRecognition?: ConstructorReconocimientoVoz;
    };
    const Constructor = ventana.SpeechRecognition || ventana.webkitSpeechRecognition;
    if (!Constructor) {
      setDictadoCompatible(false);
      setMensajeDictado("El dictado por voz no está disponible en este navegador. Puedes escribir el diagnóstico.");
      return;
    }
    const instancia = new Constructor();
    setDictadoCompatible(true);
    reconocimiento.current = instancia;
    textoBaseDictado.current = texto.trim();
    instancia.lang = "es-NI";
    instancia.continuous = true;
    instancia.interimResults = true;
    instancia.onresult = (evento) => {
      let transcripcion = "";
      for (let indice = 0; indice < evento.results.length; indice += 1) {
        transcripcion += `${evento.results[indice][0].transcript} `;
      }
      setTexto(`${textoBaseDictado.current}${textoBaseDictado.current ? " " : ""}${transcripcion.trim()}`);
      setMensajeDictado("Escuchando… revisa el texto antes de guardarlo.");
    };
    instancia.onerror = (evento) => {
      const mensajes: Record<string, string> = {
        "not-allowed": "Permite el acceso al micrófono para usar el dictado.",
        "no-speech": "No se detectó voz. Toca el micrófono para intentarlo nuevamente.",
      };
      setMensajeDictado(mensajes[evento.error] ?? "El dictado se interrumpió. Puedes escribir o intentarlo nuevamente.");
    };
    instancia.onend = () => {
      setDictando(false);
      reconocimiento.current = null;
    };
    try {
      instancia.start();
      setDictando(true);
      setMensajeDictado("Escuchando… habla con claridad.");
    } catch {
      setMensajeDictado("No fue posible iniciar el dictado. Inténtalo nuevamente.");
    }
  }

  const tieneCambios = texto.trim() !== (diagnostico?.diagnostico ?? "").trim() || fotografias.length > 0;
  const diagnosticoGuardado = Boolean(diagnostico?.diagnostico?.trim()) && !tieneCambios;

  return (
    <section className="formulario-diagnostico">
      <div className="cabecera-diagnostico">
        <span className="icono-llamada"><ClipboardList size={28} /></span>
        <div><span className="sobrelinea">Diagnóstico</span><h2>¿Qué tiene el vehículo?</h2><p>Explica los hallazgos y qué se recomienda hacer. No necesitas agregar todavía productos ni precios.</p></div>
      </div>
      <label className="campo-diagnostico">
        Diagnóstico del mecánico
        <textarea value={texto} onChange={(evento) => setTexto(evento.target.value)} required minLength={5} maxLength={4000} rows={7} placeholder="Ej. Se detectó desgaste en las pastillas delanteras y vibración al frenar…" />
        <small>{texto.length}/4000 caracteres</small>
      </label>
      <div className="controles-dictado">
        <button type="button" className={`boton-dictado ${dictando ? "activo" : ""}`} disabled={dictadoCompatible === false || guardando} onClick={alternarDictado}>
          {dictando ? <Square size={18} fill="currentColor" /> : <Mic size={20} />}
          {dictando ? "Detener dictado" : "Dictar diagnóstico"}
        </button>
        <span className={dictando ? "estado-dictado escuchando" : "estado-dictado"} role="status" aria-live="polite">{mensajeDictado || (dictadoCompatible ? "Puedes hablar y luego corregir el texto." : "Dictado no disponible; escribe el diagnóstico.")}</span>
      </div>
      <div className="bloque-carga-fotos">
        <label className="zona-fotos">
          <input
            ref={entradaFotos}
            type="file"
            accept="image/jpeg,image/png,image/webp"
            capture="environment"
            multiple
            disabled={guardando}
            onChange={(evento) => {
              const seleccionadas = Array.from(evento.target.files ?? []);
              const disponibles = 8 - (diagnostico?.evidencias.length ?? 0);
              const acumuladas = [...fotografias, ...seleccionadas];
              if (acumuladas.length > disponibles) {
                sincronizarFotografiasEntrada(evento.target, fotografias);
                const restantes = Math.max(0, disponibles - fotografias.length);
                setErrorFotos(restantes > 0
                  ? `Puede agregar ${restantes} fotografías más al diagnóstico.`
                  : "El diagnóstico ya tiene el máximo de 8 fotografías.");
                return;
              }

              setErrorFotos("");
              setFotografias(acumuladas);
              sincronizarFotografiasEntrada(evento.target, acumuladas);
            }}
          />
          {fotografias.length > 0 ? <ImagePlus size={25} /> : <Camera size={25} />}
          <span>
            <strong>{fotografias.length > 0 ? `${fotografias.length} ${fotografias.length === 1 ? "fotografía seleccionada" : "fotografías seleccionadas"}` : "Agregar fotografías"}</strong>
            <small>Hallazgos, piezas afectadas y evidencia del diagnóstico</small>
          </span>
          <Plus size={20} />
        </label>
        {errorFotos && <p className="error-fotografias" role="alert">{errorFotos}</p>}
      </div>
      {((diagnostico?.evidencias.length ?? 0) > 0 || vistasPrevias.length > 0) && (
        <div className="galeria-evidencias galeria-edicion galeria-diagnostico">
          {diagnostico?.evidencias.map((evidencia) => (
            <figure key={evidencia.id}>
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={direccionEvidenciaDiagnostico(ordenServicioId, evidencia.id)} alt={`Evidencia del diagnóstico ${evidencia.nombreArchivo}`} loading="lazy" />
              <figcaption>Guardada</figcaption>
              <button className="boton-eliminar-evidencia" type="button" disabled={guardando} onClick={() => alEliminarEvidencia(evidencia.id)} aria-label={`Eliminar ${evidencia.nombreArchivo}`}><Trash2 size={18} /></button>
            </figure>
          ))}
          {vistasPrevias.map((vista, indice) => (
            <figure key={vista.direccion}>
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={vista.direccion} alt={`Vista previa ${vista.nombre}`} />
              <figcaption>Por guardar</figcaption>
              <button
                className="boton-eliminar-evidencia"
                type="button"
                disabled={guardando}
                aria-label={`Quitar ${vista.nombre}`}
                onClick={() => {
                  const actualizadas = fotografias.filter((_, posicion) => posicion !== indice);
                  setFotografias(actualizadas);
                  setErrorFotos("");
                  if (entradaFotos.current) sincronizarFotografiasEntrada(entradaFotos.current, actualizadas);
                }}
              >
                <Trash2 size={18} />
              </button>
            </figure>
          ))}
        </div>
      )}
      <button type="button" className="boton-secundario boton-ancho" disabled={guardando || texto.trim().length < 5 || !tieneCambios} onClick={async () => { await alGuardar(texto.trim(), fotografias); setFotografias([]); setErrorFotos(""); if (entradaFotos.current) entradaFotos.current.value = ""; }}><Check size={20} />Guardar diagnóstico</button>
      {diagnostico?.tokenPublico && (
        <AccionesEnlacePublico
          token={diagnostico.tokenPublico}
          telefono={cliente?.telefono ?? ""}
          nombreCliente={cliente?.nombre ?? "cliente"}
          numeroOrden={numeroOrden}
        />
      )}
      {alEnviarAprobacion && <button type="button" className="boton-primario boton-ancho" disabled={guardando || !diagnosticoGuardado} onClick={alEnviarAprobacion}><MessageCircleMore size={20} />Solicitar aprobación por WhatsApp</button>}
      {alEnviarAprobacion && !diagnosticoGuardado && <small className="ayuda-envio-diagnostico">Guarda el diagnóstico antes de enviarlo al cliente.</small>}
    </section>
  );
}

function AccionesEnlacePublico({
  token,
  telefono,
  nombreCliente,
  numeroOrden,
  alMostrarAviso,
}: {
  token: string;
  telefono: string;
  nombreCliente: string;
  numeroOrden: string;
  alMostrarAviso?: (mensaje: string) => void;
}) {
  const direccion = typeof window === "undefined" ? "" : `${window.location.origin}/orden/${token}`;
  const telefonoWhatsapp = normalizarTelefonoWhatsapp(telefono);
  const direccionWhatsapp = direccion && telefonoWhatsapp
    ? crearDireccionWhatsappAprobacion({ telefono, nombreCliente, numeroOrden, token })
    : "";
  async function copiar() {
    await navigator.clipboard.writeText(direccion);
    alMostrarAviso?.("Enlace público copiado");
  }
  return (
    <div className="acciones-enlace-publico">
      <div><Share2 size={20} /><span><strong>Enlace para el cliente</strong><small>Permite consultar y autorizar el diagnóstico sin iniciar sesión.</small></span></div>
      <div>
        <button type="button" className="boton-secundario" onClick={copiar}><Copy size={17} />Copiar</button>
        {direccionWhatsapp ? (
          <a className="boton-whatsapp" href={direccionWhatsapp} target="_blank" rel="noreferrer"><MessageCircleMore size={18} />Abrir WhatsApp</a>
        ) : (
          <button type="button" className="boton-whatsapp" disabled title="El cliente no tiene un teléfono válido"><MessageCircleMore size={18} />Teléfono no disponible</button>
        )}
      </div>
    </div>
  );
}

function ResumenCargosOrden({
  resumen,
  cargando,
  guardando,
  alEliminar,
  soloLectura = false,
}: {
  resumen: ResumenDetallesOrdenServicioApi;
  cargando: boolean;
  guardando: boolean;
  alEliminar: (detalle: DetalleOrdenServicioApi) => void;
  soloLectura?: boolean;
}) {
  if (cargando) return null;
  if (resumen.detalles.length === 0) return <div className="sin-cargos"><ClipboardList size={20} /><span>Aún no hay productos ni cargos en esta orden.</span></div>;
  return (
    <div className="resumen-cargos">
      <div className="cabecera-cargos"><span>Descripción</span><span>Cantidad</span><span>Precio</span><span>Subtotal</span><span /></div>
      {resumen.detalles.map((detalle) => (
        <article key={detalle.id}>
          <div><strong>{detalle.descripcion}</strong><small>{detalle.codigoProducto || "Detalle manual"}{detalle.existenciaDescontada ? " · Existencia descontada" : detalle.tipo === "Inventario" ? " · Sin descuento de existencia" : ""}</small></div>
          <span>{detalle.cantidad} {detalle.unidadMedida || ""}</span>
          <span>{formatearMoneda(detalle.precioUnitario)}</span>
          <strong>{formatearMoneda(detalle.subtotal)}</strong>
          {!soloLectura ? <button type="button" disabled={guardando} onClick={() => alEliminar(detalle)} aria-label={`Eliminar ${detalle.descripcion}`}><Trash2 size={17} /></button> : <span />}
        </article>
      ))}
      <div className="total-cargos"><span>Total de la orden</span><strong>{formatearMoneda(resumen.total)}</strong></div>
    </div>
  );
}

function FormularioRecepcion({
  orden,
  inspeccionInicial,
  alEnviar,
  alEliminarEvidencia,
}: {
  orden: OrdenTaller;
  inspeccionInicial?: InspeccionVisual;
  alEnviar: (evento: FormEvent<HTMLFormElement>) => Promise<void>;
  alEliminarEvidencia: (evidenciaId: number) => Promise<void>;
}) {
  const [tipoDanio, setTipoDanio] = useState<TipoDanio>("Rayón");
  const [severidad, setSeveridad] = useState<SeveridadDanio>("Leve");
  const [danios, setDanios] = useState<DanioVisual[]>(inspeccionInicial?.danios || []);
  const [danioEditandoId, setDanioEditandoId] = useState<string | null>(null);
  const [cantidadFotos, setCantidadFotos] = useState(0);
  const [fotografias, setFotografias] = useState<File[]>([]);
  const entradaFotografias = useRef<HTMLInputElement | null>(null);
  const [errorFotos, setErrorFotos] = useState("");
  const [evidenciaPorEliminar, setEvidenciaPorEliminar] = useState<EvidenciaInspeccion | null>(null);
  const [evidenciaEliminandoId, setEvidenciaEliminandoId] = useState<number | null>(null);
  const [guardando, setGuardando] = useState(false);
  const vistasPrevias = useMemo(
    () => fotografias.map((fotografia) => ({
      nombre: fotografia.name,
      direccion: URL.createObjectURL(fotografia),
    })),
    [fotografias],
  );
  useEffect(
    () => () => vistasPrevias.forEach((vista) => URL.revokeObjectURL(vista.direccion)),
    [vistasPrevias],
  );
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

  async function eliminarEvidenciaConfirmada() {
    if (!evidenciaPorEliminar) return;
    setEvidenciaEliminandoId(evidenciaPorEliminar.id);
    setErrorFotos("");
    try {
      await alEliminarEvidencia(evidenciaPorEliminar.id);
      setEvidenciaPorEliminar(null);
    } catch (error) {
      setErrorFotos(
        error instanceof Error
          ? error.message
          : "No fue posible eliminar la fotografía.",
      );
    } finally {
      setEvidenciaEliminandoId(null);
    }
  }

  return (
    <>
    <form
      className="formulario formulario-recepcion-pagina"
      aria-busy={guardando}
      onSubmit={async (evento) => {
        if (guardando) return;
        setGuardando(true);
        try {
          await alEnviar(evento);
        } finally {
          setGuardando(false);
        }
      }}
    >
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
      <div className="bloque-carga-fotos">
      <label className="zona-fotos">
        <input
          ref={entradaFotografias}
          type="file"
          name="fotografias"
          accept="image/jpeg,image/png,image/webp"
          capture="environment"
          multiple
          onChange={(evento) => {
            const seleccionadas = Array.from(evento.target.files || []);
            const disponibles = 12 - (inspeccionInicial?.evidencias.length || 0);
            const acumuladas = [...fotografias, ...seleccionadas];
            if (acumuladas.length > disponibles) {
              sincronizarFotografiasEntrada(evento.target, fotografias);
              const restantes = Math.max(0, disponibles - fotografias.length);
              setErrorFotos(restantes > 0
                ? `Puede agregar ${restantes} fotografías más a esta inspección.`
                : "La inspección ya tiene el máximo de 12 fotografías.");
              return;
            }

            setErrorFotos("");
            setFotografias(acumuladas);
            setCantidadFotos(acumuladas.length);
            sincronizarFotografiasEntrada(evento.target, acumuladas);
          }}
        />
        {cantidadFotos > 0 ? <ImagePlus size={25} /> : <Camera size={25} />}
        <span>
          <strong>{cantidadFotos > 0 ? `${cantidadFotos} ${cantidadFotos === 1 ? "fotografía seleccionada" : "fotografías seleccionadas"}` : "Agregar fotografías"}</strong>
          <small>Frente, laterales y evidencia de cada daño</small>
        </span>
        <Plus size={20} />
      </label>
      {errorFotos && <p className="error-fotografias" role="alert">{errorFotos}</p>}
      {(inspeccionInicial?.evidencias.length || vistasPrevias.length > 0) && (
        <div className="galeria-evidencias galeria-edicion">
          {inspeccionInicial?.evidencias.map((evidencia) => (
            <figure key={evidencia.id}>
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={direccionEvidencia(orden.id, evidencia.id)}
                alt={`Evidencia ${evidencia.nombreArchivo}`}
                loading="lazy"
              />
              <figcaption>Guardada</figcaption>
              <button
                className="boton-eliminar-evidencia"
                type="button"
                disabled={evidenciaEliminandoId !== null}
                aria-label={`Eliminar ${evidencia.nombreArchivo}`}
                onClick={() => { setErrorFotos(""); setEvidenciaPorEliminar(evidencia); }}
              >
                <Trash2 size={18} />
              </button>
            </figure>
          ))}
          {vistasPrevias.map((vista, indice) => (
            <figure key={vista.direccion}>
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={vista.direccion} alt={`Vista previa ${vista.nombre}`} />
              <figcaption>Por guardar</figcaption>
              <button
                className="boton-eliminar-evidencia"
                type="button"
                disabled={guardando}
                aria-label={`Quitar ${vista.nombre}`}
                onClick={() => {
                  const actualizadas = fotografias.filter((_, posicion) => posicion !== indice);
                  setFotografias(actualizadas);
                  setCantidadFotos(actualizadas.length);
                  setErrorFotos("");
                  if (entradaFotografias.current) {
                    sincronizarFotografiasEntrada(entradaFotografias.current, actualizadas);
                  }
                }}
              >
                <Trash2 size={18} />
              </button>
            </figure>
          ))}
        </div>
      )}
      </div>
      <div className="lista-comprobacion"><label><input type="checkbox" name="dejaLlaves" defaultChecked={inspeccionInicial?.dejaLlaves} />Deja llaves</label><label><input type="checkbox" name="dejaDocumentos" defaultChecked={inspeccionInicial?.dejaDocumentos} />Deja documentos</label><label><input type="checkbox" name="aceptaPruebaRuta" />Acepta prueba de ruta</label></div>
      <button className="boton-primario boton-ancho" type="submit" disabled={guardando}>
        <Sparkles size={20} />
        {inspeccionInicial
          ? "Guardar cambios de inspección"
          : "Guardar y pasar a diagnóstico"}
      </button>
    </form>
    {evidenciaPorEliminar && (
      <ModalConfirmacion
        titulo="Eliminar fotografía"
        descripcion={`¿Deseas eliminar ${evidenciaPorEliminar.nombreArchivo} de la inspección?`}
        detalle="La fotografía guardada se eliminará de forma permanente."
        etiquetaConfirmar="Eliminar fotografía"
        procesando={evidenciaEliminandoId !== null}
        mensajeError={errorFotos}
        alCancelar={() => setEvidenciaPorEliminar(null)}
        alConfirmar={eliminarEvidenciaConfirmada}
      />
    )}
    </>
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
  ordenServicioId,
  inspeccion,
  alEditar,
}: {
  ordenServicioId: number;
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
      {inspeccion && inspeccion.evidencias.length > 0 && (
        <div className="bloque-evidencias">
          <div>
            <span className="sobrelinea">Evidencia fotográfica</span>
            <strong>{inspeccion.evidencias.length} fotografías</strong>
          </div>
          <div className="galeria-evidencias">
            {inspeccion.evidencias.map((evidencia) => (
              <a
                key={evidencia.id}
                href={direccionEvidencia(ordenServicioId, evidencia.id)}
                target="_blank"
                rel="noreferrer"
                aria-label={`Abrir ${evidencia.nombreArchivo}`}
              >
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={direccionEvidencia(ordenServicioId, evidencia.id)}
                  alt={`Evidencia de inspección ${evidencia.nombreArchivo}`}
                  loading="lazy"
                />
              </a>
            ))}
          </div>
        </div>
      )}
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
async function listarExistenciasApi(bodegaId: number, criterio?: string, senal?: AbortSignal): Promise<ArticuloInventarioApi[]> { const parametros = new URLSearchParams({ bodegaId: String(bodegaId) }); if (criterio) parametros.set("criterio", criterio); const respuesta = await fetch(`${obtenerDireccionApi()}/api/inventario/existencias?${parametros}`, { credentials: "include", signal: senal }); if (!respuesta.ok) throw new Error("No fue posible consultar las existencias."); return await respuesta.json() as ArticuloInventarioApi[]; }

async function obtenerDetallesOrdenApi(
  ordenServicioId: number,
  senal?: AbortSignal,
): Promise<ResumenDetallesOrdenServicioApi> {
  const respuesta = await fetch(
    `${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/detalles`,
    { credentials: "include", signal: senal },
  );
  if (!respuesta.ok) throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible consultar los cargos."));
  return await respuesta.json() as ResumenDetallesOrdenServicioApi;
}

async function obtenerDiagnosticoOrdenApi(
  ordenServicioId: number,
  senal?: AbortSignal,
): Promise<DiagnosticoOrdenServicioApi> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/diagnostico`, {
    credentials: "include",
    signal: senal,
  });
  if (!respuesta.ok) throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible consultar el diagnóstico."));
  return await respuesta.json() as DiagnosticoOrdenServicioApi;
}

async function guardarDiagnosticoOrdenApi(
  ordenServicioId: number,
  diagnostico: string,
): Promise<DiagnosticoOrdenServicioApi> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/diagnostico`, {
    method: "PUT",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ diagnostico }),
  });
  if (!respuesta.ok) throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible guardar el diagnóstico."));
  return await respuesta.json() as DiagnosticoOrdenServicioApi;
}

async function cargarEvidenciasDiagnosticoApi(
  ordenServicioId: number,
  fotografias: File[],
): Promise<DiagnosticoOrdenServicioApi> {
  const datos = new FormData();
  fotografias.forEach((fotografia) => datos.append("fotografias", fotografia));
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/diagnostico/evidencias`, {
    method: "POST",
    credentials: "include",
    body: datos,
  });
  if (!respuesta.ok) throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible guardar la evidencia del diagnóstico."));
  return await respuesta.json() as DiagnosticoOrdenServicioApi;
}

async function eliminarEvidenciaDiagnosticoApi(ordenServicioId: number, evidenciaId: number) {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/diagnostico/evidencias/${evidenciaId}`, {
    method: "DELETE",
    credentials: "include",
  });
  if (!respuesta.ok) throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible eliminar la evidencia."));
}

function direccionEvidenciaDiagnostico(ordenServicioId: number, evidenciaId: number) {
  return `${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/diagnostico/evidencias/${evidenciaId}/contenido`;
}

async function agregarDetalleInventarioApi(
  ordenServicioId: number,
  solicitud: { bodegaId: number; productoId: number; cantidad: number },
): Promise<ResumenDetallesOrdenServicioApi> {
  return solicitarResumenDetalles(
    `${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/detalles/inventario`,
    "POST",
    solicitud,
  );
}

async function agregarDetalleManualApi(
  ordenServicioId: number,
  solicitud: { descripcion: string; unidadMedida: string | null; cantidad: number; precioUnitario: number },
): Promise<ResumenDetallesOrdenServicioApi> {
  return solicitarResumenDetalles(
    `${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/detalles/manual`,
    "POST",
    solicitud,
  );
}

async function eliminarDetalleOrdenApi(
  ordenServicioId: number,
  detalleId: number,
): Promise<ResumenDetallesOrdenServicioApi> {
  return solicitarResumenDetalles(
    `${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/detalles/${detalleId}`,
    "DELETE",
  );
}

async function solicitarResumenDetalles(
  direccion: string,
  metodo: "POST" | "DELETE",
  solicitud?: object,
): Promise<ResumenDetallesOrdenServicioApi> {
  const respuesta = await fetch(direccion, {
    method: metodo,
    credentials: "include",
    headers: solicitud ? { "Content-Type": "application/json" } : undefined,
    body: solicitud ? JSON.stringify(solicitud) : undefined,
  });
  if (!respuesta.ok) throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible actualizar los cargos."));
  return await respuesta.json() as ResumenDetallesOrdenServicioApi;
}

async function cambiarEstadoOrdenApi(
  ordenServicioId: number,
  estado: "PendienteAprobacion" | "Reparacion" | "ListaParaEntrega",
  descripcion: string,
): Promise<OrdenTaller> {
  const respuesta = await fetch(
    `${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/estado`,
    {
      method: "PUT",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ estado, descripcion }),
    },
  );
  if (!respuesta.ok) throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible avanzar la orden."));
  return convertirOrdenApi(await respuesta.json() as OrdenServicioApi);
}

async function cargarDatosApi(senal: AbortSignal): Promise<{
  ordenes: OrdenTaller[];
  clientes: ClienteTaller[];
  vehiculos: VehiculoTaller[];
  marcasVehiculo: MarcaVehiculoApi[];
  modelosVehiculo: ModeloVehiculoApi[];
}> {
  const [ordenes, clientesApi, vehiculosApi, marcasVehiculo, modelosVehiculo] = await Promise.all([
    cargarOrdenesApi(senal),
    cargarClientesApi(senal),
    cargarVehiculosApi(senal),
    cargarMarcasVehiculoApi(senal),
    cargarModelosVehiculoApi(senal),
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

  return { ordenes, clientes, vehiculos, marcasVehiculo, modelosVehiculo };
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
  telefono: string;
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
    modeloVehiculoId: number;
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

async function cargarMarcasVehiculoApi(senal: AbortSignal): Promise<MarcaVehiculoApi[]> {
  const respuesta = await fetch(
    `${obtenerDireccionApi()}/api/catalogos-vehiculos/marcas?incluirInactivas=true`,
    { credentials: "include", signal: senal },
  );
  if (!respuesta.ok) throw new Error("No fue posible consultar el catálogo de marcas.");
  return (await respuesta.json()) as MarcaVehiculoApi[];
}

async function cargarModelosVehiculoApi(senal: AbortSignal): Promise<ModeloVehiculoApi[]> {
  const respuesta = await fetch(
    `${obtenerDireccionApi()}/api/catalogos-vehiculos/modelos?incluirInactivos=true`,
    { credentials: "include", signal: senal },
  );
  if (!respuesta.ok) throw new Error("No fue posible consultar el catálogo de modelos.");
  return (await respuesta.json()) as ModeloVehiculoApi[];
}

async function guardarMarcaVehiculoApi(nombre: string): Promise<MarcaVehiculoApi> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/catalogos-vehiculos/marcas`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ nombre, activa: true }),
  });
  if (!respuesta.ok) {
    throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible guardar la marca."));
  }
  return (await respuesta.json()) as MarcaVehiculoApi;
}

async function guardarModeloVehiculoApi(
  marcaVehiculoId: number,
  nombre: string,
): Promise<ModeloVehiculoApi> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/catalogos-vehiculos/modelos`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ marcaVehiculoId, nombre, activo: true }),
  });
  if (!respuesta.ok) {
    throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible guardar el modelo."));
  }
  return (await respuesta.json()) as ModeloVehiculoApi;
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
    telefono: cliente.telefono,
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
    marcaVehiculoId: vehiculo.marcaVehiculoId,
    marca: vehiculo.marca,
    modeloVehiculoId: vehiculo.modeloVehiculoId,
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
  fotografias: File[],
): Promise<{ inspeccion: InspeccionVisual; advertencia?: string }> {
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

  if (!respuesta.ok) {
    throw new Error(await obtenerMensajeErrorApi(respuesta, "No fue posible registrar la recepción."));
  }

  const recepcion = (await respuesta.json()) as RecepcionVehiculoApi;
  const inspeccionGuardada = convertirRecepcionApi(recepcion, ordenServicioId);
  if (fotografias.length === 0) return { inspeccion: inspeccionGuardada };

  const formulario = new FormData();
  fotografias.forEach((fotografia) => formulario.append("fotografias", fotografia));
  const respuestaEvidencias = await fetch(
    `${direccionApi}/api/ordenes-servicio/${ordenServicioId}/recepcion/evidencias`,
    {
      method: "POST",
      credentials: "include",
      body: formulario,
    },
  );
  if (!respuestaEvidencias.ok) {
    const motivo = await obtenerMensajeErrorApi(
      respuestaEvidencias,
      "las fotografías no pudieron cargarse",
    );
    return {
      inspeccion: inspeccionGuardada,
      advertencia: `La inspección se guardó, pero ${motivo.toLocaleLowerCase("es")}. Puede volver a editarla para reintentar.`,
    };
  }

  const evidencias = (await respuestaEvidencias.json()) as EvidenciaInspeccion[];
  return {
    inspeccion: {
      ...inspeccionGuardada,
      evidencias: [...inspeccionGuardada.evidencias, ...evidencias],
    },
  };
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
  return convertirRecepcionApi(recepcion, ordenServicioId);
}

function convertirRecepcionApi(
  recepcion: RecepcionVehiculoApi,
  ordenServicioId: number,
): InspeccionVisual {
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
    evidencias: recepcion.evidencias || [],
  };
}

function direccionEvidencia(ordenServicioId: number, evidenciaId: number) {
  return `${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/recepcion/evidencias/${evidenciaId}/contenido`;
}

function sincronizarFotografiasEntrada(entrada: HTMLInputElement, fotografias: File[]) {
  const transferencia = new DataTransfer();
  fotografias.forEach((fotografia) => transferencia.items.add(fotografia));
  entrada.files = transferencia.files;
}

function normalizarTelefonoWhatsapp(telefono: string) {
  let digitos = telefono.replace(/\D/g, "");
  if (digitos.startsWith("00")) digitos = digitos.slice(2);
  if (digitos.length === 8) digitos = `505${digitos}`;
  return digitos.length >= 10 && digitos.length <= 15 ? digitos : "";
}

function crearDireccionWhatsappAprobacion({
  telefono,
  nombreCliente,
  numeroOrden,
  token,
}: {
  telefono: string;
  nombreCliente: string;
  numeroOrden: string;
  token: string;
}) {
  const telefonoWhatsapp = normalizarTelefonoWhatsapp(telefono);
  const direccionPublica = `${window.location.origin}/orden/${token}`;
  const mensaje = `Hola ${nombreCliente}, compartimos el diagnóstico de su vehículo correspondiente a la orden ${numeroOrden}. Puede revisarlo y autorizar el trabajo en el siguiente enlace: ${direccionPublica}`;
  return `https://wa.me/${telefonoWhatsapp}?text=${encodeURIComponent(mensaje)}`;
}

async function eliminarEvidenciaApi(
  ordenServicioId: number,
  evidenciaId: number,
) {
  const respuesta = await fetch(
    `${obtenerDireccionApi()}/api/ordenes-servicio/${ordenServicioId}/recepcion/evidencias/${evidenciaId}`,
    {
      method: "DELETE",
      credentials: "include",
    },
  );
  if (!respuesta.ok) {
    throw new Error(await obtenerMensajeErrorApi(
      respuesta,
      "No fue posible eliminar la fotografía.",
    ));
  }
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
    Cotizacion: "Por aprobar",
    PendienteAprobacion: "Por aprobar",
    PreparacionReparacion: "Reparación",
    Reparacion: "Reparación",
    ControlCalidad: "Reparación",
    ListaParaEntrega: "Lista para entregar",
    Entregada: "Lista para entregar",
    Cerrada: "Lista para entregar",
  };
  return equivalencias[estado] || "Recepción";
}

function formatearMoneda(valor: number) {
  return new Intl.NumberFormat("es-NI", {
    style: "currency",
    currency: "NIO",
    minimumFractionDigits: 2,
  }).format(valor);
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
