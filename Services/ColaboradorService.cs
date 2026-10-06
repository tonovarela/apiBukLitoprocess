using System.Net;
using System.Text.Json;
using apiBukLitoprocess.Clases;
using apiBukLitoprocess.conf;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.helpers;
using apiBukLitoprocess.mappers;
using apiBukLitoprocess.Models;
using apiBukLitoprocess.Services.Acciones;
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
    private readonly IAusenciaRepository _ausenciaRepository;
    private readonly IEnumerable<IAccionCambioColaborador> _accionesCambio;

    private readonly HashSet<string> EventosValidos = new(StringComparer.OrdinalIgnoreCase){
        "employee_update",
        "job_hire",
        "job_termination",
        "job_movement"
        };
    public ColaboradorService(RestClientService restClient, IColaboradorRepository colaboradorRepository, IAusenciaRepository ausenciaRepository, IEnumerable<IAccionCambioColaborador>? accionesCambio = null)
    {
        _restClient = restClient;
        _colaboradorRepository = colaboradorRepository;
        _ausenciaRepository = ausenciaRepository;
        _accionesCambio = accionesCambio ?? [];
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
        string? detalle = null;        
        try
        {
            switch (eventType)
            {
                case "employee_update" or "job_movement":
                    var colaboradorDB = await _colaboradorRepository.ObtenerPorUsuario(idEmployeeBuk.ToString());
                    if (colaboradorDB is not null)
                    {
                        Console.WriteLine($"Actualizando colaborador existente con ID Buk: {idEmployeeBuk}");
                        colaborador.SalarioDiario=0;                           
                        var cambios = ColaboradorComparador.Comparar(colaboradorDB, colaborador);
                        //await _colaboradorRepository.Actualizar(colaborador);
                        await EjecutarAccionesAsync(idEmployeeBuk, eventType, colaborador, cambios);
                        detalle = cambios.Count > 0 ? $"Cambios: {string.Join("; ", cambios)}" : null;
                        Console.WriteLine($"Cambios detectados: {detalle ?? "Ninguno"}");
                    }
                    else
                    {
                        Console.WriteLine($"Colaborador con ID Buk: {idEmployeeBuk} no existe en la base de datos. Procediendo a registrar como nuevo colaborador.");
                        await ProcesarJobHireAsync(idEmployeeBuk, colaborador);
                    }
                    break;               
                case "job_termination":

                    await _colaboradorRepository.RegistrarBaja(idEmployeeBuk.ToString(), colaborador.ConceptoBaja!, colaborador.FechaBaja!);
                    break;                
            }
            await RegistrarBitacoraAsync(BitacoraDTO.Exito(idEmployeeBuk, eventType, detalle));
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






    internal static DateOnly UltimoTresLunesAnteriores(DateTimeOffset referencia)
    {
        int diasDesdeLunes = ((int)referencia.DayOfWeek + 6) % 7;
        DateOnly lunesSemanaActual = DateOnly.FromDateTime(referencia.Date.AddDays(-diasDesdeLunes));
        DateOnly lunes = lunesSemanaActual.AddDays(-21);
        return lunes;
    }


    public async Task<List<SolicitudDTO>> ObtenerSolicitudesVacaciones()
    {
        DateOnly fechaInicial = UltimoTresLunesAnteriores(DateTimeOffset.Now);

        var solicitudes = new List<SolicitudDTO>();
        Console.WriteLine($"Vacaciones obtenidas: Antes");
        var vacaciones = await _restClient.ObtenerPaginadoAsync<ResponseVacaciones, VacacionesRest, VacacionesRest>(
            ApiClientNames.Buk,
            page => page == 1
                ? $"vacations/requested?date={fechaInicial:yyyy-MM-dd}&page_size=100&status=approved"
                : $"vacations/requested?date={fechaInicial:yyyy-MM-dd}&page_size=100&status=approved&page={page}",
            response => response.Data,
            response => response.pagination?.TotalPages ?? 1,
            vacaciones => vacaciones
            );
        Console.WriteLine($"Vacaciones obtenidas: {vacaciones.Count}");

        solicitudes = vacaciones.Where(vacaciones => vacaciones.PuedeMapearseASolicitud())
                                .Select(vacaciones => vacaciones.toSolicitudDTO())
                                .ToList();

        await AsignarIDSIntelisis(solicitudes);
        await _ausenciaRepository.BorrarVacacionesDesde(fechaInicial);
        await _ausenciaRepository.RegistrarSolicitudesVacaciones(solicitudes);
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
        await _ausenciaRepository.RegistrarAusencias(incapacidades, "Incapacidad");
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

            await _ausenciaRepository.BorrarAusenciasDesde(fechaConsulta);
            await _ausenciaRepository.RegistrarAusencias(permisosAprobados, "Permiso");
            await _ausenciaRepository.BorrarAusenciasPendientes();
            await _ausenciaRepository.RegistrarPermisosPendientes(permisosPendientes, "Permiso");
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


        await _ausenciaRepository.RegistrarAusencias(ausencias, "Ausencia");
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


        await RegistrarSiNoExisteAsync(colaborador);

        await _restClient.PatchAsync(ApiClientNames.Buk, $"employees/{idEmployeeBuk}", new
        {
            custom_attributes = new { idColaborador = colaborador.IdColaborador }
        });
    }

    internal async Task<GetColaboradorResult> GetColaboradorByIdBuk(long idEmployeeBuk)
    {
        try
        {
            //Console.WriteLine($"Obteniendo colaborador de Buk para Employee ID: {ApiClientNames.Buk}");
            var response = await _restClient.GetAsync<ResponseColaborador>(ApiClientNames.Buk, $"employees/{idEmployeeBuk}");
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
        if (string.IsNullOrEmpty(colaborador.RFC))
        {
            colaborador.ReportaA = String.Empty;
            return;
        }
        string? personalBos = await _colaboradorRepository.BuscarPersonalPorRFC(colaborador.RFCBoss!);
        colaborador.ReportaA = personalBos ?? String.Empty;
    }

    private async Task RegistrarSiNoExisteAsync(ColaboradorDTO colaborador)
    {

        int nuevoCodigoPersonal;        
        nuevoCodigoPersonal = await _colaboradorRepository.ObtenerSiguienteClavePersonal(colaborador.esBecario);
        colaborador.IdColaborador = nuevoCodigoPersonal.ToString();
        await _colaboradorRepository.Insertar(colaborador, nuevoCodigoPersonal);


    }

    // Se ejecuta después del UPDATE: una acción que falla se registra en bitácora sin revertir la actualización.
    private async Task EjecutarAccionesAsync(long idEmployeeBuk, string eventType, ColaboradorDTO colaborador, IReadOnlyList<CambioColaborador> cambios)
    {
        foreach (var cambio in cambios)
        {
            foreach (var accion in _accionesCambio.Where(a => a.Campo == cambio.Campo))
            {
                try
                {
                    await accion.EjecutarAsync(colaborador, cambio);
                }
                catch (Exception ex)
                {
                    await RegistrarBitacoraAsync(BitacoraDTO.Error(idEmployeeBuk, eventType, $"Error en {accion.GetType().Name} ({cambio}): {ex.GetBaseException().Message}"));
                }
            }
        }
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

