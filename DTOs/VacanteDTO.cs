
namespace apiBukLitoprocess.DTOs
{
    public class VacanteDTO
    {
        public required int id_vacante { get; set; }
        public required string nombre { get; set; }

        public required string estatus { get; set; }
        public DateOnly fecha_registro { get; set; }
    }
}