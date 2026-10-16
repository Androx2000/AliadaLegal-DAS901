# Componentes MVC y arquitectura en capas

**Proyecto:** Aliada Legal — ASP.NET Core MVC (.NET) + Entity Framework Core + Identity
**Responsable:** Blanca Rivas (Integrante 1 — UX y documentación)

Este documento describe los componentes del proyecto `AliadaLegal.Web` según la arquitectura en capas acordada en la Fase 1:

**Presentación MVC → Servicios → Repositorios → Base de datos**

Los controladores **no** usan `ApplicationDbContext` directamente. Llaman a servicios, los servicios aplican las reglas de negocio y los repositorios son los únicos que acceden a la base de datos.

---

## 1. Estructura de carpetas

```
AliadaLegal.Web/
├── Controllers/        → Capa de presentación: reciben la petición, validan ModelState y eligen la vista
├── Views/              → Vistas Razor (.cshtml) por controlador
│   └── Shared/         → _Layout, parciales, validación
├── ViewModels/         → Datos de formularios y pantallas (CrearClienteViewModel, CrearTramiteViewModel…)
├── Dtos/               → Objetos de salida (TramiteSeguimientoDto)
├── Services/           → Capa de servicios: reglas de negocio (interfaces + implementaciones)
├── Repositories/       → Capa de repositorios: acceso a datos con EF Core
├── Models/             → Entidades del dominio y enums
├── Data/               → ApplicationDbContext (EF Core + Identity)
├── wwwroot/            → CSS, JS y librerías (Bootstrap, jQuery)
├── Program.cs          → Registro de servicios y repositorios (inyección de dependencias), roles y rutas
└── appsettings.json    → Cadena de conexión y configuración
```

## 2. Capas y responsabilidades

| Capa | Componentes | Responsabilidad | No debe |
|---|---|---|---|
| **Presentación (MVC)** | Controllers, Views, ViewModels | Recibir la petición, validar `ModelState`, aplicar `[Authorize(Roles)]`, llamar al servicio y mostrar la vista | Acceder a `ApplicationDbContext` ni contener reglas de negocio |
| **Servicios** | `Services/I*Service` + implementaciones | Reglas de negocio: transiciones de estado, un envío por trámite, generar código de seguimiento, restricciones del Aliado, mapear entidades a DTO | Conocer vistas ni HTML |
| **Repositorios** | `Repositories/I*Repository` + implementaciones | Consultas y guardado con EF Core (`Include`, filtros, `SaveChangesAsync`) | Tomar decisiones de negocio |
| **Datos** | `ApplicationDbContext`, SQL Server | Mapeo objeto-relacional, migraciones e Identity | — |

## 3. Modelo de dominio

### 3.1 Entidades

| Entidad | Propiedades principales | Relaciones |
|---|---|---|
| `ApplicationUser` | NombreCompleto (+ campos de `IdentityUser`) | Usuarios con rol Administrador, Asesor o Aliado |
| `Cliente` | Nombre, Email, Telefono, TipoCliente, FechaRegistro | 1 Cliente → N Trámites |
| `Pais` | Nombre, CodigoIso (2 letras) | 1 País → N Trámites; 1 País → N Aliados |
| `TipoDocumento` | Nombre, Descripcion, Activo | 1 Tipo → N Trámites |
| `Aliado` | NombreCompleto, Email, Telefono, Tipo, Activo, PaisId | 1 Aliado → N Trámites; se vincula con su cuenta de Identity por Email |
| `Tramite` | ClienteId, PaisId, TipoDocumentoId, Estado, FechaSolicitud, Observaciones, AliadoId? | N:1 con Cliente, País, TipoDocumento y Aliado (opcional); **1:0..1 con Envío** |
| `Envio` | TramiteId, EmpresaTransporte, NumeroRastreo, FechaEnvio, FechaEntrega? | **0..1 por Trámite** |

### 3.2 Decisión de diseño: un envío opcional por trámite

En esta versión cada trámite tiene **como máximo un envío** (relación 1 a 0..1). Un trámite recién creado no tiene envío; el envío se registra cuando el documento ya fue obtenido o apostillado.

- Se implementa con un **índice único** sobre `Envio.TramiteId` (configurado en `ApplicationDbContext`) y, opcionalmente, una propiedad de navegación `Envio? Envio` en `Tramite`.
- `EnvioService` verifica que el trámite no tenga envío antes de crear uno; si ya existe, se edita.
- Si en el futuro se necesitan reenvíos o envíos parciales, la relación puede pasar a 1 a N sin cambiar las demás entidades.

