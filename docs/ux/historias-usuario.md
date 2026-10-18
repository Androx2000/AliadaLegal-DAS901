# Historias de usuario priorizadas

**Proyecto:** Aliada Legal
**Responsable:** Blanca Rivas (Integrante 1 — UX y documentación)

Formato: *Como [usuario], quiero [acción], para [beneficio].*
Prioridad con el método **MoSCoW**: **M** = Must (obligatoria), **S** = Should (importante), **C** = Could (deseable), **W** = Won't (fuera de esta versión).
Estimación en puntos de historia (escala Fibonacci).
Roles según RF05 de la Fase 1: **Administrador**, **Asesor** y **Aliado**. El cliente consulta sin cuenta (RF06).

---

## Resumen del backlog

| ID | Historia | Usuario | Requisito | Prioridad | Puntos |
|---|---|---|:---:|:---:|:---:|
| HU-01 | Iniciar sesión según rol | Administrador / Asesor / Aliado | RF05 | M | 3 |
| HU-02 | Registrar cliente | Administrador / Asesor | | M | 3 |
| HU-03 | Registrar trámite y generar código de seguimiento | Administrador / Asesor | RF06 | M | 5 |
| HU-04 | Cambiar estado del trámite | Administrador / Asesor | | M | 3 |
| HU-05 | Asignar aliado a un trámite | Administrador / Asesor | | M | 3 |
| HU-06 | Registrar envío con número de rastreo | Administrador / Asesor | | M | 3 |
| HU-07 | Listar y filtrar trámites | Administrador / Asesor | | M | 5 |
| HU-14 | **Consultar el estado con código de seguimiento** | Cliente (público) | **RF06** | **M** | 5 |
| HU-15 | **Ver y actualizar mis trámites asignados** | Aliado | **RF05** | **M** | 5 |
| HU-08 | Administrar aliados | Administrador | | S | 3 |
| HU-09 | Administrar catálogos (países y tipos de documento) | Administrador | | S | 3 |
| HU-10 | Ver dashboard por estado | Administrador / Asesor | | S | 5 |
| HU-11 | Enviar solicitud desde el sitio público | Cliente (público) | | S | 5 |
| HU-13 | Gestionar usuarios y asignar roles | Administrador | RF05 | S | 5 |
| HU-12 | Ver detalle e historial del cliente | Administrador / Asesor | | C | 3 |
| HU-16 | Portal completo para aliados (app propia, notificaciones) | Aliado | | W | 13 |

> Cambios respecto a la versión anterior: HU-14 pasó de *Won't* a *Must* porque corresponde al RF06. HU-15 se redefinió como la vista restringida del Aliado (RF05) y el portal completo quedó como HU-16 para una versión futura. El rol "Gestor" se reemplazó por **Asesor** en todo el documento.

---

## Detalle y criterios de aceptación

### HU-01 — Iniciar sesión según rol · **Must** · RF05
**Como** administrador, asesor o aliado, **quiero** iniciar sesión con mi correo y contraseña **para** acceder solo a las funciones de mi rol.

- El sistema valida correo y contraseña con ASP.NET Core Identity.
- Si los datos son incorrectos, muestra un mensaje genérico ("Correo o contraseña incorrectos").
- Después del login se redirige según el rol: Administrador y Asesor → **Dashboard**; Aliado → **Mis trámites**.
- Las rutas del panel no se pueden abrir sin sesión (`[Authorize]`) ni con un rol distinto (`[Authorize(Roles = "...")]`). Un rol sin permiso ve la página de **Acceso denegado**.
- Al cerrar sesión, regresa a la página de inicio.

### HU-02 — Registrar cliente · **Must**
**Como** asesor, **quiero** registrar los datos de un cliente **para** asociarle uno o más trámites.

- Campos obligatorios: nombre (mín. 3 caracteres), correo, teléfono (mín. 8 caracteres) y tipo de cliente (Individual / Bufete), según `CrearClienteViewModel`.
- El correo debe tener un formato válido. Los mensajes de error aparecen en español junto a cada campo.
- La fecha de registro se guarda automáticamente.
- Al guardar, se muestra un mensaje de éxito y se redirige al detalle del cliente.

### HU-03 — Registrar trámite y generar código de seguimiento · **Must** · RF06
**Como** asesor, **quiero** crear un trámite para un cliente indicando el país y el tipo de documento **para** empezar su gestión y darle al cliente un código con el que pueda consultarlo.

- Se selecciona el cliente, el país y el tipo de documento en listas desplegables (`CrearTramiteViewModel`).
- El estado inicial es **Solicitado** y la fecha de solicitud se asigna sola.
- El sistema genera un **código de seguimiento único** (formato propuesto: `AL-2026-000024`) y lo muestra en el detalle con un botón **Copiar**.
- El campo Observaciones es opcional (máx. 1000 caracteres).
- No se puede guardar sin cliente, país ni tipo de documento.

### HU-04 — Cambiar estado del trámite · **Must**
**Como** asesor, **quiero** actualizar el estado de un trámite **para** reflejar su avance real.

- Estados en orden: Solicitado → En gestión → Documento obtenido → Apostillado → En tránsito → Entregado.
- El estado se ve como una etiqueta de color en la lista y en el detalle.
- Para pasar a **En tránsito**, el trámite debe tener un envío registrado.
- El cambio se refleja de inmediato en la consulta pública.

