using apiBukLitoprocess.Clases;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;

namespace apiBukLitoprocess.Services.Acciones;

public class AccionCambioReportaA : IAccionCambioColaborador
{
    public CampoColaborador Campo => CampoColaborador.ReportaA;

    private readonly IHistorialRepository _historialRepository;
    public AccionCambioReportaA(IHistorialRepository historialRepository)
    {
        _historialRepository = historialRepository;
    }

    public async Task EjecutarAsync(ColaboradorDTO colaborador, CambioColaborador cambio)
    {
        EventLogger.Info("cambio_colaborador", new { colaborador.IdColaborador, cambio.Campo, cambio.Anterior, cambio.Nuevo });        
        string? id = await _historialRepository.InsertarHeader("Cambio de jefe");
        await _historialRepository.InsertarDetalle(colaborador.IdColaborador, id ?? "0");
    }
}
