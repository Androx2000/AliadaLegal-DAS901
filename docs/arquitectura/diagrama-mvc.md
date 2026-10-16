# Diagrama MVC

**Proyecto:** Aliada Legal
**Responsable:** Blanca Rivas (Integrante 1 — UX y documentación)

---

## 1. Arquitectura general

```mermaid
flowchart LR
    U[Navegador del usuario] -- Petición HTTP --> R[Routing ASP.NET Core<br/>Controller/Action/id]
    R --> AU{Middleware de<br/>autenticación / autorización}
    AU -- Sin permiso --> LG[Redirige a /Account/Login]
    AU -- OK --> C[Controller]
    C -- Consulta / guarda --> DB[(ApplicationDbContext<br/>EF Core)]
    DB --> SQL[(SQL Server)]
    C -- Modelo / ViewModel --> V[Vista Razor .cshtml]
    V -- HTML + Bootstrap --> U
```

## 2. Secuencia: cambiar el estado de un trámite

```mermaid
sequenceDiagram
    actor G as Gestor
    participant V as Vista Tramites/Details
    participant C as TramitesController
    participant DB as ApplicationDbContext
    G->>V: Elige "En tránsito" y confirma
    V->>C: POST /Tramites/CambiarEstado/24
    C->>DB: Tramites.Find(24) + ¿tiene Envío?
    alt Sin envío registrado
        C-->>V: Error: "Registre el envío primero"
    else Con envío
        C->>DB: Estado = EnTransito, SaveChanges()
        C-->>V: Redirect a Details + TempData "Trámite actualizado"
    end
    V-->>G: Muestra badge naranja "En tránsito"
```

## 3. Diagrama de clases del Modelo

```mermaid
classDiagram
    class ApplicationUser {
        +string NombreCompleto
    }
    class Cliente {
        +int Id
        +string Nombre
        +string Email
        +string Telefono
        +TipoCliente TipoCliente
        +DateTime FechaRegistro
    }
    class Pais {
        +int Id
        +string Nombre
        +string CodigoIso
    }
    class TipoDocumento {
        +int Id
        +string Nombre
        +string Descripcion
        +bool Activo
    }
    class Aliado {
        +int Id
        +string NombreCompleto
        +string Email
        +string Telefono
        +TipoAliado Tipo
        +bool Activo
        +int PaisId
    }
    class Tramite {
        +int Id
        +int ClienteId
        +int PaisId
        +int TipoDocumentoId
        +EstadoTramite Estado
        +DateTime FechaSolicitud
        +string Observaciones
        +int? AliadoId
    }
    class Envio {
        +int Id
        +int TramiteId
        +string EmpresaTransporte
        +string NumeroRastreo
        +DateTime FechaEnvio
        +DateTime? FechaEntrega
    }
    class EstadoTramite {
        <<enumeration>>
        Solicitado
        EnGestion
        DocumentoObtenido
        Apostillado
        EnTransito
        Entregado
    }

    Cliente "1" --> "*" Tramite
    Pais "1" --> "*" Tramite
    Pais "1" --> "*" Aliado
    TipoDocumento "1" --> "*" Tramite
    Aliado "0..1" --> "*" Tramite
    Tramite "1" --> "*" Envio
    Tramite ..> EstadoTramite
```

## 4. Capas y responsabilidades

```mermaid
flowchart TB
    subgraph Presentación
        V1[Views/*.cshtml]
        V2[wwwroot: Bootstrap, jQuery Validation, site.css]
    end
    subgraph Control
        C1[Controllers]
        C2[Filtros Authorize por rol]
    end
    subgraph Modelo
        M1[Entidades + DataAnnotations]
        M2[ViewModels]
        M3[ApplicationDbContext]
    end
    Presentación <--> Control
    Control <--> Modelo
    M3 --> BD[(Base de datos)]
```

| Capa | Responsabilidad | No debe |
|---|---|---|
| **Vista** | Mostrar datos y capturar entradas | Consultar la base de datos |
| **Controlador** | Validar `ModelState`, aplicar reglas de flujo (ej. no pasar a En tránsito sin envío), elegir la vista | Tener HTML ni lógica de presentación |
| **Modelo** | Definir datos, validaciones y relaciones | Saber qué vista lo muestra |
