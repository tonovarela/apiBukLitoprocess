using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.Models;

namespace apiBukLitoprocess.Services.AccionesCambio;

/// <summary>
/// Acción que se ejecuta cuando cambia un campo del colaborador al procesar
/// employee_update / job_movement. Cada implementación se registra en Program.cs:
/// builder.Services.AddScoped&lt;IAccionCambioColaborador, MiAccion&gt;();
/// </summary>
public interface IAccionCambioColaborador
{
    CampoColaborador Campo { get; }
    Task EjecutarAsync(ColaboradorDTO colaborador, CambioColaborador cambio);
}
