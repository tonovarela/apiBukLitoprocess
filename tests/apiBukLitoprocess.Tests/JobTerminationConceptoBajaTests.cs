using System.Net;
using apiBukLitoprocess.Clases;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;
using apiBukLitoprocess.Services;
using Moq;
using Xunit;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// En el evento job_termination la baja siempre debe llevar concepto:
/// si Buk no manda termination_reason se usa "Problemas Familiares".
/// </summary>
public class JobTerminationConceptoBajaTests
{
    private static ColaboradorService CrearServicio(
        string jsonBody,
        Mock<IColaboradorRepository>? repositorio = null,
        Mock<IHistorialRepository>? historial = null)
    {
        var handler = new FakeBukHttpMessageHandler(HttpStatusCode.OK, jsonBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://buk.fake/") };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        return new ColaboradorService(
            new RestClientService(factory.Object),
            (repositorio ?? new Mock<IColaboradorRepository>()).Object,
            new Mock<IAusenciaRepository>().Object,
            (historial ?? new Mock<IHistorialRepository>()).Object);
    }

    private static string JsonColaborador(string terminationReason) => $$"""
    {
      "data": {
        "id": 12345,
        "first_name": "Juan",
        "surname": "Perez",
        "rfc": "PELJ900101AB1",
        "active_until": "2026-11-20",
        {{terminationReason}}
        "custom_attributes": { "idColaborador": "1001" },
        "current_job": { "wage": 30000, "role": { "name": "Analista" } }
      }
    }
    """;

    private static WebhookPayloadBody Payload(string eventType) => new()
    {
        EventType = eventType,
        TenantUrl = "https://buk.fake/",
        EmployeeId = 12345
    };

    [Theory]
    [InlineData("")]
    [InlineData("\"termination_reason\": null,")]
    [InlineData("\"termination_reason\": \"\",")]
    public async Task JobTermination_SinConcepto_UsaProblemasFamiliares(string terminationReason)
    {
        var servicio = CrearServicio(JsonColaborador(terminationReason));

        var result = await servicio.handleEventWebhook(Payload("job_termination"));

        Assert.False(result.IsError);
        Assert.Equal("Problemas Familiares", result.colaborador!.ConceptoBaja);
    }

    [Fact]
    public async Task JobTermination_ConCodigo_UsaEquivalencia()
    {
        var servicio = CrearServicio(JsonColaborador("\"termination_reason\": \"renuncia_voluntaria\","));

        var result = await servicio.handleEventWebhook(Payload("job_termination"));

        Assert.False(result.IsError);
        Assert.Equal("Separ. Voluntaria", result.colaborador!.ConceptoBaja);
    }

    [Fact]
    public async Task JobTermination_ColaboradorExiste_RegistraBajaEnUnaSolaLlamada()
    {
        var repositorio = new Mock<IColaboradorRepository>();
        repositorio.Setup(r => r.ObtenerPorUsuario("12345")).ReturnsAsync(new Colaborador
        {
            Personal = "1001",
            SueldoDiario = 1000m,
            ReportaA = "",
            Puesto = "Analista",
            Departamento = "",
            CentroCostos = "",
            Jornada = ""
        });
        var historial = new Mock<IHistorialRepository>();
        historial.Setup(h => h.InsertarBaja(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("99");

        var servicio = CrearServicio(JsonColaborador("\"termination_reason\": \"renuncia_voluntaria\","), repositorio, historial);

        var result = await servicio.handleEventWebhook(Payload("job_termination"));

        Assert.False(result.IsError);
        historial.Verify(h => h.InsertarBaja("2026-11-20", "Separ. Voluntaria", "1001"), Times.Once);
        repositorio.Verify(r => r.InsertarBitacora(It.Is<BitacoraDTO>(b => b.Estado == BitacoraEstado.Exito)), Times.Once);
    }

    [Fact]
    public async Task JobTermination_ColaboradorNoExiste_SoloRegistraOmitido()
    {
        var repositorio = new Mock<IColaboradorRepository>();
        var historial = new Mock<IHistorialRepository>();

        var servicio = CrearServicio(JsonColaborador(""), repositorio, historial);

        var result = await servicio.handleEventWebhook(Payload("job_termination"));

        Assert.False(result.IsError);
        historial.Verify(h => h.InsertarBaja(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        repositorio.Verify(r => r.InsertarBitacora(It.Is<BitacoraDTO>(b => b.Estado == BitacoraEstado.Omitido)), Times.Once);
        repositorio.Verify(r => r.InsertarBitacora(It.Is<BitacoraDTO>(b => b.Estado == BitacoraEstado.Exito)), Times.Never);
    }
}
