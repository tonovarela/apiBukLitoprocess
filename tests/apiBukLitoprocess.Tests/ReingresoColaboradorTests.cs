using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;
using apiBukLitoprocess.Services.Reingreso;
using Moq;
using Xunit;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// Reingreso: un colaborador de Buk cuyo RFC corresponde a un personal de baja en Intelisis.
/// </summary>
public class ReingresoColaboradorTests
{
    private const string Rfc = "PELJ900101AB1";

    private static ColaboradorDTO ColaboradorBuk(string estatus, long? id = 12345) => new()
    {
        id = id,
        Nombre = "JUAN",
        ApellidoPaterno = "PEREZ",
        ApellidoMaterno = "LOPEZ",
        IdColaborador = "1001",
        CURP = "PELJ900101HDFRRN00",
        RFC = Rfc,
        Estatus = estatus,
        FechaNacimiento = "1990-01-01",
        EstadoCivil = "Soltero"
    };

    private static readonly Colaborador PersonalBaja = new()
    {
        Personal = "1001",
        ReportaA = "",
        Puesto = "",
        Departamento = "",
        CentroCostos = "",
        Jornada = ""
    };

    private readonly Mock<IColaboradorRepository> _colaboradores = new();
    private readonly Mock<IHistorialRepository> _historial = new();

    private ReingresoColaborador Crear() => new(_colaboradores.Object, _historial.Object);

    [Fact]
    public async Task SinPersonalDeBaja_NoAplica()
    {
        _colaboradores.Setup(r => r.ObtenerPorRFC(Rfc, false)).ReturnsAsync((Colaborador?)null);

        var resultado = await Crear().ProcesarAsync(ColaboradorBuk("activo"));

        Assert.Equal(EstadoReingreso.NoAplica, resultado.Estado);
        _historial.Verify(h => h.InsertarReingreso(It.IsAny<string>()), Times.Never);
        _colaboradores.Verify(r => r.Actualizar(It.IsAny<long>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeBajaEnIntelisisEInactivoEnBuk_NoRegistraNada()
    {
        _colaboradores.Setup(r => r.ObtenerPorRFC(Rfc, false)).ReturnsAsync(PersonalBaja);

        var resultado = await Crear().ProcesarAsync(ColaboradorBuk("inactivo"));

        Assert.Equal(EstadoReingreso.InactivoEnBuk, resultado.Estado);
        Assert.Equal("1001", resultado.Personal);
        _historial.Verify(h => h.InsertarReingreso(It.IsAny<string>()), Times.Never);
        _colaboradores.Verify(r => r.Actualizar(It.IsAny<long>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("activo")]
    [InlineData("Activo")]
    public async Task DeBajaEnIntelisisYActivoEnBuk_RegistraReingresoYLigaUsuario(string estatus)
    {
        _colaboradores.Setup(r => r.ObtenerPorRFC(Rfc, false)).ReturnsAsync(PersonalBaja);
        _historial.Setup(h => h.InsertarReingreso("1001")).ReturnsAsync("555");

        var resultado = await Crear().ProcesarAsync(ColaboradorBuk(estatus));

        Assert.Equal(EstadoReingreso.Realizado, resultado.Estado);
        Assert.Equal("1001", resultado.Personal);
        Assert.Equal("555", resultado.IdMovimiento);
        _historial.Verify(h => h.InsertarReingreso("1001"), Times.Once);
        _colaboradores.Verify(r => r.Actualizar(12345L, "1001"), Times.Once);
    }

    [Fact]
    public async Task SinIdDeBuk_LanzaExcepcionSinRegistrar()
    {
        _colaboradores.Setup(r => r.ObtenerPorRFC(Rfc, false)).ReturnsAsync(PersonalBaja);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Crear().ProcesarAsync(ColaboradorBuk("activo", id: null)));

        _historial.Verify(h => h.InsertarReingreso(It.IsAny<string>()), Times.Never);
    }
}
