
using apiBukLitoprocess.repository.interfaces;

namespace apiBukLitoprocess.repository.implementation
{
    public class HistorialRepository : IHistorialRepository
    {
        public Task InsertarDetalle(string personal, string idHistorial)
        {
            return Task.FromResult<object?>(null);
        }

        public Task<string?> InsertarHeader(string personal, string motivo)
        {
            return Task.FromResult<string?>(null);
        }
    }
}