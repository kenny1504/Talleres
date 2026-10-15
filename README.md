# Talleres

Sistema web tablet-first para gestionar el flujo operativo de un taller automotriz. Incluye un frontal táctil en React y una API construida con ASP.NET Core 9, Entity Framework Core 9 y SQL Server.

## Experiencia web para tablet

El frontal vive en `src/Talleres.Web` y está diseñado primero para tablet horizontal y vertical. Incluye:

- Panel operativo con métricas, bahías y decisiones pendientes.
- Las órdenes listas para entregar permanecen 36 horas en el panel de inicio y luego continúan disponibles únicamente en el listado general de órdenes.
- Navegación lateral para tablet horizontal y navegación inferior para formato vertical.
- Gestión visual de órdenes, clientes, vehículos e inventario.
- Flujo interactivo para crear órdenes y registrar la recepción del vehículo.
- Flujo operativo completo desde recepción hasta lista para entregar, con transiciones explícitas.
- Diagnóstico persistido con dictado por voz, edición manual y evidencia fotográfica opcional.
- Enlace público no predecible para consultar el diagnóstico y autorizar la continuación desde móvil o WhatsApp.
- La vista pública incluye la revisión operativa persistida y su observación, con etiquetas compactas de «Correcto», «Con problema» o «Sin revisar», junto a los elementos entregados. `GET /api/publico/ordenes-servicio/{token}` y la respuesta de autorización incluyen `revisionOperativa` y `observacionRevisionOperativa`; una revisión sin registrar devuelve `[]` y `null`.
- Registro táctil de productos de inventario, compras externas, mano de obra y otros cargos durante la reparación.
- Total calculado y visible desde el detalle de la orden y la preparación de la entrega.
- Estado de cuenta por cliente con cargos de todas sus órdenes no canceladas, placa, marca, modelo y año del vehículo atendido en cada orden, abonos globales y saldo pendiente o a favor.
- Inspección visual táctil por zonas, tipo y severidad del daño, con fotografías públicas en Amazon S3 que pueden consultarse desde el detalle de la orden.
- Descripción editable para cada daño y observaciones generales opcionales de la recepción, con confirmación al guardar sin ellas.
- Dictado por voz en las observaciones generales de la inspección y en la revisión operativa, con edición manual y límite de 2000 caracteres. El guardado se habilita al finalizar el dictado.
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
- Catálogo privado por empresa de marcas y modelos dependientes, reutilizado al registrar vehículos.
- Catálogo de técnicos y mecánicos por empresa con una casilla para indicar el predeterminado al registrarlo o cambiarlo después. El primero queda predeterminado automáticamente y cada orden permite elegir o reasignar un responsable.
- Al crear una orden, omitir `tecnicoTallerId` utiliza el técnico predeterminado activo; si aún no hay técnicos, la orden queda pendiente de asignación.
- Creación y consulta de órdenes de servicio.
- Recepción física del vehículo y avance automático a diagnóstico.
- Diagnóstico obligatorio antes de solicitar autorización; el presupuesto y sus evidencias permanecen opcionales.
- La aprobación digital del cliente es opcional: con el diagnóstico guardado, el taller puede iniciar la reparación directamente desde diagnóstico o desde una orden por aprobar, sin teléfono ni interacción del cliente con el sistema. El historial registra si existía autorización digital y no completa su fecha cuando el inicio lo decide el taller. El enlace público y WhatsApp siguen disponibles para quienes prefieran autorizar por esa vía.
- Transiciones controladas del estado de las órdenes.
- Detalles de reparación persistidos con precio, cantidad, subtotal, origen y total de la orden.
- Pagos registrados directamente al cliente, sin asignación a una orden. Solo el monto es obligatorio; la forma de pago y la referencia son opcionales. Los pagos anulados se conservan en el historial y dejan de reducir el saldo.
- El saldo se calcula con los cargos actuales de órdenes no canceladas menos pagos vigentes. Los cargos de órdenes en reparación pueden variar hasta terminar el trabajo; los cobros anteriores que no estén registrados en Talleres deben incorporarse para reflejar un saldo histórico correcto.
- Descuento auditable de inventario mediante una salida de consumo interno de SMART TPV NOVA cuando hay existencia suficiente; los faltantes se conservan como cargos sin alterar stock.
- Historial de estados.
- Aislamiento multitenant por `EmpresaId` en consultas y escrituras.
- Empresa activa obtenida de una cookie de sesión HTTP-only; el cliente no puede elegirla mediante encabezados.
- Denegación de acceso para usuarios normales cuya empresa no esté configurada como taller sincronizado.
- Validaciones estructurales mediante Data Annotations.
- Excepciones de negocio convertidas a `ProblemDetails` por middleware global.
- Evidencias de inspección aisladas por empresa, con metadatos en SQL Server y objetos públicos en Amazon S3.

