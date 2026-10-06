using System;

namespace apiBukLitoprocess.DTOs;

public record BitacoraDTO
{
    public long IdEmpleado { get; init; }
    public string Evento { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string? Detalle { get; init; }
    

    public static BitacoraDTO Exito(long id, string evento, string? detalle = null)
        => new() { IdEmpleado = id, Evento = evento, Estado = BitacoraEstado.Exito, Detalle = Truncar(detalle) };

    public static BitacoraDTO Error(long id, string evento, string detalle)
        => new() { IdEmpleado = id, Evento = evento, Estado = BitacoraEstado.Error, Detalle = Truncar(detalle) };

    public static BitacoraDTO NoSoportado(long id, string evento)
        => new() { IdEmpleado = id, Evento = evento, Estado = BitacoraEstado.NoSoportado };

    public static BitacoraDTO Omitido(long id, string evento, string detalle)
        => new() { IdEmpleado = id, Evento = evento, Estado = BitacoraEstado.Omitido, Detalle = Truncar(detalle) };

    private static string? Truncar(string? texto, int max = 500)
        => texto?.Length > max ? texto[..max] : texto;
}

public static class BitacoraEstado
{
    public const string Exito = "OK";
    public const string Error = "ERROR";
    public const string NoSoportado = "NO_SOPORTADO";
    public const string Omitido = "OMITIDO";
}
