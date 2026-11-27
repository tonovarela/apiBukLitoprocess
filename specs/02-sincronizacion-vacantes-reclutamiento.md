# SPEC 02 — Sincronización de vacantes (procesos de selección) desde Buk

> **Estado:** Aprobado
> **Depende de:** SPEC 01 (solo la convención `ToUpperInvariant` para puestos)
> **Fecha:** 2026-11-27
> **Objetivo:** Que `GET /api/reclutamiento/vacantes/sync` descargue los procesos de selección de Buk creados desde una fecha, los mapee a `VacanteDTO` en `ReclutamientoService` y los guarde con upsert por `id_vacante` en `Buk.dbo.Vacante`.

## Por qué existe esta spec

Esta spec **reemplaza** la SPEC 02 de la rama `solicitudes` (`02-sincronizacion-vacantes-buk.md`). Aquella tomaba las vacantes de `recruiting/applications`, que en realidad son postulaciones de candidatos. Aquí una vacante es un **proceso de selección** (`recruiting/selection_processes`). Se parte de `main` y no se reutiliza código de esa rama.

La forma de la respuesta se verificó contra Buk el 2026-11-27:

- `GET https://litoprocess.buk.mx/api/v1/mexico/recruiting/selection_processes?page_size=100&from=2026-01-01` respondió 200.
- Devolvió 29 procesos en 1 página (`pagination.total_pages = 1`, `pagination.count = 29`), con `id` únicos entre 100 y 931.
- `from` filtra por `created_at`, comparado en UTC. Con `from=2026-09-01` entró un proceso creado `2026-08-31T18:19:19-06:00`.
- `from` con una fecha inválida (`2026-13-45`) responde **400**.
- En la práctica Buk usa un `page_size` mínimo de 25. `total_pages` y `page=N` funcionan. Con `page_size=10` respondió 25 + 4 en 2 páginas.
- Sin `/mexico` en la ruta (`/api/v1/recruiting/selection_processes`) también responde 200 con los mismos 29 registros.

Estructura relevante de cada elemento de `data[]` (ejemplo **sintético**):

```json
{
  "id": 298,
  "name": "Ayudante - GERENCIA DE PLANTA, DIRECCION DE OPERACIONES, JEFATURA DE LITOGRAFIA",
  "role": { "id": 260, "name": "Ayudante" },
  "status": "cerrado",
  "created_at": "2026-07-20T13:48:25-06:00"
}
```

Valores observados:

- `status`: cerrado (24), iniciado (3) y cancelado (2). El valor más largo mide 9 caracteres.
- `role.name` viene en los 29 registros, igual que `created_at`. El valor más largo mide 30 caracteres.
- La respuesta trae además `area`, `company`, `location`, `minimum_salary`, `maximum_salary`, `supervisor`, `recruiters`, `description`, `requirements`, `process_stages`, `public_token` y otros campos. Ninguno se usa.

El esquema de la tabla destino se leyó de `INFORMATION_SCHEMA` y `sys.indexes` el 2026-11-27, en la BD del `.env`, después de que el usuario definió la PK:

**`Buk.dbo.Vacante`** (0 filas)

| Columna          | Tipo           | Nulos | Llave |
| ---------------- | -------------- | ----- | ----- |
| `id_vacante`     | `int`          | No    | PK    |
| `puesto`         | `varchar(250)` | Sí    |       |
| `estado`         | `varchar(50)`  | Sí    |       |
| `fecha_registro` | `datetime`     | Sí    |       |

- La PK es `PK_Vacante`, `CLUSTERED` sobre `id_vacante`. La columna **no** es `IDENTITY`, así que el id lo pone Buk.
- No tiene otros índices ni triggers.
- Las columnas no se llaman igual que las propiedades del DTO: `nombre` → `puesto` y `estatus` → `estado`.

## Alcance

**Incluye:**

