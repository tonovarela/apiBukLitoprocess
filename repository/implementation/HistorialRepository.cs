
using apiBukLitoprocess.Data;
using apiBukLitoprocess.repository.interfaces;
using Microsoft.Data.SqlClient;

namespace apiBukLitoprocess.repository.implementation
{
    public class HistorialRepository : IHistorialRepository
    {

        private readonly DbConnectionFactory _dbConnectionFactory;
         private readonly ILogger<HistorialRepository> _logger;
         private readonly ILogger _sqlLogger;

        public HistorialRepository(DbConnectionFactory dbConnectionFactory, ILogger<HistorialRepository> logger, ILogger sqlLogger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
            _sqlLogger = sqlLogger;
        }

        public async Task InsertarDetalle(string personal,string idHistorial)
        {
            string sql=@"
                        Insert Into RHD (ID, Renglon, Personal, SueldoDiario, SDI, TipoContrato, PeriodoTipo, Jornada, TipoSueldo, Categoria,
                                Departamento, Puesto, Grupo, FechaAlta, FechaAntiguedad, ReportaA, CentroCostos)
                        Values
                        Select @ID, 2048, Personal, SueldoDiario, SDI, TipoContrato, PeriodoTipo, Jornada, TipoSueldo, Categoria,
                        Departamento, Puesto, Grupo, FechaAlta, FechaAntiguedad, ReportaA, CentroCostos
                        From Personal
                        Where Personal=@personal                        
                        ";          
            using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@personal", personal);
            command.Parameters.AddWithValue("@ID", idHistorial);    
            _logger.LogInformation("Inserting detalle for personal: {personal} with idHistorial: {idHistorial}", personal, idHistorial);
            _sqlLogger.LogInformation("Executing SQL: {sql} with parameters: personal={personal}, idHistorial={idHistorial}", sql, personal, idHistorial);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task<string?> InsertarHeader(string motivo)
        {
            const string sql = @"                
                Insert Into RH (Empresa, Mov, FechaEmision, Concepto, Moneda, TipoCambio, Usuario, Estatus, Ejercicio, Periodo)
                Values ('LITO', 'Modificaciones', convert(date, getdate()), @motivo, 'Pesos', 1, 'JGARCIA', 'SINAFECTAR', YEAR(GETDATE()), MONTH(GETDATE()));
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@motivo", motivo);

            await connection.OpenAsync();
            _logger.LogInformation("Inserting header with motivo: {motivo}", motivo);
            _sqlLogger.LogInformation("Executing SQL: {sql} with parameter: motivo={motivo}", sql, motivo);
            var id = await command.ExecuteScalarAsync();
            return id?.ToString();
        }
    }
}