## Estado del negocio

La sección **Estado del negocio** está disponible en la navegación principal y en **Más** en tablet vertical y móvil. Presenta cobros, importe de trabajos, cartera actual de clientes, principales clientes por importe y vehículos, marcas y modelos con más visitas. No incluye gastos, ganancia ni obligaciones con proveedores.

- El período inicial comprende desde el primer día del mes hasta hoy. Permite seleccionar hoy, esta semana, este mes o un rango de hasta 366 días y agrupar los cobros por día, semana (lunes a domingo) o mes.
- Los cobros son abonos vigentes cuya fecha de registro pertenece al período. El importe de trabajos suma los cargos actuales, redondeados por orden, de órdenes no canceladas **ingresadas** en el período; incluye trabajos en proceso y no equivale a dinero cobrado ni a facturación definitiva.
- La cartera consulta todo el historial actual registrado, independientemente del período. Excluye órdenes canceladas y pagos anulados; muestra cuentas con cargos o abonos, incluso de clientes inactivos. Los saldos a favor se muestran separados y nunca compensan deudas de otros clientes. Un cliente sin movimientos no cuenta como liquidado.
- Los pagos continúan siendo globales por cliente; no se identifica ninguna orden como pagada. Abrir una cuenta conserva el período del tablero y vuelve a consultar los indicadores al regresar. Los vehículos permiten abrir su historial completo.
- **Pagos de clientes** muestra únicamente pagos vigentes en filas compactas: cliente con fecha y hora local debajo, e importe a la derecha. No presenta formas de pago, referencias ni estados. Permite abrir el estado de cuenta, buscar por nombre del cliente y limitarse al período del dashboard o consultar todo el historial. Carga diez registros por página desde el servidor y limita la altura del listado a 340 px con desplazamiento vertical; la paginación permanece fuera del área desplazable.
- `GET /api/estado-negocio/pagos?pagina=1&tamanoPagina=10` consulta pagos vigentes; `busqueda` filtra únicamente por nombre de cliente (hasta 120 caracteres) y `tamanoPagina` admite de 1 a 50, con diez por defecto. Para limitarlo al período, indicar conjuntamente `desde`, `hasta` y `desfaseHorarioMinutos`, con las mismas reglas de fechas que el resumen. Los anulados se excluyen en el servidor antes del conteo y de la paginación; la respuesta devuelve cliente, monto y fecha con zona UTC explícita. El historial y la anulación siguen disponibles en el estado de cuenta individual.
- `GET /api/estado-negocio?desde=2026-10-01&hasta=2026-10-14&desfaseHorarioMinutos=360&agrupacion=Dia` es una consulta autenticada de solo lectura, aislada por `EmpresaId`. Las fechas son inclusivas y el desfase enviado por el navegador sigue `getTimezoneOffset` (UTC menos hora local); la agrupación admite `Dia`, `Semana` y `Mes`. No requiere migraciones ni cambios en SMART TPV NOVA.
- Los importes solo reflejan cargos y abonos registrados en Talleres. Ante una consulta fallida se muestra el error y la opción de reintentar, sin sustituir datos por ejemplos.

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
- Acceso de lectura a catálogos y existencias de SMART TPV NOVA, y permiso para ejecutar `dbo.sp_RegistrarSalida` al consumir inventario y `dbo.sp_AnularSalida` al devolverlo desde una orden.
- Herramienta `dotnet-ef` 9.x para administrar migraciones.
- Acceso de escritura al bucket de Amazon S3 cuando se usarán evidencias fotográficas.

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
$env:ConnectionStrings__SmartNova = "Server=sql.example.com,1433;Database=Galileo;User ID=usuario_integracion;Password=clave;Encrypt=True;TrustServerCertificate=False"
```

También se admiten `TALLERES_CONNECTION_STRING` y `SMART_NOVA_CONNECTION_STRING`. En desarrollo, si
las variables correspondientes no están definidas, la API las carga desde el
archivo `.env` ubicado junto a `Talleres.sln`. Esto permite usar `dotnet run` y los perfiles del
IDE sin copiar credenciales a `launchSettings.json`. Las variables del proceso siempre tienen
precedencia sobre el archivo local.

Las fotografías de inspección usan por defecto el bucket público `biossoft-mereb-crm` en `eu-north-1`, igual que SMART TPV NOVA, y se aíslan bajo el prefijo `empresas/{empresaId}/recepciones/{recepcionId}/`. Para cargar y eliminar archivos defina `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` y, cuando corresponda, `AWS_SESSION_TOKEN` en el `.env` local ignorado. La identidad de AWS debe tener como mínimo `s3:PutObject` y `s3:DeleteObject` sobre ese prefijo. También se aceptan los nombres históricos `AmazonS3__UrlBucket`, `AmazonS3__Region`, `AmazonS3__AwsAccesKey` y `AmazonS3__AwsSecretKey`. Los objetos se publican con ACL `PublicRead` y la API redirige a su URL permanente `https://{bucket}.s3.amazonaws.com/{clave}` para mostrarlos en la inspección. Nunca versione las credenciales.

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

