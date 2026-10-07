using apiBukLitoprocess.Clases;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;

namespace apiBukLitoprocess.Services.Acciones;

/// <summary>
/// Base para acciones que registran el cambio en el historial.
/// Las clases derivadas solo definen el Campo y el Motivo.
/// </summary>
public abstract class AccionCambioHistorialBase(IHistorialRepository historialRepository) : IAccionCambioColaborador
{
    public abstract CampoColaborador Campo { get; }
    protected abstract string Motivo { get; }

    public virtual async Task EjecutarAsync(ColaboradorDTO colaborador, CambioColaborador cambio)
    {
        EventLogger.Info("cambio_colaborador", new { colaborador.IdColaborador, cambio.Campo, cambio.Anterior, cambio.Nuevo });
        string? id = await historialRepository.InsertarHeader(Motivo);
        await historialRepository.InsertarDetalle(colaborador.IdColaborador, id ?? "0");
    }
}
