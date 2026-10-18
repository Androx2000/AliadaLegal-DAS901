# Wireframes — Aliada Legal

**Responsable:** Integrante 2 — UI y vistas Razor
**Nivel:** baja fidelidad (estructura y jerarquía, sin color). Los mockups de alta fidelidad están en [`../mockups`](../mockups/README.md).

Convenciones: `[ Botón ]` · `( Campo )` · `▾` lista desplegable · `●` estado / insignia · `☰` menú hamburguesa.
Cada pantalla tiene versión **escritorio (≥ 992 px)** y **móvil (≤ 576 px)**.

---

## WF-01 · Layout compartido (`_Layout.cshtml`)

```
ESCRITORIO
┌──────────────────────────────────────────────────────────────────────────┐
│ (A) Aliada Legal   Inicio  Clientes  Trámites  Aliados  Seguimiento   👤 Nombre [ROL] ▾ │
├──────────────────────────────────────────────────────────────────────────┤
│  [ Mensaje TempData: éxito / error  ✕ ]                                   │
│                                                                          │
│                         @RenderBody()                                    │
│                                                                          │
├──────────────────────────────────────────────────────────────────────────┤
│ © Aliada Legal                               Consultar trámite · Privacidad │
└──────────────────────────────────────────────────────────────────────────┘

MÓVIL
┌──────────────────────────┐
│ (A) Aliada Legal      ☰  │  ← el menú se despliega hacia abajo
├──────────────────────────┤
│ [ Mensaje ✕ ]            │
│      @RenderBody()       │
├──────────────────────────┤
│ © Aliada Legal           │
│ Consultar · Privacidad   │
└──────────────────────────┘
```

Menú según rol: **Público** → Inicio, Servicios, Seguimiento, [Ingresar] · **Administrador/Asesor** → Inicio, Clientes, Trámites, Aliados, Seguimiento · **Aliado** → Mis trámites, Seguimiento.

---

## WF-02 · Inicio público (`Home/Index`)

```
ESCRITORIO
┌────────────────────────────────────────────────────────────────┐
│ TRÁMITES LEGALES Y MIGRATORIOS          ┌──────────────────────┐ │
│ Sus documentos del país de origen,      │ ¿Ya tiene un código? │ │
│ sin viajar.                             │ (AL-2026-XXXXXX) [→] │ │
│ Texto de presentación…                  └──────────────────────┘ │
│ [ Consultar mi trámite ] [ Ver servicios ]                        │
└────────────────────────────────────────────────────────────────┘
Servicios
┌──────────┐ ┌──────────┐ ┌──────────┐
│ ▣ Partidas│ │ ▣ Antec. │ │ ▣ Actas  │   (3 columnas → 2 en tablet → 1 en móvil)
└──────────┘ └──────────┘ └──────────┘
┌──────────┐ ┌──────────┐ ┌──────────┐
│ ▣ Apostil.│ │ ▣ Traduc.│ │ ▣ Envío  │
└──────────┘ └──────────┘ └──────────┘
¿Cómo funciona?   ① Solicite  ② Gestionamos  ③ Apostillamos  ④ Enviamos
```

Usuario con sesión: la misma ruta muestra el **Panel de trabajo** con accesos a Clientes, Trámites y Aliados, y los botones [Nuevo cliente] [Nuevo trámite].

---

## WF-03 · Consulta de seguimiento (`Seguimiento/Index`) — *mobile first*

```
MÓVIL                                  RESULTADO
┌──────────────────────────┐          ┌──────────────────────────┐
│   Consulte su trámite    │          │ Trámite        ● Estado  │
│ Escriba su código…       │          │ AL-2026-K7M2QX           │
│ ┌──────────────────────┐ │          │ Documento: …             │
│ │ Código de seguimiento│ │          │ País: …                  │
│ │ (AL-2026-XXXXXX)     │ │          │ Fecha solicitud: …       │
│ │ [     Consultar     ]│ │          │ Avance ▓▓▓▓▓▓░░░ 67 %    │
│ │ Ejemplo: AL-2026-…   │ │          │ ETAPAS                   │
│ └──────────────────────┘ │          │ ✔ Solicitado   01/10     │
└──────────────────────────┘          │ ✔ En gestión   02/10     │
                                      │ ✔ Doc. obtenido 08/10    │
NO ENCONTRADO                         │ ◉ Apostillado  13/10     │
┌──────────────────────────┐          │ ○ En tránsito  Pendiente │
│  ?  No encontramos ese   │          │ ○ Entregado    Pendiente │
│     código. Revise…      │          └──────────────────────────┘
└──────────────────────────┘          🔒 No muestra datos personales
```

En escritorio la tarjeta se centra con ancho máximo de 7 columnas.

---

## WF-04 · Lista de clientes (`Clientes/Index`)

