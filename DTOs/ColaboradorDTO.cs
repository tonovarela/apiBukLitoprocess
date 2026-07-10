using System;

namespace apiBukLitoprocess.DTOs;

public class ColaboradorDTO
{

    public long?  id { get; set; }
    public required string Nombre { get; set; }
    public required string ApellidoPaterno { get ; set; }
    public required string ApellidoMaterno { get; set; }
    
    public required string IdColaborador  { get; set; }

    public required string CURP { get; set; }

    public required string RFC { get; set; }

    public  string? Correo { get; set; }

     public string? Correo_Personal { get; set; }
    public string? NivelAcademico { get; set; }

    public string? Ext { get; set; } 

    public string NSS { get; set; } = "Sin NSS";

    public string Direccion { get; set; } = "Sin dirección";

    public string? Sexo { get; set; }

    public string Colonia { get; set; } = "Sin colonia";

     public string Delegacion { get; set; } = "Sin delegación";

     public string Poblacion { get; set; } = "Sin población";

     public string Estado { get; set; } = "Sin estado";

     public string Pais { get; set; } = "Mexico";

     public string CodigoPostal { get; set; } = "Sin código postal";

     public string Telefono { get; set; } = "Sin teléfono";

     public required string FechaNacimiento { get; set; } 

     public required string EstadoCivil { get; set; }

    public string? TipoSangre { get; set; }

    public string? TipoColaborador { get; set; }

    public string?  Alergias { get; set; }



    

    public string? NumInt { get; set; }
    public string? NumExt { get; set; }

    public string?  CentroCostos { get; set; }

    
     public string? ReportaA { get; set; }

     public string? Departamento { get; set; }



    public long? BossId { get; set; }


    

    #region Beneficiarios

    public string?  Beneficiario1 { get; set; }
    public string?  Beneficiario2 { get; set; }
    public string?  Beneficiario3 { get; set; }

    public string?  ParentescoBeneficiario1 { get; set; }
    public string?  ParentescoBeneficiario2 { get; set; }
    public string?  ParentescoBeneficiario3 { get; set; }
    
    public string?  PorcentajeBeneficiario1 { get; set; }
    public string?  PorcentajeBeneficiario2 { get; set; }
    public string?  PorcentajeBeneficiario3 { get; set; }

    public string? FechaNacimientoBeneficiario1 { get; set; }
    public string? FechaNacimientoBeneficiario2 { get; set; }   
    public string? FechaNacimientoBeneficiario3 { get; set; }


    #endregion

    #region Contactos de emergencia

    public string?  ContactoEmergencia1 { get; set; }
    public string?  ContactoEmergencia2 { get; set; }
    public string?  ParentescoContactoEmerg1 { get; set; }
    public string?  ParentescoContactoEmerg2 { get; set; }

    public string? TelContactoEmerg1 { get; set; }
    public string? TelContactoEmerg2 { get; set; }





    #endregion



 public string? Puesto { get; set; }

 public string? FechaBaja { get; set; }

 public string? FechaAlta { get; set; }

 public string? ConceptoBaja { get; set; }

 public double Salario { get; set; }






    

    public override string ToString()
    {
        return $"Id: {id}, " +
               $"IdColaborador: {IdColaborador}, " +
               $"Nombre: {Nombre}, " +
               $"ApellidoPaterno: {ApellidoPaterno}, " +
               $"ApellidoMaterno: {ApellidoMaterno}, " +
               $"CURP: {CURP}, " +
               $"RFC: {RFC}, " +
               $"NSS: {NSS}, " +
               $"Correo: {Correo}, " +
               $"FechaNacimiento: {FechaNacimiento}, " +
               $"Sexo: {Sexo}, " +
               $"EstadoCivil: {EstadoCivil}, " +
               $"Direccion: {Direccion}, " +
               $"NumExt: {NumExt}, " +
               $"NumInt: {NumInt}, " +
               $"Colonia: {Colonia}, " +
               $"Delegacion: {Delegacion}, " +
               $"Poblacion: {Poblacion}, " +
               $"Estado: {Estado}, " +
               $"Pais: {Pais}, " +
               $"CodigoPostal: {CodigoPostal}, " +
               $"Telefono: {Telefono}, " +
               $"Ext: {Ext}, " +
               $"NivelAcademico: {NivelAcademico}, " +
               $"TipoSangre: {TipoSangre}, " +
               $"TipoColaborador: {TipoColaborador}, " +
               $"Alergias: {Alergias}, " +
               $"CentroCostos: {CentroCostos}";
    }  



    

    
    















}
