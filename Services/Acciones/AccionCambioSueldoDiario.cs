using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;

namespace apiBukLitoprocess.Services.Acciones;

public class AccionCambioSueldoDiario(IHistorialRepository historialRepository)
    : AccionCambioHistorialBase(historialRepository)
{
    public override CampoColaborador Campo => CampoColaborador.SueldoDiario;
    protected override string Motivo => "Aumento Sueldo";
}
