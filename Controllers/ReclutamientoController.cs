using apiBukLitoprocess.Services;
using Microsoft.AspNetCore.Mvc;

namespace apiBukLitoprocess.Controllers;

[ApiController]
[Route("api/reclutamiento")]
public class ReclutamientoController : ControllerBase
{
    private readonly ReclutamientoService _reclutamientoService;

    public ReclutamientoController(ReclutamientoService reclutamientoService)
    {
        _reclutamientoService = reclutamientoService;
    }

    /// <summary>
    /// Sincroniza las vacantes (procesos de selección) de Buk creadas desde <paramref name="from"/>
    /// (yyyy-MM-dd). Si no se envía, usa ReclutamientoService.FechaDesdePorDefecto.
    /// </summary>
    [HttpGet("vacantes/sync")]
    public async Task<IActionResult> SincronizarVacantes([FromQuery] DateOnly? from)
    {
        try
        {
            var vacantes = await _reclutamientoService.SincronizarVacantes(from ?? ReclutamientoService.FechaDesdePorDefecto);
            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = vacantes
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
}
