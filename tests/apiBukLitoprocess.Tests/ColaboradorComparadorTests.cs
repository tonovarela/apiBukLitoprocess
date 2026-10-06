using System.Net;
using apiBukLitoprocess.Clases;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.helpers;
using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;
using apiBukLitoprocess.Services;
using apiBukLitoprocess.Services.AccionesCambio;
using Moq;
using Xunit;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// Pruebas de ColaboradorComparador y del despacho de acciones por campo en ColaboradorService.
/// </summary>
public class ColaboradorComparadorTests
{
    private static Colaborador ColaboradorBD() => new()
    {
        Personal = "1001",
        SueldoDiario = 350.5000m,
        ReportaA = "1234",
        Puesto = "Analista",
        Departamento = "Sistemas",
        CentroCostos = "CC-100",
        Jornada = "VW"
    };

    private static ColaboradorDTO ColaboradorBuk() => new()
    {
        Nombre = "JUAN",
        ApellidoPaterno = "PEREZ",
        ApellidoMaterno = "LOPEZ",
        IdColaborador = "1001",
        CURP = "PELJ900101HDFRRN00",
        RFC = "PELJ900101AB1",
        FechaNacimiento = "1990-01-01",
        EstadoCivil = "Soltero",
        SalarioDiario = 350.50,
        ReportaA = "1234",
        Puesto = "Analista",
        CentroCostos = "CC-100",
        FactorJornada = "VW"
    };

    [Fact]
    public void Comparar_SinCambios_RetornaListaVacia()
    {
        Assert.Empty(ColaboradorComparador.Comparar(ColaboradorBD(), ColaboradorBuk()));
    }

    [Fact]
    public void Comparar_RellenoDeCharYMayusculas_NoEsCambio()
    {
        var buk = ColaboradorBuk();
        buk.ReportaA = "1234      ";
        buk.Puesto = "ANALISTA ";

        Assert.Empty(ColaboradorComparador.Comparar(ColaboradorBD(), buk));
    }

    [Fact]
    public void Comparar_NullYVacio_NoEsCambio()
    {
        var bd = ColaboradorBD();
        bd.ReportaA = string.Empty;
        var buk = ColaboradorBuk();
        buk.ReportaA = null;

        Assert.Empty(ColaboradorComparador.Comparar(bd, buk));
    }

    [Fact]
    public void Comparar_SueldoWageEntre30_RedondeaADosDecimales()
    {
        var bd = ColaboradorBD();
        bd.SueldoDiario = 1033.3333m;
        var buk = ColaboradorBuk();
        buk.SalarioDiario = 31000d / 30;

        Assert.Empty(ColaboradorComparador.Comparar(bd, buk));
    }

    [Fact]
    public void Comparar_PuestoYSueldoDistintos_RetornaAmbosCambios()
    {
        var buk = ColaboradorBuk();
        buk.Puesto = "Gerente";
        buk.SalarioDiario = 500;

        var cambios = ColaboradorComparador.Comparar(ColaboradorBD(), buk);

        Assert.Equal(2, cambios.Count);
        Assert.Contains(new CambioColaborador(CampoColaborador.SueldoDiario, "350.50", "500.00"), cambios);
        Assert.Contains(new CambioColaborador(CampoColaborador.Puesto, "Analista", "Gerente"), cambios);
    }

    [Fact]
    public void Comparar_SueldoNuloEnBD_EsCambio()
    {
        var bd = ColaboradorBD();
        bd.SueldoDiario = null;

        var cambio = Assert.Single(ColaboradorComparador.Comparar(bd, ColaboradorBuk()));
        Assert.Equal(CampoColaborador.SueldoDiario, cambio.Campo);
        Assert.Null(cambio.Anterior);
    }

    private sealed class AccionFalsa(CampoColaborador campo, bool falla = false) : IAccionCambioColaborador
    {
        public CampoColaborador Campo { get; } = campo;
        public List<CambioColaborador> Ejecutadas { get; } = [];

        public Task EjecutarAsync(ColaboradorDTO colaborador, CambioColaborador cambio)
        {
            Ejecutadas.Add(cambio);
            return falla ? Task.FromException(new InvalidOperationException("falla")) : Task.CompletedTask;
        }
    }

    private const string JsonBuk = """
    {
      "data": {
        "id": 12345,
        "first_name": "Juan",
        "surname": "Perez",
        "second_surname": "Lopez",
        "rfc": "PELJ900101AB1",
        "gender": "M",
        "custom_attributes": { "idColaborador": "1001" },
        "current_job": { "wage": 30000, "role": { "name": "Analista" } }
      }
    }
    """;

    [Fact]
    public async Task HandleEventWebhook_JobMovement_EjecutaSoloAccionesDelCampoCambiado()
    {
        var httpClient = new HttpClient(new FakeBukHttpMessageHandler(HttpStatusCode.OK, JsonBuk))
        {
            BaseAddress = new Uri("https://buk.fake/")
        };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var bd = ColaboradorBD();
        bd.SueldoDiario = 900m; // Buk manda wage 30000 / 30 = 1000

        var repositorio = new Mock<IColaboradorRepository>();
        repositorio.Setup(r => r.ObtenerPorUsuario("12345")).ReturnsAsync(bd);

        var accionSueldo = new AccionFalsa(CampoColaborador.SueldoDiario, falla: true);
        var accionPuesto = new AccionFalsa(CampoColaborador.Puesto);

        var servicio = new ColaboradorService(
            new RestClientService(factory.Object),
            repositorio.Object,
            new Mock<IAusenciaRepository>().Object,
            accionesCambio: [accionSueldo, accionPuesto]);

        var resultado = await servicio.handleEventWebhook(new WebhookPayloadBody
        {
            EventType = "job_movement",
            TenantUrl = "https://buk.fake",
            EmployeeId = 12345
        });

        Assert.False(resultado.IsError);
        repositorio.Verify(r => r.Actualizar(It.IsAny<ColaboradorDTO>()), Times.Once);

        var cambio = Assert.Single(accionSueldo.Ejecutadas);
        Assert.Equal("900.00", cambio.Anterior);
        Assert.Equal("1000.00", cambio.Nuevo);
        Assert.Empty(accionPuesto.Ejecutadas);

        // La falla de la acción se registra en bitácora sin romper el evento.
        repositorio.Verify(r => r.InsertarBitacora(It.Is<BitacoraDTO>(b => b.Estado == BitacoraEstado.Error)), Times.Once);
        repositorio.Verify(r => r.InsertarBitacora(It.Is<BitacoraDTO>(b => b.Estado == BitacoraEstado.Exito && b.Detalle!.Contains("SueldoDiario"))), Times.Once);
    }
}
