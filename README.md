# Talleres

Sistema web tablet-first para gestionar el flujo operativo de un taller automotriz. Incluye un frontal táctil en React y una API construida con ASP.NET Core 9, Entity Framework Core 9 y SQL Server.

## Experiencia web para tablet

El frontal vive en `src/Talleres.Web` y está diseñado primero para tablet horizontal y vertical. Incluye:

- Panel operativo con métricas, bahías, agenda y decisiones pendientes.
- Navegación lateral para tablet horizontal y navegación inferior para formato vertical.
- Gestión visual de órdenes, clientes, vehículos e inventario.
- Flujo interactivo para crear órdenes y registrar la recepción del vehículo.
- Inspección visual táctil por zonas, tipo y severidad del daño, con fotografías desde la cámara de la tablet.
- Descripción editable para cada daño y observaciones generales de la recepción.
- Consulta y edición posterior de los hallazgos visuales dentro del detalle de la orden.
- Órdenes, recepción e inspección en páginas de trabajo completas; no utilizan paneles laterales ni modales estrechos.
- Distribución de dos columnas en tablet horizontal y computador, y flujo secuencial en tablet vertical.
- Controles táctiles amplios, estados visibles y adaptación posterior a escritorio y móvil.
- Carga de órdenes, clientes y vehículos exclusivamente desde la API configurada.
- Inicio de sesión con usuarios de SMART TPV NOVA y selección táctil de taller para superusuarios.
- Información legal, contacto, dirección y horario del taller activo obtenida directamente de NOVA.

## Alcance implementado

- Registro, actualización, consulta y listado de clientes.
- Registro y consulta de vehículos asociados a clientes.
- Creación y consulta de órdenes de servicio.
- Recepción física del vehículo y avance automático a diagnóstico.
- Transiciones controladas del estado de las órdenes.
- Historial de estados.
- Aislamiento multitenant por `EmpresaId` en consultas y escrituras.
- Empresa activa obtenida de una cookie de sesión HTTP-only; el cliente no puede elegirla mediante encabezados.
- Denegación de acceso para usuarios normales cuya empresa no esté configurada como taller sincronizado.
- Validaciones estructurales mediante Data Annotations.
- Excepciones de negocio convertidas a `ProblemDetails` por middleware global.

## Arquitectura

```text
src/
├── Talleres.Dominio/          Entidades, enumeraciones y excepciones del negocio
├── Talleres.Aplicacion/       DTO, contratos y servicios de aplicación
├── Talleres.Infraestructura/  DbContext, configuraciones y migraciones de EF Core
├── Talleres.Api/              Controladores, middleware y composición de dependencias
└── Talleres.Web/              Sistema web tablet-first
tests/
└── Talleres.Pruebas/          Pruebas de reglas de negocio y multitenencia
```

Los controladores delegan el trabajo a servicios. La aplicación depende de `ITallerDbContext`, implementado por infraestructura, sin repositorios genéricos adicionales.

## Requisitos

- SDK de .NET 9.
- Node.js 22.13 o superior.
- Una instancia remota de SQL Server accesible y configurada con TLS 1.2 o posterior.
- Acceso de lectura a la base de SMART TPV NOVA (`AspNetUsers` y `Empresa`).
- Herramienta `dotnet-ef` 9.x para administrar migraciones.

## Puesta en marcha

```powershell
dotnet restore Talleres.sln
dotnet ef database update `
  --project src/Talleres.Infraestructura `
  --startup-project src/Talleres.Infraestructura
dotnet run --project src/Talleres.Api
```

En otra terminal, inicia el sistema web:

```powershell
cd src/Talleres.Web
Copy-Item .env.example .env.local
npm ci
npm run dev
```

Luego abre `http://localhost:4173`. La interfaz muestra un error explícito si no se crea
`.env.local` o si la API configurada no está disponible; nunca sustituye la respuesta con datos simulados.

La API no contiene una cadena de conexión predeterminada y rechaza servidores locales.
Para ejecutarla fuera de Docker, puede configurar la conexión remota mediante una variable de entorno:

```powershell
$env:ConnectionStrings__TallerDb = "Server=sql.example.com,1433;Database=Talleres;User ID=usuario;Password=clave;Encrypt=True;TrustServerCertificate=False"
$env:ConnectionStrings__SmartNova = "Server=sql.example.com,1433;Database=Galileo;User ID=usuario_lectura;Password=clave;Encrypt=True;TrustServerCertificate=False"
```

También se admiten `TALLERES_CONNECTION_STRING` y `SMART_NOVA_CONNECTION_STRING`. En desarrollo, si
las variables correspondientes no están definidas, la API las carga desde el
archivo `.env` ubicado junto a `Talleres.sln`. Esto permite usar `dotnet run` y los perfiles del
IDE sin copiar credenciales a `launchSettings.json`. Las variables del proceso siempre tienen
precedencia sobre el archivo local.

El inicio de sesión se realiza mediante:

```http
POST /api/autenticacion/iniciar
Content-Type: application/json

{
  "usuario": "usuario@ejemplo.com",
  "contrasena": "su-contraseña",
  "recordarme": false
}
```