- Modelo de la respuesta de Buk en `ResponseApi/ResponseVacante.cs`, con el patrón de `ResponseVacaciones.cs`.
- Mapper `mappers/VacanteExtensions.cs`, que construye `VacanteDTO` a partir de `VacanteRest`.
- Interfaz `repository/interfaces/IVacanteRepository.cs` con `GuardarVacantes`.
- Implementación `repository/implementation/VacanteRepository.cs`, con un upsert por `id_vacante` en `Buk.dbo.Vacante` dentro de una transacción de todo o nada.
- Servicio `Services/ReclutamientoService.cs` con toda la lógica de negocio: armar la URL con `from`, paginar, filtrar, mapear y llamar al repositorio.
- Controller nuevo `Controllers/ReclutamientoController.cs` (`api/reclutamiento`) con `GET vacantes/sync?from=yyyy-MM-dd`.
- Registro de `ReclutamientoService` y de `IVacanteRepository` → `VacanteRepository` en `Program.cs`.
- Pruebas `VacanteExtensionsTests.cs` y `ReclutamientoServiceTests.cs` con JSON sintético.
- Verificación del guardado contra la BD del `.env`, llamando el endpoint dos veces y consultando la tabla.

**Fuera de alcance (para specs futuras):**

- Cambios de esquema en `Buk.dbo.Vacante`, como índices adicionales, `NOT NULL` en otras columnas o columnas nuevas. La PK ya la definió el usuario.
- Cambios a `DTOs/VacanteDTO.cs`.
- Pruebas automáticas del SQL contra una BD. El SQL se verifica a mano contra la BD real.
- Aspirantes, postulaciones (`recruiting/applications`) y etapas del proceso (`process_stages`).
- Guardar área, salarios, supervisor, reclutadores, ubicación, descripción o cualquier otro campo fuera de `VacanteDTO`.
- Traducir o normalizar `status`.
- Borrar en BD las vacantes que Buk deje de devolver.
- Programar la sincronización automáticamente (cron, hosted service).
- Mover `from` a `appsettings`.

## Modelo de datos

### `VacanteDTO` — ya existe, no cambia

```csharp
public class VacanteDTO
{
    public required int id_vacante { get; set; }
    public required string nombre { get; set; }
    public required string estatus { get; set; }
    public DateOnly fecha_registro { get; set; }
}
```

### Respuesta de Buk — `ResponseApi/ResponseVacante.cs`

Namespace `apiBukLitoprocess.responseApi`. Solo se declaran los campos que usa el mapper.

```csharp
public class ResponseVacante
{
    [JsonPropertyName("pagination")] public required Pagination pagination { get; set; }
    [JsonPropertyName("data")]       public required List<VacanteRest> Data { get; set; }
}

public class VacanteRest
{
    [JsonPropertyName("id")] [JsonConverter(typeof(FlexibleLongConverter))]
    public long id { get; set; }
    [JsonPropertyName("role")]       public RolVacanteRest? role { get; set; }
    [JsonPropertyName("status")]     public string? status { get; set; }
    [JsonPropertyName("created_at")] public DateTimeOffset? created_at { get; set; }
}

public class RolVacanteRest { [JsonPropertyName("name")] public string? name { get; set; } }
```

No se declaran `supervisor`, `recruiters`, `public_token`, `description` ni `requirements`. Así no se cargan nombres de personas ni tokens innecesarios.

### Mapeo `VacanteRest` → `VacanteDTO` — `mappers/VacanteExtensions.cs`

| `VacanteDTO`     | Origen en Buk | Regla |
| ---------------- | ------------- | ----- |
| `id_vacante`     | `id`          | `checked((int)id)` |
| `nombre`         | `role.name`   | `Trim().ToUpperInvariant()`, conservando acentos y Ñ (convención de la SPEC 01). Sin `role` o `name` → `""` |
| `estatus`        | `status`      | Tal cual (`"cerrado"`, `"iniciado"`, `"cancelado"`). `null` → `""` |
| `fecha_registro` | `created_at`  | Se convierte a la zona `America/Mexico_City` y se toma la fecha. Si es `null`, el registro no se mapea |

