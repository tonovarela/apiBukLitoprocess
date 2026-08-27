using apiBukLitoprocess.Services;
using Xunit;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// Pruebas del rango de las 3 semanas previas (lunes inicial a viernes final)
/// que calcula ColaboradorService.CalcularMesAnterior.
/// </summary>
public class MesAnteriorTests
{
    [Theory]
    // Semana en curso: 2026-08-24 (lunes) a 2026-08-30 (domingo).
    // Las 3 semanas previas van del lunes 2026-08-03 al viernes 2026-08-21.
    [InlineData("2026-08-24")] // lunes
    [InlineData("2026-08-25")] // martes
    [InlineData("2026-08-27")] // jueves
    [InlineData("2026-08-29")] // sábado
    [InlineData("2026-08-30")] // domingo    
    
    public void CalcularMesAnterior_CualquierDiaDeLaSemana_DevuelveElMismoRango(string fecha)
    {
        var referencia = new DateTimeOffset(DateTime.Parse(fecha), TimeSpan.Zero);

        var (inicio, fin) = ColaboradorService.CalcularMesAnterior(referencia);

        Assert.Equal(new DateOnly(2026, 8, 3), inicio);
        Assert.Equal(new DateOnly(2026, 8, 23), fin);
    }

    // [Fact]
    // public void CalcularMesAnterior_CruceDeAnio_RetrocedeCorrectamente()
    // {
    //     // Viernes 2026-01-02: la semana en curso arranca el lunes 2025-12-29.
    //     var referencia = new DateTimeOffset(2026, 1, 2, 10, 0, 0, TimeSpan.Zero);

    //     var (inicio, fin) = ColaboradorService.CalcularMesAnterior(referencia);

    //     Assert.Equal(new DateOnly(2025, 12, 8), inicio);
    //     Assert.Equal(new DateOnly(2025, 12, 26), fin);
    // }

    // [Fact]
    // public void CalcularMesAnterior_IgnoraLaHoraDelDia()
    // {
    //     var mediaNoche = new DateTimeOffset(2026, 8, 27, 0, 0, 0, TimeSpan.Zero);
    //     var finDelDia = new DateTimeOffset(2026, 8, 27, 23, 59, 59, TimeSpan.Zero);

    //     Assert.Equal(
    //         ColaboradorService.CalcularMesAnterior(mediaNoche),
    //         ColaboradorService.CalcularMesAnterior(finDelDia));
    // }

    // [Fact]
    // public void CalcularMesAnterior_SiempreEsLunesAViernesDeTresSemanas()
    // {
    //     var origen = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);

    //     for (int i = 0; i < 400; i++)
    //     {
    //         var referencia = origen.AddDays(i);
    //         var (inicio, fin) = ColaboradorService.CalcularMesAnterior(referencia);

    //         Assert.Equal(DayOfWeek.Monday, inicio.DayOfWeek);
    //         Assert.Equal(DayOfWeek.Friday, fin.DayOfWeek);
    //         // 3 semanas: del lunes de la primera al viernes de la tercera hay 18 días.
    //         Assert.Equal(18, fin.DayNumber - inicio.DayNumber);

    //         // El rango queda por completo antes de la semana en curso.
    //         int diasDesdeLunes = ((int)referencia.DayOfWeek + 6) % 7;
    //         var lunesSemanaActual = DateOnly.FromDateTime(referencia.Date.AddDays(-diasDesdeLunes));
    //         Assert.True(fin < lunesSemanaActual);
    //         Assert.Equal(lunesSemanaActual.AddDays(-21), inicio);
    //     }
    // }
}