### 3.3 Enumeraciones

| Enum | Valores |
|---|---|
| `TipoCliente` | Individual, Bufete |
| `TipoAliado` | Abogado, Notario |
| `EstadoTramite` | Solicitado, EnGestion, DocumentoObtenido, Apostillado, EnTransito, Entregado |

### 3.4 Código de seguimiento (RF06)

`TramiteSeguimientoDto` ya define `CodigoSeguimiento`. Para que la consulta pública funcione, cada trámite necesita un código único que no se pueda adivinar fácilmente a partir del Id. Se propone agregar `CodigoSeguimiento` a `Tramite` con índice único, generado por `TramiteService` al crear el trámite (formato propuesto `AL-2026-000024` o similar acordado por el equipo).

## 4. ViewModels y DTO

| Clase | Carpeta | Uso | Estado |
|---|---|---|---|
| `CrearClienteViewModel` | ViewModels | Formulario de nuevo cliente | Implementado |
| `CrearTramiteViewModel` | ViewModels | Formulario de nuevo trámite | Implementado |
| `TramiteSeguimientoDto` | Dtos | Respuesta de la consulta pública (sin datos personales) | Implementado |
| `TramiteFiltroViewModel` | ViewModels | Filtros de la lista de trámites | Propuesto |
| `DashboardViewModel` | ViewModels | Conteo por estado + últimos trámites | Propuesto |
| `RegistrarEnvioViewModel` | ViewModels | Formulario de envío | Propuesto |
| `LoginViewModel` | ViewModels | Correo, contraseña, recordarme | Propuesto |

## 5. Servicios (reglas de negocio)

| Interfaz | Métodos principales | Reglas que aplica |
|---|---|---|
| `IClienteService` | `ListarAsync`, `ObtenerAsync`, `CrearAsync(CrearClienteViewModel)`, `EditarAsync` | Correo con formato válido; reutilizar cliente por correo en la solicitud pública |
| `ITramiteService` | `ListarAsync(filtro)`, `ObtenerAsync`, `CrearAsync(CrearTramiteViewModel)`, `CambiarEstadoAsync`, `AsignarAliadoAsync` | Estado inicial Solicitado; genera código de seguimiento; orden de estados; no pasa a En tránsito sin envío; solo aliados activos |
| `ISeguimientoService` | `ConsultarPorCodigoAsync(codigo)` → `TramiteSeguimientoDto?` | Normaliza el código (mayúsculas, sin espacios); devuelve solo datos públicos |
| `IEnvioService` | `RegistrarAsync`, `EditarAsync`, `MarcarEntregadoAsync` | Máximo un envío por trámite; cambia el estado a En tránsito / Entregado |
| `IAliadoService` | `ListarAsync`, `CrearAsync`, `EditarAsync`, `DesactivarAsync`, `ListarTramitesAsignadosAsync(email)`, `ActualizarEstadoAsignadoAsync` | El aliado solo ve sus trámites y solo mueve estados de su etapa |
| `ICatalogoService` | Países y tipos de documento (CRUD) | Código ISO de 2 letras; desactivar en lugar de borrar |
| `IDashboardService` | `ObtenerResumenAsync` | Conteo por estado |

## 6. Repositorios (acceso a datos)

| Interfaz | Responsabilidad |
|---|---|
| `IRepository<T>` (genérico) | `GetByIdAsync`, `ListAsync`, `AddAsync`, `Update`, `SaveChangesAsync` |
| `IClienteRepository` | Buscar por correo |
| `ITramiteRepository` | Listar con filtros e `Include` (Cliente, País, TipoDocumento, Aliado, Envío); buscar por código de seguimiento; listar por `AliadoId`; contar por estado |
| `IEnvioRepository` | Obtener el envío de un trámite |
| `IAliadoRepository` | Listar activos; buscar por correo |
| `IPaisRepository`, `ITipoDocumentoRepository` | Catálogos |

Registro en `Program.cs` (inyección de dependencias):

```csharp
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ITramiteRepository, TramiteRepository>();
builder.Services.AddScoped<ITramiteService, TramiteService>();
builder.Services.AddScoped<ISeguimientoService, SeguimientoService>();
// … resto de repositorios y servicios
```

## 7. Controladores (presentación)

