# SPEC 01 — Puesto y Departamento en mayúsculas al sincronizar de Buk a Intelisis

> **Estado:** Aprobado
> **Depende de:** Ninguna
> **Fecha:** 2026-11-05
> **Objetivo:** Que los valores de Puesto y Departamento que la API escribe en Intelisis, tanto en el alta como en el movimiento de cambio, queden siempre en mayúsculas.

## Por qué existe esta spec

Hoy el comportamiento no es uniforme:

- **Departamento** se obtiene de `TablaStD` en `ObtenerDepartamento` y se pasa a mayúsculas con `.ToUpper()` en dos lugares distintos (`ColaboradorRepository.cs:190` en `Actualizar` y `:508` en `Insertar`).
- **Puesto en el alta** (`Insertar`) se pasa a mayúsculas con `colaborador.Puesto!.ToUpper()` (`:504`). Si `Puesto` llega nulo, lanza `NullReferenceException`, porque el `?? DBNull.Value` nunca llega a evaluarse.
- **Puesto en la actualización** no se escribe en `Personal.Puesto`. El cambio se registra como movimiento `RHD` desde `AccionCambioPuesto` → `HistorialRepository.InsertarDetalle`, y ahí `@valorNuevo` se guarda **tal cual viene de Buk** (por ejemplo `"Gerente"`). Este es el caso que no cumple el requerimiento.

## Alcance

**Incluye:**

- Convertir `Puesto` a mayúsculas en el mapper `mappers/ColaboradorExtensions.cs`, al construir el `ColaboradorDTO`.
- Centralizar la conversión de `Departamento` a mayúsculas dentro de `ObtenerDepartamento` en `repository/implementation/ColaboradorRepository.cs`.
- Quitar los `.ToUpper()` redundantes de `Insertar` y `Actualizar`, que quedan cubiertos por los dos puntos anteriores.
- Quitar el operador `!` de `@Puesto` en `Insertar` para que un Puesto nulo no lance excepción.
- Usar `ToUpperInvariant()` y conservar acentos y Ñ.
- Ajustar y agregar las pruebas unitarias afectadas.

**Fuera de alcance (para specs futuras):**

- Corregir registros que ya existen en `Personal` o `RHD` con Puesto o Departamento en minúsculas, ya sea con un script SQL de corrección o un backfill.
- Escribir `Personal.Puesto` directamente en `Actualizar`. El flujo sigue siendo el movimiento RHD "Cambio de puesto" que afecta Intelisis.
- Quitar acentos o normalizar el texto más allá de las mayúsculas.
- Pasar a mayúsculas otros campos, como `CentroCostos`, `ReportaA` o `Jornada`.
- Cambiar los `.ToUpper()` existentes de `Nombre`, `ApellidoPaterno` y `ApellidoMaterno` a `ToUpperInvariant()`.

## Modelo de datos

Esta feature no introduce estructuras de datos nuevas. Reutiliza `ColaboradorDTO.Puesto` (`string?`) y las columnas existentes `Personal.Puesto`, `Personal.Departamento`, `RHD.Puesto` y `RHD.Departamento`.

El único cambio es la regla sobre el valor que se escribe:

```csharp
// mappers/ColaboradorExtensions.cs
Puesto = colaborador.current_job?.role?.name?.ToUpperInvariant() ?? String.Empty,
```

```csharp
// ColaboradorRepository.ObtenerDepartamento — valor devuelto
return (reader["Departamento"].ToString() ?? String.Empty).ToUpperInvariant();
```

Ejemplos esperados:

| Origen (Buk / TablaStD) | Valor escrito en Intelisis |
| ----------------------- | -------------------------- |
| `Analista`              | `ANALISTA`                 |
| `Diseñador Gráfico`     | `DISEÑADOR GRÁFICO`        |
| `null` (Puesto)         | `""`                       |
| sin fila en `TablaStD`  | `""`                       |

## Plan de implementación

1. **Puesto en el mapper.** En `mappers/ColaboradorExtensions.cs:93`, cambiar a `colaborador.current_job?.role?.name?.ToUpperInvariant() ?? String.Empty`. Actualizar `tests/apiBukLitoprocess.Tests/ColaboradorServiceBukTests.cs:99` para que espere `"ANALISTA"`. Ejecutar `dotnet test`: debe pasar.
2. **Prueba de acentos.** Agregar en `ColaboradorServiceBukTests` un caso cuyo `role.name` sea `"Diseñador Gráfico"` y que verifique `colaborador.Puesto == "DISEÑADOR GRÁFICO"`. Agregar otro caso sin `role` que verifique `colaborador.Puesto == ""`. Ejecutar `dotnet test`.
3. **Limpiar `Insertar`.** En `ColaboradorRepository.cs:504`, reemplazar `colaborador.Puesto!.ToUpper() ?? (object)DBNull.Value` por `colaborador.Puesto ?? (object)DBNull.Value`. El valor ya llega en mayúsculas desde el mapper.
4. **Departamento centralizado.** En `ObtenerDepartamento`, devolver el valor con `.ToUpperInvariant()`. En `Actualizar` (`:190`) e `Insertar` (`:508`), reemplazar `departamento.ToUpper() ?? (object)DBNull.Value` por `departamento`.
5. **Verificar el movimiento RHD.** No se modifica `HistorialRepository.InsertarDetalle`. `cambio.Nuevo` sale de `nuevo.Puesto` en `ColaboradorComparador`, así que ya llega en mayúsculas. Agregar en `ColaboradorComparadorTests` un caso en el que el DTO venga con `Puesto = "GERENTE"` y el actual con `"Analista"`, y que verifique que `CambioColaborador.Nuevo == "GERENTE"`.

