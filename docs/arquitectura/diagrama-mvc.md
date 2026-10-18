# Diagrama MVC y arquitectura en capas

**Proyecto:** Aliada Legal
**Responsable:** Blanca Rivas (Integrante 1 — UX y documentación)

Arquitectura acordada en la Fase 1: **Presentación MVC → Servicios → Repositorios → Base de datos**.

---

## 1. Arquitectura general por capas

```mermaid
flowchart TB
    U[Navegador del usuario]

    subgraph P[Capa de presentación · MVC]
        R[Routing + Middleware de autenticación y autorización por rol]
        C[Controllers]
        V[Views Razor + ViewModels]
    end

    subgraph S[Capa de servicios · reglas de negocio]
        S1[ITramiteService]
        S2[ISeguimientoService]
        S3[IEnvioService]
        S4[IAliadoService]
        S5[IClienteService / ICatalogoService / IDashboardService]
    end

    subgraph RP[Capa de repositorios · acceso a datos]
        R1[ITramiteRepository]
        R2[IEnvioRepository]
        R3[IAliadoRepository]
        R4[IClienteRepository / IPaisRepository / ITipoDocumentoRepository]
        RG[IRepository genérico]
    end

    subgraph D[Capa de datos]
        CTX[(ApplicationDbContext · EF Core + Identity)]
        SQL[(SQL Server)]
    end

    U -- Petición HTTP --> R --> C
    C -- ViewModel --> V -- HTML + Bootstrap --> U
    C --> S1 & S2 & S3 & S4 & S5
    S1 & S2 --> R1
    S3 --> R2
    S4 --> R3
    S5 --> R4
    R1 & R2 & R3 & R4 -.hereda.-> RG
    RG --> CTX --> SQL
```

Regla principal: cada capa solo llama a la capa inmediatamente inferior. Los controladores nunca usan `ApplicationDbContext`.

## 2. Secuencia: consulta pública de seguimiento (RF06)

```mermaid
sequenceDiagram
    actor CL as Cliente
    participant V as Vista Seguimiento
    participant C as SeguimientoController
    participant S as SeguimientoService
    participant R as TramiteRepository
    participant DB as ApplicationDbContext
    CL->>V: Escribe el código y pulsa Consultar
    V->>C: GET /Seguimiento/Consultar?codigo=AL-2026-000024
    C->>S: ConsultarPorCodigoAsync(codigo)
    S->>S: Normaliza el código (mayúsculas, sin espacios)
    S->>R: ObtenerPorCodigoAsync(codigo)
    R->>DB: Tramites + Include(TipoDocumento, Pais)
    DB-->>R: Tramite o null
    R-->>S: Tramite o null
    alt No existe
        S-->>C: null
        C-->>V: Mensaje "No encontramos un trámite con ese código"
    else Existe
        S-->>C: TramiteSeguimientoDto (sin datos personales)
        C-->>V: Estado, documento, país, fecha y línea de tiempo
    end
    V-->>CL: Muestra el resultado
```

## 3. Secuencia: el Asesor cambia el estado de un trámite

```mermaid
sequenceDiagram
    actor A as Asesor
    participant V as Vista Tramites/Details
    participant C as TramitesController
    participant S as TramiteService
    participant R as TramiteRepository
    A->>V: Elige "En tránsito" y confirma
    V->>C: POST /Tramites/CambiarEstado/24
    C->>S: CambiarEstadoAsync(24, EnTransito)
    S->>R: ObtenerConEnvioAsync(24)
    R-->>S: Tramite (+ Envio o null)
    alt Sin envío registrado
        S-->>C: Error de negocio "Registre el envío primero"
        C-->>V: Muestra el error
    else Con envío
        S->>R: Update(tramite) + SaveChangesAsync()
        S-->>C: OK
        C-->>V: Redirect a Details + TempData "Trámite actualizado"
    end
    V-->>A: Badge naranja "En tránsito"
```

## 4. Secuencia: el Aliado actualiza un trámite asignado (RF05)

```mermaid
sequenceDiagram
    actor AL as Aliado
    participant C as MisTramitesController
    participant S as AliadoService
    participant R as TramiteRepository
    AL->>C: POST /MisTramites/CambiarEstado/24 (Apostillado)
    C->>S: ActualizarEstadoAsignadoAsync(24, Apostillado, email del usuario)
    S->>R: GetByIdAsync(24)
    R-->>S: Tramite
    alt El trámite no es de este aliado o el estado está fuera de su etapa
        S-->>C: Denegado
        C-->>AL: Acceso denegado / mensaje de error
    else Válido
        S->>R: Update + SaveChangesAsync()
        C-->>AL: Mis trámites con el estado actualizado
    end
```

## 5. Diagrama de clases del modelo

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
        +string CodigoSeguimiento
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
    class TramiteSeguimientoDto {
        <<DTO>>
        +string CodigoSeguimiento
        +string TipoDocumento
        +string Pais
        +string Estado
        +DateTime FechaSolicitud
    }

    Cliente "1" --> "*" Tramite
    Pais "1" --> "*" Tramite
    Pais "1" --> "*" Aliado
    TipoDocumento "1" --> "*" Tramite
    Aliado "0..1" --> "*" Tramite : asignado
    Tramite "1" --> "0..1" Envio : envío opcional
    Tramite ..> EstadoTramite
    Tramite ..> TramiteSeguimientoDto : se mapea en SeguimientoService
    ApplicationUser ..> Aliado : rol Aliado, vinculado por Email
    note for ApplicationUser "Roles de Identity: Administrador, Asesor, Aliado"
    note for Tramite "CodigoSeguimiento: propuesto, índice único (RF06)"
    note for Envio "TramiteId con índice único: máximo un envío por trámite"
```

> **Decisión de diseño de esta versión:** un trámite tiene **cero o un envío** (1 a 0..1). Se garantiza con un índice único sobre `Envio.TramiteId` y con la validación de `EnvioService`. Si más adelante se necesitan reenvíos, la relación puede pasar a 1 a N.

## 6. Resumen de responsabilidades

| Capa | Hace | No debe |
|---|---|---|
| **Vista** | Mostrar datos y capturar entradas; menú según rol | Consultar la base de datos |
| **Controlador** | Validar `ModelState`, aplicar `[Authorize(Roles)]`, llamar al servicio y elegir la vista | Usar `ApplicationDbContext` o tener reglas de negocio |
| **Servicio** | Reglas de negocio (estados, un envío por trámite, código de seguimiento, permisos del Aliado) y mapeo a DTO | Conocer vistas o HTML |
| **Repositorio** | Consultas y guardado con EF Core | Tomar decisiones de negocio |
| **Modelo** | Definir datos, validaciones y relaciones | Saber qué vista lo muestra |
