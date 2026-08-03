using System.Net;
using System.Text.Json;
using apiBukLitoprocess.Clases;
using apiBukLitoprocess.conf;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.mappers;
using apiBukLitoprocess.repository.interfaces;
using apiBukLitoprocess.responseApi;

namespace apiBukLitoprocess.Services;

public record GetColaboradorResult(bool IsError, int StatusCode, string? ErrorMessage, ColaboradorDTO? colaborador)
{
    public static GetColaboradorResult Ok(ColaboradorDTO colaborador) => new(false, 200, null, colaborador);
    public static GetColaboradorResult Fail(string message, int codeError) => new(true, codeError, message, null);
}

public class ColaboradorService
{
    private readonly RestClientService _restClient;
    private readonly IColaboradorRepository _colaboradorRepository;
    private readonly IConfiguration? _configuration;

    /// <summary>
    /// Cuando es true, GetColaboradorByIdBuk devuelve la respuesta emulada
    /// (ver <see cref="BukEmulador"/>) en lugar de llamar al webservice real de Buk.
    /// Se controla con la clave de configuración "BukApiSettings:Emular".
    /// </summary>
    private bool EmulacionBukActiva => _configuration?.GetValue<bool>("BukApiSettings:Emular") ?? false;

    private readonly HashSet<string> EventosValidos = new(StringComparer.OrdinalIgnoreCase){
        "employee_update",
        "job_hire",
        "job_termination",
        "job_movement"
        };
    public ColaboradorService(RestClientService restClient, IColaboradorRepository colaboradorRepository, IConfiguration? configuration = null)
    {
        _restClient = restClient;
        _colaboradorRepository = colaboradorRepository;
        _configuration = configuration;
    }


    public async Task<GetColaboradorResult> handleEventWebhook(WebhookPayloadBody bodyPayload)
    {
        string eventType = bodyPayload.EventType;
        int idEmployeeBuk = bodyPayload.EmployeeId;

        EventLogger.Info("webhook_event", bodyPayload);

        if (string.IsNullOrWhiteSpace(eventType))
        {
            await RegistrarBitacoraAsync(BitacoraDTO.NoSoportado(idEmployeeBuk, eventType));
            return GetColaboradorResult.Fail("Evento inválido", 400);
        }

        if (!EventosValidos.Contains(eventType))
        {
            await RegistrarBitacoraAsync(BitacoraDTO.NoSoportado(idEmployeeBuk, eventType));
            return GetColaboradorResult.Fail("Evento no soportado", 400);
        }

        GetColaboradorResult result = await GetColaboradorByIdBuk(idEmployeeBuk);
        if (result.IsError || result.colaborador is null)
        {
            await RegistrarBitacoraAsync(BitacoraDTO.Error(idEmployeeBuk, eventType, $"No se pudo obtener el colaborador de Buk: {result.ErrorMessage}"));
            return result;
        }
        ColaboradorDTO colaborador = result.colaborador;
        await AsignarJefeAsync(colaborador);
        try
        {
            switch (eventType)
            {
                case "employee_update" or "job_movement":
                    await _colaboradorRepository.Actualizar(colaborador);
                    break;

                case "job_hire":                    
                    await ProcesarJobHireAsync(idEmployeeBuk, colaborador);
                    break;

                case "job_termination":

                    await _colaboradorRepository.RegistrarBaja(idEmployeeBuk.ToString(), colaborador.ConceptoBaja!, colaborador.FechaBaja!);
                    break;
            }
            await RegistrarBitacoraAsync(BitacoraDTO.Exito(idEmployeeBuk, eventType));
            return GetColaboradorResult.Ok(colaborador);
        }
        catch (InvalidOperationException ex)
        {
            await RegistrarBitacoraAsync(BitacoraDTO.Omitido(idEmployeeBuk, eventType, $"{ex.Message}"));
            return GetColaboradorResult.Fail(ex.Message, 409);
        }
        catch (Exception ex)
        {
            await RegistrarBitacoraAsync(BitacoraDTO.Error(idEmployeeBuk, eventType, $"Error al procesar evento: {ex.GetBaseException().Message}"));
            return GetColaboradorResult.Fail($"Error al procesar evento {eventType}: {ex.Message}", 500);
        }
    }

