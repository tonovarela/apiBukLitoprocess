using System;
using System.Text.Json.Serialization;

namespace apiBukLitoprocess.responseApi;
public class ResponseIncapacidad
{
    [JsonPropertyName("pagination")]
    public required Pagination Pagination { get; set; }  

    [JsonPropertyName("data")]
    public required IncapacidadRest[] Data { get; set; }
}

public class IncapacidadRest
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("start_date")]
    public required DateOnly Fecha_inicio { get; set; }

    [JsonPropertyName("end_date")]
    public required DateOnly Fecha_fin { get; set; }
    
    [JsonPropertyName("days_count")]
    public float dias { get; set; }

    [JsonPropertyName("day_percent")]
    public float dias_proporcional { get; set; }

    [JsonPropertyName("status")]
    public required string Estado { get; set; }

    [JsonPropertyName("justification")]
    public required string Justificacion { get; set; }

    [JsonPropertyName("licence_number")]
    public required string licencia { get; set; }

    
    [JsonPropertyName("employee_id")]
    public required long EmployeeId { get; set; }

    [JsonPropertyName("licence_type")]
    public required string Tipo{ get; set; }

    
}
