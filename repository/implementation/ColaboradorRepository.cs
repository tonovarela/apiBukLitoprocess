using System.Data;
using System.Globalization;
using apiBukLitoprocess.Data;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.helpers;
using apiBukLitoprocess.repository.interfaces;
using Microsoft.Data.SqlClient;


namespace apiBukLitoprocess.repository.implementation;

public class ColaboradorRepository : IColaboradorRepository
{

    private readonly DbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<ColaboradorRepository> _logger;
    private readonly ILogger _sqlLogger;

    private static readonly CultureInfo CulturaMx = new("es-MX");

    public ColaboradorRepository(DbConnectionFactory dbConnectionFactory, ILogger<ColaboradorRepository> logger, ILoggerFactory loggerFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
        _sqlLogger = loggerFactory.CreateLogger("SqlQueries");
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

    public async Task ActualizarCampoExtra(string personal, string campo, string valor)
    {
        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        var query = "Update CtoCampoExtra set Valor= @valor Where Tipo='Personal' and CampoExtra=@campo and clave = @personal";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@valor", valor ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@personal", personal);
        command.Parameters.AddWithValue("@campo", campo);
        _sqlLogger.LogInformation("[SQL ActualizarCampoExtra] {Query}", SqlQueryInterpolator.Interpolar(command));
        await command.ExecuteNonQueryAsync();
        Console.WriteLine($"[DEBUG] CtoCampoExtra: personal={personal}, campo={campo}, valor={valor}");
    }


