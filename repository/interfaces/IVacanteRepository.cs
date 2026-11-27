using apiBukLitoprocess.DTOs;

namespace apiBukLitoprocess.repository.interfaces;

public interface IVacanteRepository
{
    /// <summary>
    /// Upsert por id_vacante en Buk.dbo.Vacante: inserta las nuevas, actualiza
    /// puesto, estado y fecha_registro de las existentes y no borra las que no vienen.
    /// </summary>
    Task GuardarVacantes(List<VacanteDTO> vacantes);
}
