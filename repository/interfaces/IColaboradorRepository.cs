using System;
using System.Xml.Serialization;
using apiBukLitoprocess.DTOs;

namespace apiBukLitoprocess.repository.interfaces;

public interface IColaboradorRepository
{


public Task Actualizar(ColaboradorDTO colaborador);


public Task<Boolean> ExisteColaborador(string id);

public Task  Actualizar(long id, string idColaborador);

public Task Insertar(ColaboradorDTO colaborador,int nuevoIdColaborador);

public Task InsertarBitacora(BitacoraDTO bitacora);

public Task<int> ObtenerSiguienteClavePersonal();

public Task RegistrarBaja(string idPersonalBuk, string conceptoBaja,string fechaBaja);


//public Task<String> ObtenerEquivalenciaArea(long idAreaBuk);  

 public Task RegistrarSolicitudesVacaciones(List<SolicitudDTO> solicitudes);

 public Task RegistrarAusencias(List<AusenciaDTO> ausencias,string clasificacion);

}
