# Directrices obligatorias del proyecto Talleres

Este archivo gobierna todo el repositorio. Sus reglas son requisitos obligatorios, no recomendaciones. Un `AGENTS.md` ubicado en una carpeta hija puede agregar reglas más específicas, pero no contradecir estas directrices.

## 1. Producto y prioridad de experiencia

- Talleres es un **sistema web**, no una API aislada. La API existe para respaldar la experiencia del producto.
- La interfaz se diseña y valida primero para tablet táctil, tanto en orientación horizontal como vertical.
- El sistema también debe funcionar correctamente en computador y adaptarse a móvil, sin degradar la experiencia principal de tablet.
- La interfaz, los textos del negocio y los mensajes dirigidos al usuario se escriben en español claro.
- Los flujos operativos deben ser rápidos, legibles y utilizables con las manos ocupadas: controles amplios, jerarquía visual evidente y pocas acciones por paso.

## 2. Arquitectura y dependencias

La solución mantiene estas responsabilidades:

```text
src/
├── Talleres.Dominio/          Entidades, enumeraciones, reglas y excepciones
├── Talleres.Aplicacion/       DTO, contratos y servicios de aplicación
├── Talleres.Infraestructura/  EF Core, configuraciones, migraciones e integraciones
├── Talleres.Api/              HTTP, middleware y composición de dependencias
└── Talleres.Web/              Sistema web tablet-first
tests/
└── Talleres.Pruebas/          Pruebas automatizadas
```

- `Dominio` no depende de infraestructura, API ni presentación.
- `Aplicacion` coordina casos de uso mediante contratos y depende del dominio.
- `Infraestructura` implementa persistencia e integraciones.
- `Api` expone los casos de uso y configura inyección de dependencias.
- `Web` consume contratos HTTP; no reproduce reglas de negocio que deben vivir en el servidor.
- No introducir referencias circulares ni mover reglas de negocio al controlador o al frontal.

## 3. Lenguaje del código

- Entidades, propiedades, DTO, enumeraciones, interfaces, servicios, métodos, parámetros, validadores, comandos, consultas y tipos propios se nombran en español.
- Los nombres expresan la intención del negocio; quedan prohibidos nombres ambiguos como `GetData`, `Process`, `Execute`, `HandleData` o `UtilService`.
- En el frontal, los tipos, estados, funciones y componentes propios del negocio también se nombran en español. Los nombres impuestos por bibliotecas o frameworks se conservan.
- Mantener una convención uniforme; no mezclar inglés y español en un mismo contrato.

## 4. Persistencia con Entity Framework Core

