namespace apiBukLitoprocess.DTOs;

public class AusenciaDTO
{
    public long id_Ausencia { get; set; }
    public  string? personal { get; set; }
    public required long id_colaborador { get; set; }
    public required string tipo { get; set; }
    public required string fecha_inicio { get; set; }
    public required string fecha_fin { get; set; }

    public required float dias { get; set; }
    public required float dias_proporcional { get; set; }
    public required string justificacion { get; set; }
    public string? horaEntrada { get; set; }
    public string? horaSalida { get; set; }

    public bool ConGoceSueldo { get; set; }

    public string? estado { get; set; }




}
