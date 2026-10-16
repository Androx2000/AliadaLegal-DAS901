# Flujos de navegación

**Proyecto:** Aliada Legal
**Responsable:** Blanca Rivas (Integrante 1 — UX y documentación)

Los diagramas están en Mermaid y GitHub los muestra automáticamente.

---

## 1. Flujo de autenticación

```mermaid
flowchart TD
    A[Inicio público] --> B[Iniciar sesión]
    B --> C{¿Credenciales válidas?}
    C -- No --> D[Mensaje de error] --> B
    C -- Sí --> E[Dashboard]
    E --> F[Cerrar sesión] --> A
    X[Usuario sin sesión abre una URL del panel] --> B
```

## 2. Flujo principal: ciclo de vida de un trámite

Este es el flujo más importante del sistema y sigue los estados del enum `EstadoTramite`.

```mermaid
flowchart TD
    S1[Trámites → Nuevo] --> S2{¿Existe el cliente?}
    S2 -- No --> S3[Clientes → Nuevo] --> S1
    S2 -- Sí --> S4[Elegir país y tipo de documento]
    S4 --> S5[Guardar → estado: Solicitado]
    S5 --> S6[Detalle del trámite]
    S6 --> S7[Asignar aliado → En gestión]
    S7 --> S8[Aliado obtiene el documento → Documento obtenido]
    S8 --> S9{¿Requiere apostilla?}
    S9 -- Sí --> S10[Apostillado]
    S9 -- No --> S11
    S10 --> S11[Registrar envío → En tránsito]
    S11 --> S12[Registrar fecha de entrega → Entregado]
```

### Diagrama de estados

```mermaid
stateDiagram-v2
    [*] --> Solicitado
    Solicitado --> EnGestion : asignar aliado
    EnGestion --> DocumentoObtenido
    DocumentoObtenido --> Apostillado
    DocumentoObtenido --> EnTransito : registrar envío
    Apostillado --> EnTransito : registrar envío
    EnTransito --> Entregado : fecha de entrega
    Entregado --> [*]
```

## 3. Flujo de solicitud pública (cliente)

```mermaid
flowchart TD
    P1[Inicio] --> P2[Servicios]
    P1 --> P3[Solicitar documento]
    P2 --> P3
    P3 --> P4[Formulario: datos + país + documento]
    P4 --> P5{¿Datos válidos?}
    P5 -- No --> P4
    P5 -- Sí --> P6[Confirmación con N.º de solicitud]
    P6 --> P1
```

## 4. Flujo de administración (solo Administrador)

```mermaid
flowchart LR
    D[Dashboard] --> AL[Aliados]
    D --> CA[Catálogos]
    D --> US[Usuarios]
    AL --> AL1[Lista] --> AL2[Nuevo / Editar / Desactivar]
    CA --> CA1[Países] --> CA3[CRUD]
    CA --> CA2[Tipos de documento] --> CA3
    US --> US1[Lista] --> US2[Crear gestor / asignar rol]
```

## 5. Reglas de navegación

1. **Máximo 3 clics** desde el Dashboard hasta cualquier acción frecuente (crear trámite, cambiar estado, registrar envío).
2. **Breadcrumbs** en todas las páginas del panel (ej. *Dashboard / Trámites / #24*).
3. Después de crear o editar, se redirige al **detalle** del registro y se muestra un mensaje de éxito (TempData + alerta de Bootstrap).
4. **Botón Cancelar** en todos los formularios para volver a la lista sin guardar.
5. Las acciones destructivas (desactivar aliado) piden **confirmación** en un modal.