La migración `AgregarTalleresSincronizados` crea la configuración local e incluye inicialmente la empresa NOVA `3071`. Agregar otra empresa a esa tabla la habilita como taller, siempre que exista en NOVA. La migración `AgregarDetallesOrdenServicio` agrega el detalle económico de las órdenes y `AgregarDiagnosticoYEnlacePublico` incorpora el diagnóstico, sus evidencias y la autorización pública, sin modificar el esquema de SMART TPV NOVA.

## Ejecución completa con Docker

Docker Compose levanta el frontal y la API de forma coordinada. La base de datos debe ser una instancia remota de SQL Server accesible desde el contenedor de la API.

La preparación del entorno se realiza una única vez:

```powershell
Copy-Item .env.example .env
```

Edite `.env` y reemplace `TALLERES_CONNECTION_STRING` y `SMART_NOVA_CONNECTION_STRING` por las cadenas reales. La cuenta de NOVA debe limitarse a las consultas indicadas y a ejecutar los procedimientos oficiales de salida y anulación de inventario. La corrección aislada que registra una anulación como movimiento `ENTRADA` está versionada en `src/Talleres.Infraestructura/Integraciones/SmartNova/Sql/Talleres/20260921_CorregirTipoMovimientoAnulacionSalida.sql`. El archivo `.env` está excluido de Git y no debe subirse al repositorio.

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
| `SMART_NOVA_CONNECTION_STRING` | Sí | Cadena secreta de SMART TPV NOVA con lectura de catálogos y ejecución y anulación de salidas de inventario |
| `TALLERES_APLICAR_MIGRACIONES` | No | `false`; habilitarla solo deliberadamente |
| `TALLERES_COOKIE_SEGURA` | No | `true` cuando el dominio use exclusivamente HTTPS |
| `TALLERES_URL_PUBLICA` | Sí para OAuth externo | Origen público del frontal, por ejemplo `https://talleres.example.com`, sin barra final |
| `TALLERES_WEB_IP_PUBLICACION` | No | `127.0.0.1` |
| `TALLERES_WEB_PORT` | No | `0`, para que Docker asigne un puerto anfitrión libre |
| `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` | Solo si se usa Google | Credenciales secretas del proveedor OAuth |
| `MICROSOFT_CLIENT_ID` / `MICROSOFT_CLIENT_SECRET` | Solo si se usa Microsoft | Credenciales secretas del proveedor OAuth |
| `AWS_S3_BUCKET` | No | Bucket de evidencias; por defecto `biossoft-mereb-crm` |
| `AWS_REGION` | No | Región del bucket; por defecto `eu-north-1` |
| `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` | Según despliegue | Credenciales secretas; omitir cuando el contenedor usa un rol IAM |
| `AWS_SESSION_TOKEN` | Solo para credenciales temporales | Token de sesión de AWS |

