using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;
using apiBukLitoprocess.Services.Acciones;
using Moq;
using Xunit;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// Pruebas de las acciones de cambio que registran en historial (AccionCambioHistorialBase).
/// </summary>
public class AccionesCambioTests
{
    private static readonly ColaboradorDTO Colaborador = new()
    {
        Nombre = "JUAN",
        ApellidoPaterno = "PEREZ",
        ApellidoMaterno = "LOPEZ",
        IdColaborador = "1001",
        CURP = "PELJ900101HDFRRN00",
        RFC = "PELJ900101AB1",
        FechaNacimiento = "1990-01-01",
        EstadoCivil = "Soltero"
    };

    private static IAccionCambioColaborador Crear(Type tipo, IHistorialRepository historial) =>
        (IAccionCambioColaborador)Activator.CreateInstance(tipo, historial)!;

    public static TheoryData<Type, CampoColaborador, string> Acciones => new()
    {
        { typeof(AccionCambioPuesto), CampoColaborador.Puesto, "Cambio de puesto" },
        { typeof(AccionCambioSueldoDiario), CampoColaborador.SueldoDiario, "Aumento Sueldo" },
        { typeof(AccionCambioReportaA), CampoColaborador.ReportaA, "Cambio de jefe" },
        { typeof(AccionCambioCentroCostos), CampoColaborador.CentroCostos, "Cambio de Centro de costos" },
        { typeof(AccionCambioJornada), CampoColaborador.Jornada, "Cambio en jornada" },
    };

    [Theory]
    [MemberData(nameof(Acciones))]
    public async Task EjecutarAsync_InsertaHeaderConMotivoYDetalleConSuId(Type tipo, CampoColaborador campo, string motivo)
    {
        var historial = new Mock<IHistorialRepository>();
        historial.Setup(h => h.InsertarHeader(motivo)).ReturnsAsync("77");
        var accion = Crear(tipo, historial.Object);

        await accion.EjecutarAsync(Colaborador, new CambioColaborador(campo, "antes", "despues"));

        Assert.Equal(campo, accion.Campo);
        historial.Verify(h => h.InsertarHeader(motivo), Times.Once);
        historial.Verify(h => h.InsertarDetalle("1001", "77"), Times.Once);
        historial.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EjecutarAsync_HeaderSinId_InsertaDetalleConCero()
    {
        var historial = new Mock<IHistorialRepository>();
        historial.Setup(h => h.InsertarHeader(It.IsAny<string>())).ReturnsAsync((string?)null);
        var accion = new AccionCambioPuesto(historial.Object);

        await accion.EjecutarAsync(Colaborador, new CambioColaborador(CampoColaborador.Puesto, "Analista", "Gerente"));

        historial.Verify(h => h.InsertarDetalle("1001", "0"), Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_FallaInsertarHeader_PropagaExcepcionSinInsertarDetalle()
    {
        var historial = new Mock<IHistorialRepository>();
        historial.Setup(h => h.InsertarHeader(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("bd"));
        var accion = new AccionCambioJornada(historial.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            accion.EjecutarAsync(Colaborador, new CambioColaborador(CampoColaborador.Jornada, "VW", "XX")));

        historial.Verify(h => h.InsertarDetalle(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
