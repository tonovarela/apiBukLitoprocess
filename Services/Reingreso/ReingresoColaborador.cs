using apiBukLitoprocess.DTOs;
using apiBukLitoprocess.repository.interfaces;

namespace apiBukLitoprocess.Services.Reingreso;

public class ReingresoColaborador : IReingresoColaborador
{
    private const string EstatusActivoBuk = "activo";

    private readonly IColaboradorRepository _colaboradorRepository;
    private readonly IHistorialRepository _historialRepository;

    public ReingresoColaborador(IColaboradorRepository colaboradorRepository, IHistorialRepository historialRepository)
    {
        _colaboradorRepository = colaboradorRepository;
        _historialRepository = historialRepository;
    }

    public async Task<ResultadoReingreso> ProcesarAsync(ColaboradorDTO colaborador)
    {
        var personalBaja = await _colaboradorRepository.ObtenerPorRFC(colaborador.RFC, false);
        if (personalBaja is null)
            return ResultadoReingreso.NoAplica;

        if (!string.Equals(colaborador.Estatus, EstatusActivoBuk, StringComparison.OrdinalIgnoreCase))
            return new ResultadoReingreso(EstadoReingreso.InactivoEnBuk, personalBaja.Personal);

        long idBuk = colaborador.id
            ?? throw new InvalidOperationException($"El colaborador con RFC {colaborador.RFC} no trae ID de Buk; no se puede registrar el reingreso.");

        string idMovimiento = await _historialRepository.InsertarReingreso(personalBaja.Personal);
        await _colaboradorRepository.Actualizar(idBuk, personalBaja.Personal);

        return new ResultadoReingreso(EstadoReingreso.Realizado, personalBaja.Personal, idMovimiento);
    }
}
