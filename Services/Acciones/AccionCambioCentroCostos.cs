using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;

namespace apiBukLitoprocess.Services.Acciones;

public class AccionCambioCentroCostos(IHistorialRepository historialRepository)
    : AccionCambioHistorialBase(historialRepository)
{
    public override CampoColaborador Campo => CampoColaborador.CentroCostos;
    protected override string Motivo => "Cambio de Centro de costos";
}
