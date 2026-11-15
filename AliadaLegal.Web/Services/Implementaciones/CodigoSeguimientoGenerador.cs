using System.Security.Cryptography;

namespace AliadaLegal.Web.Services;

public class CodigoSeguimientoGenerador : ICodigoSeguimientoGenerador
{
    // Sin 0/O ni 1/I para evitar errores al dictar el código por teléfono o WhatsApp.
    private const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int LongitudSufijo = 6;

    private readonly TimeProvider _reloj;

    public CodigoSeguimientoGenerador(TimeProvider reloj)
    {
        _reloj = reloj;
    }

    public string Generar()
    {
        var letras = new char[LongitudSufijo];
        for (var i = 0; i < letras.Length; i++)
            letras[i] = Alfabeto[RandomNumberGenerator.GetInt32(Alfabeto.Length)];

        return $"AL-{_reloj.GetUtcNow().Year}-{new string(letras)}";
    }
}
