using System.Data;
using apiBukLitoprocess.Data;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.repository.interfaces;
using Microsoft.Data.SqlClient;

namespace apiBukLitoprocess.repository.implementation;

public class VacanteRepository : IVacanteRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<VacanteRepository> _logger;

    public VacanteRepository(DbConnectionFactory dbConnectionFactory, ILogger<VacanteRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    // El rollback solo aplica si la transaccion sigue viva: si fallo el propio Commit o se
    // cayo la conexion, tx queda zombie y Rollback() lanzaria una excepcion desde dentro del
    // catch, ocultando el error original.
    private void RevertirTransaccion(SqlTransaction tx)
    {
        try
        {
            if (tx.Connection is not null)
            {
                tx.Rollback();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo revertir la transaccion");
        }
    }

    /// <summary>
    /// Upsert por id_vacante en una sola transaccion (todo o nada).
    /// UPDLOCK + SERIALIZABLE bloquea la llave entre el UPDATE y el INSERT para que dos
    /// sincronizaciones simultaneas no choquen con PK_Vacante.
    /// </summary>
    public async Task GuardarVacantes(List<VacanteDTO> vacantes)
    {
        if (vacantes.Count == 0)
        {
            return;
        }

        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        using var tx = connection.BeginTransaction();

        const string sql = @"
        DECLARE @actualizadas int;

        UPDATE Buk.dbo.Vacante WITH (UPDLOCK, SERIALIZABLE)
        SET puesto = @Puesto,
            estado = @Estado,
            fecha_registro = @FechaRegistro
        WHERE id_vacante = @IdVacante;

        SET @actualizadas = @@ROWCOUNT;

        IF @actualizadas = 0
            INSERT INTO Buk.dbo.Vacante (id_vacante, puesto, estado, fecha_registro)
            VALUES (@IdVacante, @Puesto, @Estado, @FechaRegistro);

        SELECT @actualizadas;";

        try
        {
            using var cmd = new SqlCommand(sql, connection, tx);
            cmd.Parameters.Add("@IdVacante", SqlDbType.Int);
            cmd.Parameters.Add("@Puesto", SqlDbType.VarChar, 250);
            cmd.Parameters.Add("@Estado", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@FechaRegistro", SqlDbType.DateTime);

            int insertadas = 0, actualizadas = 0;
            foreach (var v in vacantes)
            {
                cmd.Parameters["@IdVacante"].Value = v.id_vacante;
                cmd.Parameters["@Puesto"].Value = v.nombre;
                cmd.Parameters["@Estado"].Value = v.estatus;
                cmd.Parameters["@FechaRegistro"].Value = v.fecha_registro.ToDateTime(TimeOnly.MinValue);

                var filas = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                if (filas == 0)
                {
                    insertadas++;
                }
                else
                {
                    actualizadas++;
                }
            }

            tx.Commit();
            _logger.LogInformation("Vacante: {Insertadas} insertadas, {Actualizadas} actualizadas, de {Total} recibidas", insertadas, actualizadas, vacantes.Count);
        }
        catch
        {
            RevertirTransaccion(tx);
            throw;
        }
    }
}
