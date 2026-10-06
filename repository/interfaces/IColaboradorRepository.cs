using System;
using System.Xml.Serialization;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.Models;

namespace apiBukLitoprocess.repository.interfaces;

public interface IColaboradorRepository
{


public Task<Colaborador?> Obtener(string personal);
public Task<Colaborador?> ObtenerPorUsuario(string idBuk);
public Task Actualizar(ColaboradorDTO colaborador);

public Task<string?> BuscarPersonalPorRFC(string rfc);

public Task<Boolean> ExisteColaborador(string id);

public Task  Actualizar(long id, string idColaborador);

public Task Insertar(ColaboradorDTO colaborador,int nuevoIdColaborador);

public Task InsertarBitacora(BitacoraDTO bitacora);

public Task<int> ObtenerSiguienteClavePersonal(bool esBecario);

public Task RegistrarBaja(string idPersonalBuk, string conceptoBaja,string fechaBaja);



}