La API establece una cookie HTTP-only. El `EmpresaId` usado por todas las operaciones se obtiene de esa identidad autenticada. Los superusuarios pueden llamar `POST /api/autenticacion/seleccionar-taller`; los usuarios normales quedan limitados al `IdEmpresa` registrado en NOVA. El endpoint `GET /salud` permanece público.

El acceso también admite Google y Microsoft. Cada proveedor es opcional y solo se habilita cuando están configurados tanto su identificador como su secreto. Configure `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`, `MICROSOFT_CLIENT_ID` y `MICROSOFT_CLIENT_SECRET` como secretos del entorno. Como la API se publica a través del frontal, registre los callbacks con el dominio público y el prefijo `/backend`, por ejemplo `https://talleres.example.com/backend/api/autenticacion/externo/google/callback` y `https://talleres.example.com/backend/api/autenticacion/externo/microsoft/callback`. El nombre `api:8080` es exclusivamente interno de Docker y no es una URL OAuth válida.

La migración `AgregarTalleresSincronizados` crea la configuración local e incluye inicialmente la empresa NOVA `3071`. Agregar otra empresa a esa tabla la habilita como taller, siempre que exista en NOVA.

## Ejecución completa con Docker

Docker Compose levanta el frontal y la API de forma coordinada. La base de datos debe ser una instancia remota de SQL Server accesible desde el contenedor de la API.

La preparación del entorno se realiza una única vez:

```powershell
Copy-Item .env.example .env
```

Edite `.env` y reemplace `TALLERES_CONNECTION_STRING` y `SMART_NOVA_CONNECTION_STRING` por las cadenas reales. La cuenta de NOVA debe tener el mínimo acceso de lectura necesario. El archivo `.env` está excluido de Git y no debe subirse al repositorio.

`TALLERES_APLICAR_MIGRACIONES` permanece en `false` por defecto. Cámbielo a `true` únicamente cuando la API tenga autorización para aplicar las migraciones de EF Core sobre esa base.

Después, todo el sistema se construye y levanta con una sola llamada desde la raíz:

```powershell
docker compose up --build
```

Servicios publicados:

| Servicio | Dirección |
|---|---|
| Sistema web | `http://localhost:3000` |
| API | Interna en `http://api:8080`; el frontal la consume mediante `/backend` |

La API recibe la conexión remota mediante una variable de entorno y no almacena la cadena dentro de la imagen. Su comprobación de salud valida también la conectividad con SQL Server, y el frontal espera a que esa comprobación sea correcta antes de iniciar.

Para usar el sistema desde una tablet conectada a la misma red, abra `http://IP_DEL_COMPUTADOR:3000`. El frontal reenvía internamente las solicitudes a la API, por lo que no debe cambiar `localhost` en el navegador ni publicar nombres internos de Docker.

Para detener el sistema sin eliminar la base de datos:

```powershell
docker compose down
```

Los puertos, la dirección de publicación del frontal, ambas cadenas remotas, la seguridad de la cookie y la aplicación controlada de migraciones pueden configurarse en `.env` antes de levantar los servicios. Al copiar `.env.example`, `TALLERES_WEB_IP_PUBLICACION=0.0.0.0` permite abrir el frontal desde una tablet de la red local.

La API no publica un puerto en el anfitrión; desde el navegador se accede a sus funciones mediante el frontal y su proxy interno. Docker no crea, almacena ni elimina la base de datos remota.

Las claves de protección usadas para firmar las cookies de sesión se conservan en el volumen Docker `claves-proteccion-api`. Este volumen debe mantenerse entre despliegues para evitar cerrar las sesiones activas cada vez que se sustituya el contenedor. Al arrancar, la imagen corrige únicamente los permisos de ese directorio y luego ejecuta la API con el usuario sin privilegios de .NET.

## Despliegue en Coolify

Coolify debe desplegar el repositorio como **Docker Compose**, usando `/compose.yaml` como ruta del archivo. No seleccione `src/Talleres.Api/Dockerfile` como recurso independiente: ese Dockerfile solo construye la API, mientras que `compose.yaml` construye y coordina la API y el sistema web en contenedores separados.

Configure estas variables en la sección **Environment Variables** de Coolify:

| Variable | Requerida | Valor recomendado |
|---|---:|---|
| `TALLERES_CONNECTION_STRING` | Sí | Cadena secreta de la base propia de Talleres |
| `SMART_NOVA_CONNECTION_STRING` | Sí | Cadena secreta de SMART TPV NOVA; preferiblemente con permisos de lectura |
| `TALLERES_APLICAR_MIGRACIONES` | No | `false`; habilitarla solo deliberadamente |
| `TALLERES_COOKIE_SEGURA` | No | `true` cuando el dominio use exclusivamente HTTPS |
| `TALLERES_URL_PUBLICA` | Sí para OAuth externo | Origen público del frontal, por ejemplo `https://talleres.example.com`, sin barra final |
| `TALLERES_WEB_IP_PUBLICACION` | No | `127.0.0.1` |
| `TALLERES_WEB_PORT` | No | `0`, para que Docker asigne un puerto anfitrión libre |
| `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` | Solo si se usa Google | Credenciales secretas del proveedor OAuth |
| `MICROSOFT_CLIENT_ID` / `MICROSOFT_CLIENT_SECRET` | Solo si se usa Microsoft | Credenciales secretas del proveedor OAuth |

