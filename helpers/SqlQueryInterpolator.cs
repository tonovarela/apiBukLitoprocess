using System.Globalization;
using Microsoft.Data.SqlClient;

namespace apiBukLitoprocess.helpers;

/// <summary>
/// Reconstruye un query parametrizado sustituyendo sus parametros por los valores reales,
/// para poder inspeccionarlo o pasarlo al DBA.
/// NOTA: solo para depuracion. La version parametrizada sigue siendo la segura contra
/// inyeccion SQL; no ejecutar el resultado interpolado en produccion sin revisar.
/// </summary>
public static class SqlQueryInterpolator
{
    public static string Interpolar(SqlCommand command)
    {
        var query = command.CommandText;

        // Reemplazar primero los nombres mas largos evita que, por ejemplo,
        // @Id sobreescriba parte de @IdColaborador.
        foreach (SqlParameter p in command.Parameters
                     .Cast<SqlParameter>()
                     .OrderByDescending(p => p.ParameterName.Length))
        {
            query = query.Replace(p.ParameterName, FormatearValor(p.Value));
        }

        return query;
    }

    private static string FormatearValor(object? valor)
    {
        if (valor is null || valor == DBNull.Value)
        {
            return "NULL";
        }

        switch (valor)
        {
            case string s:
                return $"'{s.Replace("'", "''")}'";
            case DateTime d:
                return $"'{d:yyyy-MM-dd HH:mm:ss}'";
            case bool b:
                return b ? "1" : "0";
            case TimeSpan t:
                return $"'{t}'";
            case byte or short or int or long or float or double or decimal:
                return Convert.ToString(valor, CultureInfo.InvariantCulture) ?? "NULL";
            default:
                return $"'{valor.ToString()?.Replace("'", "''")}'";
        }
    }
}
