using System;

namespace apiBukLitoprocess.Models
{
    public class Colaborador
    {
        
        
      public required string Personal { get; set; }  
      public decimal? SueldoDiario { get; set; }

      public required string ReportaA { get; set; }

      public required string Puesto { get; set; }

      public required string Departamento { get; set; }

      public required string CentroCostos { get; set; }   

      public required string Jornada { get; set; }


    }
}