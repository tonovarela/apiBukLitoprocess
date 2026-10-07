using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;

namespace apiBukLitoprocess.Services.Acciones;

public class AccionCambioReportaA(IHistorialRepository historialRepository)
    : AccionCambioHistorialBase(historialRepository)
{
    public override CampoColaborador Campo => CampoColaborador.ReportaA;
    protected override string Motivo => "Cambio de jefe";
}
