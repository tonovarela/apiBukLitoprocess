using System;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.responseApi;

namespace apiBukLitoprocess.mappers;

public static class AusenciaExtensions
{


     public static AusenciaDTO toAusenciaDTO(this IncapacidadRest incapacidadRest)
    {
        return new AusenciaDTO
        {
            id_Ausencia = incapacidadRest.Id,            
            id_colaborador = incapacidadRest.EmployeeId,
            tipo = incapacidadRest.Tipo,
            dias = incapacidadRest.dias,
            dias_proporcional = incapacidadRest.dias_proporcional,
            fecha_inicio= incapacidadRest.Fecha_inicio.ToString("yyyy-MM-dd"),
            fecha_fin = incapacidadRest.Fecha_fin.ToString("yyyy-MM-dd"),
            justificacion = incapacidadRest.Justificacion,
            horaEntrada = null,
            horaSalida = null ,
            ConGoceSueldo = false        
        };
    }
    
    public static AusenciaDTO toAusenciaDTO(this AusenciaRest ausencia)
    {
        return new AusenciaDTO
        {
            id_Ausencia = ausencia.Id,            
            id_colaborador = ausencia.EmployeeId,
            tipo = ausencia.Tipo,
            dias = ausencia.dias,
            dias_proporcional = ausencia.dias_proporcional,
            fecha_inicio= ausencia.Fecha_inicio.ToString("yyyy-MM-dd"),
            fecha_fin = ausencia.Fecha_fin.ToString("yyyy-MM-dd"),
            justificacion = ausencia.Justificacion,
            horaEntrada = null,
            horaSalida = null ,
            ConGoceSueldo = ausencia.ConGoceSueldo           
        };
    }


    public static AusenciaDTO toAusenciaDTO(this PermisoRest permiso)
    {
        return new AusenciaDTO
        {
            id_Ausencia = permiso.Id,            
            id_colaborador = permiso.EmployeeId,
            tipo =permiso.Tipo,
            dias = permiso.dias,
            dias_proporcional = permiso.dias_proporcional,            
            fecha_inicio = permiso.Fecha_inicio.ToString("yyyy-MM-dd"),
            fecha_fin = permiso.Fecha_fin.ToString("yyyy-MM-dd"),
            justificacion = permiso.Justificacion,
            horaEntrada = permiso.Inicio,
            horaSalida = permiso.Fin            ,
            ConGoceSueldo = permiso.ConGoceSueldo,
            estado = permiso.Estado
        };
    }


}
