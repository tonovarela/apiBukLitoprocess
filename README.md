# apiBukLitoprocess

Este proyecto es una API en ASP.NET Core (.NET 8) que integra los colaboradores de **Buk** con Intelisis. Recibe webhooks de Buk, sincroniza colaboradores y consulta ausencias, permisos, incapacidades y vacaciones.

## Características
- Recibe eventos de Buk (`employee_update`, `job_hire`, `job_termination`, `job_movement`) mediante un endpoint webhook.
- Deserializa el colaborador de Buk y lo mapea a `ColaboradorDTO` para persistirlo.
- Sincronización masiva de colaboradores y consulta de ausencias/permisos/incapacidades/vacaciones.
- **Modo emulación de Buk** para desarrollo local sin consumir la API real.

## Uso

1. Instala .NET 8.0 si no lo tienes.
2. Ejecuta el proyecto:
   ```bash
   dotnet watch run
   ```
3. El endpoint principal es:
   - POST `/api/colaborador/webhook`
   - Recibe un JSON con la estructura:
     ```json
     {
       "data": {
         "event_type": "employee_update",
         "date": "2026-02-26T18:34:25-06:00",
         "tenant_url": "litoprocess.buk.mx",
         "employee_id": 3256,
         "employment_status": "activo"
       }
     }
     ```

## Configuración

La configuración vive en `appsettings.json` / `appsettings.Development.json`:

- `BukApiSettings` — URL, token y ajustes del webservice de Buk.
- `AsistenciaApiSettings` — API de control de asistencia.
- `ConnectionStrings:DefaultConnection` — base de datos SQL Server.

## Modo emulación de Buk

Permite que `ColaboradorService.GetColaboradorByIdBuk` devuelva un colaborador de ejemplo (JSON embebido) **sin llamar al webservice real de Buk**, útil para desarrollo y pruebas locales.

- Se activa con la clave `BukApiSettings:Emular`:
  ```json
  "BukApiSettings": {
    "Url_API": "https://litoprocess.buk.mx/api/v1/mexico",
    "Token": "...",
    "Environment": "Development",
    "Emular": true
  }
  ```
- `true` → devuelve la respuesta emulada (ver `Services/BukEmulador.cs`, JSON en `Services/colaborador_emulado.json`).
- `false` o ausente → consume la API real de Buk (comportamiento por defecto en producción).
- Al estar activo se registra en consola: `[EMULACIÓN Buk] Devolviendo colaborador emulado...`.

> Nota: el emulador devuelve siempre el mismo colaborador sin importar el `id`, por lo que la asignación de jefe también usará ese mismo registro.

## Pruebas

El proyecto de pruebas (xUnit + Moq) está en `tests/apiBukLitoprocess.Tests` y valida `GetColaboradorByIdBuk` mockeando el webservice de Buk a nivel HTTP (`FakeBukHttpMessageHandler`), sin salir a la red.

```bash
dotnet test
```

## Estructura del proyecto
- `controllers/` — endpoints de la API.
- `Services/` — lógica de negocio (`ColaboradorService`, `AsistenciaService`, `RestClientService`, `BukEmulador`).
- `DTOs/` — objetos de transferencia.
- `mappers/` — mapeo de respuestas de Buk a DTOs.
- `responseApi/` — modelos de las respuestas de Buk.
- `repository/` — acceso a datos (interfaces e implementación).
- `tests/` — proyecto de pruebas.

## Recomendaciones
- Configura variables de entorno y archivos de configuración según tu entorno.
- Usa ngrok para exponer localmente el endpoint si necesitas pruebas externas:
  ```bash
  ngrok http 80
  ```

## Docker
```bash
docker build --platform linux/amd64 -t tonovarela/apibuklitoprocess:7.0.1 -t tonovarela/apibuklitoprocess:latest . \
  && docker push tonovarela/apibuklitoprocess:7.0.1 \
  && docker push tonovarela/apibuklitoprocess:latest
```

## Licencia
Este proyecto es de uso educativo y puede ser modificado según tus necesidades.
