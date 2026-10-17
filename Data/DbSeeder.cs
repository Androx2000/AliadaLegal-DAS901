using AliadaLegal.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Data;

/// <summary>
/// Crea la base de datos (si no existe) y carga datos de demostración.
/// Solo inserta cuando las tablas están vacías, por lo que es seguro ejecutarlo en cada arranque.
/// </summary>
public static class DbSeeder
{
    public static async Task InicializarAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();

        // Para el prototipo se crea el esquema directamente.
        // Si la cátedra exige migraciones: dotnet ef migrations add Inicial && dotnet ef database update
        await db.Database.EnsureCreatedAsync();

        if (await db.Roles.AnyAsync()) return;

        // ---- Roles ----
        var admin = new Rol { NombreRol = Roles.Administrador };
        var asesor = new Rol { NombreRol = Roles.Asesor };
        var rolAliado = new Rol { NombreRol = Roles.Aliado };
        db.Roles.AddRange(admin, asesor, rolAliado);

        // ---- Países ----
        var sv = new Pais { Nombre = "El Salvador", CodigoIso = "SLV" };
        var gt = new Pais { Nombre = "Guatemala", CodigoIso = "GTM" };
        var hn = new Pais { Nombre = "Honduras", CodigoIso = "HND" };
        var ni = new Pais { Nombre = "Nicaragua", CodigoIso = "NIC" };
        var mx = new Pais { Nombre = "México", CodigoIso = "MEX" };
        var co = new Pais { Nombre = "Colombia", CodigoIso = "COL" };
        var us = new Pais { Nombre = "Estados Unidos", CodigoIso = "USA" };
        db.Paises.AddRange(sv, gt, hn, ni, mx, co, us);

        // ---- Tipos de documento ----
        var partida = new TipoDocumento
        {
            Nombre = "Partida de nacimiento",
            Descripcion = "Certificación del registro civil del país de origen.",
            RequiereApostilla = true
        };
        var antecedentes = new TipoDocumento
        {
            Nombre = "Antecedentes penales",
            Descripcion = "Constancia de antecedentes emitida por la autoridad competente.",
            RequiereApostilla = true
        };
        var acta = new TipoDocumento
        {
            Nombre = "Acta notarial",
            Descripcion = "Documento notarial para trámites de residencia o visa.",
            RequiereApostilla = true
        };
        var traduccion = new TipoDocumento
        {
            Nombre = "Traducción certificada",
            Descripcion = "Traducción oficial de un documento ya existente.",
            RequiereApostilla = false
        };
        db.TiposDocumento.AddRange(partida, antecedentes, acta, traduccion);

        // ---- Aliados ----
        var aliadoSv = new Aliado
        {
            Nombre = "Lic. Carolina Méndez", Correo = "carolina.mendez@bufete-sv.example",
            Telefono = "+503 7000 1111", TipoAliado = TipoAliado.Abogado, PaisAsignado = sv
        };
        var aliadoGt = new Aliado
        {
            Nombre = "Notario Rodrigo Pineda", Correo = "rodrigo.pineda@notaria-gt.example",
            Telefono = "+502 5555 2222", TipoAliado = TipoAliado.Notario, PaisAsignado = gt
        };
        var aliadoHn = new Aliado
        {
            Nombre = "Lic. Daniela Zelaya", Correo = "daniela.zelaya@bufete-hn.example",
            Telefono = "+504 9999 3333", TipoAliado = TipoAliado.Abogado, PaisAsignado = hn
        };
        var aliadoMx = new Aliado
        {
            Nombre = "Notario Esteban Rojas", Correo = "esteban.rojas@notaria-mx.example",
            Telefono = "+52 55 1234 4444", TipoAliado = TipoAliado.Notario, PaisAsignado = mx
        };
        db.Aliados.AddRange(aliadoSv, aliadoGt, aliadoHn, aliadoMx);

        // ---- Usuarios de demostración (contraseña: Demo1234!) ----
        Usuario NuevoUsuario(string nombre, string correo, Rol rol, Aliado? aliado = null)
        {
            var u = new Usuario { Nombre = nombre, Correo = correo, Rol = rol, Aliado = aliado };
            u.PasswordHash = hasher.HashPassword(u, "Demo1234!");
            return u;
        }

