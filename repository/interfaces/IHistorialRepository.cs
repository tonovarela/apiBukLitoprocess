
using apiBukLitoprocess.Models;

namespace apiBukLitoprocess.repository.interfaces
{
    public interface IHistorialRepository
    {
        public Task<string?> InsertarHeader(string motivo);
        public Task InsertarDetalle(string personal, string idHistorial, CambioColaborador cambio);

        /// <summary>Inserta header (RH) y detalle (RHD) de la baja en una sola transacción. Devuelve el ID del movimiento.</summary>
        public Task<string> InsertarBaja(string fecha_baja, string motivo, string personal);

         public Task<string> InsertarReingreso(string personal);
    }
}