Asigne el dominio público solamente al servicio `web` e indique el puerto interno `3000` en el dominio de Coolify, por ejemplo `https://talleres.example.com:3000`. No asigne un dominio al servicio `api`: no publica ningún puerto del servidor y el frontal reenvía `/backend` de forma privada a `http://api:8080` dentro de la red de Docker.

Para Google, configure como origen JavaScript autorizado `https://talleres.example.com` y como URI de redirección autorizada `https://talleres.example.com/backend/api/autenticacion/externo/google/callback`. Ambos valores deben coincidir exactamente con el dominio real, incluido `https`. Después de cambiar `TALLERES_URL_PUBLICA` o las credenciales externas, vuelva a desplegar para aplicar la configuración.

El proxy conserva individualmente todas las cookies emitidas por la API. Esto es necesario durante el retorno de Google o Microsoft, porque en una misma respuesta se establece la identidad externa y se elimina la cookie temporal de correlación.

El archivo `.env` es solo para ejecución local y nunca debe subirse a Git. En Coolify, las cadenas de conexión se guardan como variables secretas del recurso. No copie las comillas exteriores usadas en `.env.example` al campo de valor de Coolify y conserve la variable como literal cuando su contraseña contenga `$` u otros caracteres que el sistema pueda interpretar.

Para una base nueva, ejecute un primer despliegue controlado con `TALLERES_APLICAR_MIGRACIONES=true` y una cuenta con permisos para modificar el esquema. Cuando finalice correctamente, cambie inmediatamente la variable a `false` y vuelva a desplegar. Los despliegues ordinarios deben mantenerla en `false`. El período inicial del chequeo de salud permite completar ese primer arranque sin declarar prematuramente que la API está degradada.

Si la API queda `unhealthy`, revise los logs del servicio `api`: `/salud` comprueba las conexiones de `TALLERES_CONNECTION_STRING` y `SMART_NOVA_CONNECTION_STRING`. Verifique credenciales, acceso del servidor Coolify al puerto de SQL Server, reglas de firewall y configuración TLS. No deje en Coolify los valores demostrativos `sql.example.com`, `usuario_integracion` o `clave` de `.env.example`.

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
| `GET` | `/api/clientes/{id}/estado-cuenta` | Consulta órdenes, pagos y saldo global del cliente |
| `POST` | `/api/clientes/{id}/pagos` | Registra un abono al cliente; `monto` es el único dato obligatorio |
| `POST` | `/api/clientes/{id}/pagos/{pagoId}/anular` | Anula un pago conservando el registro |
| `POST` | `/api/vehiculos` | Crea un vehículo |
| `GET` | `/api/vehiculos` | Lista vehículos |
| `GET` | `/api/vehiculos/{id}` | Obtiene un vehículo |
| `GET` | `/api/vehiculos/por-cliente/{id}` | Lista vehículos del cliente |
| `GET` / `POST` / `PUT` | `/api/catalogos-vehiculos/marcas` | Consulta y administra marcas de la empresa |
| `GET` / `POST` / `PUT` | `/api/catalogos-vehiculos/modelos` | Consulta y administra modelos dependientes de una marca |
| `GET` | `/api/ordenes-servicio` | Lista órdenes |
| `POST` | `/api/ordenes-servicio` | Crea una orden |
| `PUT` | `/api/ordenes-servicio/{id}/estado` | Cambia el estado |
| `GET` / `PUT` | `/api/ordenes-servicio/{id}/diagnostico` | Consulta o guarda el diagnóstico y genera su token público |
| `POST` / `DELETE` | `/api/ordenes-servicio/{id}/diagnostico/evidencias` | Administra evidencia fotográfica opcional del diagnóstico |
| `GET` | `/api/publico/ordenes-servicio/{token}` | Muestra al cliente la información limitada de su orden sin iniciar sesión |
| `POST` | `/api/publico/ordenes-servicio/{token}/autorizar` | Registra la autorización del cliente para continuar |
| `GET` | `/api/ordenes-servicio/{id}/detalles` | Consulta productos, cargos y total |
| `POST` | `/api/ordenes-servicio/{id}/detalles/inventario` | Agrega un producto y descuenta existencia cuando está disponible |
| `POST` | `/api/ordenes-servicio/{id}/detalles/manual` | Agrega mano de obra, una compra externa u otro cargo manual |
| `DELETE` | `/api/ordenes-servicio/{id}/detalles/{detalleId}` | Elimina un cargo que no haya generado una salida de inventario |
| `GET` / `POST` / `PUT` | `/api/ordenes-servicio/{id}/recepcion` | Consulta, registra o corrige la recepción e inspección |
| `POST` | `/api/ordenes-servicio/{id}/recepcion/evidencias` | Carga fotografías JPEG, PNG o WebP a S3 |
| `GET` | `/api/ordenes-servicio/{id}/recepcion/evidencias/{evidenciaId}/contenido` | Valida la empresa y redirige a la URL pública permanente de la evidencia |
| `DELETE` | `/api/ordenes-servicio/{id}/recepcion/evidencias/{evidenciaId}` | Elimina la fotografía de S3 y de la inspección |

