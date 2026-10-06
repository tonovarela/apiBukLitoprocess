
using apiBukLitoprocess.Clases;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;

namespace apiBukLitoprocess.Services.Acciones;

public class AccionCambioCentroCostos : IAccionCambioColaborador
{
    public CampoColaborador Campo => CampoColaborador.CentroCostos;

    private readonly IHistorialRepository _historialRepository;

    public AccionCambioCentroCostos(IHistorialRepository historialRepository)
    {
        _historialRepository = historialRepository;
    }

    public async Task EjecutarAsync(ColaboradorDTO colaborador, CambioColaborador cambio)
    {
        EventLogger.Info("cambio_colaborador", new { colaborador.IdColaborador, cambio.Campo, cambio.Anterior, cambio.Nuevo });        
        string? id =await _historialRepository.InsertarHeader("Cambio de Centro de costos");
        await _historialRepository.InsertarDetalle(colaborador.IdColaborador, id??"0");



        
    }
}
