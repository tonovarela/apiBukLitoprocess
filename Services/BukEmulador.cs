using System.Reflection;
using System.Text.Json;
using apiBukLitoprocess.responseApi;

namespace apiBukLitoprocess.Services;

/// <summary>
/// Emulador del webservice de Buk.
///
/// Devuelve una respuesta fija (el JSON de un colaborador real embebido como
/// recurso) usando el MISMO modelo <see cref="ResponseColaborador"/> y las mismas
/// opciones de deserialización que <c>RestClientService</c>. Así el resultado pasa
/// por el mismo mapeo (ToColaboradorDTO) que en producción, permitiendo probar
/// GetColaboradorByIdBuk y el resto del flujo sin consumir la API real.
///
/// Se activa con la configuración "BukApiSettings:Emular" = true.
/// </summary>
internal static class BukEmulador
{
    private const string NombreRecurso = "colaborador_emulado.json";

    private static readonly JsonSerializerOptions Opciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static string? _jsonCache;

    /// <summary>
    /// Deserializa el JSON emulado al mismo tipo que devuelve la llamada real a Buk.
    /// </summary>
    public static ResponseColaborador? ObtenerResponse()
        => JsonSerializer.Deserialize<ResponseColaborador>(LeerJson(), Opciones);

    private static string LeerJson()
    {
        if (_jsonCache is not null)
        {
            return _jsonCache;
        }

        var assembly = Assembly.GetExecutingAssembly();
        var recurso = assembly.GetManifestResourceNames()
            .Single(nombre => nombre.EndsWith(NombreRecurso, StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(recurso)!;
        using var reader = new StreamReader(stream);
        _jsonCache = reader.ReadToEnd();
        return _jsonCache;
    }
}
