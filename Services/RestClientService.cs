using System.Text.Json;
namespace apiBukLitoprocess.Clases;

public class RestClientService
{    
    private readonly IHttpClientFactory _httpClientFactory;
    public RestClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
     public async Task<T?> GetAsync<T>(string clientName, string url)
    {
    
        var client = _httpClientFactory.CreateClient(clientName);        
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    


    public async Task PatchAsync<TRequest>(string clientName, string url, TRequest data)
    {
        var client = _httpClientFactory.CreateClient(clientName);
        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = JsonContent.Create(data)
        };
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }



  /// <summary>
    /// Recupera todos los registros de un endpoint paginado, obteniendo la primera página,
    /// calculando el total de páginas disponibles y consultando el resto en paralelo.
    /// </summary>
    /// <typeparam name="TResponse">
    /// Tipo de la respuesta deserializada devuelta por el cliente HTTP.
    /// </typeparam>
    /// <typeparam name="TOrigen">
    /// Tipo de los elementos contenidos en la colección de la respuesta.
    /// </typeparam>
    /// <typeparam name="TDestino">
    /// Tipo final al que se transforma cada elemento antes de devolverlo.
    /// </typeparam>
    /// <param name="clientName">
    /// Nombre del cliente HTTP configurado que se utilizará para hacer la petición.
    /// </param>
    /// <param name="construirUrl">
    /// Función que construye la URL a consultar en función del número de página.
    /// </param>
    /// <param name="obtenerItems">
    /// Función que extrae la colección de elementos desde la respuesta del servicio.
    /// </param>
    /// <param name="obtenerTotalPaginas">
    /// Función que obtiene el total de páginas disponibles a partir de la primera respuesta.
    /// </param>
    /// <param name="mapear">
    /// Función que transforma cada elemento recuperado al tipo de salida esperado.
    /// </param>
    /// <returns>
    /// Lista con todos los elementos de todas las páginas ya transformados al tipo destino.
    /// Si la respuesta inicial es nula o no contiene elementos, devuelve una lista vacía.
    /// </returns>

    public async Task<List<TDestino>> ObtenerPaginadoAsync<TResponse, TOrigen, TDestino>(
       string clientName,
       Func<int, string> construirUrl,
       Func<TResponse, IEnumerable<TOrigen>?> obtenerItems,
       Func<TResponse, int> obtenerTotalPaginas,
       Func<TOrigen, TDestino> mapear)
    {
        var resultado = new List<TDestino>();

        var primeraRespuesta = await GetAsync<TResponse>(clientName, construirUrl(1));
        if (primeraRespuesta == null)
        {
            return resultado;
        }

        var primerosItems = obtenerItems(primeraRespuesta);
        if (primerosItems == null)
        {
            return resultado;
        }

        resultado.AddRange(primerosItems.Select(mapear));

        int totalPaginas = obtenerTotalPaginas(primeraRespuesta);
        if (totalPaginas <= 1)
        {
            return resultado;
        }

        var tareasPaginas = Enumerable.Range(2, totalPaginas - 1)
            .Select(page => GetAsync<TResponse>(clientName, construirUrl(page)));

        var respuestas = await Task.WhenAll(tareasPaginas);

        foreach (var respuesta in respuestas)
        {
            if (respuesta == null)
            {
                continue;
            }

            var items = obtenerItems(respuesta);
            if (items == null)
            {
                continue;
            }

            resultado.AddRange(items.Select(mapear));
        }

        return resultado;
    }






}
