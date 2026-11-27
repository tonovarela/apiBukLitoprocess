using System.Net;
using apiBukLitoprocess.Clases;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.repository.interfaces;
using apiBukLitoprocess.Services;
using Moq;
using Xunit;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// Pruebas de ReclutamientoService.SincronizarVacantes con un Buk simulado
/// (FakeBukHttpMessageHandler) y un repositorio mock. Todos los datos son sintéticos.
/// </summary>
public class ReclutamientoServiceTests
{
    private static ReclutamientoService CrearServicio(
        HttpStatusCode statusCode,
        string jsonBody,
        out FakeBukHttpMessageHandler handler,
        out Mock<IVacanteRepository> repositorio)
    {
        handler = new FakeBukHttpMessageHandler(statusCode, jsonBody);

        var fakeHandler = handler;
        var factory = new Mock<IHttpClientFactory>();
        factory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(fakeHandler, disposeHandler: false)
            {
                BaseAddress = new Uri("https://buk.fake/")
            });

        repositorio = new Mock<IVacanteRepository>();
        return new ReclutamientoService(new RestClientService(factory.Object), repositorio.Object);
    }

    private static string Proceso(long id, string? createdAt = "2026-07-20T13:48:25-06:00")
    {
        var created = createdAt is null ? "null" : $"\"{createdAt}\"";
        return $$"""
        {
          "id": {{id}},
          "name": "Puesto {{id}} - AREA SINTETICA",
          "role": { "id": 1, "name": "Puesto {{id}}" },
          "status": "iniciado",
          "created_at": {{created}}
        }
        """;
    }

    private static string Respuesta(int totalPaginas, params string[] procesos) => $$"""
    {
      "pagination": { "next": null, "previous": null, "count": {{procesos.Length}}, "total_pages": {{totalPaginas}} },
      "data": [ {{string.Join(",", procesos)}} ]
    }
    """;

    [Fact]
    public async Task TresRegistros_ConsultaLaUrlEsperadaYGuardaTres()
    {
        var servicio = CrearServicio(HttpStatusCode.OK, Respuesta(1, Proceso(1), Proceso(2), Proceso(3)), out var handler, out var repo);

        var resultado = await servicio.SincronizarVacantes(new DateOnly(2026, 1, 1));

        Assert.Equal(3, resultado.Count);
        Assert.Equal(1, handler.NumeroLlamadas);
        Assert.Contains("recruiting/selection_processes?page_size=100&from=2026-01-01", handler.UltimaPeticion!.RequestUri!.ToString());
        repo.Verify(r => r.GuardarVacantes(It.Is<List<VacanteDTO>>(l => l.Count == 3)), Times.Once);
    }

    [Fact]
    public async Task FechaDesde_SeEnviaComoFrom()
    {
        var servicio = CrearServicio(HttpStatusCode.OK, Respuesta(1, Proceso(1)), out var handler, out _);

        await servicio.SincronizarVacantes(new DateOnly(2026, 9, 1));

        Assert.Contains("from=2026-09-01", handler.UltimaPeticion!.RequestUri!.ToString());
    }

    [Fact]
    public async Task RegistroSinCreatedAt_NoSeGuarda()
    {
        var servicio = CrearServicio(HttpStatusCode.OK, Respuesta(1, Proceso(1), Proceso(2, createdAt: null), Proceso(3)), out _, out var repo);

        var resultado = await servicio.SincronizarVacantes(ReclutamientoService.FechaDesdePorDefecto);

        Assert.Equal(2, resultado.Count);
        Assert.DoesNotContain(resultado, v => v.id_vacante == 2);
        repo.Verify(r => r.GuardarVacantes(It.Is<List<VacanteDTO>>(l => l.Count == 2)), Times.Once);
    }

    [Fact]
    public async Task SinRegistros_NoLlamaAlRepositorio()
    {
        var servicio = CrearServicio(HttpStatusCode.OK, Respuesta(1), out _, out var repo);

        var resultado = await servicio.SincronizarVacantes(ReclutamientoService.FechaDesdePorDefecto);

        Assert.Empty(resultado);
        repo.Verify(r => r.GuardarVacantes(It.IsAny<List<VacanteDTO>>()), Times.Never);
    }

    [Fact]
    public async Task DosPaginas_HaceDosPeticiones()
    {
        var servicio = CrearServicio(HttpStatusCode.OK, Respuesta(2, Proceso(1)), out var handler, out _);

        await servicio.SincronizarVacantes(ReclutamientoService.FechaDesdePorDefecto);

        Assert.Equal(2, handler.NumeroLlamadas);
        Assert.Contains("page=2", handler.UltimaPeticion!.RequestUri!.ToString());
    }

    [Fact]
    public async Task BukRespondeError_PropagaExcepcionYNoGuarda()
    {
        var servicio = CrearServicio(HttpStatusCode.InternalServerError, "{}", out _, out var repo);

        await Assert.ThrowsAsync<HttpRequestException>(() => servicio.SincronizarVacantes(ReclutamientoService.FechaDesdePorDefecto));
        repo.Verify(r => r.GuardarVacantes(It.IsAny<List<VacanteDTO>>()), Times.Never);
    }

    [Fact]
    public async Task RepositorioFalla_PropagaExcepcion()
    {
        var servicio = CrearServicio(HttpStatusCode.OK, Respuesta(1, Proceso(1)), out _, out var repo);
        repo.Setup(r => r.GuardarVacantes(It.IsAny<List<VacanteDTO>>()))
            .ThrowsAsync(new InvalidOperationException("falla sql simulada"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.SincronizarVacantes(ReclutamientoService.FechaDesdePorDefecto));
    }
}
