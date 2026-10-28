
using System.Globalization;
using apiBukLitoprocess.Data;
using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;
using Microsoft.Data.SqlClient;

namespace apiBukLitoprocess.repository.implementation
{
    public class HistorialRepository : IHistorialRepository
    {

        private readonly DbConnectionFactory _dbConnectionFactory;
         private readonly ILogger<HistorialRepository> _logger;
         private readonly ILogger _sqlLogger;

        public HistorialRepository(DbConnectionFactory dbConnectionFactory, ILogger<HistorialRepository> logger, ILoggerFactory loggerFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
            _sqlLogger = loggerFactory.CreateLogger("SqlQueries");
        }

        public async Task InsertarDetalle(string personal, string idHistorial, CambioColaborador cambio)
        {
            // La columna del campo que cambió se toma de @valorNuevo; el resto se copia de Personal.
            string Col(string columna, CampoColaborador campo) => cambio.Campo == campo ? "@valorNuevo" : columna;

            string sql=$@"
                        Insert Into RHD (ID, Renglon, Personal, SueldoDiario, SDI, TipoContrato, PeriodoTipo, Jornada, TipoSueldo, Categoria,
                                Departamento, Puesto, Grupo, FechaAlta, FechaAntiguedad, ReportaA, CentroCostos)
                        Select @ID, 2048, Personal, {Col("SueldoDiario", CampoColaborador.SueldoDiario)}, SDI, TipoContrato, PeriodoTipo, {Col("Jornada", CampoColaborador.Jornada)}, TipoSueldo, Categoria,
                        Departamento, {Col("Puesto", CampoColaborador.Puesto)}, Grupo, FechaAlta, FechaAntiguedad, {Col("ReportaA", CampoColaborador.ReportaA)}, {Col("CentroCostos", CampoColaborador.CentroCostos)}
                        From Personal
                        Where Personal=@personal
                        ";

            object valorNuevo = cambio.Campo == CampoColaborador.SueldoDiario
                ? decimal.Parse(cambio.Nuevo!, CultureInfo.InvariantCulture)
                : (object?)cambio.Nuevo ?? DBNull.Value;

            using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@personal", personal);
            command.Parameters.AddWithValue("@ID", idHistorial);
            command.Parameters.AddWithValue("@valorNuevo", valorNuevo);
            _logger.LogInformation("Inserting detalle for personal: {personal} with idHistorial: {idHistorial}, campo: {campo}, valorNuevo: {valorNuevo}", personal, idHistorial, cambio.Campo, cambio.Nuevo);
            _sqlLogger.LogInformation("Executing SQL: {sql} with parameters: personal={personal}, idHistorial={idHistorial}, valorNuevo={valorNuevo}", sql, personal, idHistorial, cambio.Nuevo);
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

            _logger.LogInformation("Inserting header with motivo: {motivo}", motivo);
            _sqlLogger.LogInformation("Executing SQL: {sql} with parameter: motivo={motivo}", sql, motivo);
            var id = await command.ExecuteScalarAsync();
            return id?.ToString();
        }
    }
}