    public async Task Actualizar(ColaboradorDTO colaborador)
    {


        string? reportaA = await BuscarPersonalPorRFC(colaborador.RFC);        
        string  departamento = await ObtenerDepartamento(colaborador.CentroCostos ?? "");
        await ActualizarCampoExtra(colaborador.IdColaborador, "MailLitoprocess", colaborador.Correo_Corporativo ?? "");
        Console.WriteLine($"[DEBUG] Actualizar: personal={colaborador.IdColaborador},  banco={colaborador.Banco}, reportaA={reportaA}, departamento={departamento}");
        try
        {

            using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
            {
                var query = @"UPDATE dbo.Personal set
                                ApellidoMaterno=@ApellidoMaterno,
                                ApellidoPaterno=@ApellidoPaterno,
                                Beneficiario = @Beneficiario1,
                                Beneficiario2 = @Beneficiario2,
                                Beneficiario2Nacimiento = @BeneficiarioNacimiento2,
                                Beneficiario3 = @Beneficiario3,
                                Beneficiario3Nacimiento = @BeneficiarioNacimiento3,
                                BeneficiarioNacimiento = @BeneficiarioNacimiento1,
                                CentroCostos = @CentroCostos,
                                CodigoPostal=@CodigoPostal,
                                Colonia=@Colonia,
                                CtaDinero=@CtaDinero,
                                Delegacion=@Delegacion,
                                Departamento = @Departamento,
                                DiasPeriodo=@DiasPeriodo,
                                Direccion=@Direccion,
                                DireccionNumero = @NumExt,
                                DireccionNumeroInt = @NumInt,
                                email=@CorreoPersonal,
                                Empresa=@Empresa,
                                Estado=@Estado,
                                EstadoCivil=@EstadoCivil,
                                Jornada=@FactorJornada,
                                FechaAlta = @FechaAlta,
                                FechaNacimiento=@FechaNacimiento,
                                FormaPago=@FormaPago,
                                Hijos = @NumeroHijos,
                                Moneda=@Moneda,
                                MovNomina=@MovNomina,
                                Nacionalidad=@Nacionalidad,
                                NivelAcademico=@NivelAcademico,
                                Nombre=@Nombre,
                                Pais=@Pais,
                                Parentesco = @ParentescoBeneficiario1,
                                Parentesco2 = @ParentescoBeneficiario2,
                                Parentesco3 = @ParentescoBeneficiario3,
                                PeriodoTipo=@PeriodoTipo,
                                PersonalSucursal=@PersonalSucursal,
                                PersonalCuenta=@PersonalCuenta,
                                Poblacion=@Poblacion,
                                Porcentaje = @PorcentajeBeneficiario1,
                                Porcentaje2 = @PorcentajeBeneficiario2,
                                Porcentaje3 = @PorcentajeBeneficiario3,
                                Puesto = @Puesto,
                                Registro=@Curp,
                                Registro2=@Rfc,
                                Registro3=@NSS,
                                ReportaA=@ReportaA,
                                Sexo=@Sexo,
                                Sindicato = @Sindicato,
                                SucursalTrabajo=@SucursalTrabajo,
                                SueldoDiario=@SalarioDiario,
                                Telefono=@Telefono,
                                TipoContrato = @TipoContrato,
                                TipoSueldo=@TipoSueldo,
                                usuario=@Id,
                                ZonaEconomica=@ZonaEconomica,
                                Categoria=@Categoria,
                                FechaAntiguedad=@FechaAntiguedad,
                                LugarNacimiento=@LugarNacimiento
                                where personal=@personal";
                using var command = new SqlCommand(query, connection);

                command.CommandTimeout = 300;
                command.Parameters.AddWithValue("@CtaDinero", "PAGOS7631");
                command.Parameters.AddWithValue("@DiasPeriodo", "Dias Periodo");
                command.Parameters.AddWithValue("@Empresa", "LITO");
                command.Parameters.AddWithValue("@FormaPago", "Nomina Transferencia Electronica");
                command.Parameters.AddWithValue("@Moneda", "Pesos");
                command.Parameters.AddWithValue("@MovNomina", "Nomina Lito");
                command.Parameters.AddWithValue("@Nacionalidad", "Mexicana");
                command.Parameters.AddWithValue("@SucursalTrabajo", 0);
                command.Parameters.AddWithValue("@TipoSueldo", "Variable");
                command.Parameters.AddWithValue("@ZonaEconomica", "A");
                command.Parameters.AddWithValue("@Id", colaborador.id);
                command.Parameters.AddWithValue("@ApellidoPaterno", colaborador.ApellidoPaterno);
                command.Parameters.AddWithValue("@ApellidoMaterno", colaborador.ApellidoMaterno);
                command.Parameters.AddWithValue("@Nombre", colaborador.Nombre);
                command.Parameters.AddWithValue("@personal", colaborador.IdColaborador);
                command.Parameters.AddWithValue("@Curp", colaborador.CURP);
                command.Parameters.AddWithValue("@Rfc", colaborador.RFC);
                command.Parameters.AddWithValue("@CorreoPersonal", colaborador.Correo_Personal);
                command.Parameters.AddWithValue("@NSS", colaborador.NSS);
                command.Parameters.AddWithValue("@Direccion", colaborador.Direccion);
                command.Parameters.AddWithValue("@Colonia", colaborador.Colonia);
                command.Parameters.AddWithValue("@Delegacion", colaborador.Delegacion);
                command.Parameters.AddWithValue("@Poblacion", colaborador.Poblacion);
                command.Parameters.AddWithValue("@Estado", colaborador.Estado);
                command.Parameters.AddWithValue("@Pais", colaborador.Pais);
                command.Parameters.AddWithValue("@CodigoPostal", colaborador.CodigoPostal);
                command.Parameters.AddWithValue("@Telefono", colaborador.Telefono);
                command.Parameters.AddWithValue("@FechaNacimiento", colaborador.FechaNacimiento);
                command.Parameters.AddWithValue("@EstadoCivil", colaborador.EstadoCivil);
                command.Parameters.AddWithValue("@NivelAcademico", colaborador.NivelAcademico ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Sexo", colaborador.Sexo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Beneficiario1", colaborador.Beneficiario1 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@BeneficiarioNacimiento1", colaborador.FechaNacimientoBeneficiario1 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ParentescoBeneficiario1", colaborador.ParentescoBeneficiario1 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PorcentajeBeneficiario1", colaborador.PorcentajeBeneficiario1 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PersonalSucursal", colaborador.Banco ?? (object)DBNull.Value);    
                command.Parameters.AddWithValue("@PersonalCuenta", colaborador.PersonalCuenta ?? (object)DBNull.Value);

                command.Parameters.AddWithValue("@Beneficiario2", colaborador.Beneficiario2 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@BeneficiarioNacimiento2", colaborador.FechaNacimientoBeneficiario2 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ParentescoBeneficiario2", colaborador.ParentescoBeneficiario2 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PorcentajeBeneficiario2", colaborador.PorcentajeBeneficiario2 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Beneficiario3", colaborador.Beneficiario3 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@BeneficiarioNacimiento3", colaborador.FechaNacimientoBeneficiario3 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ParentescoBeneficiario3", colaborador.ParentescoBeneficiario3 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PorcentajeBeneficiario3", colaborador.PorcentajeBeneficiario3 ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NumExt", colaborador.NumExt ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NumInt", colaborador.NumInt ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CentroCostos", colaborador.CentroCostos ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Puesto", colaborador.Puesto ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Departamento", departamento ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FechaAlta", colaborador.FechaAlta ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SalarioDiario", colaborador.SalarioDiario);
                command.Parameters.AddWithValue("@NumeroHijos", colaborador.NumeroHijos ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ReportaA", reportaA ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PeriodoTipo", colaborador.PeriodoTipo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FactorJornada", colaborador.FactorJornada ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TipoContrato", colaborador.TipoContrato ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Sindicato", colaborador.Sindicato ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Categoria", colaborador.Categoria ?? (object)DBNull.Value);
                
                command.Parameters.AddWithValue("@FechaAntiguedad", colaborador.FechaAntiguedad ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LugarNacimiento", colaborador.LugarNacimiento ?? (object)DBNull.Value);

                _sqlLogger.LogInformation("[SQL Actualizar] {Query}", SqlQueryInterpolator.Interpolar(command));

                await command.ExecuteNonQueryAsync();

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar colaborador con id {IdColaborador}", colaborador.IdColaborador);
            throw;
        }
    }

    public async Task Actualizar(long id, string idColaborador)
    {
        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        var query = "UPDATE dbo.Personal set usuario=@Id where personal=@personal";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@personal", idColaborador);
        await command.ExecuteNonQueryAsync();
    }


    public async Task<string?> BuscarPersonalPorRFC(string rfc)
    {
        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        var query = "SELECT personal FROM dbo.Personal where Registro2=@rfc";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@rfc", rfc);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return reader["personal"].ToString();
        }
        return null;
    }


    public async Task<string> ObtenerDepartamento(string centro_costos)
    {
        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        var query = @"
                    select
                    Descripcion as Departamento from centrocostos
                    where estatus = 'alta'
                    and centrocostos=@centro_costos";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@centro_costos", centro_costos);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return reader["Departamento"].ToString() ?? String.Empty;
        }
        return String.Empty;
    }

    public async Task InsertarBitacora(BitacoraDTO bitacoraDTO)
    {
        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        var query = "INSERT INTO Buk.dbo.BitacoraPersonal (id_colaborador_buk, evento,estado,detalle) VALUES (@IdColaborador, @Evento, @Estado, @Detalle)";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdColaborador", bitacoraDTO.IdEmpleado);
        command.Parameters.AddWithValue("@Evento", bitacoraDTO.Evento);
        command.Parameters.AddWithValue("@Estado", bitacoraDTO.Estado ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Detalle", bitacoraDTO.Detalle ?? (object)DBNull.Value);
        await command.ExecuteNonQueryAsync();
    }


    public async Task<int> ObtenerSiguienteClavePersonal()
    {

        string sql = @"SELECT
                   MAX(cast(Personal as int)) + 1 siguiente
                   FROM dbo.Personal
                   WHERE Tipo<>'Becario'
                   AND cast(Personal as int) < 9000";
        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task RegistrarBaja(string idPersonalBuk, string conceptoBaja, string fechaBaja)
    {

        try
        {
            using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
            const string sql = @"
                            UPDATE dbo.Personal SET Estatus='BAJA',
                                                   FechaBaja=@FechaBaja,
                                                   ConceptoBaja=@ConceptoBaja
                                                   WHERE Usuario=@idPersonalBuk
                                                   ";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@idPersonalBuk", idPersonalBuk);
            command.Parameters.AddWithValue("@ConceptoBaja", conceptoBaja);
            command.Parameters.AddWithValue("@FechaBaja", fechaBaja);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error SQL al registrar colaborador {idPersonalBuk}: {ex.Message}");
            throw;
        }
    }



    public async Task Insertar(ColaboradorDTO colaborador, int nuevoIdColaborador)
    {
        string? reportaA = await BuscarPersonalPorRFC(colaborador.RFC);
        string departamento = await ObtenerDepartamento(colaborador.CentroCostos ?? "");

        try
        {
            using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();

            var query = @"
            INSERT INTO dbo.Personal
            (
                ApellidoMaterno,
                ApellidoPaterno,
                Beneficiario,
                Beneficiario2,
                Beneficiario2Nacimiento,
                Beneficiario3,
                Beneficiario3Nacimiento,
                BeneficiarioNacimiento,
                Categoria,
                CentroCostos,
                CodigoPostal,
                Colonia,
                CtaDinero,
                Delegacion,
                Departamento,
                DiasPeriodo,
                Direccion,
                DireccionNumero,
                DireccionNumeroInt,
                email,
                Empresa,
                Estado,
                EstadoCivil,
                Estatus,
                Jornada,
                FechaAlta,
                FechaNacimiento,
                FormaPago,
                Hijos,
                Moneda,
                MovNomina,
                Nacionalidad,
                NivelAcademico,
                Nombre,
                Pais,
                Parentesco,
                Parentesco2,
                Parentesco3,
                PeriodoTipo,
                Personal,
                PersonalCuenta,
                PersonalSucursal,
                Poblacion,
                Porcentaje,
                Porcentaje2,
                Porcentaje3,
                Puesto,
                Registro,
                Registro2,
                Registro3,
                reportaA,
                Sexo,
                Sindicato,
                SucursalTrabajo,
                SueldoDiario,
                Telefono,
                Tipo,
                TipoContrato,
                TipoSueldo,
                Usuario,
                ZonaEconomica,
                FechaAntiguedad,
                LugarNacimiento
            )
            VALUES
            (
                @ApellidoMaterno,
                @ApellidoPaterno,
                @Beneficiario1,
                @Beneficiario2,
                @BeneficiarioNacimiento2,
                @Beneficiario3,
                @BeneficiarioNacimiento3,
                @BeneficiarioNacimiento1,
                @Categoria,
                @CentroCostos,
                @CodigoPostal,
                @Colonia,
                @CtaDinero,
                @Delegacion,
                @Departamento,
                @DiasPeriodo,
                @Direccion,
                @DireccionNumero,
                @DireccionNumeroInt,
                @Email,
                @Empresa,
                @Estado,
                @EstadoCivil,
                @Estatus,
                @FactorJornada,
                @FechaAlta,
                @FechaNacimiento,
                @FormaPago,
                @NumeroHijos,
                @Moneda,
                @MovNomina,
                @Nacionalidad,
                @NivelAcademico,
                @Nombre,
                @Pais,
                @ParentescoBeneficiario1,
                @ParentescoBeneficiario2,
                @ParentescoBeneficiario3,
                @PeriodoTipo,
                @Personal,
                @PersonalCuenta,
                @PersonalSucursal,
                @Poblacion,
                @PorcentajeBeneficiario1,
                @PorcentajeBeneficiario2,
                @PorcentajeBeneficiario3,
                @Puesto,
                @Registro,
                @Registro2,
                @Registro3,
                @ReportaA,
                @Sexo,
                @Sindicato,
                @SucursalTrabajo,
                @SalarioDiario,
                @Telefono,
                @Tipo,
                @TipoContrato,
                @TipoSueldo,
                @Usuario,
                @ZonaEconomica,
                @FechaAntiguedad,
                @LugarNacimiento
            );";

            using var command = new SqlCommand(query, connection);
            command.CommandTimeout = 300;

            command.Parameters.AddWithValue("@CtaDinero", "PAGOS7631");
            command.Parameters.AddWithValue("@DiasPeriodo", "Dias Periodo");
            command.Parameters.AddWithValue("@Empresa", "LITO");
            command.Parameters.AddWithValue("@Estatus", "ALTA");
            command.Parameters.AddWithValue("@FormaPago", "Nomina Transferencia Electronica");
            command.Parameters.AddWithValue("@Moneda", "Pesos");
            command.Parameters.AddWithValue("@MovNomina", "Nomina Lito");
            command.Parameters.AddWithValue("@Nacionalidad", "Mexicana");
            command.Parameters.AddWithValue("@SucursalTrabajo", 0);
            command.Parameters.AddWithValue("@Tipo", "Empleado");
            command.Parameters.AddWithValue("@TipoSueldo", "Variable");
            command.Parameters.AddWithValue("@ZonaEconomica", "A");
            command.Parameters.AddWithValue("@Personal", nuevoIdColaborador);
            command.Parameters.AddWithValue("@Usuario", colaborador.id ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Nombre", colaborador.Nombre ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ApellidoPaterno", colaborador.ApellidoPaterno ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ApellidoMaterno", colaborador.ApellidoMaterno ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SalarioDiario", colaborador.SalarioDiario);
            command.Parameters.AddWithValue("@Registro", colaborador.CURP ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Registro2", colaborador.RFC ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", colaborador.Correo_Personal ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Registro3", colaborador.NSS ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Direccion", colaborador.Direccion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Colonia", colaborador.Colonia ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Delegacion", colaborador.Delegacion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Poblacion", colaborador.Poblacion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Estado", colaborador.Estado ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Pais", colaborador.Pais ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CodigoPostal", colaborador.CodigoPostal ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FactorJornada", colaborador.FactorJornada ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Telefono", colaborador.Telefono ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FechaNacimiento", colaborador.FechaNacimiento ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@NivelAcademico", colaborador.NivelAcademico ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Sexo", colaborador.Sexo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EstadoCivil", colaborador.EstadoCivil ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ReportaA", reportaA ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Beneficiario1", colaborador.Beneficiario1 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@BeneficiarioNacimiento1", colaborador.FechaNacimientoBeneficiario1 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ParentescoBeneficiario1", colaborador.ParentescoBeneficiario1 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PorcentajeBeneficiario1", colaborador.PorcentajeBeneficiario1 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Beneficiario2", colaborador.Beneficiario2 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@BeneficiarioNacimiento2", colaborador.FechaNacimientoBeneficiario2 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ParentescoBeneficiario2", colaborador.ParentescoBeneficiario2 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PorcentajeBeneficiario2", colaborador.PorcentajeBeneficiario2 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Beneficiario3", colaborador.Beneficiario3 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@BeneficiarioNacimiento3", colaborador.FechaNacimientoBeneficiario3 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ParentescoBeneficiario3", colaborador.ParentescoBeneficiario3 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PorcentajeBeneficiario3", colaborador.PorcentajeBeneficiario3 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CentroCostos", colaborador.CentroCostos ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Puesto", colaborador.Puesto ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TipoContrato", colaborador.TipoContrato ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Sindicato", colaborador.Sindicato ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FechaAlta", colaborador.FechaAlta ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Departamento", departamento ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DireccionNumero", colaborador.NumExt ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DireccionNumeroInt", colaborador.NumInt ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@NumeroHijos", colaborador.NumeroHijos ?? (object)DBNull.Value);            
            command.Parameters.AddWithValue("@PeriodoTipo", colaborador.PeriodoTipo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Categoria", colaborador.Categoria ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PersonalSucursal", colaborador.Banco ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PersonalCuenta", colaborador.PersonalCuenta ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FechaAntiguedad", colaborador.FechaAntiguedad ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LugarNacimiento", colaborador.LugarNacimiento ?? (object)DBNull.Value);

            _sqlLogger.LogInformation("[SQL Insertar] {Query}", SqlQueryInterpolator.Interpolar(command));

            await command.ExecuteNonQueryAsync();

            await ActualizarCampoExtra(nuevoIdColaborador.ToString(), "MailLitoprocess", colaborador.Correo_Corporativo ?? "");
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error SQL al registrar colaborador {NuevoIdColaborador}", nuevoIdColaborador);
            throw;
        }

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

    public async Task<bool> ExisteColaborador(string id)
    {
        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        var query = "SELECT COUNT(*) FROM dbo.Personal where usuario=@id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
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
}