Funciones públicas del mapper:

```csharp
public static bool PuedeMapearseAVacante(this VacanteRest vacante); // created_at.HasValue
public static VacanteDTO ToVacanteDTO(this VacanteRest vacante);
```

Ejemplos esperados:

| Buk | `VacanteDTO` |
| --- | ------------ |
| `role.name = "Auxiliar De Nomina"` | `nombre = "AUXILIAR DE NOMINA"` |
| `role.name = "Diseñador Gráfico"` | `nombre = "DISEÑADOR GRÁFICO"` |
| sin `role` | `nombre = ""` |
| `status = "iniciado"` | `estatus = "iniciado"` |
| `created_at = "2026-07-20T13:48:25-06:00"` | `fecha_registro = 2026-07-20` |
| `created_at = "2026-07-21T03:00:00Z"` | `fecha_registro = 2026-07-20` (21:00 en México) |

### Interfaz del repositorio — `repository/interfaces/IVacanteRepository.cs`

Namespace `apiBukLitoprocess.repository.interfaces`.

```csharp
public interface IVacanteRepository
{
    Task GuardarVacantes(List<VacanteDTO> vacantes);
}
```

### Implementación — `repository/implementation/VacanteRepository.cs`

Namespace `apiBukLitoprocess.repository.implementation`. Sigue el patrón de `AusenciaRepository`.

- Constructor: `(DbConnectionFactory dbConnectionFactory, ILogger<VacanteRepository> logger)`.
- Con la lista vacía, regresa de inmediato sin abrir conexión.
- Abre **una** conexión y **una** transacción para toda la lista. Usa un solo `SqlCommand` con parámetros tipados que se reutiliza en cada fila.
- Cada fila ejecuta el SQL con `ExecuteScalarAsync`, que devuelve cuántas filas actualizó el `UPDATE`:
  - `0` → se insertó y suma a `insertadas`.
  - `1` → suma a `actualizadas`. Con la PK no puede haber más de una fila por id.
- Si la lista trae el mismo `id_vacante` dos veces, la segunda ocurrencia actualiza la fila que insertó la primera. No viola la PK. Buk hoy no repite ids.
- Al terminar hace `Commit` y deja un `LogInformation` con el resumen: `"Vacante: {Insertadas} insertadas, {Actualizadas} actualizadas, de {Total} recibidas"`.
- **Todo o nada.** Ante cualquier excepción llama a `RevertirTransaccion(tx)` y relanza con `throw;`. El servicio no atrapa la excepción, así que el controller responde 500. `RevertirTransaccion` es un método privado con el mismo cuerpo que el de `AusenciaRepository`: hace rollback solo si `tx.Connection` no es `null`, y si el rollback falla lo registra con `_logger`.

Mapeo DTO → parámetro → columna:

| Parámetro        | `SqlDbType`     | Valor                                          | Columna          |
| ---------------- | --------------- | ---------------------------------------------- | ---------------- |
| `@IdVacante`     | `Int`           | `id_vacante`                                   | `id_vacante`     |
| `@Puesto`        | `VarChar`, 250  | `nombre`                                       | `puesto`         |
| `@Estado`        | `VarChar`, 50   | `estatus`                                      | `estado`         |
| `@FechaRegistro` | `DateTime`      | `fecha_registro.ToDateTime(TimeOnly.MinValue)` | `fecha_registro` |

`""` se guarda como `''`, no como `NULL`.

```sql
DECLARE @actualizadas int;

UPDATE Buk.dbo.Vacante WITH (UPDLOCK, SERIALIZABLE)
SET puesto = @Puesto,
    estado = @Estado,
    fecha_registro = @FechaRegistro
WHERE id_vacante = @IdVacante;

SET @actualizadas = @@ROWCOUNT;

IF @actualizadas = 0
    INSERT INTO Buk.dbo.Vacante (id_vacante, puesto, estado, fecha_registro)
    VALUES (@IdVacante, @Puesto, @Estado, @FechaRegistro);

SELECT @actualizadas;
```

