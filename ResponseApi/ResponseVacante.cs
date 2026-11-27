namespace apiBukLitoprocess.responseApi;

using System.Text.Json.Serialization;
using apiBukLitoprocess.helpers;

/// <summary>
/// Respuesta de Buk para GET recruiting/selection_processes (procesos de selección = vacantes).
/// Solo se declaran los campos que usa el mapper; supervisor, reclutadores, public_token,
/// descripción y requisitos no se deserializan.
/// </summary>
public class ResponseVacante
{
    [JsonPropertyName("pagination")]
    public required Pagination pagination { get; set; }

    [JsonPropertyName("data")]
    public required List<VacanteRest> Data { get; set; }
}

public class VacanteRest
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleLongConverter))]
    public long id { get; set; }

    [JsonPropertyName("role")]
    public RolVacanteRest? role { get; set; }

    [JsonPropertyName("status")]
    public string? status { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? created_at { get; set; }
}

public class RolVacanteRest
{
    [JsonPropertyName("name")]
    public string? name { get; set; }
}
