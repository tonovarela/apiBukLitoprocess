
using apiBukLitoprocess.Clases;
using Microsoft.AspNetCore.Mvc;
using apiBukLitoprocess.Services;
using Microsoft.Identity.Client;

namespace apiBukLitoprocess.Controllers;

[ApiController]
[Route("api/colaborador")]
public class ColaboradorController : ControllerBase
{
    private readonly ColaboradorService _colaboradorService;
    private readonly AsistenciaService _asistenciaService;
    public ColaboradorController(ColaboradorService colaboradorService, AsistenciaService asistenciaService)
    {
        _colaboradorService = colaboradorService;
        _asistenciaService = asistenciaService;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> webhook(WebhookPayload payload)
    {

        Console.WriteLine($"Received webhook event:for Employee ID: {payload.Data.EmployeeId}");
        var result = await _colaboradorService.handleEventWebhook(payload.Data);
        if (result.IsError)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                statusCode = result.StatusCode,
                message = result.ErrorMessage
            });
        }
        return Ok(new
        {
            success = true,
            statusCode = 200,
            data = result.colaborador
        });
    }

    [HttpGet("sync")]
    public async Task<IActionResult> sync()
    {
        try
        {
            var colaboradores = await _colaboradorService.Sincronizar();
            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = colaboradores
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetBaseException().Message);
            return StatusCode(500, new
            {
                success = false,
                statusCode = 500,
                message = "Internal server error"
            });
        }

    }


    [HttpGet("ausencias")]
    public async Task<IActionResult> GetAusencias([FromQuery] int diasAtras = 1)
    {
        if (diasAtras <= 0)
        {
            diasAtras = 1;
        }
        diasAtras = diasAtras * -1;
        try
        {
            var permisos = await _colaboradorService.ObtenerPermisos(diasAtras);
            var ausencias = await _colaboradorService.ObtenerAusencias(diasAtras);
            var incapacidades = await _colaboradorService.ObtenerIncapacidades(diasAtras);
            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = new
                {
                    permisos,
                    ausencias,
                    incapacidades
                }
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetBaseException().Message);
            return StatusCode(500, new
            {
                success = false,
                statusCode = 500,
                message = "Internal server error"
            });
        }
    }

    [HttpGet("vacaciones")]
    public async Task<IActionResult> GetVacaciones()
    {
        try
        {
            var vacaciones = await _colaboradorService.ObtenerSolicitudesVacaciones();
            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = vacaciones
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetBaseException().Message);
            return StatusCode(500, new
            {
                success = false,
                statusCode = 500,
                message = "Internal server error"
            });
        }
    }

    [HttpGet("checadas")]
    public async Task<IActionResult> GetChecadas([FromQuery] int diasAtras = 1)
    {
        if (diasAtras <= 0)
        {
            diasAtras = -1;
        }

        try
        {
            var checadas = await _asistenciaService.RegistroChecadas(DateOnly.FromDateTime(DateTime.Now.AddDays(diasAtras * (-1))));
            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = checadas
            });
        }
        catch (Exception e)
        {

            Console.WriteLine(e.GetBaseException().Message);

            return StatusCode(500, new
            {
                success = false,
                statusCode = 500,
                message = "Internal server error"
            });
        }
    }


    [HttpGet("jornadas")]
    public async Task<IActionResult> GetJornadas([FromQuery] int diasAtras = 10)
    {
        if (diasAtras <= 0)
        {
            diasAtras = 10;
        }
     DateOnly desde = DateOnly.FromDateTime(DateTime.Now.AddDays(diasAtras * (-1)));
     DateOnly hasta = DateOnly.FromDateTime(DateTime.Now.AddDays(diasAtras));
     DateOnly fechaEliminacion = DateOnly.FromDateTime(DateTime.Now.AddDays((diasAtras-1) * (-1)));
        try
        {
            await _asistenciaService.EliminarJornadasDesde(fechaEliminacion);
            var jornadas = await _asistenciaService.registroJornada(desde, hasta);
            return Ok(new
            {
                fecha = new { inicio = desde, fin = hasta , fechaEliminacion},
                total=jornadas.Count,
                data = jornadas
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetBaseException().Message);
            return StatusCode(500, new
            {
                success = false,
                statusCode = 500,
                message = "Internal server error"
            });
        }
    }

    
    // [HttpGet("jornadas")]
    // public async Task<IActionResult> GetAsistencias([FromQuery] int diasAtras = 20)
    // {
    //     if (diasAtras <= 0)
    //     {
    //         diasAtras = 20;
    //     }
    //  DateOnly desde = DateOnly.FromDateTime(DateTime.Now.AddDays(diasAtras * (-1)));
    //  DateOnly fechaEliminacion = DateOnly.FromDateTime(DateTime.Now.AddDays(15 * (-1)));
    //     try
    //     {
    //         await _asistenciaService.EliminarJornadasDesde(fechaEliminacion);
    //         var asistencias = await _asistenciaService.ObtenerAsistencias(desde);
    //         return Ok(new
    //         {
    //             fecha = new { inicio =desde,fin = DateOnly.FromDateTime(DateTime.Now) },
    //             total=asistencias.Count,
    //             data = asistencias
    //         });
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e.GetBaseException().Message);
    //         return StatusCode(500, new
    //         {
    //             success = false,
    //             statusCode = 500,
    //             message = "Internal server error"
    //         });
    //     }
    // }

}
