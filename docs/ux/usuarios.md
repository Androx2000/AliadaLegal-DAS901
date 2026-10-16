# Identificación de usuarios

**Proyecto:** Aliada Legal — Sistema web de gestión de trámites legales y migratorios internacionales
**Responsable:** Blanca Rivas (Integrante 1 — UX y documentación)
**Rama:** `feature/ux-documentacion`

---

## 1. Contexto del sistema

Aliada Legal es una agencia intermediaria que gestiona documentos legales en el país de origen de personas migrantes y de bufetes de abogados aliados: partidas de nacimiento, antecedentes penales, actas notariales, traducciones certificadas y apostillas.

Hoy el proceso se maneja con mensajes, hojas de cálculo y correos. Por eso es difícil saber en qué etapa va cada trámite, qué aliado lo tiene asignado y cuándo se envió el documento. El sistema centraliza esa información en un panel web construido con ASP.NET Core MVC.

## 2. Tipos de usuario

| Usuario | Tipo | Accede al sistema | Relación con el modelo de datos |
|---|---|---|---|
| Administrador | Interno | Sí (login) | `ApplicationUser` con rol **Administrador** |
| Gestor de trámites | Interno | Sí (login) | `ApplicationUser` con rol **Gestor** |
| Cliente individual | Externo | Sitio público (solicitud) | `Cliente` con `TipoCliente.Individual` |
| Cliente bufete | Externo | Sitio público (solicitud) | `Cliente` con `TipoCliente.Bufete` |
| Aliado (abogado / notario) | Externo indirecto | No en esta versión | `Aliado` con `TipoAliado.Abogado` / `Notario` |

## 3. Perfiles (personas)

### 3.1 Administrador — "Marta, dueña de la agencia"

- **Edad / contexto:** 30–45 años. Dirige la agencia y atiende clientes en inglés y español.
- **Objetivos:** ver de un vistazo cuántos trámites hay en cada estado, controlar a los aliados y mantener al día los catálogos de países y tipos de documento.
- **Frustraciones:** la información está repartida en WhatsApp y Excel, y tiene que preguntar a cada aliado cómo va cada documento.
- **Nivel tecnológico:** medio-alto. Usa la computadora todo el día.
- **Dispositivo principal:** laptop. Revisa desde el celular de forma ocasional.
- **Permisos:** todo. Usuarios, catálogos, clientes, trámites, aliados, envíos y reportes.

### 3.2 Gestor de trámites — "Carlos, asistente operativo"

- **Edad / contexto:** 22–35 años. Da seguimiento diario a los casos.
- **Objetivos:** registrar trámites rápido, asignar aliados, actualizar estados y registrar el número de rastreo del envío.
- **Frustraciones:** escribir el mismo dato varias veces y no tener un historial claro del caso.
- **Nivel tecnológico:** medio.
- **Dispositivo principal:** laptop.
- **Permisos:** clientes, trámites, asignación de aliados y envíos. No administra usuarios ni catálogos.

### 3.3 Cliente individual — "Rosa, migrante en EE. UU."

- **Edad / contexto:** 25–60 años. Necesita su partida de nacimiento apostillada para un proceso migratorio.
- **Objetivos:** pedir el documento sin viajar y saber cuándo le va a llegar.
- **Frustraciones:** no saber en qué va su trámite y desconfiar de intermediarios.
- **Nivel tecnológico:** bajo-medio. Usa principalmente el celular.
- **Necesidades UX:** formularios cortos, lenguaje sencillo en español, botones grandes y confirmación clara de que su solicitud se recibió.

### 3.4 Cliente bufete — "Lic. Jenkins, bufete de inmigración"

- **Contexto:** paralegal o abogado de un bufete que pide documentos para varios clientes a la vez.
- **Objetivos:** enviar varias solicitudes y recibir los documentos a tiempo para sus fechas de corte.
- **Frustraciones:** retrasos sin aviso y falta de números de rastreo.
- **Nivel tecnológico:** alto.
- **Necesidades UX:** datos precisos, estados claros y comunicación profesional.

### 3.5 Aliado — "Lic. Hernández, notario en El Salvador"

- **Rol:** obtiene físicamente el documento en el país de origen.
- **Interacción con el sistema:** indirecta. El Administrador o el Gestor lo registra y le asigna trámites. En una versión futura podría tener su propio portal.

## 4. Matriz de permisos por módulo

| Módulo | Administrador | Gestor | Visitante / Cliente |
|---|:---:|:---:|:---:|
| Inicio público y servicios | ✔ | ✔ | ✔ |
| Formulario de solicitud | — | — | ✔ |
| Dashboard | ✔ | ✔ | — |
| Clientes (CRUD) | ✔ | ✔ | — |
| Trámites (CRUD y cambio de estado) | ✔ | ✔ | — |
| Asignar aliado | ✔ | ✔ | — |
| Envíos | ✔ | ✔ | — |
| Aliados (CRUD) | ✔ | Solo lectura | — |
| Catálogos (Países, Tipos de documento) | ✔ | — | — |
| Usuarios y roles | ✔ | — | — |
