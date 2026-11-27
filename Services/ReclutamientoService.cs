using System.Globalization;
using apiBukLitoprocess.Clases;
using apiBukLitoprocess.conf;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.mappers;
using apiBukLitoprocess.repository.interfaces;
using apiBukLitoprocess.responseApi;

namespace apiBukLitoprocess.Services;

/// <summary>
/// Lógica de negocio del módulo de reclutamiento de Buk.
/// Hoy solo sincroniza las vacantes (procesos de selección).
/// </summary>
public class ReclutamientoService
{
    /// <summary>Fecha usada para el filtro "from" de Buk cuando el endpoint no recibe una.</summary>
    public static readonly DateOnly FechaDesdePorDefecto = new(2026, 1, 1);

    private const string RutaProcesosSeleccion = "recruiting/selection_processes";

    private readonly RestClientService _restClient;
    private readonly IVacanteRepository _vacanteRepository;

    public ReclutamientoService(RestClientService restClient, IVacanteRepository vacanteRepository)
    {
        _restClient = restClient;
        _vacanteRepository = vacanteRepository;
    }

    /// <summary>
    /// Descarga los procesos de selección creados desde <paramref name="desde"/>, los mapea a
    /// VacanteDTO y los guarda con upsert. No atrapa excepciones: un error de Buk o de SQL
    /// se propaga al controller.
    /// </summary>
    public async Task<List<VacanteDTO>> SincronizarVacantes(DateOnly desde)
    {
        var from = desde.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var procesos = await _restClient.ObtenerPaginadoAsync<ResponseVacante, VacanteRest, VacanteRest>(
            ApiClientNames.Buk,
            page => page == 1
                ? $"{RutaProcesosSeleccion}?page_size=100&from={from}"
                : $"{RutaProcesosSeleccion}?page={page}&page_size=100&from={from}",
            response => response.Data,
            response => response.pagination?.TotalPages ?? 1,
            proceso => proceso);

        var omitidos = procesos.Where(p => !p.PuedeMapearseAVacante()).ToList();
        if (omitidos.Count > 0)
        {
            EventLogger.Info("Vacantes omitidas sin created_at", new
            {
                total = omitidos.Count,
                ids = omitidos.Select(p => p.id).ToList()
            });
        }

        var vacantes = procesos
            .Where(p => p.PuedeMapearseAVacante())
            .Select(p => p.ToVacanteDTO())
            .ToList();

        if (vacantes.Count > 0)
        {
            await _vacanteRepository.GuardarVacantes(vacantes);
        }

        return vacantes;
    }
}