        db.Usuarios.AddRange(
            NuevoUsuario("Blanca Rivas (Administradora)", "admin@aliadalegal.example", admin),
            NuevoUsuario("Marcos Alvarado (Asesor)", "asesor@aliadalegal.example", asesor),
            NuevoUsuario("Carolina Méndez (Aliada SV)", "aliado.sv@aliadalegal.example", rolAliado, aliadoSv),
            NuevoUsuario("Rodrigo Pineda (Aliado GT)", "aliado.gt@aliadalegal.example", rolAliado, aliadoGt));

        // ---- Clientes ----
        var cliente1 = new Cliente
        {
            Nombre = "María Hernández", Correo = "maria.hernandez@example.com", Telefono = "+1 305 555 0101",
            CanalContacto = "WhatsApp", TipoCliente = TipoCliente.Individual, PaisOrigen = sv
        };
        var cliente2 = new Cliente
        {
            Nombre = "José Ramírez", Correo = "jose.ramirez@example.com", Telefono = "+1 713 555 0102",
            CanalContacto = "Correo", TipoCliente = TipoCliente.Individual, PaisOrigen = gt
        };
        var cliente3 = new Cliente
        {
            Nombre = "Bufete Torres & Asociados", Correo = "contacto@torresasociados.example",
            Telefono = "+1 212 555 0103", CanalContacto = "Correo",
            TipoCliente = TipoCliente.Bufete, PaisOrigen = us
        };
        var cliente4 = new Cliente
        {
            Nombre = "Lucía Fuentes", Correo = "lucia.fuentes@example.com", Telefono = "+34 600 555 104",
            CanalContacto = "Llamada", TipoCliente = TipoCliente.Individual, PaisOrigen = hn
        };
        db.Clientes.AddRange(cliente1, cliente2, cliente3, cliente4);

        await db.SaveChangesAsync();

        // ---- Trámites de ejemplo en distintos estados ----
        Tramite Crear(string codigo, Cliente c, TipoDocumento t, Pais p, Aliado? a, int diasAtras,
            params EstadoTramite[] recorrido)
        {
            var inicio = DateTime.UtcNow.AddDays(-diasAtras);
            var tramite = new Tramite
            {
                CodigoSeguimiento = codigo, Cliente = c, TipoDocumento = t, PaisDestino = p,
                FechaSolicitud = inicio, FechaUltimaActualizacion = inicio, Estado = EstadoTramite.Solicitado
            };
            tramite.Historial.Add(new HistorialEstado
            {
                EstadoAnterior = null, EstadoNuevo = EstadoTramite.Solicitado, Fecha = inicio,
                Responsable = "Marcos Alvarado (Asesor)", Comentario = "Solicitud registrada."
            });
            if (a != null) tramite.AsignarAliado(a);

            var paso = 1;
            foreach (var estado in recorrido)
            {
                var fecha = inicio.AddDays(paso * Math.Max(1, diasAtras / (recorrido.Length + 1)));
                tramite.Historial.Add(new HistorialEstado
                {
                    EstadoAnterior = tramite.Estado, EstadoNuevo = estado, Fecha = fecha,
                    Responsable = a?.Nombre ?? "Marcos Alvarado (Asesor)"
                });
                tramite.Estado = estado;
                tramite.FechaUltimaActualizacion = fecha;
                paso++;
            }
            return tramite;
        }

        var t1 = Crear("AL-2026-K7M2QX", cliente1, partida, sv, aliadoSv, 20,
            EstadoTramite.EnGestion, EstadoTramite.DocumentoObtenido, EstadoTramite.Apostillado,
            EstadoTramite.EnTransito, EstadoTramite.Entregado);
        t1.Envio = new Envio
        {
            NumeroGuia = "DHL-8841-2207", EmpresaEnvio = "DHL Express", FechaDespacho = DateTime.UtcNow.AddDays(-6)
        };

        var t2 = Crear("AL-2026-R4N8WD", cliente2, antecedentes, gt, aliadoGt, 12,
            EstadoTramite.EnGestion, EstadoTramite.DocumentoObtenido);

        var t3 = Crear("AL-2026-H9T3ZB", cliente3, acta, mx, aliadoMx, 8,
            EstadoTramite.EnGestion);

        var t4 = Crear("AL-2026-C6P5VF", cliente4, partida, hn, null, 1);

        var t5 = Crear("AL-2026-X2J7LA", cliente3, traduccion, sv, aliadoSv, 5,
            EstadoTramite.EnGestion, EstadoTramite.DocumentoObtenido);

        db.Tramites.AddRange(t1, t2, t3, t4, t5);
        await db.SaveChangesAsync();
    }
}
