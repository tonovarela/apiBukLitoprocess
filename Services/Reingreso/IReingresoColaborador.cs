using apiBukLitoprocess.DTOs;

namespace apiBukLitoprocess.Services.Reingreso;

public enum EstadoReingreso
{
    /// <summary>No hay personal de baja en Intelisis con ese RFC: sigue el flujo normal de actualización.</summary>
    NoAplica,
    /// <summary>Existe de baja en Intelisis pero en Buk no está activo: no se hace nada.</summary>
    InactivoEnBuk,
    /// <summary>Se registró el movimiento de reingreso y se ligó el personal con el ID de Buk.</summary>
    Realizado
}

public record ResultadoReingreso(EstadoReingreso Estado, string? Personal = null, string? IdMovimiento = null)
{
    public static readonly ResultadoReingreso NoAplica = new(EstadoReingreso.NoAplica);
}

public interface IReingresoColaborador
{
    /// <summary>
    /// Si el colaborador de Buk corresponde (por RFC) a un personal dado de baja en Intelisis
    /// y en Buk está activo, registra el reingreso.
    /// </summary>
    Task<ResultadoReingreso> ProcesarAsync(ColaboradorDTO colaborador);
}
