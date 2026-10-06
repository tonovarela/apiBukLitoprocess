using apiBukLitoprocess.Clases;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;

namespace apiBukLitoprocess.Services.Acciones;

public class AccionCambioSueldoDiario : IAccionCambioColaborador
{
    public CampoColaborador Campo => CampoColaborador.SueldoDiario;

    private readonly IHistorialRepository _historialRepository;
    public AccionCambioSueldoDiario(IHistorialRepository historialRepository)
    {
        _historialRepository = historialRepository;
    }
    public async Task EjecutarAsync(ColaboradorDTO colaborador, CambioColaborador cambio)
    {
        EventLogger.Info("cambio_colaborador", new { colaborador.IdColaborador, cambio.Campo, cambio.Anterior, cambio.Nuevo });        
        string? id = await _historialRepository.InsertarHeader("Aumento Sueldo");
        await _historialRepository.InsertarDetalle(colaborador.IdColaborador, id ?? "0");
    }
}