- Todo acceso a datos usa Entity Framework Core mediante `DbContext`, `DbSet`, Fluent API y migraciones.
- No usar ADO.NET directo, Dapper u otro mecanismo sin una justificación técnica concreta y documentada.
- Las configuraciones se separan por entidad mediante `IEntityTypeConfiguration<T>`; evitar configuraciones extensas en `OnModelCreating`.
- Las consultas de solo lectura usan `AsNoTracking()` cuando corresponde.
- Las operaciones son asíncronas (`ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `SaveChangesAsync`, etc.) y reciben `CancellationToken`.
- No crear repositorios genéricos ni una unidad de trabajo sobre EF Core si únicamente duplican las capacidades del contexto.
- Los servicios acceden a persistencia a través de `ITallerDbContext` u otro contrato específico que aporte valor real.
- Los procesos con varias escrituras relacionadas deben ser atómicos. Usar una transacción explícita solo cuando un único `SaveChangesAsync` no garantice la consistencia requerida.
- Todo cambio del modelo persistido debe incluir una nueva migración y actualizar el snapshot. No editar una migración ya compartida para simular una nueva evolución.

## 5. Servicios y controladores

- Los controladores son delgados: reciben la petición, resuelven el contexto autorizado, delegan al servicio y construyen la respuesta HTTP.
- Los controladores no contienen consultas de EF Core, transacciones, cálculos, cambios de estado, reglas de inventario, auditoría, numeración de órdenes ni integraciones externas.
- La lógica de aplicación y las reglas de negocio viven en servicios pequeños con responsabilidad clara.
- No crear servicios gigantes como `TallerServicio`, `ServicioGeneral`, `ServicioMaestro` o equivalentes.
- Cada servicio de aplicación o integración tiene una interfaz.
- Cada interfaz de servicio documenta sus miembros con XML útil: propósito, parámetros relevantes, resultado y comportamiento importante.
- Las implementaciones no repiten la documentación XML del contrato salvo que agreguen un comportamiento específico.
- Todos los servicios se registran mediante inyección de dependencias. No instanciarlos manualmente desde controladores u otros servicios.

## 6. Validaciones, errores y contratos HTTP

- Separar las validaciones estructurales (obligatoriedad, formato, longitud y rangos) de las reglas de negocio (pertenencia, estado, autorización y disponibilidad).
- Las reglas de negocio producen excepciones de dominio descriptivas; no retornan `false`, `null` ni textos ambiguos como mecanismo general de error.
- El middleware global convierte las excepciones de dominio en respuestas HTTP coherentes, preferentemente `ProblemDetails`.
- Los DTO de entrada no exponen entidades de EF Core ni permiten asignar campos protegidos por el cliente.
- Una modificación de contratos HTTP debe mantener sincronizados el frontal, las solicitudes de ejemplo y la documentación.

## 7. Multitenencia y seguridad

- Toda lectura y escritura respeta el aislamiento por `EmpresaId`, incluidos recursos relacionados, búsquedas por identificador y validaciones de existencia.
- Nunca aceptar como válida una relación entre registros hasta comprobar que ambos pertenecen a la empresa activa.
- En producción, `EmpresaId` debe provenir de una identidad autenticada y autorizada. El encabezado `X-Empresa-Id` es únicamente un mecanismo de desarrollo mientras se implementa autenticación.
- No confiar en identificadores, estados, precios, permisos ni porcentajes enviados por el frontal sin validación del servidor.
- No registrar secretos, datos sensibles completos ni contenido privado de fotografías en logs.
- No guardar credenciales en el repositorio. Usar variables de entorno, secretos de desarrollo o el proveedor de configuración del entorno.

## 8. Flujo de órdenes, recepción e inspección

- La recepción del vehículo y la inspección visual son parte del flujo principal de una orden; no son pantallas decorativas ni datos solo locales.
- Una inspección conserva kilometraje, combustible, elementos entregados, descripción general del estado y hallazgos visuales.
- Cada hallazgo conserva como mínimo zona, tipo, severidad y observación propia.
- Las observaciones generales y la descripción de cada hallazgo deben poder verse y editarse posteriormente desde el detalle de la orden.
- La edición debe cargar los valores persistidos, permitir corregirlos y guardar el conjunto completo sin perder hallazgos previos.
- Toda consulta o actualización de una inspección valida que la orden y la recepción pertenezcan a la empresa activa.
- Los cambios de estado de la orden se realizan mediante transiciones explícitas y validadas; no se asignan estados arbitrariamente desde la interfaz.
- Si se persisten fotografías o evidencias, hacerlo mediante un servicio de almacenamiento específico. El controlador no administra archivos ni reglas de evidencia directamente.

## 9. Experiencia web tablet-first

- Los procesos largos o críticos —crear orden, recibir vehículo, inspeccionar, diagnosticar o controlar calidad— se presentan como páginas de trabajo completas.
- No usar paneles laterales ni modales estrechos para esos procesos. Los modales se reservan para confirmaciones breves o información pequeña y descartable.
- En tablet horizontal y computador se favorecen distribuciones de dos columnas que mantengan contexto y formulario visibles.
- En tablet vertical se usa un flujo de una columna, ordenado y secuencial, sin desplazamiento horizontal.
- La navegación global no debe competir con un proceso activo; puede simplificarse u ocultarse mientras el usuario trabaja y debe ofrecer un regreso evidente.
- Las acciones principales deben permanecer accesibles sin tapar contenido. Los controles táctiles tienen un área mínima recomendada de 44 × 44 px y separación suficiente.
- No depender de `hover`, clic derecho ni precisión de puntero. Toda interacción debe funcionar con tacto, teclado y mouse.
- Formularios y botones deben mostrar estados de carga, éxito, error, deshabilitado y validación. Evitar envíos dobles.
- Mantener foco visible, etiquetas accesibles, contraste suficiente y orden de tabulación lógico.
- Probar al menos anchos representativos de tablet vertical, tablet horizontal y escritorio antes de cerrar un cambio visual.

## 10. Frontal y consumo de la API

- Mantener TypeScript estricto y evitar `any` salvo integración inevitable y documentada.
- Centralizar la URL de la API en `NEXT_PUBLIC_API_URL`; no incrustar direcciones de producción en el código.
- Los datos de demostración son solo para desarrollo y deben estar claramente diferenciados. Un error de producción no puede ocultarse sustituyendo silenciosamente datos reales por datos simulados.
- No almacenar secretos en variables `NEXT_PUBLIC_*`: todo valor con ese prefijo llega al navegador.
- La interfaz puede validar estructura y mejorar la interacción, pero el servidor conserva la autoridad sobre reglas, pertenencia, estados y persistencia.
- Dividir componentes cuando una pantalla acumule responsabilidades; priorizar componentes orientados al flujo y evitar abstracciones visuales prematuras.
- Preservar la estructura de compilación de `vinext`, el worker y `.openai/hosting.json` mientras el proyecto se publique mediante Sites.

## 11. Archivos y repositorio

- Sí se versionan: código fuente, solución y proyectos, migraciones, pruebas, documentación, configuración sin secretos, `.env.example`, `package.json` y `package-lock.json`.
- No se versionan: `bin`, `obj`, `node_modules`, `.next`, `.vinext`, `dist`, coberturas, resultados de pruebas, cachés, bases de datos locales, logs, archivos del IDE, archivos `.env` reales, credenciales ni evidencias cargadas por usuarios.
- `.env.example` contiene únicamente nombres y valores seguros de ejemplo; debe actualizarse cuando se agregue una variable requerida.
- Los archivos `appsettings*.json` versionados no contienen contraseñas ni tokens. Las variantes personales usan un nombre ignorado, como `appsettings.Local.json`.
- El lockfile de Node se mantiene sincronizado con `package.json` y se usa `npm ci` en integración continua.
- No agregar al repositorio una carpeta `.git` anidada.

## 12. Contenedores y ejecución local

- `compose.yaml` es la entrada única para levantar web y API; mantener funcional `docker compose up --build`.
- Cada proceso vive en su propio contenedor. No ejecutar API y frontal bajo un supervisor dentro del mismo contenedor.
- Las imágenes usan compilación multietapa, usuario sin privilegios cuando la imagen lo permite y contextos sin secretos ni artefactos locales.
- La base de datos no se ejecuta en Docker. La API obtiene la cadena de SQL Server remoto desde `TALLERES_CONNECTION_STRING` en el `.env` local ignorado.
- Nunca fijar cadenas de conexión, contraseñas ni credenciales reales en `compose.yaml`, Dockerfiles o archivos versionados.
- La API aplica migraciones al iniciar únicamente cuando `BaseDatos:AplicarMigracionesAlIniciar` está habilitado por el entorno.
- Las migraciones automáticas sobre una base remota permanecen desactivadas por defecto y solo se habilitan de manera deliberada.
- En Docker, el navegador consume `/backend` y el frontal reenvía al nombre interno del servicio API. No publicar `http://api:8080` en código cliente.
- La salud de la API incluye conectividad con la base remota. Mantener comprobaciones de salud y dependencias de arranque para evitar que el frontal se declare listo antes de la API o la base de datos.
- Todo cambio de puertos, variables, imágenes o comandos Docker debe quedar documentado en `README.md` y `.env.example`.

## 13. Estilo y mantenibilidad

- Priorizar claridad, cohesión, rendimiento razonable, nombres descriptivos y métodos pequeños.
- Los comentarios explican decisiones o reglas no evidentes; no narran literalmente el código.
- Evitar duplicación, abstracciones sin uso actual y dependencias que no aporten al producto.
- Conservar los cambios del usuario y no reformatear archivos ajenos al alcance de la tarea.
- Actualizar `README.md` cuando cambien requisitos, puesta en marcha, variables, arquitectura o comandos.

## 14. Definición de terminado

Antes de considerar terminada cualquier funcionalidad:

1. Confirmar que controladores y frontal no contienen reglas de negocio que correspondan al servicio.
2. Confirmar que el servicio tiene responsabilidad clara, interfaz documentada e inyección de dependencias.
3. Verificar nombres propios en español, multitenencia y cancelación asíncrona.
4. Revisar consultas de lectura, reglas de negocio, manejo de errores y migraciones.
5. Validar la experiencia táctil en tablet vertical y horizontal cuando exista un cambio de interfaz.
6. Desde la raíz, ejecutar `dotnet build Talleres.sln` y `dotnet test Talleres.sln`.
7. Desde `src/Talleres.Web`, ejecutar `npm ci`, `npm run lint` y `npm test`.
8. Validar `docker compose config` y, cuando Docker esté disponible, construir y comprobar la salud de los servicios afectados.
9. Revisar `git status --short --ignored` para confirmar que no se incluirán secretos ni artefactos generados.

Si alguna verificación no puede ejecutarse, debe informarse explícitamente; no se presume que el cambio está terminado.