Contrato de `GuardarVacantes`:

- Si la vacante no existe, se inserta.
- Si ya existe, se actualizan `puesto`, `estado` y `fecha_registro`.
- No borra las filas que no vienen en la lista.

### Servicio — `Services/ReclutamientoService.cs`

```csharp
public class ReclutamientoService
{
    public static readonly DateOnly FechaDesdePorDefecto = new(2026, 1, 1);
    public ReclutamientoService(RestClientService restClient, IVacanteRepository vacanteRepository);
    public Task<List<VacanteDTO>> SincronizarVacantes(DateOnly desde);
}
```

Flujo de `SincronizarVacantes`:

1. Formatear `desde` como `yyyy-MM-dd` con `CultureInfo.InvariantCulture`.
2. Llamar a `ObtenerPaginadoAsync<ResponseVacante, VacanteRest, VacanteRest>` con `ApiClientNames.Buk` y estos parámetros:
   - Página 1: `recruiting/selection_processes?page_size=100&from={desde}`.
   - Página N: `recruiting/selection_processes?page={page}&page_size=100&from={desde}`.
   - Elementos: `r => r.Data`.
   - Total de páginas: `r => r.pagination?.TotalPages ?? 1`.
   - Mapeo: identidad.
3. Separar los registros con `PuedeMapearseAVacante`. Los omitidos se registran con `EventLogger.Info`, indicando cuántos fueron y sus `id`.
4. Mapear los válidos con `ToVacanteDTO`.
5. Si hay al menos una vacante, llamar a `_vacanteRepository.GuardarVacantes(vacantes)`. Con 0 no se llama.
6. Devolver la lista.

El servicio **no** atrapa excepciones. Si Buk responde con un código distinto de 2xx, `EnsureSuccessStatusCode` lanza `HttpRequestException`. Si falla el SQL, la excepción del repositorio sube igual.

### Endpoint — `Controllers/ReclutamientoController.cs`

`[ApiController]` con `[Route("api/reclutamiento")]` y `[HttpGet("vacantes/sync")]`. Recibe `[FromQuery] DateOnly? from` y usa `from ?? ReclutamientoService.FechaDesdePorDefecto`.

- Éxito: `200` con `{ success = true, statusCode = 200, data = List<VacanteDTO> }`.
- `from` con un formato inválido: `400` automático del model binding de `[ApiController]`, sin llamar a Buk.
- Error: `Console.WriteLine(e.GetBaseException().Message)` y `500` con `{ success = false, statusCode = 500, message = "Internal server error" }`, igual que `ColaboradorController.sync`.

## Plan de implementación

1. **Modelo de respuesta.** Crear `ResponseApi/ResponseVacante.cs` con `ResponseVacante`, `VacanteRest` y `RolVacanteRest`. Ejecutar `dotnet build`: debe compilar.
2. **Mapper y sus pruebas.**
   - Crear `mappers/VacanteExtensions.cs` con `PuedeMapearseAVacante` y `ToVacanteDTO`.
   - Crear `tests/apiBukLitoprocess.Tests/VacanteExtensionsTests.cs` con un caso por cada fila de la tabla de ejemplos.
   - Agregar un caso con `created_at = null` (`PuedeMapearseAVacante == false`) y un caso de `id` que se copie a `id_vacante`.
   - Ejecutar `dotnet test`: debe pasar.
