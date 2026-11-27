using System.Text.Json;
using apiBukLitoprocess.mappers;
using apiBukLitoprocess.responseApi;
using Xunit;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// Pruebas del mapper VacanteRest → VacanteDTO (SPEC 02). Todos los datos son sintéticos.
/// </summary>
public class VacanteExtensionsTests
{
    private static VacanteRest Deserializar(string json) =>
        JsonSerializer.Deserialize<VacanteRest>(json)!;

    private static VacanteRest Vacante(string? rol = "Ayudante", string? status = "cerrado", string? createdAt = "2026-07-20T13:48:25-06:00", long id = 298)
    {
        var rolJson = rol is null ? "null" : $$"""{ "id": 1, "name": {{JsonSerializer.Serialize(rol)}} }""";
        var statusJson = status is null ? "null" : JsonSerializer.Serialize(status);
        var createdJson = createdAt is null ? "null" : JsonSerializer.Serialize(createdAt);
        return Deserializar($$"""
        {
          "id": {{id}},
          "role": {{rolJson}},
          "status": {{statusJson}},
          "created_at": {{createdJson}}
        }
        """);
    }

    [Theory]
    [InlineData("Auxiliar De Nomina", "AUXILIAR DE NOMINA")]
    [InlineData("Diseñador Gráfico", "DISEÑADOR GRÁFICO")]
    [InlineData("  Operador  ", "OPERADOR")]
    public void Nombre_SaleDeRoleName_EnMayusculas(string rol, string esperado)
    {
        var dto = Vacante(rol: rol).ToVacanteDTO();

        Assert.Equal(esperado, dto.nombre);
    }

    [Fact]
    public void SinRole_NombreVacio()
    {
        var dto = Vacante(rol: null).ToVacanteDTO();

        Assert.Equal("", dto.nombre);
    }

    [Theory]
    [InlineData("iniciado")]
    [InlineData("cerrado")]
    [InlineData("cancelado")]
    public void Estatus_TalCual(string status)
    {
        var dto = Vacante(status: status).ToVacanteDTO();

        Assert.Equal(status, dto.estatus);
    }

    [Fact]
    public void SinStatus_EstatusVacio()
    {
        var dto = Vacante(status: null).ToVacanteDTO();

        Assert.Equal("", dto.estatus);
    }

    [Theory]
    [InlineData("2026-07-20T13:48:25-06:00", 2026, 7, 20)]
    [InlineData("2026-07-21T03:00:00Z", 2026, 7, 20)]
    public void FechaRegistro_EsCreatedAtEnHoraDeMexico(string createdAt, int anio, int mes, int dia)
    {
        var dto = Vacante(createdAt: createdAt).ToVacanteDTO();

        Assert.Equal(new DateOnly(anio, mes, dia), dto.fecha_registro);
    }

    [Fact]
    public void IdVacante_EsElIdDeBuk()
    {
        var dto = Vacante(id: 931).ToVacanteDTO();

        Assert.Equal(931, dto.id_vacante);
    }

    [Fact]
    public void SinCreatedAt_NoPuedeMapearse()
    {
        Assert.False(Vacante(createdAt: null).PuedeMapearseAVacante());
        Assert.True(Vacante().PuedeMapearseAVacante());
    }
}
