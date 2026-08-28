using apiBukLitoprocess.Services;
using Xunit;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// Pruebas del lunes con el que inician las 3 semanas previas a la semana en curso
/// que calcula ColaboradorService.UltimoTresLunesAnteriores.
/// </summary>
public class UltimoTresLunesAnterioresTests
{
    [Theory]
    // Semana en curso: 2026-08-24 (lunes) a 2026-08-30 (domingo).
    // El lunes de 3 semanas atras es el 2026-08-03.
    [InlineData("2026-08-24")] // lunes
    [InlineData("2026-08-25")] // martes
    [InlineData("2026-08-26")] // miercoles
    [InlineData("2026-08-27")] // jueves
    [InlineData("2026-08-28")] // viernes
    [InlineData("2026-08-29")] // sabado
    [InlineData("2026-08-30")] // domingo
    public void UltimoTresLunesAnteriores_CualquierDiaDeLaSemana_DevuelveElMismoLunes(string fecha)
    {
        var referencia = new DateTimeOffset(DateTime.Parse(fecha), TimeSpan.Zero);

        var lunes = ColaboradorService.UltimoTresLunesAnteriores(referencia);

        Assert.Equal(new DateOnly(2026, 8, 3), lunes);
    }

    [Fact]
    public void UltimoTresLunesAnteriores_CruceDeAnio_RetrocedeCorrectamente()
    {
        // Viernes 2026-01-02: la semana en curso arranca el lunes 2025-12-29.
        var referencia = new DateTimeOffset(2026, 1, 2, 10, 0, 0, TimeSpan.Zero);

        var lunes = ColaboradorService.UltimoTresLunesAnteriores(referencia);

        Assert.Equal(new DateOnly(2025, 12, 8), lunes);
    }

    [Fact]
    public void UltimoTresLunesAnteriores_IgnoraLaHoraDelDia()
    {
        var mediaNoche = new DateTimeOffset(2026, 8, 28, 0, 0, 0, TimeSpan.Zero);
        var finDelDia = new DateTimeOffset(2026, 8, 28, 23, 59, 59, TimeSpan.Zero);

        Assert.Equal(
            ColaboradorService.UltimoTresLunesAnteriores(mediaNoche),
            ColaboradorService.UltimoTresLunesAnteriores(finDelDia));
    }
    
}