3. **Interfaz del repositorio.** Crear `repository/interfaces/IVacanteRepository.cs`. Ejecutar `dotnet build`.
4. **Servicio y sus pruebas.** Crear `Services/ReclutamientoService.cs`. Todavía no se registra en DI. Crear `tests/apiBukLitoprocess.Tests/ReclutamientoServiceTests.cs`, armado igual que `ColaboradorServiceBukTests`: `FakeBukHttpMessageHandler`, `Mock<IHttpClientFactory>` y `Mock<IVacanteRepository>`. Los casos son:
   - JSON con 3 registros y `total_pages = 1`. La petición contiene `recruiting/selection_processes?page_size=100&from=2026-01-01` y `GuardarVacantes` se llama 1 vez con 3 elementos.
   - Con `desde = 2026-09-01`, la URL lleva `from=2026-09-01`.
   - Un registro sin `created_at`. `GuardarVacantes` recibe 2 elementos.
   - `data: []`. `GuardarVacantes` no se llama y se devuelve una lista vacía.
   - `total_pages = 2`. El handler recibe 2 peticiones.
   - Buk responde 500. Se lanza `HttpRequestException` y `GuardarVacantes` no se llama.
   - El repositorio lanza una excepción. `SincronizarVacantes` la propaga.

   Ejecutar `dotnet test`: debe pasar.
5. **Repositorio.** Crear `repository/implementation/VacanteRepository.cs` con el SQL, los parámetros y el manejo de la transacción descritos arriba. Ejecutar `dotnet build`.
6. **Endpoint y registro en DI.**
   - Crear `Controllers/ReclutamientoController.cs`.
   - En `Program.cs`, agregar `builder.Services.AddScoped<ReclutamientoService>();` junto a los demás servicios, y `builder.Services.AddScoped<IVacanteRepository, VacanteRepository>();` junto a los demás repositorios.
   - Ejecutar `dotnet run` y llamar `GET /api/reclutamiento/vacantes/sync` **dos veces**.
   - Consultar `Buk.dbo.Vacante` en modo de solo lectura para verificar los criterios de la BD.

## Criterios de aceptación

**Mapper y servicio**

- [ ] `dotnet build` compila sin errores y `dotnet test` pasa.
- [ ] `role.name = "Auxiliar De Nomina"` produce `nombre == "AUXILIAR DE NOMINA"`, y `"Diseñador Gráfico"` produce `"DISEÑADOR GRÁFICO"`.
- [ ] Un registro sin `role` produce `nombre == ""` y no lanza excepción.
- [ ] `status = "iniciado"` produce `estatus == "iniciado"`, sin traducir ni cambiar mayúsculas.
- [ ] `created_at = "2026-07-20T13:48:25-06:00"` produce `fecha_registro == new DateOnly(2026, 7, 20)`.
- [ ] `created_at = "2026-07-21T03:00:00Z"` produce `fecha_registro == new DateOnly(2026, 7, 20)`.
- [ ] `id_vacante` es igual al `id` de Buk.
- [ ] Un registro sin `created_at` no llega a `GuardarVacantes` y queda en el log con su `id`.
- [ ] `SincronizarVacantes(new DateOnly(2026, 1, 1))` consulta `recruiting/selection_processes?page_size=100&from=2026-01-01` con el cliente `ApiClientNames.Buk`.
- [ ] Con `total_pages = 2`, el servicio hace 2 peticiones a Buk.
- [ ] Con 0 registros, `GuardarVacantes` no se llama.
- [ ] Si Buk responde con un código distinto de 2xx, `GuardarVacantes` no se llama y la excepción se propaga.

**Endpoint**

- [ ] La API arranca en Development sin errores de validación de DI.
- [ ] Contra Buk real, `GET /api/reclutamiento/vacantes/sync` responde 200 con `success = true` y 29 elementos en `data`. Hoy son 29 registros con `from=2026-01-01`, y el número puede crecer.
- [ ] `GET /api/reclutamiento/vacantes/sync?from=2026-13-45` responde 400 y no hace ninguna petición a Buk.
- [ ] Con un token inválido en `BukApiSettings__Token`, el endpoint responde 500 con `success = false`.

**Base de datos (BD del `.env`)**

