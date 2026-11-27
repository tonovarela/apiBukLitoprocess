using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.responseApi;

namespace apiBukLitoprocess.mappers;

public static class VacanteExtensions
{
    // Zona horaria explícita: el contenedor corre en UTC (misma convención que AusenciaExtensions).
    private static readonly TimeZoneInfo ZonaMx = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");

    /// <summary>
    /// Un proceso de selección solo se puede mapear si trae created_at,
    /// porque de ahí sale VacanteDTO.fecha_registro.
    /// </summary>
    public static bool PuedeMapearseAVacante(this VacanteRest vacante)
    {
        return vacante.created_at.HasValue;
    }

    /// <summary>
    /// Convierte un proceso de selección de Buk en VacanteDTO.
    /// Debe llamarse solo si PuedeMapearseAVacante devuelve true.
    /// </summary>
    public static VacanteDTO ToVacanteDTO(this VacanteRest vacante)
    {
        var creadoMx = TimeZoneInfo.ConvertTime(vacante.created_at!.Value, ZonaMx);

        return new VacanteDTO
        {
            id_vacante = checked((int)vacante.id),
            nombre = vacante.role?.name?.Trim().ToUpperInvariant() ?? string.Empty,
            estatus = vacante.status ?? string.Empty,
            fecha_registro = DateOnly.FromDateTime(creadoMx.DateTime)
        };
    }
}
