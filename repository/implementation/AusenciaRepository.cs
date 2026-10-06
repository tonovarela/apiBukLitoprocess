using System.Data;
using System.Globalization;
using apiBukLitoprocess.Data;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.repository.interfaces;
using Microsoft.Data.SqlClient;


namespace apiBukLitoprocess.repository.implementation;

public class AusenciaRepository : IAusenciaRepository
{

    private readonly DbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<AusenciaRepository> _logger;

    private static readonly CultureInfo CulturaMx = new("es-MX");

    public AusenciaRepository(DbConnectionFactory dbConnectionFactory, ILogger<AusenciaRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    // 2601 = indice unico duplicado, 2627 = violacion de PK/unique constraint.
    // Cualquier otro numero es un error real que no debe ignorarse.
    private static bool EsClaveDuplicada(SqlException ex) => ex.Number is 2601 or 2627;

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

    // Devuelve false solo si la hora viene con formato invalido; vacia o nula es un valor legitimo (DBNull).
    private static bool TryParseHora(string? hora, out object valor)
    {
        valor = DBNull.Value;
        if (string.IsNullOrEmpty(hora))
        {
            return true;
        }
        if (!TimeSpan.TryParse(hora, CulturaMx, out var parsed))
        {
            return false;
        }
        valor = parsed;
        return true;
    }

    public async Task RegistrarSolicitudesVacaciones(List<SolicitudDTO> solicitudes)
    {
        if (solicitudes.Count == 0)
        {
            return;
        }

        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        using var tx = connection.BeginTransaction();

        const string sql = @"
        INSERT INTO Buk.dbo.Vacaciones
            (id_solicitud, id_colaborador,personal,dias, fecha_solicitud,fecha_inicio,fecha_fin,fecha_autorizacion,id_autorizo)
        VALUES
            (@IdSolicitud, @IdColaborador, @Personal, @Dias, @FechaSolicitud, @FechaInicio, @FechaFin, @FechaAutorizacion, @IdAutorizo);";

        try
        {

            using var cmd = new SqlCommand(sql, connection, tx);
            cmd.Parameters.Add("@IdSolicitud", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@IdColaborador", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Personal", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Dias", SqlDbType.Float);
            cmd.Parameters.Add("@FechaSolicitud", SqlDbType.DateTime);
            cmd.Parameters.Add("@FechaInicio", SqlDbType.DateTime);
            cmd.Parameters.Add("@FechaFin", SqlDbType.DateTime);
            cmd.Parameters.Add("@FechaAutorizacion", SqlDbType.DateTime);
            cmd.Parameters.Add("@IdAutorizo", SqlDbType.VarChar, 50);
            int insertadas = 0, duplicadas = 0, fallidas = 0;
            foreach (var s in solicitudes)
            {
                cmd.Parameters["@IdSolicitud"].Value = s.id_solicitud;
                cmd.Parameters["@IdColaborador"].Value = s.id_colaborador;
                cmd.Parameters["@Personal"].Value = s.personal;
                cmd.Parameters["@Dias"].Value = s.diasHabiles;
                cmd.Parameters["@FechaSolicitud"].Value = s.fechaSolicitud ?? (object)DBNull.Value;
                cmd.Parameters["@FechaInicio"].Value = s.fechaInicio;
                cmd.Parameters["@FechaFin"].Value = s.fechaFin;
                cmd.Parameters["@FechaAutorizacion"].Value = s.fechaAutorizacion;
                cmd.Parameters["@IdAutorizo"].Value = s.id_autorizo;
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
                    _logger.LogError(ex, "Error al registrar solicitud de vacaciones {IdSolicitud} del colaborador {IdColaborador}", s.id_solicitud, s.id_colaborador);
                }
            }
            tx.Commit();
            _logger.LogInformation("Solicitudes de vacaciones: {Insertadas} insertadas, {Duplicadas} duplicadas, {Fallidas} con error SQL, de {Total} recibidas", insertadas, duplicadas, fallidas, solicitudes.Count);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al preparar comando SQL para registrar solicitudes de vacaciones: {ex.Message}");
            RevertirTransaccion(tx);
        }

    }

    public async Task RegistrarAusencias(List<AusenciaDTO> ausencias, string clasificacion)
    {
        if (ausencias.Count == 0)
        {
            return;
        }

        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        using var tx = connection.BeginTransaction();

        const string sql = @"
        INSERT INTO Buk.dbo.Ausencias
            (id_ausencia, id_colaborador,personal,justificacion, tipo, fecha_inicio,fecha_fin, hora_inicio, hora_fin, clasificacion,dias, dias_percent,goce_sueldo,licencia)
        VALUES
            (@IdAusencia, @IdColaborador, @Personal, @Justificacion, @Tipo, @FechaInicio, @FechaFin, @HoraEntrada, @HoraSalida, @Clasificacion, @Dias, @DiasProporcional, @ConGoceSueldo, @Licencia);";

        try
        {

            using var cmd = new SqlCommand(sql, connection, tx);
            cmd.Parameters.Add("@IdAusencia", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@IdColaborador", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Personal", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Justificacion", SqlDbType.VarChar, 500);
            cmd.Parameters.Add("@Tipo", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@FechaInicio", SqlDbType.DateTime);
            cmd.Parameters.Add("@FechaFin", SqlDbType.DateTime);
            cmd.Parameters.Add("@HoraEntrada", SqlDbType.Time);
            cmd.Parameters.Add("@HoraSalida", SqlDbType.Time);
            cmd.Parameters.Add("@Clasificacion", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Dias", SqlDbType.Float);
            cmd.Parameters.Add("@DiasProporcional", SqlDbType.Float);
            cmd.Parameters.Add("@ConGoceSueldo", SqlDbType.Bit);
            cmd.Parameters.Add("@Licencia", SqlDbType.VarChar, 50);
            int insertadas = 0, duplicadas = 0, invalidas = 0, fallidas = 0;
            foreach (var s in ausencias)
            {
                // El parseo va antes de tocar el comando: una fecha u hora mal formada
                // solo descarta esta ausencia, no aborta el lote completo.
                if (!DateTime.TryParse(s.fecha_inicio, CulturaMx, out var fechaInicio) ||
                    !DateTime.TryParse(s.fecha_fin, CulturaMx, out var fechaFin))
                {
                    invalidas++;
                    _logger.LogWarning("Ausencia {IdAusencia} del colaborador {IdColaborador} omitida: fechas invalidas (inicio='{FechaInicio}', fin='{FechaFin}')", s.id_Ausencia, s.id_colaborador, s.fecha_inicio, s.fecha_fin);
                    continue;
                }
                if (!TryParseHora(s.horaEntrada, out var horaEntrada) || !TryParseHora(s.horaSalida, out var horaSalida))
                {
                    invalidas++;
                    _logger.LogWarning("Ausencia {IdAusencia} del colaborador {IdColaborador} omitida: horas invalidas (entrada='{HoraEntrada}', salida='{HoraSalida}')", s.id_Ausencia, s.id_colaborador, s.horaEntrada, s.horaSalida);
                    continue;
                }

                cmd.Parameters["@IdAusencia"].Value = s.id_Ausencia;
                cmd.Parameters["@IdColaborador"].Value = s.id_colaborador;
                cmd.Parameters["@Personal"].Value = s.personal;
                cmd.Parameters["@Justificacion"].Value = s.justificacion;
                cmd.Parameters["@Tipo"].Value = s.tipo;
                cmd.Parameters["@FechaInicio"].Value = fechaInicio;
                cmd.Parameters["@FechaFin"].Value = fechaFin;
                cmd.Parameters["@Clasificacion"].Value = clasificacion;
                cmd.Parameters["@HoraEntrada"].Value = horaEntrada;
                cmd.Parameters["@HoraSalida"].Value = horaSalida;
                cmd.Parameters["@Dias"].Value = s.dias;
                cmd.Parameters["@DiasProporcional"].Value = s.dias_proporcional;
                cmd.Parameters["@ConGoceSueldo"].Value = s.ConGoceSueldo;
                cmd.Parameters["@Licencia"].Value = s.licencia ?? (object)DBNull.Value;

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
                    _logger.LogError(ex, "Error al registrar ausencia {IdAusencia} del colaborador {IdColaborador}", s.id_Ausencia, s.id_colaborador);
                }
            }
            tx.Commit();
            _logger.LogInformation("Ausencias ({Clasificacion}): {Insertadas} insertadas, {Duplicadas} duplicadas, {Invalidas} invalidas, {Fallidas} con error SQL, de {Total} recibidas", clasificacion, insertadas, duplicadas, invalidas, fallidas, ausencias.Count);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al preparar comando SQL para registrar ausencias: {ex.Message}");
            RevertirTransaccion(tx);
        }
    }

    public async Task RegistrarPermisosPendientes(List<AusenciaDTO> ausencias, string clasificacion)
    {
          if (ausencias.Count == 0)
        {
            return;
        }
            
        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();                
        using var tx = connection.BeginTransaction();
        const string sql = @"
        INSERT INTO Buk.dbo.AusenciasPendientes
            (id_ausencia, id_colaborador,personal,justificacion, tipo, fecha_inicio,fecha_fin, hora_inicio, hora_fin, clasificacion,dias, dias_percent,goce_sueldo)
        VALUES
            (@IdAusencia, @IdColaborador, @Personal, @Justificacion, @Tipo, @FechaInicio, @FechaFin, @HoraEntrada, @HoraSalida, @Clasificacion, @Dias, @DiasProporcional, @ConGoceSueldo);";
        try
        {

            using var cmd = new SqlCommand(sql, connection, tx);
            cmd.Parameters.Add("@IdAusencia", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@IdColaborador", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Personal", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Justificacion", SqlDbType.VarChar, 500);
            cmd.Parameters.Add("@Tipo", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@FechaInicio", SqlDbType.DateTime);
            cmd.Parameters.Add("@FechaFin", SqlDbType.DateTime);
            cmd.Parameters.Add("@HoraEntrada", SqlDbType.Time);
            cmd.Parameters.Add("@HoraSalida", SqlDbType.Time);
            cmd.Parameters.Add("@Clasificacion", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@Dias", SqlDbType.Float);
            cmd.Parameters.Add("@DiasProporcional", SqlDbType.Float);
            cmd.Parameters.Add("@ConGoceSueldo", SqlDbType.Bit);
            int insertadas = 0, duplicadas = 0, invalidas = 0, fallidas = 0;
            foreach (var s in ausencias)
            {
                // El parseo va antes de tocar el comando: una fecha u hora mal formada
                // solo descarta este permiso, no aborta el lote completo.
                if (!DateTime.TryParse(s.fecha_inicio, CulturaMx, out var fechaInicio) ||
                    !DateTime.TryParse(s.fecha_fin, CulturaMx, out var fechaFin))
                {
                    invalidas++;
                    _logger.LogWarning("Permiso pendiente {IdAusencia} del colaborador {IdColaborador} omitido: fechas invalidas (inicio='{FechaInicio}', fin='{FechaFin}')", s.id_Ausencia, s.id_colaborador, s.fecha_inicio, s.fecha_fin);
                    continue;
                }
                if (!TryParseHora(s.horaEntrada, out var horaEntrada) || !TryParseHora(s.horaSalida, out var horaSalida))
                {
                    invalidas++;
                    _logger.LogWarning("Permiso pendiente {IdAusencia} del colaborador {IdColaborador} omitido: horas invalidas (entrada='{HoraEntrada}', salida='{HoraSalida}')", s.id_Ausencia, s.id_colaborador, s.horaEntrada, s.horaSalida);
                    continue;
                }

                cmd.Parameters["@IdAusencia"].Value = s.id_Ausencia;
                cmd.Parameters["@IdColaborador"].Value = s.id_colaborador;
                cmd.Parameters["@Personal"].Value = s.personal;
                cmd.Parameters["@Justificacion"].Value = s.justificacion;
                cmd.Parameters["@Tipo"].Value = s.tipo;
                cmd.Parameters["@FechaInicio"].Value = fechaInicio;
                cmd.Parameters["@FechaFin"].Value = fechaFin;
                cmd.Parameters["@Clasificacion"].Value = clasificacion;
                cmd.Parameters["@HoraEntrada"].Value = horaEntrada;
                cmd.Parameters["@HoraSalida"].Value = horaSalida;
                cmd.Parameters["@Dias"].Value = s.dias;
                cmd.Parameters["@DiasProporcional"].Value = s.dias_proporcional;
                cmd.Parameters["@ConGoceSueldo"].Value = s.ConGoceSueldo;

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
                    _logger.LogError(ex, "Error al registrar permiso pendiente {IdAusencia} del colaborador {IdColaborador}", s.id_Ausencia, s.id_colaborador);
                }
            }
            tx.Commit();
            _logger.LogInformation("Permisos pendientes ({Clasificacion}): {Insertadas} insertados, {Duplicadas} duplicados, {Invalidas} invalidos, {Fallidas} con error SQL, de {Total} recibidos", clasificacion, insertadas, duplicadas, invalidas, fallidas, ausencias.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al preparar comando SQL para registrar permisos pendientes: {ex.Message}");
            RevertirTransaccion(tx);
        }
    }

    public async Task BorrarAusenciasPendientes()
    {
        try
        {
            using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
            var query = "DELETE FROM Buk.dbo.AusenciasPendientes";
            using var command = new SqlCommand(query, connection);
            var filas = await command.ExecuteNonQueryAsync();
            Console.WriteLine($"[DEBUG] BorrarAusenciasPendientes: {filas} filas eliminadas");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al borrar ausencias pendientes: {ex.Message}");
            throw;
        }
    }

    public async Task BorrarVacacionesDesde(DateOnly fecha)
    {
        try
        {
            using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
            var query = "DELETE FROM Buk.dbo.Vacaciones WHERE fecha_inicio >= @Fecha";
            using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@Fecha", SqlDbType.DateTime).Value = fecha.ToDateTime(TimeOnly.MinValue);
            var filas = await command.ExecuteNonQueryAsync();
            Console.WriteLine($"[DEBUG] BorrarVacacionesDesde {fecha:yyyy-MM-dd}: {filas} filas eliminadas");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al borrar vacaciones desde {fecha}: {ex.Message}");
            throw;
        }
    }

    public async Task BorrarAusenciasDesde(DateOnly fecha)
    {
        try
        {
            using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
            var query = "DELETE FROM Buk.dbo.Ausencias WHERE fecha_inicio >= @Fecha";
            using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@Fecha", SqlDbType.DateTime).Value = fecha.ToDateTime(TimeOnly.MinValue);
            var filas = await command.ExecuteNonQueryAsync();
            Console.WriteLine($"[DEBUG] BorrarAusenciasDesde {fecha:yyyy-MM-dd}: {filas} filas eliminadas");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al borrar ausencias desde {fecha}: {ex.Message}");
            throw;
        }
    }
}
