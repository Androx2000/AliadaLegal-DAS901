# Manual de estilos — Aliada Legal

**Proyecto:** Aliada Legal — ASP.NET Core MVC (.NET 8)
**Responsable:** Integrante 2 — UI y vistas Razor (rama `feature/ui-vistas`)
**Archivos:** `wwwroot/css/site.css`, `wwwroot/js/site.js`, `Views/Shared/_Layout.cshtml`, `TagHelpers/`, `Helpers/EstadoTramiteUi.cs`

Este manual define la identidad visual y los componentes que usan **todas** las vistas. Quien cree una vista nueva (por ejemplo `Views/Aliados` o `Views/Account`) debe usar estas clases para que la interfaz sea coherente.

---

## 1. Principios

1. **Confianza y formalidad:** azul marino como color principal y dorado como acento (sellos, apostillas).
2. **Claridad:** una acción principal por pantalla, botón primario a la derecha (en móvil, arriba).
3. **Estado siempre visible:** cada trámite muestra su estado con color **y** texto.
4. **Mobile first** en lo público: el cliente migrante consulta desde el celular.
5. **Accesibilidad AA:** contraste mínimo 4.5:1, foco visible, etiquetas en todos los campos.

## 2. Colores

| Token CSS | Hex | Uso |
|---|---|---|
| `--al-azul` | `#1f3a5f` | Color primario: barra de navegación, botones primarios, títulos |
| `--al-azul-oscuro` | `#152a46` | Hover de botones, pie de página |
| `--al-azul-claro` | `#e8eef6` | Fondo del código de seguimiento |
| `--al-dorado` | `#b8862f` | Acento: botón «Ingresar», «Guardar envío», etapa actual, foco |
| `--al-dorado-claro` | `#f6eedd` | Fondo de íconos de servicios |
| `--al-fondo` | `#f5f6f8` | Fondo de página |
| `--al-superficie` | `#ffffff` | Tarjetas, inputs, tablas |
| `--al-borde` | `#dde2e8` | Bordes y separadores |
| `--al-texto` | `#1c2430` | Texto principal |
| `--al-texto-suave` | `#5b6675` | Texto secundario, etiquetas |
| `--al-exito` | `#1e7b45` | Mensajes de éxito |
| `--al-error` | `#b42318` | Errores de validación |

### 2.1 Colores de estado del trámite

| Estado (`EstadoTramite`) | Texto mostrado | Clase CSS | Color | Ícono |
|---|---|---|---|---|
| `Solicitado` | Solicitado | `estado-solicitado` | Gris `#5f6b7a` | `bi-inbox` |
| `EnGestion` | En gestión | `estado-engestion` | Azul `#1d5fbf` | `bi-hourglass-split` |
| `DocumentoObtenido` | Documento obtenido | `estado-documentoobtenido` | Cian `#0b7285` | `bi-file-earmark-check` |
| `Apostillado` | Apostillado | `estado-apostillado` | Morado `#6741b5` | `bi-patch-check` |
| `EnTransito` | En tránsito | `estado-entransito` | Naranja `#b35c00` | `bi-truck` |
| `Entregado` | Entregado | `estado-entregado` | Verde `#1e7b45` | `bi-check-circle` |

Todos los pares texto/fondo cumplen contraste AA. La clase CSS es `estado-` + el nombre del enum en minúsculas, así que el servicio puede generarla con `estado.ToString().ToLowerInvariant()`.

## 3. Tipografía

Fuentes locales en `wwwroot/lib/fonts` (no dependen de Internet):

| Uso | Fuente | Peso |
|---|---|---|
| Títulos `h1`–`h3`, marca | **Lora** (serif) | 600–700 |
| Texto, formularios, tablas | **Inter** (sans-serif) | 400–700 |
| Códigos de seguimiento y guías | Monoespaciada del sistema (`.texto-mono`) | 700 |

Tamaño base: 15 px en móvil, 16 px desde 768 px.

## 4. Espaciado, bordes y sombras

- Radio de tarjetas `--al-radio` (10 px); radio de botones e inputs `--al-radio-sm` (6 px).
- Separación entre tarjetas: `g-3` (1 rem). Padding interno de tarjetas: 1.25 rem (móvil) / 1.5 rem (escritorio).
- Sombra suave `--al-sombra` en tarjetas; `--al-sombra-lg` en menús desplegables.

