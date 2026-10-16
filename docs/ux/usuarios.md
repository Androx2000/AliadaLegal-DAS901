# Identificación de usuarios

**Proyecto:** Aliada Legal — Sistema web de gestión de trámites legales y migratorios internacionales
**Responsable:** Blanca Rivas (Integrante 1 — UX y documentación)
**Rama:** `feature/ux-documentacion`

---

## 1. Contexto del sistema

Aliada Legal es una agencia intermediaria que gestiona documentos legales en el país de origen de personas migrantes y de bufetes de abogados aliados: partidas de nacimiento, antecedentes penales, actas notariales, traducciones certificadas y apostillas.

Hoy el proceso se maneja con mensajes, hojas de cálculo y correos. Por eso es difícil saber en qué etapa va cada trámite, qué aliado lo tiene asignado y cuándo se envió el documento. El sistema centraliza esa información en un panel web construido con ASP.NET Core MVC y permite que el cliente consulte el estado de su trámite sin iniciar sesión.

## 2. Roles del sistema (RF05)

Se usan los tres roles definidos en los requisitos de la Fase 1. Son los mismos nombres que se registran en ASP.NET Core Identity y los que se usan en los atributos `[Authorize(Roles = "...")]` de los controladores.

| Rol / usuario | Tipo | Cómo accede | Relación con el modelo de datos |
|---|---|---|---|
| **Administrador** | Interno | Login | `ApplicationUser` con rol `Administrador` |
| **Asesor** | Interno | Login | `ApplicationUser` con rol `Asesor` |
| **Aliado** (abogado / notario) | Externo con cuenta | Login (vista restringida) | `ApplicationUser` con rol `Aliado`, vinculado a un registro `Aliado` |
| **Cliente / visitante** | Externo sin cuenta | Sitio público: consulta de seguimiento (RF06) | `Cliente` (`TipoCliente.Individual` o `TipoCliente.Bufete`) |

> El cliente no es un rol de Identity. No necesita cuenta: consulta su trámite con el **código de seguimiento** que le entrega la agencia.

## 3. Perfiles (personas)

### 3.1 Administrador — "Marta, dueña de la agencia"

- **Contexto:** 30–45 años. Dirige la agencia y atiende clientes en inglés y español.
- **Objetivos:** ver de un vistazo cuántos trámites hay en cada estado, controlar a los aliados y mantener al día los catálogos de países y tipos de documento.
- **Frustraciones:** la información está repartida en WhatsApp y Excel, y tiene que preguntar a cada aliado cómo va cada documento.
- **Nivel tecnológico:** medio-alto. **Dispositivo:** laptop. Revisa desde el celular de forma ocasional.
- **Permisos:** control total. Usuarios y roles, catálogos, clientes, trámites, aliados, envíos y reportes.

### 3.2 Asesor — "Carlos, asesor de trámites"

- **Contexto:** 22–35 años. Atiende a los clientes y da seguimiento diario a los casos.
- **Objetivos:** registrar clientes y trámites rápido, asignar aliados, actualizar estados, registrar el envío y entregar el código de seguimiento al cliente.
- **Frustraciones:** escribir el mismo dato varias veces y no tener un historial claro del caso.
- **Nivel tecnológico:** medio. **Dispositivo:** laptop.
- **Permisos:** clientes, trámites, asignación de aliados y envíos. Ve los aliados en modo de solo lectura. No administra usuarios ni catálogos.

### 3.3 Aliado — "Lic. Hernández, notario en El Salvador"

- **Contexto:** abogado o notario que obtiene físicamente el documento en el país de origen.
- **Objetivos:** saber qué trámites tiene asignados y avisar a la agencia cuando ya obtuvo o apostilló el documento.
- **Frustraciones:** recibir los encargos por mensajes sueltos y tener que reportar el avance por teléfono.
- **Nivel tecnológico:** medio. **Dispositivo:** celular y laptop.
- **Permisos (alcance de esta fase):** inicia sesión y ve **solo los trámites asignados a él**. Puede ver el detalle y actualizar el estado dentro de su etapa (En gestión → Documento obtenido → Apostillado) y agregar observaciones. No ve otros trámites ni datos de contacto completos de los clientes, y no administra nada.
- **Cómo se atiende el requisito:** no se construye un portal independiente. Se usa el mismo panel MVC con una vista filtrada ("Mis trámites") protegida con `[Authorize(Roles = "Aliado")]`.

### 3.4 Cliente individual — "Rosa, migrante en EE. UU."

- **Contexto:** 25–60 años. Necesita su partida de nacimiento apostillada para un proceso migratorio.
- **Objetivos:** saber en qué etapa va su trámite y cuándo le va a llegar, sin tener que escribirle a nadie.
- **Frustraciones:** no saber en qué va su trámite y desconfiar de intermediarios.
- **Nivel tecnológico:** bajo-medio. Usa principalmente el celular.
- **Necesidades UX:** una sola caja para escribir el código, lenguaje sencillo en español, estado claro con color y texto, y que funcione bien en el celular.

### 3.5 Cliente bufete — "Lic. Jenkins, bufete de inmigración"

- **Contexto:** paralegal o abogado de un bufete que tramita documentos para varios clientes a la vez.
- **Objetivos:** consultar el estado de varios trámites para cumplir sus fechas de corte.
- **Frustraciones:** retrasos sin aviso y falta de números de rastreo.
- **Nivel tecnológico:** alto.
- **Necesidades UX:** datos precisos, estados claros y comunicación profesional.

## 4. Matriz de permisos por módulo

| Módulo | Administrador | Asesor | Aliado | Cliente (público) |
|---|:---:|:---:|:---:|:---:|
| Inicio y servicios | ✔ | ✔ | ✔ | ✔ |
| **Consulta de seguimiento (RF06)** | ✔ | ✔ | ✔ | ✔ |
| Dashboard | ✔ | ✔ | — | — |
| Clientes (CRUD) | ✔ | ✔ | — | — |
| Trámites (CRUD y cambio de estado) | ✔ | ✔ | — | — |
| Asignar aliado | ✔ | ✔ | — | — |
| Envíos | ✔ | ✔ | — | — |
| **Mis trámites asignados** | — | — | ✔ | — |
| Actualizar estado de un trámite asignado | ✔ | ✔ | ✔ (solo su etapa) | — |
| Aliados (CRUD) | ✔ | Solo lectura | — | — |
| Catálogos (países y tipos de documento) | ✔ | — | — | — |
| Usuarios y roles | ✔ | — | — | — |

## 5. Datos que ve cada usuario sobre un trámite

| Dato | Administrador / Asesor | Aliado | Cliente (consulta pública) |
|---|:---:|:---:|:---:|
| Código de seguimiento | ✔ | ✔ | ✔ |
| Tipo de documento y país | ✔ | ✔ | ✔ |
| Estado y fecha de solicitud | ✔ | ✔ | ✔ |
| Nombre del cliente | ✔ | ✔ | — |
| Correo y teléfono del cliente | ✔ | — | — |
| Aliado asignado | ✔ | ✔ | — |
| Envío (empresa y número de rastreo) | ✔ | — | — |
| Observaciones internas | ✔ | ✔ | — |

La consulta pública devuelve solo los campos de `TramiteSeguimientoDto` (código, tipo de documento, país, estado y fecha de solicitud), para no exponer datos personales a quien conozca un código. Mostrar el número de rastreo al cliente queda como mejora futura.
