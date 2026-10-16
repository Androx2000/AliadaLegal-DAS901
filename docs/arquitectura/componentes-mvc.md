# Componentes MVC

**Proyecto:** Aliada Legal — ASP.NET Core MVC (.NET) + Entity Framework Core + Identity
**Responsable:** Blanca Rivas (Integrante 1 — UX y documentación)

Este documento describe los componentes del patrón Modelo–Vista–Controlador del proyecto `AliadaLegal.Web` y cómo se conectan con las pantallas definidas en `docs/ux/`.

---

## 1. Estructura de carpetas

```
AliadaLegal.Web/
├── Controllers/        → Reciben las peticiones y coordinan modelo y vista
├── Models/             → Entidades del dominio y enums
├── Data/               → ApplicationDbContext (EF Core + Identity)
├── Views/              → Vistas Razor (.cshtml) por controlador
│   └── Shared/         → _Layout, parciales, validación
├── wwwroot/            → CSS, JS y librerías (Bootstrap, jQuery)
├── Program.cs          → Configuración de servicios y rutas
└── appsettings.json    → Cadena de conexión y configuración
```

## 2. Modelo (M)

### 2.1 Entidades

| Entidad | Propiedades principales | Relaciones |
|---|---|---|
| `ApplicationUser` | NombreCompleto (+ campos de `IdentityUser`) | Usuarios internos (Admin, Gestor) |
| `Cliente` | Nombre, Email, Telefono, TipoCliente, FechaRegistro | 1 Cliente → N Trámites |
| `Pais` | Nombre, CodigoIso (2 letras) | 1 País → N Trámites; 1 País → N Aliados |
| `TipoDocumento` | Nombre, Descripcion, Activo | 1 Tipo → N Trámites |
| `Aliado` | NombreCompleto, Email, Telefono, Tipo, Activo, PaisId | 1 Aliado → N Trámites |
| `Tramite` | ClienteId, PaisId, TipoDocumentoId, Estado, FechaSolicitud, Observaciones, AliadoId? | N:1 con Cliente, País, TipoDocumento y Aliado (opcional) |
| `Envio` | TramiteId, EmpresaTransporte, NumeroRastreo, FechaEnvio, FechaEntrega? | N:1 con Trámite |

### 2.2 Enumeraciones

| Enum | Valores |
|---|---|
| `TipoCliente` | Individual, Bufete |
| `TipoAliado` | Abogado, Notario |
| `EstadoTramite` | Solicitado, EnGestion, DocumentoObtenido, Apostillado, EnTransito, Entregado |

### 2.3 Acceso a datos
`ApplicationDbContext` hereda de `IdentityDbContext<ApplicationUser>` y expone los `DbSet`: `Clientes`, `Paises`, `TiposDocumento`, `Tramites`, `Aliados` y `Envios`.

### 2.4 ViewModels propuestos
Sirven para no pasar las entidades directo a las vistas, sobre todo en formularios con listas desplegables:

| ViewModel | Uso |
|---|---|
| `TramiteFormViewModel` | Crear/editar trámite con `SelectList` de clientes, países y documentos |
| `TramiteFiltroViewModel` | Filtros de la lista (estado, país, texto) |
| `DashboardViewModel` | Conteo por estado + últimos trámites |
| `SolicitudPublicaViewModel` | Formulario público (datos del cliente + trámite) |
| `LoginViewModel` | Correo, contraseña, recordarme |

## 3. Vista (V)

| Carpeta de vistas | Vistas | Notas UX |
|---|---|---|
| `Views/Shared` | `_Layout`, `_LayoutAdmin`, `_ValidationScriptsPartial`, `_EstadoBadge` | `_EstadoBadge` pinta el color de cada estado |
| `Views/Home` | Index, Servicios, Privacy | Zona pública |
| `Views/Solicitud` | Crear, Confirmacion | Formulario mobile first |
| `Views/Account` | Login | |
| `Views/Dashboard` | Index | Tarjetas por estado |
| `Views/Clientes` | Index, Create, Edit, Details | |
| `Views/Tramites` | Index, Create, Edit, Details | Details concentra acciones |
| `Views/Envios` | Create, Edit | |
| `Views/Aliados` | Index, Create, Edit, Details | |
| `Views/Paises`, `Views/TiposDocumento` | Index, Create, Edit | |

## 4. Controlador (C)

| Controlador | Acciones | Autorización |
|---|---|---|
| `HomeController` | Index, Servicios, Privacy, Error | Pública |
| `SolicitudController` | Crear (GET/POST), Confirmacion | Pública |
| `AccountController` | Login (GET/POST), Logout | Pública / autenticado |
| `DashboardController` | Index | `[Authorize]` |
| `ClientesController` | Index, Details, Create, Edit | `[Authorize]` |
| `TramitesController` | Index, Details, Create, Edit, CambiarEstado, AsignarAliado | `[Authorize]` |
| `EnviosController` | Create, Edit | `[Authorize]` |
| `AliadosController` | Index, Details, Create, Edit, Desactivar | `[Authorize(Roles="Administrador")]` (Index/Details también Gestor) |
| `PaisesController` | Index, Create, Edit | `[Authorize(Roles="Administrador")]` |
| `TiposDocumentoController` | Index, Create, Edit | `[Authorize(Roles="Administrador")]` |
| `UsuariosController` | Index, Create, AsignarRol | `[Authorize(Roles="Administrador")]` |

## 5. Relación historias ↔ componentes

| Historia | Controlador | Vista | Modelo |
|---|---|---|---|
| HU-01 | Account | Login | ApplicationUser |
| HU-02 | Clientes | Create | Cliente |
| HU-03 | Tramites | Create | Tramite |
| HU-04 | Tramites | Details | Tramite (Estado) |
| HU-05 | Tramites | Details | Tramite, Aliado |
| HU-06 | Envios | Create / Edit | Envio |
| HU-07 | Tramites | Index | Tramite |
| HU-08 | Aliados | Index / Create / Edit | Aliado |
| HU-09 | Paises, TiposDocumento | Index / Create / Edit | Pais, TipoDocumento |
| HU-10 | Dashboard | Index | DashboardViewModel |
| HU-11 | Solicitud | Crear / Confirmacion | Cliente, Tramite |

> Nota: los controladores y vistas marcados en este documento son la **propuesta de diseño**. Los modelos ya están implementados en la rama `feature/modelos-datos`.