### HU-05 — Asignar aliado · **Must**
**Como** asesor, **quiero** asignar un abogado o notario aliado a un trámite **para** que obtenga el documento en el país de origen.

- Solo se muestran aliados **activos**.
- Se puede cambiar el aliado mientras el trámite no esté Entregado.
- El trámite aparece de inmediato en **Mis trámites** del aliado asignado.

### HU-06 — Registrar envío · **Must**
**Como** asesor, **quiero** registrar la empresa de transporte y el número de rastreo **para** dar seguimiento a la entrega.

- Cada trámite tiene **como máximo un envío** en esta versión. Si ya existe, el botón cambia a **Editar envío**.
- Campos obligatorios: empresa de transporte y número de rastreo.
- La fecha de envío se guarda automáticamente. La fecha de entrega se completa después.
- Al registrar la fecha de entrega, el trámite puede marcarse como **Entregado**.

### HU-07 — Listar y filtrar trámites · **Must**
**Como** asesor, **quiero** ver todos los trámites y filtrarlos por estado, país o cliente **para** encontrar rápido el caso que necesito.

- Tabla con columnas: código, cliente, documento, país, estado, aliado y fecha.
- Filtros por estado, país y texto (nombre del cliente o código).
- En celular, la tabla se adapta (Bootstrap `table-responsive`).

### HU-14 — Consultar el estado con código de seguimiento · **Must** · RF06
**Como** cliente, **quiero** escribir mi código de seguimiento en el sitio web **para** saber en qué etapa va mi trámite sin iniciar sesión ni escribirle a la agencia.

- La página **Seguimiento** es pública (`[AllowAnonymous]`) y tiene una sola caja de texto y un botón **Consultar**.
- Si el código existe, muestra los datos de `TramiteSeguimientoDto`: código, tipo de documento, país, estado y fecha de solicitud.
- El estado se muestra con su etiqueta de color, su texto y una línea de tiempo con las 6 etapas, marcando la actual.
- Si el código no existe, muestra "No encontramos un trámite con ese código. Revisa que esté bien escrito." y conserva lo escrito.
- No se muestran datos personales (correo, teléfono, observaciones ni aliado).
- Funciona bien en celular y acepta el código en mayúsculas o minúsculas.

### HU-15 — Ver y actualizar mis trámites asignados · **Must** · RF05
**Como** aliado, **quiero** ver solo los trámites que me asignaron y marcar su avance **para** que la agencia y el cliente sepan en qué va el documento sin tener que llamar.

- Al iniciar sesión, el aliado llega a **Mis trámites**: solo trámites con su `AliadoId`.
- Columnas: código, documento, país, nombre del cliente, estado y fecha.
- En el detalle puede cambiar el estado **solo dentro de su etapa**: En gestión → Documento obtenido → Apostillado. No puede registrar envíos ni marcar Entregado.
- Puede agregar observaciones (ej. "Documento retirado en alcaldía").
- Si intenta abrir un trámite que no es suyo, el sistema responde **Acceso denegado**.

### HU-08 — Administrar aliados · **Should**
**Como** administrador, **quiero** registrar, editar y desactivar aliados **para** mantener actualizada mi red de abogados y notarios.

- Campos: nombre completo, correo, teléfono, tipo (Abogado / Notario), país y activo.
- Desactivar no borra al aliado. Solo lo oculta en la asignación de trámites nuevos y bloquea su acceso.

### HU-09 — Administrar catálogos · **Should**
**Como** administrador, **quiero** gestionar los países y tipos de documento **para** ofrecer nuevos servicios sin tocar el código.

- País: nombre y código ISO de 2 letras (ej. SV, GT, HN).
- Tipo de documento: nombre, descripción y activo.

### HU-10 — Dashboard · **Should**
**Como** administrador, **quiero** ver tarjetas con la cantidad de trámites por estado **para** conocer la carga de trabajo de un vistazo.

- Una tarjeta por estado, con su número y un enlace a la lista ya filtrada.
- Lista de los 5 trámites más recientes.

### HU-13 — Gestionar usuarios y asignar roles · **Should** · RF05
**Como** administrador, **quiero** crear cuentas y asignarles el rol Administrador, Asesor o Aliado **para** controlar quién hace qué.

- Los tres roles se crean al iniciar la aplicación (*seed*) si no existen.
- Al crear una cuenta con rol **Aliado**, se vincula con un registro de la tabla Aliados (por correo electrónico).

### HU-11 — Solicitud desde el sitio público · **Should**
**Como** cliente, **quiero** llenar un formulario con mis datos y el documento que necesito **para** pedir el servicio sin llamar ni escribir por WhatsApp.

- Crea un `Cliente` (o reutiliza uno con el mismo correo) y un `Tramite` en estado Solicitado, y muestra el código de seguimiento.

### HU-12 — Historial del cliente · **Could**
**Como** asesor, **quiero** ver todos los trámites de un cliente en su ficha **para** atenderlo con contexto.

### HU-16 — Portal completo para aliados · **Won't (versión futura)**
App o portal independiente con notificaciones y carga de archivos. En esta fase el requisito del rol Aliado se cubre con HU-15.
