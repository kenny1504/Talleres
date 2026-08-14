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

## Alcance implementado

- Registro, actualización, consulta y listado de clientes.
- Registro y consulta de vehículos asociados a clientes.
- Creación y consulta de órdenes de servicio.
- Recepción física del vehículo y avance automático a diagnóstico.
- Transiciones controladas del estado de las órdenes.
- Historial de estados.
- Aislamiento multitenant por `EmpresaId` en consultas y escrituras.
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
```

También se admite `TALLERES_CONNECTION_STRING` como nombre de variable. En desarrollo, si
ninguna de esas variables está definida, la API carga `TALLERES_CONNECTION_STRING` desde el
archivo `.env` ubicado junto a `Talleres.sln`. Esto permite usar `dotnet run` y los perfiles del
IDE sin copiar credenciales a `launchSettings.json`. Las variables del proceso siempre tienen
precedencia sobre el archivo local.

Todas las rutas bajo `/api` requieren el encabezado:

```http
X-Empresa-Id: 1
```

El archivo [`src/Talleres.Api/Talleres.Api.http`](src/Talleres.Api/Talleres.Api.http) incluye solicitudes de ejemplo. El endpoint `GET /salud` no requiere empresa.

> El encabezado multitenant permite desarrollar y probar el aislamiento. Antes de publicar, `EmpresaId` debe obtenerse de una identidad autenticada y autorizada, no confiarse directamente al cliente.

## Ejecución completa con Docker

Docker Compose levanta el frontal y la API de forma coordinada. La base de datos debe ser una instancia remota de SQL Server accesible desde el contenedor de la API.

La preparación del entorno se realiza una única vez:

```powershell
Copy-Item .env.example .env
```

Edite `.env` y reemplace `TALLERES_CONNECTION_STRING` por la cadena real de la base remota. El archivo `.env` está excluido de Git y no debe subirse al repositorio.

`TALLERES_APLICAR_MIGRACIONES` permanece en `false` por defecto. Cámbielo a `true` únicamente cuando la API tenga autorización para aplicar las migraciones de EF Core sobre esa base.

Después, todo el sistema se construye y levanta con una sola llamada desde la raíz:

```powershell
docker compose up --build
```

Servicios publicados:

| Servicio | Dirección |
|---|---|
| Sistema web | `http://localhost:3000` |
| API y salud | `http://localhost:8080/salud` (solo en el computador anfitrión) |

La API recibe la conexión remota mediante una variable de entorno y no almacena la cadena dentro de la imagen. Su comprobación de salud valida también la conectividad con SQL Server, y el frontal espera a que esa comprobación sea correcta antes de iniciar.

Para usar el sistema desde una tablet conectada a la misma red, abra `http://IP_DEL_COMPUTADOR:3000`. El frontal reenvía internamente las solicitudes a la API, por lo que no debe cambiar `localhost` en el navegador ni publicar nombres internos de Docker.

Para detener el sistema sin eliminar la base de datos:

```powershell
docker compose down
```

Los puertos, la empresa de desarrollo, la cadena remota y la aplicación controlada de migraciones pueden configurarse en `.env` antes de levantar los servicios.

La API se publica únicamente sobre `127.0.0.1`; desde otros dispositivos se accede a sus funciones mediante el frontal y su proxy interno. Docker no crea, almacena ni elimina la base de datos remota.

## Rutas principales

| Método | Ruta | Operación |
|---|---|---|
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
