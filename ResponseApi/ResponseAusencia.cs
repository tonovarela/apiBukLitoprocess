using System.Text.Json.Serialization;
namespace apiBukLitoprocess.responseApi;

public class ResponseAusencia
{

    [JsonPropertyName("pagination")]
    public required Pagination Pagination { get; set; }

    [JsonPropertyName("data")]
    public required AusenciaRest[] Data { get; set; }

}

public class AusenciaRest
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("start_date")]
    public required DateOnly Fecha_inicio { get; set; }

    [JsonPropertyName("end_date")]
    public required DateOnly Fecha_fin { get; set; }

    [JsonPropertyName("absence_type_code")]
    public required string Tipo { get; set; }

    [JsonPropertyName("status")]
    public required string Estado { get; set; }

    [JsonPropertyName("justification")]
    public required string Justificacion { get; set; }

    [JsonPropertyName("employee_id")]
    public required long EmployeeId { get; set; }




   [JsonPropertyName("days_count")]
    public float dias { get; set; }

    [JsonPropertyName("day_percent")]
    public float dias_proporcional { get; set; }

    [JsonPropertyName("paid")]
    public bool ConGoceSueldo { get; set; }
 
}
