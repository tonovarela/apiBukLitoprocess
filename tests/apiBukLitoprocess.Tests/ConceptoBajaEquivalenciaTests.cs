using apiBukLitoprocess.mappers;
using Xunit;

namespace apiBukLitoprocess.Tests;

/// <summary>
/// Equivalencia del concepto de baja de Buk (termination_reason) al concepto de Lito.
/// </summary>
public class ConceptoBajaEquivalenciaTests
{
    [Theory]
    [InlineData("abandono_empleo", "Abandono de Empleo")]
    [InlineData("ausentismo", "Ausentismo")]
    [InlineData("muerte", "Defuncion")]
    [InlineData("jubilacion", "Pension")]
    [InlineData("renuncia_voluntaria", "Separ. Voluntaria")]
    [InlineData("terminacion_natural", "Termino de Contrato")]
    public void MapeaCodigosDeBuk(string buk, string esperado)
    {
        Assert.Equal(esperado, ColaboradorExtensions.MapearConceptoBaja(buk));
    }

    [Theory]
    [InlineData("Abandono de empleo", "Abandono de Empleo")]
    [InlineData("Ausentismo", "Ausentismo")]
    [InlineData("Muerte", "Defuncion")]
    [InlineData("Jubilación", "Pension")]
    [InlineData("Renuncia voluntaria", "Separ. Voluntaria")]
    [InlineData("Terminación natural del contrato por obra o tiempo determinado", "Termino de Contrato")]
    public void MapeaTextosDeBuk(string buk, string esperado)
    {
        Assert.Equal(esperado, ColaboradorExtensions.MapearConceptoBaja(buk));
    }

    [Theory]
    [InlineData("  RENUNCIA VOLUNTARIA ", "Separ. Voluntaria")]
    [InlineData("muerte", "Defuncion")]
    public void IgnoraMayusculasYEspacios(string buk, string esperado)
    {
        Assert.Equal(esperado, ColaboradorExtensions.MapearConceptoBaja(buk));
    }

    [Theory]
    [InlineData("despido_injustificado")]
    [InlineData("despido_justificado")]
    [InlineData("causa_imputable_patron")]
    [InlineData("incapacidad_permanente")]
    [InlineData("invalidez_trabajador")]
    [InlineData("reajuste_maquina")]
    [InlineData("clausura")]
    [InlineData("otras")]
    [InlineData("Despido injustificado")]
    public void ConceptoNoMapeadoDevuelveProblemasFamiliares(string buk)
    {
        Assert.Equal("Problemas Familiares", ColaboradorExtensions.MapearConceptoBaja(buk));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SinConceptoDevuelveVacio(string? buk)
    {
        Assert.Equal(string.Empty, ColaboradorExtensions.MapearConceptoBaja(buk));
    }
}