| Controlador | Acciones | Servicio que usa | Autorización |
|---|---|---|---|
| `HomeController` | Index, Servicios, Privacy, Error | — | Pública |
| `SeguimientoController` | Index, Consultar | `ISeguimientoService` | `[AllowAnonymous]` · RF06 |
| `SolicitudController` | Crear (GET/POST), Confirmacion | `IClienteService`, `ITramiteService` | Pública |
| `AccountController` | Login, Logout, AccessDenied | Identity (`SignInManager`) | Pública / autenticado |
| `DashboardController` | Index | `IDashboardService` | `[Authorize(Roles = "Administrador,Asesor")]` |
| `ClientesController` | Index, Details, Create, Edit | `IClienteService` | `[Authorize(Roles = "Administrador,Asesor")]` |
| `TramitesController` | Index, Details, Create, Edit, CambiarEstado, AsignarAliado | `ITramiteService` | `[Authorize(Roles = "Administrador,Asesor")]` |
| `EnviosController` | Create, Edit | `IEnvioService` | `[Authorize(Roles = "Administrador,Asesor")]` |
| `AliadosController` | Index, Details (Admin y Asesor); Create, Edit, Desactivar (solo Admin) | `IAliadoService` | Por acción |
| `MisTramitesController` | Index, Details, CambiarEstado | `IAliadoService` | `[Authorize(Roles = "Aliado")]` · RF05 |
| `PaisesController`, `TiposDocumentoController` | Index, Create, Edit | `ICatalogoService` | `[Authorize(Roles = "Administrador")]` |
| `UsuariosController` | Index, Create, AsignarRol | Identity (`UserManager`, `RoleManager`) | `[Authorize(Roles = "Administrador")]` |

## 8. Vistas

| Carpeta | Vistas | Notas UX |
|---|---|---|
| `Views/Shared` | `_Layout`, `_ValidationScriptsPartial`, `_EstadoBadge`, `_LineaTiempoEstado` | Menú según rol; badge de color por estado |
| `Views/Home` | Index, Servicios, Privacy | Zona pública |
| `Views/Seguimiento` | Index, Consultar | Mobile first, una sola caja de texto |
| `Views/Solicitud` | Crear, Confirmacion | Muestra el código de seguimiento |
| `Views/Account` | Login, AccessDenied | |
| `Views/Dashboard` | Index | Tarjetas por estado |
| `Views/Clientes`, `Views/Tramites` | Index, Create, Edit, Details | Details de trámite concentra acciones |
| `Views/Envios` | Create, Edit | |
| `Views/Aliados` | Index, Create, Edit, Details | |
| `Views/MisTramites` | Index, Details | Solo trámites del aliado |
| `Views/Paises`, `Views/TiposDocumento`, `Views/Usuarios` | Index, Create, Edit | |

## 9. Trazabilidad historias ↔ componentes

| Historia | Controlador | Servicio | Repositorio | Modelo / DTO |
|---|---|---|---|---|
| HU-01 | Account | Identity | — | ApplicationUser |
| HU-02 | Clientes | IClienteService | IClienteRepository | Cliente |
| HU-03 | Tramites | ITramiteService | ITramiteRepository | Tramite |
| HU-04 | Tramites | ITramiteService | ITramiteRepository | Tramite (Estado) |
| HU-05 | Tramites | ITramiteService | ITramiteRepository, IAliadoRepository | Tramite, Aliado |
| HU-06 | Envios | IEnvioService | IEnvioRepository | Envio |
| HU-07 | Tramites | ITramiteService | ITramiteRepository | Tramite |
| HU-08 | Aliados | IAliadoService | IAliadoRepository | Aliado |
| HU-09 | Paises, TiposDocumento | ICatalogoService | IPaisRepository, ITipoDocumentoRepository | Pais, TipoDocumento |
| HU-10 | Dashboard | IDashboardService | ITramiteRepository | DashboardViewModel |
| HU-11 | Solicitud | IClienteService, ITramiteService | IClienteRepository, ITramiteRepository | Cliente, Tramite |
| HU-13 | Usuarios | Identity | — | ApplicationUser, roles |
| **HU-14** | **Seguimiento** | **ISeguimientoService** | **ITramiteRepository** | **TramiteSeguimientoDto** |
| **HU-15** | **MisTramites** | **IAliadoService** | **ITramiteRepository** | **Tramite** |

> Estado: los modelos, `ApplicationDbContext`, los ViewModels de cliente y trámite y `TramiteSeguimientoDto` ya están en la rama `feature/modelos-datos`. Servicios, repositorios, controladores y vistas de este documento son la arquitectura acordada para implementar.