    public async Task<List<ColaboradorDTO>> Sincronizar()
    {
        var colaboradores = new List<ColaboradorDTO>();
        var firstPageResponse = await _restClient.GetAsync<ResponseListColaborador>(ApiClientNames.Buk, "employees");
        if (firstPageResponse?.data == null)
        {
            return colaboradores;
        }
        colaboradores.AddRange(firstPageResponse.data.Select(colaborador => colaborador.ToColaboradorDTO()));
        int totalPages = firstPageResponse.pagination?.TotalPages ?? 1;
        if (totalPages <= 1)
        {

            return colaboradores;
        }
        var pageTasks = Enumerable.Range(2, totalPages - 1).Select(page => _restClient.GetAsync<ResponseListColaborador>(ApiClientNames.Buk, $"employees?page={page}"));
        var pageResponses = await Task.WhenAll(pageTasks);
        foreach (var pageResponse in pageResponses)
        {
            if (pageResponse?.data == null)
            {
                continue;
            }
            colaboradores.AddRange(pageResponse.data.Select(colaborador => colaborador.ToColaboradorDTO()));
        }
        foreach (var colaborador in colaboradores)
        {
            await _colaboradorRepository.Actualizar(colaborador.id!.Value, colaborador.IdColaborador);
        }

        return colaboradores;
    }


    public async Task<List<SolicitudDTO>> ObtenerSolicitudesVacaciones()
    {
        DateOnly fechaConsulta = DateOnly.FromDateTime(DateTime.Now.AddDays(-50));
        var solicitudes = new List<SolicitudDTO>();
        Console.WriteLine($"Vacaciones obtenidas: Antes");
        var vacaciones = await _restClient.ObtenerPaginadoAsync<ResponseVacaciones, VacacionesRest, VacacionesRest>(
            ApiClientNames.Buk,
            page => page == 1
                ? $"vacations/requested?date={fechaConsulta:yyyy-MM-dd}&page_size=100&status=approved"
                : $"vacations/requested?date={fechaConsulta:yyyy-MM-dd}&page_size=100&status=approved&page={page}",
            response => response.Data,
            response => response.pagination?.TotalPages ?? 1,
            vacaciones => vacaciones
            );
        Console.WriteLine($"Vacaciones obtenidas: {vacaciones.Count}");

        solicitudes = vacaciones.Where(vacaciones => vacaciones.PuedeMapearseASolicitud())
                                .Select(vacaciones => vacaciones.toSolicitudDTO())
                                .ToList();

        await AsignarIDSIntelisis(solicitudes);
        await _colaboradorRepository.RegistrarSolicitudesVacaciones(solicitudes);
        return solicitudes;
    }


    public async Task<List<AusenciaDTO>> ObtenerIncapacidades(int diasAtras)
    {
        DateOnly fechaConsulta = DateOnly.FromDateTime(DateTime.Now.AddDays(diasAtras));
        Console.WriteLine($"Fecha consulta incapacidades: {fechaConsulta}");
        DateOnly fechaFinConsulta = DateOnly.FromDateTime(DateTime.Now);
        var incapacidades = new List<AusenciaDTO>();
        var _incapacidades = await _restClient.ObtenerPaginadoAsync<ResponseIncapacidad, IncapacidadRest, IncapacidadRest>(
             ApiClientNames.Buk,
             page => page == 1
                 ? $"absences/licence?from={fechaConsulta:yyyy-MM-dd}&to={fechaFinConsulta:yyyy-MM-dd}&page_size=100"
                 : $"absences/licence?from={fechaConsulta:yyyy-MM-dd}&to={fechaFinConsulta:yyyy-MM-dd}&page_size=100&page={page}",
             response => response.Data,
             response => response.Pagination?.TotalPages ?? 1,
             ausencia => ausencia
             );
        incapacidades = _incapacidades
        .Where(p => p.Estado == "approved")
                             .Select(p => p.toAusenciaDTO())
                             .ToList();

        await AsignarIDSIntelisis(incapacidades);
        await _colaboradorRepository.RegistrarAusencias(incapacidades, "Incapacidad");
        return incapacidades;

    }

