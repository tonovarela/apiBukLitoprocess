using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiBukLitoprocess.repository.interfaces
{
    public interface IHistorialRepository
    {
        public Task<string?> InsertarHeader(string personal,string motivo);
        public Task InsertarDetalle(string personal,string idHistorial);
    }
}