## Verificación

La recepción incluye una **revisión operativa** independiente de los elementos entregados: luces delanteras, bajas, altas, direccionales y laterales, luz de freno, halógenos, aceite de motor, líquido de frenos, líquido de embrague (clutch), aceite de dirección, refrigerante, recordatorio de aceite y bocina. Cada punto admite ✓ Correcto o ✕ Con problema; sin selección significa sin revisar. Volver a tocar la opción seleccionada la deja sin revisar. La observación de esta revisión es opcional, admite hasta 2000 caracteres y permite dictado en español en navegadores compatibles con acceso al micrófono. El dictado añade texto al existente y permite corregirlo antes de guardar. Debe detenerse antes de enviar la inspección.

`revisionOperativa` contiene únicamente actividades revisadas (`actividad` y `correcto`); no admite actividades desconocidas ni repetidas. `observacionRevisionOperativa` conserva su comentario independiente de `descripcionEstado`. Ambos se guardan atómicamente junto con la recepción y se consultan y editan desde el detalle de la orden. En una actualización se envía el conjunto completo de selecciones; `[]` elimina las selecciones, y omitir `revisionOperativa` conserva la revisión anterior y su observación para mantener compatibilidad con clientes anteriores. Esta revisión no modifica los indicadores de elementos entregados ni agrega cambios a SMART TPV NOVA.

Antes de ejecutar esta versión sobre una base existente debe aplicarse deliberadamente la migración `AgregarRevisionOperativaRecepcion`. Las recepciones anteriores quedan sin actividades revisadas y sin observación de revisión operativa; no se deduce su estado a partir de los elementos entregados. Las migraciones automáticas continúan desactivadas por defecto.

La recepción registra llaves, documentos, llanta de repuesto, triángulo de seguridad, gata, llave de ruedas, gancho de remolque, extintor, botiquín y chaleco reflectante. `otrosElementos` permite describir herramientas o accesorios adicionales (máximo 500 caracteres). `aceptaPruebaRuta` conserva la aceptación de la prueba de ruta. Estos datos se guardan junto con la inspección, se cargan al editarla y se muestran en el detalle y en el enlace público del cliente. Al actualizar se envía el conjunto completo de datos y hallazgos. Las solicitudes de ejemplo están en `src/Talleres.Api/Talleres.Api.http`.

La observación general (`descripcionEstado`) es opcional al registrar y actualizar la inspección, con un máximo de 2000 caracteres. La API admite omitirla, enviarla como `null` o vacía y conserva la ausencia de observaciones como texto vacío. La web solicita confirmación antes de guardar cuando el campo está vacío o solo contiene espacios; permite volver a completarlo o continuar sin observaciones. Este ajuste no requiere cambios en el esquema de la base de datos.

Para habilitar estos campos en un entorno existente, aplique deliberadamente la migración `AgregarElementosRecepcionVehiculo` antes de ejecutar la API actualizada. Los campos nuevos quedan desmarcados y `otrosElementos` vacío en las recepciones anteriores; la aceptación histórica de la prueba de ruta no se puede recuperar porque no se persistía. Las migraciones automáticas continúan desactivadas por defecto.

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