    public async Task<List<AusenciaDTO>> ObtenerPermisos(int diasAtras)
    {
        DateOnly fechaConsulta = DateOnly.FromDateTime(DateTime.Now.AddDays(diasAtras));
        Console.WriteLine($"Fecha consulta permisos: {fechaConsulta}");
        DateOnly fechaFinConsulta = DateOnly.FromDateTime(DateTime.Now);
        var permisos = new List<AusenciaDTO>();
        var _permisos = await _restClient.ObtenerPaginadoAsync<ResponsePermiso, PermisoRest, PermisoRest>(
             ApiClientNames.Buk,
             page => page == 1
                 ? $"absences/permission?from={fechaConsulta:yyyy-MM-dd}&to={fechaFinConsulta:yyyy-MM-dd}&page_size=100"
                 : $"absences/permission?from={fechaConsulta:yyyy-MM-dd}&to={fechaFinConsulta:yyyy-MM-dd}&page_size=100&page={page}",
             response => response.Data,
             response => response.Pagination?.TotalPages ?? 1,
             ausencia => ausencia
             );
        permisos = _permisos
                   .Select(p => p.toAusenciaDTO())
                   .ToList();


        await AsignarIDSIntelisis(permisos);

        var permisosAprobados = permisos.Where(p => p.estado == "approved").ToList();
        var permisosPendientes = permisos.Where(p => p.estado != "approved").ToList();

        try
        {
            await _colaboradorRepository.RegistrarAusencias(permisosAprobados, "Permiso");
            await _colaboradorRepository.BorrarAusenciasPendientes();
            await _colaboradorRepository.RegistrarPermisosPendientes(permisosPendientes, "Permiso");
        }
        catch (Exception ex)
        {
            EventLogger.Error("Error al registrar permisos", ex, new { permisos, error = ex.GetBaseException().Message });
        }


        return permisos;

    }


    public async Task<List<AusenciaDTO>> ObtenerAusencias(int diasAtras)
    {

        DateOnly fechaConsulta = DateOnly.FromDateTime(DateTime.Now.AddDays(diasAtras));
        Console.WriteLine($"Fecha consulta ausencias: {fechaConsulta}");
        DateOnly fechaFinConsulta = DateOnly.FromDateTime(DateTime.Now);
        var ausencias = new List<AusenciaDTO>();
        var _ausencias = await _restClient.ObtenerPaginadoAsync<ResponseAusencia, AusenciaRest, AusenciaRest>(
             ApiClientNames.Buk,
             page => page == 1
                 ? $"absences/absence?from={fechaConsulta:yyyy-MM-dd}&to={fechaFinConsulta:yyyy-MM-dd}&page_size=100"
                 : $"absences/absence?from={fechaConsulta:yyyy-MM-dd}&to={fechaFinConsulta:yyyy-MM-dd}&page_size=100&page={page}",
             response => response.Data,
             response => response.Pagination?.TotalPages ?? 1,
             ausencia => ausencia
             );
        ausencias = _ausencias
        .Where(p => p.Estado == "approved")
                             .Select(p => p.toAusenciaDTO())
                             .ToList();

        await AsignarIDSIntelisis(ausencias);
        await _colaboradorRepository.RegistrarAusencias(ausencias, "Ausencia");
        return ausencias;
    }


    #region Métodos privados
    private async Task AsignarIDSIntelisis(List<SolicitudDTO> solicitudes)
    {
        var distinctIds = solicitudes.Select(s => s.id_colaborador).Distinct().ToList();
        var cache = new Dictionary<long, string>();

        var tasks = distinctIds.Select(async id =>
        {
            var response = await GetColaboradorByIdBuk(id);
            var personalId = response?.colaborador?.IdColaborador ?? String.Empty;
            return new { id, personalId };
        });

        var results = await Task.WhenAll(tasks);
        foreach (var r in results)
        {
            cache[r.id] = r.personalId;
        }

        foreach (var solicitud in solicitudes)
        {
            solicitud.personal = cache.GetValueOrDefault(solicitud.id_colaborador, String.Empty);
        }
    }