## 5. Diseño responsive

Se usa el grid de **Bootstrap 5.3** con estos puntos de quiebre:

| Ancho | Comportamiento |
|---|---|
| < 576 px | Una columna. Botones de acción a todo el ancho. Formularios en una columna. |
| < 768 px | Las tablas `.al-tabla-movil` se convierten en tarjetas (cada celda muestra su `data-etiqueta`). |
| < 992 px | Menú colapsado en botón ☰. El panel de acciones del trámite pasa arriba de los datos. |
| ≥ 992 px | Menú horizontal. Detalle de trámite en dos columnas (8 + 4) con panel de acciones fijo (`.sticky-lateral`). |

Regla: ninguna pantalla debe tener scroll horizontal a 390 px de ancho (verificado en los mockups).

## 6. Componentes

### 6.1 Encabezado de página

```html
<div class="encabezado-pagina">
    <div>
        <nav class="migas" aria-label="Ruta"><a asp-action="Index">Clientes</a> / Nuevo</nav>
        <h1>Nuevo cliente</h1>
        <p class="texto-suave mb-0">Descripción corta opcional.</p>
    </div>
    <div class="acciones">
        <a class="btn btn-primary" asp-action="Create"><i class="bi bi-plus-lg me-1"></i>Nuevo</a>
    </div>
</div>
```

### 6.2 Tarjeta

```html
<div class="al-card">
    <div class="al-card-header"><h2>Título</h2></div>
    <div class="al-card-body">Contenido</div>
</div>
```

### 6.3 Botones

| Clase | Uso |
|---|---|
| `btn btn-primary` | Acción principal (Guardar, Crear, Consultar) |
| `btn btn-outline-primary` | Acciones secundarias (Editar, Filtrar, Asignar) |
| `btn btn-outline-secondary` | Cancelar / volver |
| `btn btn-acento` | Acción destacada de marca (Ingresar, Guardar envío) |

Los íconos son de **Bootstrap Icons** (`<i class="bi bi-…" aria-hidden="true"></i>`). Si el botón solo tiene ícono, agregue texto con `visually-hidden`.

### 6.4 Insignia de estado — TagHelper `<estado-badge>`

```html
<!-- Con el enum (vistas internas) -->
<estado-badge estado="@item.Estado" />

<!-- Con texto y clave (consulta pública, DTO) -->
<estado-badge texto="@dto.Estado" clave="@dto.EstadoClave" />

<!-- Sin ícono -->
<estado-badge estado="@h.Estado" icono="false" />
```

Genera `<span class="estado-badge estado-apostillado"><i class="bi bi-patch-check"></i>Apostillado</span>`. Código: `TagHelpers/EstadoBadgeTagHelper.cs` y `Helpers/EstadoTramiteUi.cs`.

### 6.5 Enlace de menú activo — TagHelper `nav-activo`

```html
<a class="nav-link" asp-controller="Clientes" asp-action="Index" nav-activo>Clientes</a>
```

Agrega `class="active"` y `aria-current="page"` cuando el controlador actual coincide.

### 6.6 Tablas responsivas

```html
<div class="table-responsive">
  <table class="table al-tabla al-tabla-movil">
    <thead><tr><th scope="col">Nombre</th>…</tr></thead>
    <tbody>
      <tr>
        <td data-etiqueta="Nombre">…</td>
        <td class="celda-acciones text-end">[botones]</td>
      </tr>
    </tbody>
  </table>
</div>
```

Cada `<td>` necesita `data-etiqueta` para que en móvil se vea «Etiqueta ……… valor».

### 6.7 Formularios

```html
<form asp-action="Create" method="post" novalidate data-evitar-doble-envio>
    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>
    <section class="seccion-form">
        <h2>Datos personales</h2>
        <label asp-for="Nombre" class="form-label">Nombre<span class="requerido">*</span></label>
        <input asp-for="Nombre" class="form-control" />
        <span asp-validation-for="Nombre"></span>
    </section>
    <div class="form-acciones">
        <a asp-action="Index" class="btn btn-outline-secondary">Cancelar</a>
        <button type="submit" class="btn btn-primary">Guardar</button>
    </div>
</form>
@section Scripts { <partial name="_ValidationScriptsPartial" /> }
```

