using System.Data;
using apiBukLitoprocess.Data;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.repository.interfaces;
using Microsoft.Data.SqlClient;

namespace apiBukLitoprocess.repository.implementation;

public class AsistenciaRepository : IAsistenciaRepository
{

    private readonly DbConnectionFactory _dbConnectionFactory;

    public AsistenciaRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    // 2601 = indice unico duplicado, 2627 = violacion de PK/unique constraint.
    // Cualquier otro numero es un error real que no debe ignorarse.
    private static bool EsClaveDuplicada(SqlException ex) => ex.Number is 2601 or 2627;

    // El rollback solo aplica si la transaccion sigue viva: si fallo el propio Commit o se
    // cayo la conexion, tx queda zombie y Rollback() lanzaria una excepcion desde dentro del
    // catch, ocultando el error original.
    private static void RevertirTransaccion(SqlTransaction tx)
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
            Console.WriteLine($"No se pudo revertir la transaccion: {ex.Message}");
        }
    }

    public async Task EliminarJornadaDesdeFecha(DateOnly fecha)
    {
        //Console.WriteLine($"Eliminando asistencias desde la fecha: { fecha.ToString("yyyy-MM-dd")}");
        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        const string sql = "DELETE FROM Buk.dbo.Jornada WHERE fecha >= @Fecha;";
        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@Fecha", SqlDbType.DateTime).Value = fecha.ToDateTime(TimeOnly.MinValue);
        await cmd.ExecuteNonQueryAsync();
    }

    // public async Task InsertarAsistenciasIgnorandoDuplicados(List<AsistenciaDTO> asistencias)
    // {
    //     Console.WriteLine("Iniciando inserción de asistencias en la base de datos...");
    //     if (asistencias is null || asistencias.Count == 0) return;

    //     using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
    //     using var tx = connection.BeginTransaction();        
    //     const string sql = @"
    //     INSERT INTO Buk.dbo.Jornada
    //         (id, rfc, fecha, turno_noche, turno, inicio, fin, jornada)
    //     VALUES
    //         (@IdAsistencia, @Rfc, @Dia, @TurnoNoche, @Turno, @Entrada, @Salida, @CodigoTurno);";
    //     try
    //     {
    //         using var cmd = new SqlCommand(sql, connection, tx);
    //         cmd.Parameters.Add("@IdAsistencia", SqlDbType.BigInt);
    //         cmd.Parameters.Add("@Rfc", SqlDbType.VarChar, 50);
    //         cmd.Parameters.Add("@Dia", SqlDbType.DateTime);
            
    //         cmd.Parameters.Add("@Turno", SqlDbType.VarChar, 50);
    //         cmd.Parameters.Add("@Entrada", SqlDbType.VarChar, 50);
    //         cmd.Parameters.Add("@Salida", SqlDbType.VarChar, 50);
    //         cmd.Parameters.Add("@CodigoTurno", SqlDbType.VarChar, 50);
    //         cmd.Parameters.Add("@TurnoNoche", SqlDbType.Bit);
    //         foreach (var a in asistencias)
    //         {
    //             cmd.Parameters["@IdAsistencia"].Value = a.id_asistencia;
    //             cmd.Parameters["@Rfc"].Value = a.rfc;
    //             cmd.Parameters["@Dia"].Value = a.dia;                
    //             cmd.Parameters["@Turno"].Value = a.turno;
    //             cmd.Parameters["@Entrada"].Value = a.entrada;
    //             cmd.Parameters["@Salida"].Value = a.salida;
    //             cmd.Parameters["@CodigoTurno"].Value = a.turno =="-"  &&  a.codigo_turno!="Base-36915"?"D":a.codigo_turno;
    //             cmd.Parameters["@TurnoNoche"].Value = a.turno_noche;
    //             try
    //             {
    //                 await cmd.ExecuteNonQueryAsync();
    //             }
    //             catch (SqlException ex)
    //             {
    //                 Console.WriteLine(ex.Message);
    //                 // Duplicate key (PK/Unique). Ignorar.
    //             }
    //         }
    //         tx.Commit();
    //     }
    //     catch
    //     {
    //         Console.WriteLine("Error durante la inserción de asistencias. Realizando rollback.");
    //         tx.Rollback();
    //         //throw;
    //     }
    // }



    public async Task InsertarJornadasIgnorandoDuplicados(List<JornadaDTO> jornadas)
    {
        
        if (jornadas is null || jornadas.Count == 0)
            return;

        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        using var tx = connection.BeginTransaction();
        const string sql = @"
            INSERT INTO Buk.dbo.Jornada
                (id, rfc, fecha, inicio, fin,turno,jornada,descanso)
            VALUES
                (@Id, @Rfc, @Fecha, @Inicio, @Fin,@Turno,@Jornada,@Descanso);";


        try
        {
            using var cmd = new SqlCommand(sql, connection, tx);
            cmd.Parameters.Add("@Id", SqlDbType.VarChar, 200);
            cmd.Parameters.Add("@Rfc", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Fecha", SqlDbType.DateTime);
            cmd.Parameters.Add("@Jornada", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Inicio", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Fin", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Turno", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Descanso", SqlDbType.VarChar, 50);
            int insertadas = 0, duplicadas = 0, fallidas = 0;
            foreach (var a in jornadas)
            {
                cmd.Parameters["@Id"].Value = a.Id_Jornada;
                cmd.Parameters["@Rfc"].Value = a.RFC;
                cmd.Parameters["@Fecha"].Value = a.Fecha;
                cmd.Parameters["@Jornada"].Value = a.Jornada;
                cmd.Parameters["@Inicio"].Value = (object?)a.Inicio ?? DBNull.Value;
                cmd.Parameters["@Fin"].Value = (object?)a.Fin ?? DBNull.Value;
                cmd.Parameters["@Turno"].Value = a.Turno;
                cmd.Parameters["@Descanso"].Value = a.Descanso;
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    insertadas++;
                }
                catch (SqlException ex) when (EsClaveDuplicada(ex))
                {
                    duplicadas++;
                }
                catch (SqlException ex)
                {
                    fallidas++;
                    Console.WriteLine($"Error al insertar jornada {a.Id_Jornada} (RFC {a.RFC}): {ex.Message}");
                }
            }

            tx.Commit();
            Console.WriteLine($"Jornadas: {insertadas} insertadas, {duplicadas} duplicadas, {fallidas} con error SQL, de {jornadas.Count} recibidas");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la inserción de jornadas, realizando rollback: {ex.Message}");
            RevertirTransaccion(tx);
            //throw;
        }
    }

    public async Task InsertarLoteChecadasIgnorandoDuplicados(List<ChecadaDTO> checadaDTOs)
    {
        Console.WriteLine("Iniciando inserción de checadas en la base de datos...");
        if (checadaDTOs is null || checadaDTOs.Count == 0) return;

        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        using var tx = connection.BeginTransaction();

        const string sql = @"
        INSERT INTO Buk.dbo.Checadas
            (id, rfc, anio, mes,dia,hora, minutos, segundos, tipo, fecha)
        VALUES
            (@Id, @Rfc, @Anio, @Mes, @Dia, @Hora, @Minutos, @Segundos, @Tipo, @Fecha);";



        try
        {
            using var cmd = new SqlCommand(sql, connection, tx);

            cmd.Parameters.Add("@Id", SqlDbType.VarChar, 200);
            cmd.Parameters.Add("@Rfc", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Anio", SqlDbType.Int);
            cmd.Parameters.Add("@Mes", SqlDbType.Int);
            cmd.Parameters.Add("@Dia", SqlDbType.Int);
            cmd.Parameters.Add("@Hora", SqlDbType.Int);
            cmd.Parameters.Add("@Minutos", SqlDbType.Int);
            cmd.Parameters.Add("@Segundos", SqlDbType.Int);
            cmd.Parameters.Add("@Tipo", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Fecha", SqlDbType.DateTime);

            int insertadas = 0, duplicadas = 0, fallidas = 0;
            foreach (var a in checadaDTOs)
            {
                cmd.Parameters["@Id"].Value = a.Id_Checada;
                cmd.Parameters["@Rfc"].Value = a.RFC;
                cmd.Parameters["@Anio"].Value = a.Anio;
                cmd.Parameters["@Mes"].Value = a.Mes;
                cmd.Parameters["@Dia"].Value = a.Dia;
                cmd.Parameters["@Hora"].Value = a.Hora;
                cmd.Parameters["@Minutos"].Value = a.Minuto;
                cmd.Parameters["@Segundos"].Value = a.Segundo;
                cmd.Parameters["@Tipo"].Value = a.Tipo;
                cmd.Parameters["@Fecha"].Value = a.Fecha;

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                    insertadas++;
                }
                catch (SqlException ex) when (EsClaveDuplicada(ex))
                {
                    duplicadas++;
                }
                catch (SqlException ex)
                {
                    fallidas++;
                    Console.WriteLine($"Error al insertar checada {a.Id_Checada} (RFC {a.RFC}): {ex.Message}");
                }
            }

            tx.Commit();
            Console.WriteLine($"Checadas: {insertadas} insertadas, {duplicadas} duplicadas, {fallidas} con error SQL, de {checadaDTOs.Count} recibidas");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la inserción de checadas, realizando rollback: {ex.Message}");
            RevertirTransaccion(tx);
            //throw;
        }





    }





}
