
namespace apiBukLitoprocess.repository.interfaces
{
    public interface IHistorialRepository
    {
        public Task<string?> InsertarHeader(string motivo);
        public Task InsertarDetalle(string personal,string idHistorial);
    }
}