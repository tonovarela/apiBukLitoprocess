using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;

namespace apiBukLitoprocess.Services.Acciones;

public class AccionCambioPuesto(IHistorialRepository historialRepository)
    : AccionCambioHistorialBase(historialRepository)
{
    public override CampoColaborador Campo => CampoColaborador.Puesto;
    protected override string Motivo => "Cambio de puesto";
}
