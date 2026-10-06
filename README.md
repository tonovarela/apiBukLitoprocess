# apiBukLitoprocess

Este proyecto es una API en ASP.NET Core (.NET 8) que integra los colaboradores de **Buk** con Intelisis. Recibe webhooks de Buk, sincroniza colaboradores y consulta ausencias, permisos, incapacidades y vacaciones.

## Características
- Recibe eventos de Buk (`employee_update`, `job_hire`, `job_termination`, `job_movement`) mediante un endpoint webhook.
- Deserializa el colaborador de Buk y lo mapea a `ColaboradorDTO` para persistirlo.
- Sincronización masiva de colaboradores y consulta de ausencias/permisos/incapacidades/vacaciones.

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

## Pruebas

El proyecto de pruebas (xUnit + Moq) está en `tests/apiBukLitoprocess.Tests` y valida `GetColaboradorByIdBuk` mockeando el webservice de Buk a nivel HTTP (`FakeBukHttpMessageHandler`), sin salir a la red.

```bash
dotnet test
```

## Estructura del proyecto
- `controllers/` — endpoints de la API.
- `Services/` — lógica de negocio (`ColaboradorService`, `AsistenciaService`, `RestClientService`).
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
docker build --platform linux/amd64 -t tonovarela/apibuklitoprocess:7.0.4 -t tonovarela/apibuklitoprocess:latest . \
  && docker push tonovarela/apibuklitoprocess:7.0.4 \
  && docker push tonovarela/apibuklitoprocess:latest
```

## Licencia
Este proyecto es de uso educativo y puede ser modificado según tus necesidades.


