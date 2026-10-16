# Historias de usuario priorizadas

**Proyecto:** Aliada Legal
**Responsable:** Blanca Rivas (Integrante 1 — UX y documentación)

Formato: *Como [usuario], quiero [acción], para [beneficio].*
Prioridad con el método **MoSCoW**: **M** = Must (obligatoria), **S** = Should (importante), **C** = Could (deseable), **W** = Won't (fuera de esta versión).
Estimación en puntos de historia (escala Fibonacci).

---

## Resumen del backlog

| ID | Historia | Usuario | Prioridad | Puntos |
|---|---|---|:---:|:---:|
| HU-01 | Iniciar sesión | Admin / Gestor | M | 3 |
| HU-02 | Registrar cliente | Admin / Gestor | M | 3 |
| HU-03 | Registrar trámite | Admin / Gestor | M | 5 |
| HU-04 | Cambiar estado del trámite | Admin / Gestor | M | 3 |
| HU-05 | Asignar aliado a un trámite | Admin / Gestor | M | 3 |
| HU-06 | Registrar envío con número de rastreo | Admin / Gestor | M | 3 |
| HU-07 | Listar y filtrar trámites | Admin / Gestor | M | 5 |
| HU-08 | Administrar aliados | Administrador | S | 3 |
| HU-09 | Administrar catálogos (países y tipos de documento) | Administrador | S | 3 |
| HU-10 | Ver dashboard por estado | Admin / Gestor | S | 5 |
| HU-11 | Enviar solicitud desde el sitio público | Cliente | S | 5 |
| HU-12 | Ver detalle e historial del cliente | Admin / Gestor | C | 3 |
| HU-13 | Gestionar usuarios y roles | Administrador | C | 5 |
| HU-14 | Consultar estado con un código de seguimiento | Cliente | W | 8 |
| HU-15 | Portal para aliados | Aliado | W | 13 |

---

## Detalle y criterios de aceptación

### HU-01 — Iniciar sesión · **Must**
**Como** administrador o gestor, **quiero** iniciar sesión con mi correo y contraseña **para** acceder al panel de forma segura.

- El sistema valida correo y contraseña con ASP.NET Core Identity.
- Si los datos son incorrectos, muestra un mensaje genérico ("Correo o contraseña incorrectos").
- Las rutas del panel no se pueden abrir sin sesión (`[Authorize]`).
- Al cerrar sesión, regresa a la página de inicio.

### HU-02 — Registrar cliente · **Must**
**Como** gestor, **quiero** registrar los datos de un cliente **para** asociarle uno o más trámites.

- Campos obligatorios: nombre, correo, teléfono y tipo de cliente (Individual / Bufete).
- El correo debe tener un formato válido. Los mensajes de error aparecen en español junto a cada campo.
- La fecha de registro se guarda automáticamente.
- Al guardar, se muestra un mensaje de éxito y se redirige al detalle del cliente.

### HU-03 — Registrar trámite · **Must**
**Como** gestor, **quiero** crear un trámite para un cliente indicando el país y el tipo de documento **para** empezar su gestión.

- Se selecciona el cliente, el país y el tipo de documento en listas desplegables.
- El estado inicial es **Solicitado** y la fecha de solicitud se asigna sola.
- El campo Observaciones es opcional (máx. 1000 caracteres).
- No se puede guardar sin cliente, país ni tipo de documento.

### HU-04 — Cambiar estado del trámite · **Must**
**Como** gestor, **quiero** actualizar el estado de un trámite **para** reflejar su avance real.

- Estados en orden: Solicitado → En gestión → Documento obtenido → Apostillado → En tránsito → Entregado.
- El estado se ve como una etiqueta de color en la lista y en el detalle.
- Para pasar a **En tránsito**, el trámite debe tener un envío registrado.

### HU-05 — Asignar aliado · **Must**
**Como** gestor, **quiero** asignar un abogado o notario aliado a un trámite **para** que obtenga el documento en el país de origen.

- Solo se muestran aliados **activos**.
- Se puede cambiar el aliado mientras el trámite no esté Entregado.
- El nombre del aliado aparece en el detalle del trámite.

### HU-06 — Registrar envío · **Must**
**Como** gestor, **quiero** registrar la empresa de transporte y el número de rastreo **para** dar seguimiento a la entrega.

- Campos obligatorios: empresa de transporte y número de rastreo.
- La fecha de envío se guarda automáticamente. La fecha de entrega se completa después.
- Al registrar la fecha de entrega, el trámite puede marcarse como **Entregado**.

### HU-07 — Listar y filtrar trámites · **Must**
**Como** gestor, **quiero** ver todos los trámites y filtrarlos por estado, país o cliente **para** encontrar rápido el caso que necesito.

- Tabla con columnas: N.º, cliente, documento, país, estado, aliado y fecha.
- Filtros por estado, país y texto (nombre del cliente).
- En celular, la tabla se adapta (Bootstrap `table-responsive`).

### HU-08 — Administrar aliados · **Should**
**Como** administrador, **quiero** registrar, editar y desactivar aliados **para** mantener actualizada mi red de abogados y notarios.

- Campos: nombre completo, correo, teléfono, tipo (Abogado / Notario), país y activo.
- Desactivar no borra al aliado. Solo lo oculta en la asignación de trámites nuevos.

### HU-09 — Administrar catálogos · **Should**
**Como** administrador, **quiero** gestionar los países y tipos de documento **para** ofrecer nuevos servicios sin tocar el código.

- País: nombre y código ISO de 2 letras (ej. SV, GT, HN).
- Tipo de documento: nombre, descripción y activo.

### HU-10 — Dashboard · **Should**
**Como** administrador, **quiero** ver tarjetas con la cantidad de trámites por estado **para** conocer la carga de trabajo de un vistazo.

- Una tarjeta por estado, con su número y un enlace a la lista ya filtrada.
- Lista de los 5 trámites más recientes.

### HU-11 — Solicitud desde el sitio público · **Should**
**Como** cliente, **quiero** llenar un formulario con mis datos y el documento que necesito **para** pedir el servicio sin llamar ni escribir por WhatsApp.

- El formulario crea un `Cliente` (o reutiliza uno con el mismo correo) y un `Tramite` en estado Solicitado.
- Muestra una pantalla de confirmación con el número de solicitud.

### HU-12 — Historial del cliente · **Could**
**Como** gestor, **quiero** ver todos los trámites de un cliente en su ficha **para** atenderlo con contexto.

### HU-13 — Usuarios y roles · **Could**
**Como** administrador, **quiero** crear cuentas de gestores y asignarles rol **para** controlar quién hace qué.

### HU-14 y HU-15 — **Won't (versión futura)**
Consulta pública con código de seguimiento y portal para aliados. Quedan documentadas para futuras iteraciones.
