using System.Net;
using apiBukLitoprocess.Clases;
using apiBukLitoprocess.conf;
using apiBukLitoprocess.repository.interfaces;
using apiBukLitoprocess.Services;
using Moq;
using Xunit;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// Pruebas del método ColaboradorService.GetColaboradorByIdBuk usando un mock del
/// webservice de Buk (FakeBukHttpMessageHandler). No se realiza ninguna llamada real.
/// </summary>
public class ColaboradorServiceBukTests
{
    /// <summary>
    /// Arma un ColaboradorService cuyo RestClientService apunta al handler falso.
    /// El repositorio se sustituye por un mock de Moq porque el constructor lo exige,
    /// pero GetColaboradorByIdBuk no lo utiliza.
    /// </summary>
    private static ColaboradorService CrearServicio(
        HttpStatusCode statusCode,
        string jsonBody,
        out FakeBukHttpMessageHandler handler)
    {
        handler = new FakeBukHttpMessageHandler(statusCode, jsonBody);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://buk.fake/")
        };

        var factory = new Mock<IHttpClientFactory>();
        factory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var restClient = new RestClientService(factory.Object);
        var repositorio = new Mock<IColaboradorRepository>();

        return new ColaboradorService(restClient, repositorio.Object);
    }

    private const string JsonColaboradorValido = """
    {
      "data": {
        "id": 12345,
        "first_name": "Juan",
        "surname": "Perez",
        "second_surname": "Lopez",
        "curp": "PELJ900101HDFRRN00",
        "rfc": "PELJ900101AB1",
        "email": "juan.perez@empresa.com",
        "personal_email": "juanp@gmail.com",
        "gender": "M",
        "social_security": "12345678901",
        "phone": "5555555555",
        "birthday": "1990-01-01T00:00:00Z",
        "period_type": "weekly",
        "custom_attributes": {
          "idColaborador": "1001",
          "TipoColaborador": "Empleado"
        },
        "current_job": {
          "cost_center": "CC-100",
          "worker_kind": "Confianza",
          "wage": 30000,
          "boss": { "id": 999 },
          "role": { "name": "Analista" }
        }
      }
    }
    """;

    [Fact]
    public async Task GetColaboradorByIdBuk_RespuestaValida_MapeaColaborador()
    {
        // Arrange
        var servicio = CrearServicio(HttpStatusCode.OK, JsonColaboradorValido, out var handler);

        // Act
        var resultado = await servicio.GetColaboradorByIdBuk(12345);

        // Assert
        Assert.False(resultado.IsError);
        Assert.Equal(200, resultado.StatusCode);
        Assert.NotNull(resultado.colaborador);

        var colaborador = resultado.colaborador!;
        Assert.Equal(12345, colaborador.id);
        Assert.Equal("JUAN", colaborador.Nombre);
        Assert.Equal("PEREZ", colaborador.ApellidoPaterno);
        Assert.Equal("LOPEZ", colaborador.ApellidoMaterno);
        Assert.Equal("1001", colaborador.IdColaborador);
        Assert.Equal("Masculino", colaborador.Sexo);
        Assert.Equal(999, colaborador.BossId);
        Assert.Equal("Analista", colaborador.Puesto);
        Assert.Equal("Semanal", colaborador.PeriodoTipo);

        // El endpoint consultado debe ser employees/{id}
        Assert.NotNull(handler.UltimaPeticion);
        Assert.Contains("employees/12345", handler.UltimaPeticion!.RequestUri!.ToString());
        Assert.Equal(1, handler.NumeroLlamadas);
    }

    [Fact]
    public async Task GetColaboradorByIdBuk_SinData_Retorna404()
    {
        // Buk responde 200 pero sin la propiedad "data" (colaborador inexistente)
        var servicio = CrearServicio(HttpStatusCode.OK, "{}", out _);

        var resultado = await servicio.GetColaboradorByIdBuk(777);

        Assert.True(resultado.IsError);
        Assert.Equal(404, resultado.StatusCode);
        Assert.Null(resultado.colaborador);
        Assert.Equal("Colaborador no encontrado", resultado.ErrorMessage);
    }

    [Fact]
    public async Task GetColaboradorByIdBuk_ErrorHttp_PropagaCodigoDeEstado()
    {
        // Buk responde 500: EnsureSuccessStatusCode lanza HttpRequestException
        var servicio = CrearServicio(HttpStatusCode.InternalServerError, "error interno", out _);

        var resultado = await servicio.GetColaboradorByIdBuk(500);

        Assert.True(resultado.IsError);
        Assert.Equal(500, resultado.StatusCode);
        Assert.Null(resultado.colaborador);
    }

    [Fact]
    public async Task GetColaboradorByIdBuk_NoEncontradoEnBuk_Retorna404()
    {
        // Buk responde 404 directamente
        var servicio = CrearServicio(HttpStatusCode.NotFound, "not found", out _);

        var resultado = await servicio.GetColaboradorByIdBuk(404);

        Assert.True(resultado.IsError);
        Assert.Equal(404, resultado.StatusCode);
        Assert.Null(resultado.colaborador);
    }

    [Fact]
    public void ApiClientNames_Buk_EsElClienteConfigurado()
    {
        // Comprobación de apoyo: el nombre del cliente HTTP usado por el servicio.
        Assert.Equal("BukApi", ApiClientNames.Buk);
    }
}