- Validación en cliente con **jQuery Validation Unobtrusive** (mensajes en español en `_ValidationScriptsPartial`). La validación en servidor la hacen las DataAnnotations del ViewModel.
- Listas desplegables en lugar de texto libre para cliente, país, tipo de documento, aliado y enums.
- Fechas con `<input type="date" asp-format="{0:yyyy-MM-dd}">`.

### 6.8 Mensajes (TempData)

El layout incluye `_Mensajes.cshtml`, que muestra automáticamente:

```csharp
TempData["Exito"] = "Cliente registrado correctamente.";   // alerta verde, se cierra a los 6 s
TempData["Error"] = "Asigne un aliado antes de continuar."; // alerta roja
```

### 6.9 Otros componentes

| Componente | Clase / archivo | Uso |
|---|---|---|
| Código de seguimiento | `.codigo-seguimiento` | Muestra `AL-2026-K7M2QX` destacado |
| Barra de progreso | `.al-progreso > span` | Avance del trámite en % |
| Línea de tiempo | `ol.linea-tiempo > li.completado / .actual / .pendiente` | Historial y etapas públicas |
| Datos en lista | `dl.datos` | Ficha de cliente y trámite (2 columnas en ≥ 576 px) |
| Estado vacío | `_EstadoVacio.cshtml` | `<partial name="_EstadoVacio" model="@(("bi-people", "Título", "Texto"))" />` |
| Filtros | `.barra-filtros` | Buscador + select + botón |

### 6.10 Comportamientos JavaScript (`site.js`)

| Atributo | Efecto |
|---|---|
| `data-evitar-doble-envio` (en `<form>`) | Deshabilita el botón y muestra «Procesando…» |
| `data-confirmar="¿Seguro?"` | Pide confirmación antes de enviar |
| `data-copiar="texto"` | Copia al portapapeles (enlace público del trámite) |
| `data-mayusculas` (en `<input>`) | Convierte a mayúsculas y quita espacios (código de seguimiento) |
| `data-autoenviar` (en `<select>`) | Envía el formulario al cambiar (filtro de estado) |
| `data-autocerrar` (en alerta) | Cierra la alerta de éxito a los 6 s |

## 7. Accesibilidad

- Enlace «Saltar al contenido» al inicio de cada página.
- `lang="es"` en el documento; `<label>` en todos los campos; `aria-label` en botones de solo ícono.
- Foco visible dorado (`:focus-visible`).
- Resultado de la consulta pública dentro de `aria-live="polite"`; la etapa actual tiene `aria-current="step"`.
- Barras de progreso con `role="progressbar"` y `aria-valuenow`.
- Se respeta `prefers-reduced-motion`.
- Estilos de impresión: se ocultan menú, pie y formularios (botón «Imprimir» del trámite).

## 8. Logo e imágenes

- `wwwroot/images/logo.svg` — isotipo (sello con la «A»). También es el favicon.
- `wwwroot/images/logo-horizontal.svg` — logo con nombre, para documentos.
- Toda imagen informativa lleva `alt`; las decorativas, `alt=""`.

---

## Anexo A. Contrato de ViewModels (coordinación con controladores)

Las vistas usan las clases que envían los controladores de la rama `feature/controladores-servicios`. Estas son las propiedades que **cada vista espera**. Si alguna cambia de nombre, avisar para ajustar la vista.

Espacios de nombres en `_ViewImports.cshtml`: `AliadaLegal.Web.Models`, `.ViewModels`, `.Dtos`, `.Helpers`.