## Criterios de aceptación

- [ ] `dotnet test` pasa sin errores.
- [ ] Un colaborador de Buk con `role.name = "Analista"` produce un `ColaboradorDTO.Puesto == "ANALISTA"`.
- [ ] Un colaborador de Buk con `role.name = "Diseñador Gráfico"` produce un `ColaboradorDTO.Puesto == "DISEÑADOR GRÁFICO"`.
- [ ] Un colaborador de Buk sin `current_job.role` produce un `ColaboradorDTO.Puesto == ""` y `Insertar` no lanza `NullReferenceException`.
- [ ] Un alta (`job_hire`) deja `Personal.Puesto` y `Personal.Departamento` en mayúsculas.
- [ ] Un cambio de puesto (`employee_update` / `job_movement`) inserta en `RHD.Puesto` el valor en mayúsculas. Se verifica en el log `[SQL]` de `InsertarDetalle` (`valorNuevo`).
- [ ] Una actualización deja `Personal.Departamento` en mayúsculas.
- [ ] No queda ningún `.ToUpper()` sobre `Puesto` o `departamento` en `ColaboradorRepository.cs`.
- [ ] Un cambio que solo difiere en mayúsculas y minúsculas (`"Analista"` en BD contra `"ANALISTA"` de Buk) **no** genera un movimiento RHD. Esto ya lo cubre `CompararTexto` con `OrdinalIgnoreCase`.

## Decisiones

- **Sí:** convertir Puesto en el mapper. Es un solo punto que alimenta tanto el alta (`Personal`) como el movimiento `RHD`, y sigue la convención existente para `Nombre` y los apellidos.
- **No:** aplicar `ToUpper` en cada punto de escritura (`Insertar` + `InsertarDetalle`). Duplica la regla, y si algún día se agrega otro punto de escritura, es fácil olvidarlo.
- **Sí:** centralizar Departamento en `ObtenerDepartamento`. El valor no viene de Buk sino de `TablaStD`, así que el mapper no lo toca.
- **Sí:** `ToUpperInvariant()` conservando acentos y Ñ. El resultado no depende de la cultura del contenedor Docker, y el texto conserva su contenido.
- **No:** quitar acentos. Cambiaría el contenido del texto, y la Ñ se perdería.
- **No:** corregir los registros históricos. Solo se corrige lo que se sincronice a partir de ahora. Si hace falta corregir lo existente, irá en otra spec con un script SQL.
- **No:** escribir `Personal.Puesto` en `Actualizar`. Se respeta el flujo actual, en el que Intelisis aplica el puesto al afectar el movimiento RHD.
- **No:** cambiar `ColaboradorComparador`. Ya compara sin distinguir mayúsculas, así que el cambio no genera movimientos falsos.

## Riesgos

| Riesgo | Mitigación |
| ------ | ---------- |
| Colaboradores existentes con Puesto en minúsculas en `Personal` | No se genera un movimiento falso, porque la comparación ignora mayúsculas. Se corregirán solos con su próximo cambio de puesto o con un script en otra spec. |
| Algún otro consumidor del DTO espera el Puesto con su capitalización original (logs, bitácora) | El único uso de `Puesto` fuera de la escritura a BD es la comparación, que ignora mayúsculas. Los logs mostrarán el valor en mayúsculas, que es aceptable. |
| Algún valor de `CCDepto` en `TablaStD` está en minúsculas a propósito | Ya hoy se escribe en mayúsculas. El comportamiento no cambia, solo se mueve de lugar. |

## Lo que **no** está en esta spec

- Script de corrección para registros existentes en `Personal` / `RHD`.
- Escritura directa de `Personal.Puesto` en `Actualizar`.
- Eliminación de acentos o cualquier otra normalización de texto.
- Mayúsculas en otros campos (`CentroCostos`, `ReportaA`, `Jornada`).
- Migrar `Nombre` y los apellidos a `ToUpperInvariant()`.

Cada uno de esos puntos, si llega a hacerse, irá en su propia spec.