```
ESCRITORIO
Clientes                                              [ + Nuevo cliente ]
┌──────────────────────────────────────────────────────────────────────┐
│ (🔍 Buscar por nombre, correo o teléfono)  [Buscar]  Limpiar          │
├──────────────┬───────────────┬──────────┬──────────┬────────┬────────┤
│ NOMBRE       │ CONTACTO      │ TIPO     │ PAÍS     │TRÁMITES│        │
├──────────────┼───────────────┼──────────┼──────────┼────────┼────────┤
│ María José H.│ correo / tel  │Individual│El Salv.  │   2    │ 👁 ✎ + │
└──────────────┴───────────────┴──────────┴──────────┴────────┴────────┘

MÓVIL (cada fila se vuelve tarjeta)
┌──────────────────────────┐
│ Nombre        María José │
│ Contacto   correo / tel  │
│ Tipo          Individual │
│ País         El Salvador │
│ Trámites               2 │
│                 [👁][✎][+]│
└──────────────────────────┘
```

## WF-05 · Formulario de cliente (`Clientes/Create`, `Clientes/Edit`)

```
Clientes / Nuevo
Nuevo cliente
┌──────────────────────────────────────────────┐
│ [! Error general del servidor]               │
│ DATOS PERSONALES                             │
│ Nombre completo *  (                       ) │
│ Tipo de cliente * (▾)   País de origen * (▾) │
│ CONTACTO                                     │
│ Correo *  (          )  Teléfono * (       ) │
│ Canal preferido (▾)                          │
│ ──────────────────────────────────────────── │
│                     [ Cancelar ] [ Guardar ] │
└──────────────────────────────────────────────┘
Móvil: una columna; botones a todo el ancho, Guardar arriba de Cancelar.
```

## WF-06 · Detalle de cliente (`Clientes/Details`) — RF07

```
Clientes / Detalle
María José Hernández                          [ Editar ] [ + Nuevo trámite ]
┌──────────────────┐ ┌─────────────────────────────────────────────┐
│ Datos del cliente│ │ Historial de trámites                    (2) │
│ Tipo             │ │ CÓDIGO       DOCUMENTO   PAÍS   ●ESTADO FECHA│
│ País             │ │ AL-2026-…    Partida     SV     ●Apost. 01/10│
│ Correo / Teléfono│ │ AL-2026-…    Matrimonio  SV     ●Tráns. 20/09│
│ Canal / Registro │ └─────────────────────────────────────────────┘
└──────────────────┘
Móvil: la ficha va arriba y el historial debajo.
```

---

## WF-07 · Lista de trámites (`Tramites/Index`)

```
Trámites                                              [ + Nuevo trámite ]
┌──────────────────────────────────────────────────────────────────────┐
│ Buscar (                     )   Estado (▾ Todos)   [Filtrar] Limpiar │
├────────┬─────────┬──────────┬──────┬──────────┬────────┬───────┬─────┤
│ CÓDIGO │ CLIENTE │ DOCUMENTO│ PAÍS │ ●ESTADO  │ ALIADO │ FECHA │     │
├────────┼─────────┼──────────┼──────┼──────────┼────────┼───────┼─────┤
│AL-2026…│ María J.│ Partida  │  SV  │●Apostill.│ Ana P. │ 01/10 │Abrir│
└────────┴─────────┴──────────┴──────┴──────────┴────────┴───────┴─────┘
Rol Aliado: título «Mis trámites», sin botón Nuevo ni columna Aliado.
```

## WF-08 · Nuevo trámite (`Tramites/Create`)

```
Trámites / Nuevo
┌──────────────────────────────────────────────┐
│ SOLICITUD                                    │
│ Cliente * (▾ Seleccione un cliente…)         │
│   ¿No aparece? Registre un cliente nuevo     │
│ Tipo de documento * (▾)   País * (▾)         │
│ Observaciones (                            ) │
│ ⓘ Al guardar se genera el código de seguimiento│
│                    [ Cancelar ] [ Crear ]    │
└──────────────────────────────────────────────┘
```

## WF-09 · Detalle de trámite (`Tramites/Details`) — centro de la navegación

```
ESCRITORIO
Trámites / Detalle
[AL-2026-K7M2QX] ● Apostillado            [Copiar enlace para el cliente] [Imprimir]
Partida de nacimiento · El Salvador
┌──────────────────────────────────────────────────────────────────────┐
│ Progreso ▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░  67 %   Solicitado … · Actualizado …      │
└──────────────────────────────────────────────────────────────────────┘
┌──────────────────────────────────────┐ ┌────────────────────────────┐
│ Datos del trámite                    │ │ Asignar / reasignar aliado │
│ Cliente        Contacto              │ │ (▾ Aliado del país) [Asignar]│
│ Documento      País                  │ ├────────────────────────────┤
│ Aliado         Envío                 │ │ Cambiar estado             │
│ Observaciones internas               │ │ Siguiente: ● En gestión    │
├──────────────────────────────────────┤ │ (Comentario)               │
│ Historial de estados                 │ │ [ Marcar como «…» ]        │
│ ● Solicitado  fecha · responsable    │ ├────────────────────────────┤
│ │ comentario                         │ │ Registrar envío            │
│ ● En gestión  …                      │ │ (Empresa) (Guía) (Fecha)   │
│ ◉ Apostillado …                      │ │ [ Guardar envío ]          │
└──────────────────────────────────────┘ └────────────────────────────┘

MÓVIL: encabezado → progreso → tarjetas de acción → datos → historial.
Cada tarjeta de acción solo aparece si el controlador la habilita
(PuedeAsignarAliado, PuedeAvanzar, PuedeRegistrarEnvio).
```