| Vista | `@model` | Propiedades usadas |
|---|---|---|
| `Clientes/Index` | `ClienteListaViewModel` | `Texto`, `Clientes` (`List<ClienteItemViewModel>`) |
| ↳ ítem | `ClienteItemViewModel` | `Id`, `Nombre`, `Correo`, `Telefono`, `TipoCliente`, `PaisOrigen`, `TotalTramites` |
| `Clientes/Details` | `ClienteDetalleViewModel` | `Id`, `Nombre`, `Correo`, `Telefono`, `CanalContacto`, `TipoCliente`, `PaisOrigen`, `FechaRegistro` (DateTime), `Tramites` (`List<TramiteItemViewModel>`) |
| `Clientes/Create`, `Edit` | `ClienteFormViewModel` | `Id`, `Nombre`, `Correo`, `Telefono`, `TipoCliente` (enum `TipoCliente`), `CanalContacto` (**string**: WhatsApp, Correo, Llamada, SMS), `PaisOrigenId`, `Paises` (`IEnumerable<SelectListItem>`) |
| `Aliados/Index` | `AliadoListaViewModel` | `Aliados` (`List<AliadoItemViewModel>`) |
| ↳ ítem | `AliadoItemViewModel` | `Id`, `Nombre`, `Correo`, `Telefono`, `TipoAliado`, `PaisAsignado`, `Activo` (bool) |
| `Aliados/Create`, `Edit` | `AliadoFormViewModel` | `Id`, `Nombre`, `Correo`, `Telefono`, `TipoAliado` (enum `TipoAliado`), `PaisAsignadoId`, `Activo` (bool), `Paises` (`IEnumerable<SelectListItem>`) |
| `Tramites/Index` | `TramiteListaViewModel` | `Texto`, `Estado` (`EstadoTramite?`), `EsAliado`, `Tramites` (`List<TramiteItemViewModel>`) |
| ↳ ítem | `TramiteItemViewModel` | `Id`, `Codigo`, `Cliente`, `TipoDocumento`, `PaisDestino`, `Estado` (`EstadoTramite`), `Aliado` (string?), `FechaSolicitud` (DateTime) |
| `Tramites/Create` | `TramiteFormViewModel` | `ClienteId`, `TipoDocumentoId`, `PaisDestinoId`, `Observaciones`, `Clientes`, `TiposDocumento`, `Paises` (`IEnumerable<SelectListItem>`) |
| `Tramites/Details` | `TramiteDetalleViewModel` | Las que arma `ConstruirDetalleAsync` en `TramitesController`. `Progreso` = entero 0–100. `Historial` = `List<HistorialItemViewModel>` (`Estado`, `Fecha`, `Responsable`, `Comentario`) |
| ↳ parcial | `AsignarAliadoViewModel` | `TramiteId`, `AliadoId` (int?), **`Aliados`** (`IEnumerable<SelectListItem>`) |
| ↳ parcial | `CambiarEstadoViewModel` | `TramiteId`, `NuevoEstado` (`EstadoTramite`), `Comentario` |
| ↳ parcial | `EnvioFormViewModel` | `TramiteId`, `EmpresaEnvio`, `NumeroGuia`, `FechaDespacho` (DateTime?) |
| `Seguimiento/Index` | `ConsultaSeguimientoViewModel` | `Codigo`, `Buscado`, `Resultado` (`TramiteSeguimientoDto?`) |
| ↳ DTO | `TramiteSeguimientoDto` | `CodigoSeguimiento`, `TipoDocumento`, `PaisDestino`, `Estado`, `EstadoClave`, `Progreso`, `FechaSolicitud`, `UltimaActualizacion`, `EmpresaEnvio`, `NumeroGuia`, `LineaDeTiempo` (`List<HitoSeguimientoDto>`: `Estado`, `EstadoClave`, `Fecha?`, `Completado`, `EsActual`) |

Notas para el equipo:

- Los enums deben tener `[Display(Name = "…")]` en cada valor para que los selects muestren el texto bonito (por ejemplo `[Display(Name = "Bufete de abogados")] Bufete`).
- `AsignarAliadoViewModel` de la rama `feature/modelos-datos` no tiene la lista `Aliados` ni `AliadoId` nullable; el controlador sí los usa. Hay que unificarlo.
- El menú usa los nombres de rol `"Administrador"`, `"Asesor"` y `"Aliado"` (los mismos de la documentación UX).
- `CanalContacto` se maneja como `string`, igual que en el modelo `Cliente`; el formulario ofrece opciones fijas (WhatsApp, Correo, Llamada, SMS).
- Las vistas de `Aliados` (Index, Create, Edit) están en esta rama. Create y Edit solo se muestran al rol Administrador; el Asesor ve la lista en solo lectura.
