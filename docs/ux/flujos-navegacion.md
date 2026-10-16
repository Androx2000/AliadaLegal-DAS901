# Flujos de navegación

**Proyecto:** Aliada Legal
**Responsable:** Blanca Rivas (Integrante 1 — UX y documentación)

Los diagramas están en Mermaid y GitHub los muestra automáticamente.

---

## 1. Flujo de autenticación por rol (RF05)

```mermaid
flowchart TD
    A[Inicio público] --> B[Iniciar sesión]
    B --> C{¿Credenciales válidas?}
    C -- No --> D[Mensaje de error] --> B
    C -- Sí --> R{¿Rol?}
    R -- Administrador / Asesor --> E[Dashboard]
    R -- Aliado --> M[Mis trámites]
    E --> F[Cerrar sesión] --> A
    M --> F
    X[Usuario sin sesión abre una URL del panel] --> B
    Y[Usuario con rol sin permiso] --> Z[Acceso denegado]
```

## 2. Flujo de consulta pública de seguimiento (RF06)

```mermaid
flowchart TD
    P1[Inicio] --> P2[Seguimiento]
    P2 --> P3[Escribir código de seguimiento]
    P3 --> P4{¿Campo vacío?}
    P4 -- Sí --> P5[Validación: escribe tu código] --> P3
    P4 -- No --> P6{¿Existe el código?}
    P6 -- No --> P7[Mensaje: no encontramos ese código] --> P3
    P6 -- Sí --> P8[Resultado: documento, país, estado y fecha]
    P8 --> P9[Línea de tiempo con las 6 etapas]
    P9 --> P10[Consultar otro código] --> P3
```

## 3. Flujo principal: ciclo de vida de un trámite

Sigue los estados del enum `EstadoTramite` e indica qué rol hace cada paso.

```mermaid
flowchart TD
    S1[Asesor: Trámites → Nuevo] --> S2{¿Existe el cliente?}
    S2 -- No --> S3[Asesor: Clientes → Nuevo] --> S1
    S2 -- Sí --> S4[Elegir país y tipo de documento]
    S4 --> S5[Guardar → Solicitado + código de seguimiento]
    S5 --> S6[Asesor entrega el código al cliente]
    S6 --> S7[Asesor: asignar aliado → En gestión]
    S7 --> S8[Aliado: Documento obtenido]
    S8 --> S9{¿Requiere apostilla?}
    S9 -- Sí --> S10[Aliado: Apostillado]
    S9 -- No --> S11
    S10 --> S11[Asesor: registrar envío → En tránsito]
    S11 --> S12[Asesor: fecha de entrega → Entregado]
```

### Diagrama de estados

```mermaid
stateDiagram-v2
    [*] --> Solicitado : Asesor crea el trámite
    Solicitado --> EnGestion : Asesor asigna aliado
    EnGestion --> DocumentoObtenido : Aliado
    DocumentoObtenido --> Apostillado : Aliado
    DocumentoObtenido --> EnTransito : Asesor registra el envío
    Apostillado --> EnTransito : Asesor registra el envío
    EnTransito --> Entregado : Asesor registra la entrega
    Entregado --> [*]
```

## 4. Flujo del Aliado (RF05)

```mermaid
flowchart TD
    L[Login como Aliado] --> MT[Mis trámites: solo los asignados]
    MT --> DT[Detalle del trámite]
    DT --> CE{¿Nuevo estado dentro de su etapa?}
    CE -- Documento obtenido / Apostillado --> G[Guardar + observación] --> MT
    CE -- Otro estado --> N[Opción no disponible]
    MT --> X[Abre un trámite ajeno por URL] --> AD[Acceso denegado]
```

## 5. Flujo de solicitud pública (cliente)

```mermaid
flowchart TD
    P1[Inicio] --> P2[Servicios]
    P1 --> P3[Solicitar documento]
    P2 --> P3
    P3 --> P4[Formulario: datos + país + documento]
    P4 --> P5{¿Datos válidos?}
    P5 -- No --> P4
    P5 -- Sí --> P6[Confirmación con código de seguimiento]
    P6 --> P7[Ir a Seguimiento]
```

## 6. Flujo de administración (solo Administrador)

```mermaid
flowchart LR
    D[Dashboard] --> AL[Aliados]
    D --> CA[Catálogos]
    D --> US[Usuarios]
    AL --> AL1[Lista] --> AL2[Nuevo / Editar / Desactivar]
    CA --> CA1[Países] --> CA3[CRUD]
    CA --> CA2[Tipos de documento] --> CA3
    US --> US1[Lista] --> US2[Crear cuenta y asignar rol: Administrador, Asesor o Aliado]
```

## 7. Reglas de navegación

1. **Máximo 3 clics** desde el Dashboard hasta cualquier acción frecuente (crear trámite, cambiar estado, registrar envío).
2. La **consulta de seguimiento** está a 1 clic desde cualquier página pública (enlace fijo en el menú).
3. **Breadcrumbs** en todas las páginas del panel (ej. *Dashboard / Trámites / AL-2026-000024*).
4. Después de crear o editar, se redirige al **detalle** del registro y se muestra un mensaje de éxito (TempData + alerta de Bootstrap).
5. **Botón Cancelar** en todos los formularios para volver a la lista sin guardar.
6. Las acciones destructivas (desactivar aliado) piden **confirmación** en un modal.
7. El menú solo muestra las opciones del rol de la sesión; si alguien entra por URL a algo no permitido, ve **Acceso denegado**.
