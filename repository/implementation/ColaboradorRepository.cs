using apiBukLitoprocess.Data;
using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.helpers;
using apiBukLitoprocess.Models;
using apiBukLitoprocess.repository.interfaces;
using System.Data;
using Microsoft.Data.SqlClient;


namespace apiBukLitoprocess.repository.implementation;

public class ColaboradorRepository : IColaboradorRepository
{

    private readonly DbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<ColaboradorRepository> _logger;
    private readonly ILogger _sqlLogger;

    public ColaboradorRepository(DbConnectionFactory dbConnectionFactory, ILogger<ColaboradorRepository> logger, ILoggerFactory loggerFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
        _sqlLogger = loggerFactory.CreateLogger("SqlQueries");
    }

    private async Task ActualizarCampoExtra(string personal, string campo, string valor)
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

    private async Task<string> ObtenerDepartamento(string centro_costos)
    {
        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        // string query = @"
        //             select
        //             Descripcion as Departamento from centrocostos
        //             where estatus = 'alta'
        //             and centrocostos=@centro_costos";
        string query = @"
                    Select 
                    Departamento= Valor
                    From TablaStD
                   where TablaSt='CCDepto'
                   and Nombre=@centro_costos
                   ";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@centro_costos", centro_costos);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return reader["Departamento"].ToString() ?? String.Empty;
        }
        return String.Empty;
    }

    public async Task Actualizar(ColaboradorDTO colaborador)
    {

        string? reportaA = colaborador.ReportaA;
        string departamento = await ObtenerDepartamento(colaborador.CentroCostos ?? "");
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
                                ZonaEconomica=@ZonaEconomica,
                                Categoria=@Categoria,
                                FechaAntiguedad=@FechaAntiguedad,
                                LugarNacimiento=@LugarNacimiento
                                where usuario=@Id";
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
                //command.Parameters.AddWithValue("@personal", colaborador.IdColaborador);
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

    public async Task<int> ObtenerSiguienteClavePersonal(bool esBecario)
    {

        string sql;

        if (esBecario)
        {
            sql = @"SELECT
                   MAX(cast(Personal as int)) + 1 siguiente
                   FROM dbo.Personal
                   WHERE Tipo='Becario'
                   AND cast(Personal as int) > 9000";
        }
        else
        {
            sql = @"SELECT
                   MAX(cast(Personal as int)) + 1 siguiente
                   FROM dbo.Personal
                   WHERE Tipo<>'Becario'
                   AND cast(Personal as int) < 9000";

        }

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
        string? reportaA = colaborador.ReportaA;        
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
            command.Parameters.AddWithValue("@Estatus", "ASPIRANTE");
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

    public async Task<bool> ExisteColaborador(string id)
    {
        using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        var query = "SELECT COUNT(*) FROM dbo.Personal where usuario=@id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    private const string SelectColaborador = @"
            SELECT TOP (1)
                Personal,
                SueldoDiario,
                ReportaA,
                Puesto,
                Departamento,
                CentroCostos,
                Jornada
            FROM dbo.Personal";

    // public Task<Colaborador?> Obtener(string personal)
    //     => ObtenerPor("Personal = @valor", SqlDbType.Char, personal);

    public Task<Colaborador?> ObtenerPorUsuario(string idBuk)
        => ObtenerPor("Usuario = @valor", SqlDbType.VarChar, idBuk);

    private async Task<Colaborador?> ObtenerPor(string filtro, SqlDbType tipo, string valor)
    {
        await using var connection = (SqlConnection)_dbConnectionFactory.CreateConnection();
        await using var command = new SqlCommand($"{SelectColaborador} WHERE {filtro}", connection);
        command.Parameters.Add("@valor", tipo, 10).Value = valor;
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }

        string Texto(int ordinal) => reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal).TrimEnd();

        return new Colaborador
        {
            Personal = Texto(0),
            SueldoDiario = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
            ReportaA = Texto(2),
            Puesto = Texto(3),
            Departamento = Texto(4),
            CentroCostos = Texto(5),
            Jornada = Texto(6)
        };
    }
}

