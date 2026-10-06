namespace apiBukLitoprocess.Models;

public enum CampoColaborador
{
    SueldoDiario,
    Puesto,
    ReportaA,
    CentroCostos,
    Jornada
}

public record CambioColaborador(CampoColaborador Campo, string? Anterior, string? Nuevo)
{
    public override string ToString() => $"{Campo}: '{Anterior}' -> '{Nuevo}'";
}