Asigne el dominio público solamente al servicio `web` e indique el puerto interno `3000` en el dominio de Coolify, por ejemplo `https://talleres.example.com:3000`. No asigne un dominio al servicio `api`: no publica ningún puerto del servidor y el frontal reenvía `/backend` de forma privada a `http://api:8080` dentro de la red de Docker.

Para Google, configure como origen JavaScript autorizado `https://talleres.example.com` y como URI de redirección autorizada `https://talleres.example.com/backend/api/autenticacion/externo/google/callback`. Ambos valores deben coincidir exactamente con el dominio real, incluido `https`. Después de cambiar `TALLERES_URL_PUBLICA` o las credenciales externas, vuelva a desplegar para aplicar la configuración.

El proxy conserva individualmente todas las cookies emitidas por la API. Esto es necesario durante el retorno de Google o Microsoft, porque en una misma respuesta se establece la identidad externa y se elimina la cookie temporal de correlación.

El archivo `.env` es solo para ejecución local y nunca debe subirse a Git. En Coolify, las cadenas de conexión se guardan como variables secretas del recurso. No copie las comillas exteriores usadas en `.env.example` al campo de valor de Coolify y conserve la variable como literal cuando su contraseña contenga `$` u otros caracteres que el sistema pueda interpretar.

Para una base nueva, ejecute un primer despliegue controlado con `TALLERES_APLICAR_MIGRACIONES=true` y una cuenta con permisos para modificar el esquema. Cuando finalice correctamente, cambie inmediatamente la variable a `false` y vuelva a desplegar. Los despliegues ordinarios deben mantenerla en `false`. El período inicial del chequeo de salud permite completar ese primer arranque sin declarar prematuramente que la API está degradada.

Si la API queda `unhealthy`, revise los logs del servicio `api`: `/salud` comprueba las conexiones de `TALLERES_CONNECTION_STRING` y `SMART_NOVA_CONNECTION_STRING`. Verifique credenciales, acceso del servidor Coolify al puerto de SQL Server, reglas de firewall y configuración TLS. No deje en Coolify los valores demostrativos `sql.example.com`, `usuario_lectura` o `clave` de `.env.example`.

## Rutas principales

| Método | Ruta | Operación |
|---|---|---|
| `POST` | `/api/autenticacion/iniciar` | Valida credenciales de NOVA e inicia sesión |
| `GET` | `/api/autenticacion/sesion` | Consulta usuario, taller activo y talleres disponibles |
| `POST` | `/api/autenticacion/seleccionar-taller` | Cambia el taller activo de un superusuario |
| `POST` | `/api/autenticacion/cerrar` | Cierra la sesión |
| `GET` | `/api/talleres-sincronizados` | Lista las empresas NOVA configuradas (solo superusuarios) |
| `POST` | `/api/talleres-sincronizados` | Agrega una empresa NOVA para sincronización (solo superusuarios) |
| `DELETE` | `/api/talleres-sincronizados/{empresaNovaId}` | Retira una empresa de la sincronización (solo superusuarios) |
| `GET` | `/api/clientes` | Lista clientes |
| `POST` | `/api/clientes` | Crea un cliente |
| `GET` | `/api/clientes/{id}` | Obtiene un cliente |
| `PUT` | `/api/clientes/{id}` | Actualiza un cliente |
| `POST` | `/api/vehiculos` | Crea un vehículo |
| `GET` | `/api/vehiculos` | Lista vehículos |
| `GET` | `/api/vehiculos/{id}` | Obtiene un vehículo |
| `GET` | `/api/vehiculos/por-cliente/{id}` | Lista vehículos del cliente |
| `GET` | `/api/ordenes-servicio` | Lista órdenes |
| `POST` | `/api/ordenes-servicio` | Crea una orden |
| `PUT` | `/api/ordenes-servicio/{id}/estado` | Cambia el estado |
| `POST` | `/api/ordenes-servicio/{id}/recepcion` | Registra la recepción |

## Verificación

```powershell
dotnet build Talleres.sln
dotnet test Talleres.sln

cd src/Talleres.Web
npm run lint
npm test
```

Las reglas que deben respetar las ampliaciones futuras están consolidadas en [`AGENTS.md`](AGENTS.md).

## Archivos locales y secretos

- Se versionan el código fuente, migraciones, pruebas, documentación, archivos de proyecto, `package-lock.json` y `.env.example`.
- Se excluyen dependencias instaladas, compilaciones, cachés, bases de datos locales, resultados de pruebas, logs, archivos del IDE y cualquier `.env` real.
- Las cadenas de conexión y credenciales de cada entorno deben configurarse mediante variables de entorno o el almacén de secretos correspondiente; no deben guardarse en Git.