    private async Task AsignarIDSIntelisis(List<AusenciaDTO> solicitudes)
    {
        var distinctIds = solicitudes.Select(s => s.id_colaborador).Distinct().ToList();
        var cache = new Dictionary<long, string>();

        var tasks = distinctIds.Select(async id =>
        {
            var response = await GetColaboradorByIdBuk(id);
            var personalId = response?.colaborador?.IdColaborador ?? String.Empty;
            return new { id, personalId };
        });

        var results = await Task.WhenAll(tasks);
        foreach (var r in results)
        {
            cache[r.id] = r.personalId;
        }

        foreach (var solicitud in solicitudes)
        {
            solicitud.personal = cache.GetValueOrDefault(solicitud.id_colaborador, String.Empty);
        }
    }

    private async Task ProcesarJobHireAsync(long idEmployeeBuk, ColaboradorDTO colaborador)
    {
        var colaboradorDB = await _colaboradorRepository.ExisteColaborador(idEmployeeBuk.ToString());
        if (colaboradorDB)
            throw new InvalidOperationException("Colaborador ya existe en la base de datos");

        await RegistrarSiNoExisteAsync(colaborador);

        await _restClient.PatchAsync(ApiClientNames.Buk, $"employees/{idEmployeeBuk}", new
        {
            custom_attributes = new { idColaborador = colaborador.IdColaborador }
        });
    }

    internal async Task<GetColaboradorResult> GetColaboradorByIdBuk(long idEmployeeBuk, Boolean forceAPICALL = false)
    {
        try
        {
            //Console.WriteLine($"Obteniendo colaborador de Buk para Employee ID: {ApiClientNames.Buk}");
            ResponseColaborador? response;
            if (EmulacionBukActiva && !forceAPICALL)
            {
                // Modo emulación: no se consume la API real, se usa el JSON embebido.
                Console.WriteLine($"[EMULACIÓN Buk] Devolviendo colaborador emulado para Employee ID: {idEmployeeBuk}");
                response = BukEmulador.ObtenerResponse();
            }
            else
            {
                response = await _restClient.GetAsync<ResponseColaborador>(ApiClientNames.Buk, $"employees/{idEmployeeBuk}");
            }
            if (response?.data == null)
            {
                return GetColaboradorResult.Fail("Colaborador no encontrado", 404);
            }

            ColaboradorDTO colaborador = response.data.ToColaboradorDTO();
            return GetColaboradorResult.Ok(colaborador);
        }
        catch (JsonException ex)
        {
            return GetColaboradorResult.Fail($"Error al deserializar JSON: {ex.Message}", 500);
        }
        catch (HttpRequestException httpEx)
        {
            var statusCode = httpEx.StatusCode.HasValue ? (int)httpEx.StatusCode.Value : (int)HttpStatusCode.InternalServerError;
            return GetColaboradorResult.Fail(httpEx.Message, statusCode);
        }
        catch (Exception e)
        {
            return GetColaboradorResult.Fail(e.GetBaseException().Message, 500);
        }
    }

    private async Task AsignarJefeAsync(ColaboradorDTO colaborador)
    {
        if (!colaborador.BossId.HasValue || colaborador.BossId.Value <= 0)
        {
            return;
        }

        var resultBoss = await GetColaboradorByIdBuk(colaborador.BossId.Value, forceAPICALL: true);
        if (!resultBoss.IsError && resultBoss.colaborador is not null)
        {
            colaborador.ReportaA = resultBoss.colaborador.IdColaborador;
        }
    }

    private async Task RegistrarSiNoExisteAsync(ColaboradorDTO colaborador)
    {

        var nuevoCodigoPersonal = await _colaboradorRepository.ObtenerSiguienteClavePersonal();
        colaborador.IdColaborador = nuevoCodigoPersonal.ToString();
        await _colaboradorRepository.Insertar(colaborador, nuevoCodigoPersonal);
    }

    private async Task RegistrarBitacoraAsync(BitacoraDTO bitacora)
    {
        try
        {
            await _colaboradorRepository.InsertarBitacora(bitacora);
        }
        catch (Exception ex)
        {
            EventLogger.Error("Error al registrar bitácora", ex, new { bitacora, error = ex.GetBaseException().Message });
        }
    }
    
    #endregion

}