- [ ] Después de la primera llamada, el log dice `Vacante: 29 insertadas, 0 actualizadas, de 29 recibidas`.
- [ ] Después de la segunda llamada, el log dice `Vacante: 0 insertadas, 29 actualizadas, de 29 recibidas`.
- [ ] `SELECT COUNT(*), COUNT(DISTINCT id_vacante) FROM Buk.dbo.Vacante` devuelve `29 | 29` después de las dos llamadas.
- [ ] Todos los valores de `puesto` están en mayúsculas y ninguno es `NULL`.
- [ ] `estado` solo contiene `cerrado`, `iniciado` o `cancelado`.
- [ ] Todas las `fecha_registro` tienen hora `00:00:00.000`.
- [ ] La fila con `id_vacante = 100` tiene `fecha_registro = 2026-07-20`.

**Higiene**

- [ ] `ResponseVacante.cs` no declara `supervisor`, `recruiters` ni `public_token`.
- [ ] Ningún archivo de prueba contiene datos reales de Buk.
- [ ] `DTOs/VacanteDTO.cs` no se modifica.

## Decisiones

- **Sí:** una vacante es un proceso de selección (`selection_processes`). **No:** usar `recruiting/applications` como en la SPEC 02 de la rama `solicitudes`. Esta spec la reemplaza por decisión del usuario.
- **Sí:** el servicio se llama `ReclutamientoService`, a pedido del usuario. Deja espacio para que aspirantes y postulaciones vivan ahí en specs futuras. **No:** `VacanteService`.
- **Sí:** `nombre` sale de `role.name` en `ToUpperInvariant`. Viene limpio en 29 de 29 registros y es el mismo catálogo de puestos que la SPEC 01. **No:** usar `name`, que mezcla el puesto con las áreas y a veces no coincide con el rol (por ejemplo `"Nominista"` frente a `"Auxiliar De Nomina"`).
- **Sí:** `estatus` se guarda tal cual viene de Buk. **No:** traducirlo ni crear un catálogo.
- **Sí:** `fecha_registro` sale de `created_at` en hora de México, con la misma zona `America/Mexico_City` que usa `AusenciaExtensions`. **No:** usar `start_date`.
- **Sí:** `from` es un query param opcional del endpoint, y por defecto vale `2026-01-01` (`ReclutamientoService.FechaDesdePorDefecto`). **No:** una constante fija sin parámetro. **No:** `appsettings`.
- **Sí:** el endpoint vive en un `ReclutamientoController` nuevo (`api/reclutamiento/vacantes/sync`), que acompaña al nombre del servicio. El usuario pidió que hubiera endpoint. La ruta concreta es la opción recomendada en la fase de preguntas.
- **Sí:** `from` llega como `DateOnly?` para que el formato inválido lo rechace ASP.NET con 400 antes de llegar a Buk, que también respondería 400 pero terminaría como 500.
- **Sí:** `page_size=100`, como pidió el usuario. Buk aplica un mínimo de 25 en la práctica, lo cual no afecta.
- **Sí:** `VacanteRepository` se implementa en esta misma spec, a pedido del usuario. La tabla `Buk.dbo.Vacante` ya existe. **No:** dejar el guardado para otra spec.
- **Sí:** `nombre` se guarda en `puesto` y `estatus` en `estado`, confirmado por el usuario. **No:** renombrar columnas ni propiedades.
- **Sí:** un `UPDATE` y, si no actualizó filas, un `INSERT`, con `UPDLOCK, SERIALIZABLE`. Con la PK clustered, el bloqueo es solo sobre la llave `id_vacante` y no sobre toda la tabla. **No:** `MERGE`. **No:** borrar y reinsertar por rango de fecha.
- **Sí:** todo o nada. Una falla de SQL revierte la transacción y el endpoint responde 500. **No:** registrar la fila que falló y continuar como `AusenciaRepository`, porque el endpoint respondería 200 con un guardado incompleto.
- **Sí:** los parámetros de texto tienen el tamaño de su columna (250 y 50), sin un helper de recorte. Hoy los valores máximos son 30 y 9 caracteres. **No:** crear `RecortadorTexto` en esta spec.
- **Sí:** la PK `PK_Vacante` sobre `id_vacante` la definió el usuario directamente en la BD, fuera del código. La spec no incluye un script de esquema. Si algo intenta insertar un id repetido, SQL Server lanza el error 2627, se revierte la transacción y el endpoint responde 500. **No:** atrapar el 2627 para ignorarlo, porque con el upsert solo puede ocurrir por un error de lógica.
- **Sí:** verificar el guardado contra la BD del `.env`. El usuario autorizó escribir en ella. **No:** pruebas automáticas del SQL.
- **Sí:** upsert por `id_vacante`, porque el `estatus` cambia con el tiempo (iniciado → cerrado).
- **Sí:** los ids se deserializan como `long` con `FlexibleLongConverter` y se convierten a `int` con `checked`. **No:** cambiar `VacanteDTO` a `long`.
- **Sí:** los registros sin `created_at` se omiten y se registran en el log, porque `fecha_registro` no admite vacío. Hoy los 29 traen fecha.
- **Sí:** el servicio no atrapa errores de Buk ni de SQL, y el controller responde 500. **No:** devolver una lista vacía, porque una falla se vería igual que "no hay vacantes".
- **Sí:** reutilizar `ApiClientNames.Buk`, `RestClientService.ObtenerPaginadoAsync` y `DbConnectionFactory`, sin `HttpClient` ni configuración nuevos.

