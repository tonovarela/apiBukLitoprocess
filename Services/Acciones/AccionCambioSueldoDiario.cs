using apiBukLitoprocess.Clases;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.Models;

namespace apiBukLitoprocess.Services.Acciones;

public class AccionCambioSueldoDiario : IAccionCambioColaborador
{
    public CampoColaborador Campo => CampoColaborador.SueldoDiario;

    public Task EjecutarAsync(ColaboradorDTO colaborador, CambioColaborador cambio)
    {
        EventLogger.Info("cambio_colaborador", new { colaborador.IdColaborador, cambio.Campo, cambio.Anterior, cambio.Nuevo });
        // TODO: persistir el cambio usando el repositorio (inyectar IColaboradorRepository por constructor).
        return Task.CompletedTask;
    }
}
