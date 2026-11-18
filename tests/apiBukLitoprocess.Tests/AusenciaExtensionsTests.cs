using apiBukLitoprocess.mappers;
using apiBukLitoprocess.responseApi;
using Xunit;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// fecha_actualizacion debe quedar en hora de México sin importar la zona horaria del servidor
/// (el contenedor corre en UTC; antes ToLocalTime() desfasaba +6h y podía cambiar el día).
/// </summary>
public class AusenciaExtensionsTests
{
    [Theory]
    [InlineData("2026-11-18T10:00:48.472-06:00", "2026-11-18 10:00:48")]
    [InlineData("2026-11-18T20:30:00-06:00", "2026-11-18 20:30:00")] // en UTC sería 2026-11-19
    [InlineData("2026-11-19T02:30:00Z", "2026-11-18 20:30:00")]      // UTC -> 20:30 en México
    public void Permiso_FechaActualizacion_SeConvierteAHoraMexico(string updatedAt, string esperado)
    {
        var permiso = new PermisoRest
        {
            Id = 1,
            EmployeeId = 1,
            Fecha_inicio = new DateOnly(2026, 11, 18),
            Fecha_fin = new DateOnly(2026, 11, 18),
            Fecha_Actualizacion = DateTimeOffset.Parse(updatedAt),
            Estado = "approved",
            Justificacion = "",
            Tipo = "X"
        };

        Assert.Equal(esperado, permiso.toAusenciaDTO().fecha_actualizacion);
    }

    [Fact]
    public void Permiso_SinFechaActualizacion_DevuelveNull()
    {
        var permiso = new PermisoRest
        {
            Id = 1,
            EmployeeId = 1,
            Fecha_inicio = new DateOnly(2026, 11, 18),
            Fecha_fin = new DateOnly(2026, 11, 18),
            Fecha_Actualizacion = null,
            Estado = "approved",
            Justificacion = "",
            Tipo = "X"
        };

        Assert.Null(permiso.toAusenciaDTO().fecha_actualizacion);
    }
}
