using System.Globalization;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.Models;

namespace apiBukLitoprocess.helpers;

/// <summary>
/// Compara el colaborador guardado en dbo.Personal contra el que llega de Buk y
/// devuelve solo los campos que cambiaron. Función pura: no accede a la BD.
/// </summary>
public static class ColaboradorComparador
{
    public static IReadOnlyList<CambioColaborador> Comparar(Colaborador actual, ColaboradorDTO nuevo)
    {
        var cambios = new List<CambioColaborador>();

        // SueldoDiario es money (4 decimales) y SalarioDiario es wage/30 en double: se comparan a 2 decimales.
        decimal? sueldoActual = actual.SueldoDiario.HasValue ? Math.Round(actual.SueldoDiario.Value, 2) : null;
        decimal sueldoNuevo = Math.Round((decimal)nuevo.SalarioDiario, 2);
        if (sueldoActual != sueldoNuevo)
        {
            cambios.Add(new CambioColaborador(
                CampoColaborador.SueldoDiario,
                sueldoActual?.ToString("0.00", CultureInfo.InvariantCulture),
                sueldoNuevo.ToString("0.00", CultureInfo.InvariantCulture)));
        }

        CompararTexto(cambios, CampoColaborador.Puesto, actual.Puesto, nuevo.Puesto);
        CompararTexto(cambios, CampoColaborador.ReportaA, actual.ReportaA, nuevo.ReportaA);
        CompararTexto(cambios, CampoColaborador.CentroCostos, actual.CentroCostos, nuevo.CentroCostos);
        CompararTexto(cambios, CampoColaborador.Jornada, actual.Jornada, nuevo.FactorJornada);

        return cambios;
    }

    // null y "" se consideran iguales; se ignoran espacios (relleno de char(10)) y mayúsculas.
    private static void CompararTexto(List<CambioColaborador> cambios, CampoColaborador campo, string? anterior, string? nuevo)
    {
        var a = anterior?.Trim() ?? string.Empty;
        var n = nuevo?.Trim() ?? string.Empty;
        if (!string.Equals(a, n, StringComparison.OrdinalIgnoreCase))
        {
            cambios.Add(new CambioColaborador(campo, a, n));
        }
    }
}
