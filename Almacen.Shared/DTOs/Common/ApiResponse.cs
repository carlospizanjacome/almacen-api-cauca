namespace Almacen.Shared.DTOs.Common;

/// <summary>
/// Respuesta estándar de la API para errores controlados.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }

    public static ApiResponse Ok(object? data = null, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse Error(string message, object? data = null)
        => new() { Success = false, Data = data, Message = message };
}