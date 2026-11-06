
using System.Text.Json.Serialization;
namespace apiBukLitoprocess.responseApi;

public class ResponsePermiso
{

    [JsonPropertyName("pagination")]
    public required Pagination Pagination { get; set; }
    
    [JsonPropertyName("data")]
    public required PermisoRest[] Data { get; set; }

}

public partial class PermisoRest
{
    
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("start_date")]
    public required DateOnly Fecha_inicio { get; set; }

    [JsonPropertyName("end_date")]
    public required DateOnly Fecha_fin { get; set; }


    [JsonPropertyName("updated_at")]
    public required  DateTimeOffset? Fecha_Actualizacion { get; set; }
    

    [JsonPropertyName("days_count")]
    public float dias { get; set; }

    [JsonPropertyName("day_percent")]
    public float dias_proporcional { get; set; }

    [JsonPropertyName("status")]
    public required string Estado { get; set; }

    [JsonPropertyName("justification")]
    public required string Justificacion { get; set; }

    [JsonPropertyName("employee_id")]
    public required long EmployeeId { get; set; }

    [JsonPropertyName("permission_type_code")]
    public required string Tipo{ get; set; }

    [JsonPropertyName("start_time")]
    public  string? Inicio { get; set; }

    [JsonPropertyName("end_time")]
    public  string? Fin { get; set; }

    [JsonPropertyName("paid")]
    public bool ConGoceSueldo { get; set; }
  

}
