using System;
using apiBukLitoprocess.DTOs;

namespace apiBukLitoprocess.repository.interfaces;

public interface IAusenciaRepository
{

  public Task RegistrarSolicitudesVacaciones(List<SolicitudDTO> solicitudes);

  public Task RegistrarAusencias(List<AusenciaDTO> ausencias, string clasificacion);

  public Task RegistrarPermisosPendientes(List<AusenciaDTO> ausencias, string clasificacion);

  public Task BorrarVacacionesDesde(DateOnly fecha);

  public Task BorrarAusenciasDesde(DateOnly fecha);

  public Task BorrarAusenciasPendientes();

}