## Riesgos

| Riesgo | Mitigación |
| ------ | ---------- |
| `from` filtra en UTC y `fecha_registro` está en hora de México. Una vacante creada después de las 18:00 del día anterior entra con `fecha_registro` un día antes de `from` | Se acepta. Es una diferencia de bordes que no pierde registros. |
| `BukApiSettings__Url_API` no termina en `/`, así que la ruta relativa resuelve a `/api/v1/recruiting/…` sin `/mexico` | Se verificó el 2026-11-27 que responde 200 con los mismos 29 registros. No se corrige en esta spec. |
| Un id de Buk mayor que `int.MaxValue` | `checked` lanza `OverflowException` y el endpoint responde 500, en vez de guardar un id corrupto. Hoy el máximo es 931. |
| Buk cambia los valores de `status` | Se guardan tal cual y no se rompe nada, siempre que midan 50 caracteres o menos. |
| Un `role.name` mayor a 250 caracteres o un `status` mayor a 50 | SqlClient recorta el valor al tamaño del parámetro sin avisar. Hoy los máximos son 30 y 9. Si llega a pasar, se agrega un recorte con log en otra spec. |
| Dos llamadas al sync al mismo tiempo | `UPDLOCK, SERIALIZABLE` bloquea la llave `id_vacante` entre el `UPDATE` y el `INSERT`, así que ninguna de las dos choca con la PK. En el peor caso, una llamada espera a la otra. Si aun así hubiera una violación, la PK la rechaza con un 500 y no se guarda un duplicado. |
| La PK se elimina o se cambia en la BD | El upsert sigue evitando duplicados por sí solo. La verificación de esta spec (`COUNT(*) = COUNT(DISTINCT id_vacante)`) lo detectaría. |
| La verificación escribe 29 filas reales en `Buk.dbo.Vacante` de la BD del `.env` | El usuario lo autorizó. Es el mismo dato que guardará el sync en uso normal. |
| La respuesta trae datos personales (supervisor, reclutadores) y un `public_token` | No se deserializan, y las pruebas usan JSON sintético. |
| El upsert no borra las vacantes que Buk deja de devolver, por ejemplo al subir `from` | Se acepta. El borrado va en otra spec. |

## Lo que **no** está en esta spec

- Cambios de esquema en `Buk.dbo.Vacante`, como índices adicionales o columnas nuevas. La PK ya existe.
- Cambios a `VacanteDTO`.
- Pruebas automáticas del SQL.
- Aspirantes, postulaciones y etapas del proceso.
- Área, salarios, supervisor, reclutadores u otros campos de la vacante.
- La traducción o normalización de `status`.
- El borrado de vacantes que ya no vienen de Buk.
- La programación automática del sync.
- `from` en `appsettings`.

Cada uno de esos puntos, si llega a hacerse, irá en su propia spec.
