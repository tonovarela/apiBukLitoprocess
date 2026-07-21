using System.Net;
using System.Text;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// Mock del webservice de Buk.
///
/// RestClientService no llama directamente a Buk: obtiene un HttpClient desde
/// IHttpClientFactory y ejecuta la petición. Sustituyendo el HttpMessageHandler
/// interceptamos la llamada HTTP y devolvemos una respuesta controlada (estado +
/// cuerpo JSON) sin salir a la red, simulando así el endpoint real de Buk
/// (GET employees/{id}).
/// </summary>
public class FakeBukHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _jsonBody;

    /// <summary>Última petición recibida, útil para verificar la URL consultada.</summary>
    public HttpRequestMessage? UltimaPeticion { get; private set; }

    /// <summary>Número de peticiones recibidas por el handler.</summary>
    public int NumeroLlamadas { get; private set; }

    public FakeBukHttpMessageHandler(HttpStatusCode statusCode, string jsonBody)
    {
        _statusCode = statusCode;
        _jsonBody = jsonBody;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        UltimaPeticion = request;
        NumeroLlamadas++;

        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_jsonBody, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}
