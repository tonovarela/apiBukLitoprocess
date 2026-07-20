using apiBukLitoprocess.Clases;
using apiBukLitoprocess.conf;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.mappers;
using apiBukLitoprocess.repository.interfaces;
using apiBukLitoprocess.responseApi;
namespace apiBukLitoprocess.Services;

public class AsistenciaService
{
    private readonly RestClientService _restClient;
    private readonly IAsistenciaRepository _asistenciaRepository;
    public AsistenciaService(RestClientService restClient, IAsistenciaRepository asistenciaRepository)
    {
        _restClient = restClient;
        _asistenciaRepository = asistenciaRepository;
    }




    public async Task EliminarJornadasDesde(DateOnly desde)
    {
        await _asistenciaRepository.EliminarJornadaDesdeFecha(desde);
    }


    public async Task<List<JornadaDTO>> registroJornada(DateOnly desde, DateOnly? hasta = null)
    {
        hasta ??= DateOnly.FromDateTime(DateTime.Now.AddDays(10));

        var todasLasJornadas = new List<ResponseJornada>();
        int page = 1;
        bool hayMasResultados = true;
        while (hayMasResultados)
        {
            string url = $"getAsignacionTurnos?token=e25710cb-6215-4577-8bf1-ef15878dd3fc&desde={desde:dd-MM-yyyy}&hasta={hasta:dd-MM-yyyy}&page_size=500&page={page}";
            var jornadasPagina = await _restClient.GetAsync<List<ResponseJornada>>(ApiClientNames.Asistencia, url);

            if (jornadasPagina == null || jornadasPagina.Count == 0)
            {
                hayMasResultados = false;
            }
            else
            {
                todasLasJornadas.AddRange(jornadasPagina);
                page++;
            }
        }
        var jornadasDTOList = todasLasJornadas
                                        .Select(j => j.toJornadaDTO())
                                        .Where(j => j.Turno != "-").ToList();
        
        await _asistenciaRepository.InsertarJornadasIgnorandoDuplicados(jornadasDTOList);
        return jornadasDTOList;

    }

    public async Task<List<ChecadaDTO>> RegistroChecadas(DateOnly desde)
    {
        List<ColaboradorDTO> colaboradores = await ObtenerColaboradoresActivos();
        List<ChecadaDTO> checadaDTOs = new List<ChecadaDTO>();
        //Console.WriteLine($"Obtenidos {colaboradores.Count} colaboradores activos para procesar checadas.");
        List<string> ListRFC = colaboradores.Select(c => c.RFC).ToList();
        //Console.WriteLine($"Total colaboradores activos: {ListRFC.Count}");
        foreach (var rfc in ListRFC)
        {
            try
            {
                var checadasColaborador = await ObtenerChecadas(rfc, desde);
                if (checadasColaborador.Count > 0)
                {
                    EventLogger.Info($"Obtenidas {checadasColaborador.Count} checadas para RFC {rfc}");
                    checadaDTOs.AddRange(checadasColaborador);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al obtener checadas para RFC {rfc}: {e.GetBaseException().Message}");
            }
        }
        await _asistenciaRepository.InsertarLoteChecadasIgnorandoDuplicados(checadaDTOs);
        return checadaDTOs;
    }


    private async Task<List<ColaboradorDTO>> ObtenerColaboradoresActivos()
    {
        return await _restClient.ObtenerPaginadoAsync<ResponseListColaborador, ColaboradorResponse, ColaboradorDTO>(
            ApiClientNames.Buk,
            page => page == 1
                ? "employees/active?page_size=100"
                : $"employees/active?page={page}&page_size=100",
            response => response.data,
            response => response.pagination?.TotalPages ?? 1,
            colaborador => colaborador.ToColaboradorDTO()
            );
    }


    public async Task<List<ChecadaDTO>> ObtenerChecadas(string RFC, DateOnly desde)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        try
        {
            return await _restClient.ObtenerPaginadoAsync<ResponseChecada, ChecadaRest, ChecadaDTO>(
                ApiClientNames.Asistencia,
                page => page == 1
                    ? $"obtenerRegistroAsistencia?obra_id=36915&from={desde:dd-MM-yyyy}&to={today:dd-MM-yyyy}&dni_colaborador={RFC}&page_size=100"
                    : $"obtenerRegistroAsistencia?obra_id=36915&from={desde:dd-MM-yyyy}&to={today:dd-MM-yyyy}&dni_colaborador={RFC}&page_size=100&page={page}",
                response => response.Data,
                response => response.Pagination?.totalPages ?? 1,
                checada => checada.ToChecadaDTO());
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.GetBaseException().Message);
            return new List<ChecadaDTO>();
        }
    }




}


