namespace AliadaLegal.Web.Services;

/// <summary>Resultado de una operación de negocio: éxito, o un mensaje de error apto para mostrar al usuario.</summary>
public class ResultadoOperacion
{
    public bool Exito { get; set; }
    public string? Error { get; set; }

    public static ResultadoOperacion Ok() => new() { Exito = true };
    public static ResultadoOperacion Fallo(string error) => new() { Exito = false, Error = error };
}

public class ResultadoOperacion<T>
{
    public bool Exito { get; set; }
    public string? Error { get; set; }
    public T? Valor { get; set; }

    public static ResultadoOperacion<T> Ok(T valor) => new() { Exito = true, Valor = valor };
    public static ResultadoOperacion<T> Fallo(string error) => new() { Exito = false, Error = error };
}
