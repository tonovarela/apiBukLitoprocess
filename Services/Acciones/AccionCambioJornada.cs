using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;

namespace apiBukLitoprocess.Services.Acciones;

public class AccionCambioJornada(IHistorialRepository historialRepository)
    : AccionCambioHistorialBase(historialRepository)
{
    public override CampoColaborador Campo => CampoColaborador.Jornada;
    protected override string Motivo => "Cambio en jornada";
}
