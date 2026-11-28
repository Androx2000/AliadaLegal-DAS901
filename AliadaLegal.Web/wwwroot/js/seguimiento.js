// =====================================================================
// Aliada Legal — consulta de seguimiento con comunicación asíncrona (Fetch API).
// Llama a GET /api/tramites/seguimiento/{codigo} y pinta el resultado
// sin recargar la página. Si JavaScript falla, el formulario sigue
// funcionando de forma normal (mejora progresiva).
// =====================================================================
(function () {
    "use strict";

    var form = document.getElementById("form-seguimiento");
    var zona = document.getElementById("resultado-seguimiento");
    if (!form || !zona || !window.fetch) return;

    var urlApi = form.getAttribute("data-api");
    var input = form.querySelector("input[name='Codigo']");
    var boton = form.querySelector("button[type=submit]");
    var textoBoton = boton ? boton.innerHTML : "";

    function esc(valor) {
        var d = document.createElement("div");
        d.textContent = valor == null ? "" : String(valor);
        return d.innerHTML;
    }

    function fecha(iso) {
        if (!iso) return "";
        var f = new Date(iso);
        return isNaN(f) ? "" : f.toLocaleDateString("es-SV", { day: "2-digit", month: "2-digit", year: "numeric" });
    }

    var iconos = {
        solicitado: "bi-inbox", engestion: "bi-hourglass-split", documentoobtenido: "bi-file-earmark-check",
        apostillado: "bi-patch-check", entransito: "bi-truck", entregado: "bi-check-circle"
    };
    function clave(c) { return String(c || "").replace("estado-", "").replace(/\s+/g, "").toLowerCase(); }

    function pintarCargando() {
        zona.innerHTML = '<div class="al-card"><div class="vacio"><div class="spinner-border" role="status" aria-hidden="true"></div>' +
            '<p class="mt-3 mb-0">Consultando…</p></div></div>';
    }

    function pintarNoEncontrado() {
        zona.innerHTML = '<div class="al-card"><div class="vacio"><i class="bi bi-question-circle" aria-hidden="true"></i>' +
            '<h2 class="h5 mt-3">No encontramos ese código</h2>' +
            '<p class="mb-0">Revise que esté completo, por ejemplo <span class="texto-mono">AL-2026-K7M2QX</span>. ' +
            'Si el problema continúa, escríbanos y le ayudamos.</p></div></div>';
    }

    function pintarResultado(r) {
        var k = clave(r.estadoClave);
        var envio = r.numeroGuia
            ? '<div><dt>Envío</dt><dd>' + esc(r.empresaEnvio) + ' · <span class="texto-mono">' + esc(r.numeroGuia) + '</span></dd></div>'
            : "";

        var hitos = (r.lineaDeTiempo || []).map(function (h) {
            var clase = h.esActual ? "actual" : h.completado ? "completado" : "pendiente";
            var kh = clave(h.estadoClave);
            var punto = (h.completado && !h.esActual) ? '<i class="bi bi-check" aria-hidden="true"></i>' : "";
            var meta = h.fecha ? fecha(h.fecha) : (h.completado ? "" : "Pendiente");
            return '<li class="' + clase + '"' + (h.esActual ? ' aria-current="step"' : "") + '>' +
                '<span class="punto">' + punto + '</span>' +
                '<div class="titulo"><i class="bi ' + (iconos[kh] || "bi-circle") + ' me-1" aria-hidden="true"></i>' + esc(h.estado) +
                (h.esActual ? '<span class="visually-hidden">(etapa actual)</span>' : "") + '</div>' +
                '<div class="meta">' + esc(meta) + '</div></li>';
        }).join("");

        zona.innerHTML =
            '<div class="al-card">' +
            '<div class="al-card-body border-bottom">' +
            '<div class="d-flex flex-wrap justify-content-between align-items-start gap-2">' +
            '<div><div class="small texto-suave">Trámite</div><div class="codigo-seguimiento fs-5">' + esc(r.codigoSeguimiento) + '</div></div>' +
            '<span class="estado-badge estado-' + esc(k) + ' fs-6"><i class="bi ' + (iconos[k] || "bi-circle") + '" aria-hidden="true"></i>' + esc(r.estado) + '</span>' +
            '</div>' +
            '<dl class="datos mt-3">' +
            '<div><dt>Documento</dt><dd>' + esc(r.tipoDocumento) + '</dd></div>' +
            '<div><dt>País</dt><dd>' + esc(r.paisDestino) + '</dd></div>' +
            '<div><dt>Fecha de solicitud</dt><dd>' + esc(fecha(r.fechaSolicitud)) + '</dd></div>' +
            '<div><dt>Última actualización</dt><dd>' + esc(fecha(r.ultimaActualizacion)) + '</dd></div>' +
            envio + '</dl>' +
            '<div class="mt-3"><div class="d-flex justify-content-between small mb-1"><span class="fw-semibold">Avance</span>' +
            '<span class="texto-suave">' + esc(r.progreso) + '%</span></div>' +
            '<div class="al-progreso" role="progressbar" aria-label="Avance del trámite" aria-valuenow="' + esc(r.progreso) +
            '" aria-valuemin="0" aria-valuemax="100"><span style="width:' + esc(r.progreso) + '%"></span></div></div>' +
            '</div>' +
            '<div class="al-card-body"><h2 class="subtitulo-seccion mb-3">Etapas</h2><ol class="linea-tiempo">' + hitos + '</ol></div>' +
            '</div>' +
            '<p class="small texto-suave text-center mt-3"><i class="bi bi-shield-lock me-1" aria-hidden="true"></i>' +
            'Por su privacidad, esta página no muestra datos personales. Guarde esta dirección para volver a consultar.</p>';
    }

    function ocupado(si) {
        if (!boton) return;
        boton.disabled = si;
        boton.innerHTML = si
            ? '<span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>Consultando…'
            : textoBoton;
    }

    form.addEventListener("submit", function (e) {
        var codigo = (input.value || "").trim().toUpperCase();
        // Validación mínima del lado del cliente (las mismas reglas que el servidor: 6 a 20 caracteres).
        if (codigo.length < 6 || codigo.length > 20) return; // deja que el servidor muestre el error

        e.preventDefault();
        ocupado(true);
        pintarCargando();

        fetch(urlApi + encodeURIComponent(codigo), { headers: { "Accept": "application/json" } })
            .then(function (resp) {
                if (resp.status === 404) { pintarNoEncontrado(); return; }
                if (!resp.ok) throw new Error("HTTP " + resp.status);
                return resp.json().then(function (datos) {
                    pintarResultado(datos);
                    // La consulta queda en una URL que el cliente puede guardar o compartir.
                    if (window.history && history.pushState) history.pushState(null, "", "/seguimiento/" + encodeURIComponent(codigo));
                });
            })
            .catch(function () {
                // Si algo falla (red, servidor), se usa el envío normal del formulario.
                form.submit();
            })
            .finally(function () { ocupado(false); });
    });
})();
