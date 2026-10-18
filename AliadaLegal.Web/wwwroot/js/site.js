// =====================================================================
// Aliada Legal — comportamiento de interfaz (sin dependencias extra).
// Responsable: Integrante 2 (UI y vistas Razor)
// =====================================================================
(function () {
    "use strict";

    // 1. Evitar doble envío: <form data-evitar-doble-envio>
    //    Deshabilita el botón submit y muestra un spinner mientras se procesa.
    document.addEventListener("submit", function (e) {
        var form = e.target;
        if (!form.matches("form[data-evitar-doble-envio]")) return;

        // Si jQuery Validation marcó errores, no bloquear el botón.
        if (window.jQuery && jQuery(form).data("validator") && !jQuery(form).valid()) return;

        var boton = form.querySelector("button[type=submit]");
        if (!boton || boton.disabled) return;
        boton.dataset.textoOriginal = boton.innerHTML;
        boton.disabled = true;
        boton.innerHTML = '<span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>Procesando…';
    });

    // Si el usuario vuelve con el botón "Atrás", reactivar los botones.
    window.addEventListener("pageshow", function () {
        document.querySelectorAll("button[data-texto-original]").forEach(function (b) {
            b.disabled = false;
            b.innerHTML = b.dataset.textoOriginal;
        });
    });

    // 2. Confirmación: <button data-confirmar="¿Seguro?">
    document.addEventListener("click", function (e) {
        var el = e.target.closest("[data-confirmar]");
        if (el && !window.confirm(el.getAttribute("data-confirmar"))) {
            e.preventDefault();
            e.stopImmediatePropagation();
        }
    }, true);

    // 3. Copiar al portapapeles: <button data-copiar="texto">
    document.addEventListener("click", function (e) {
        var el = e.target.closest("[data-copiar]");
        if (!el) return;
        var texto = el.getAttribute("data-copiar");
        var original = el.innerHTML;
        var listo = function () {
            el.innerHTML = '<i class="bi bi-check2" aria-hidden="true"></i> Copiado';
            setTimeout(function () { el.innerHTML = original; }, 1800);
        };
        if (navigator.clipboard && window.isSecureContext) {
            navigator.clipboard.writeText(texto).then(listo);
        } else {
            var t = document.createElement("textarea");
            t.value = texto; document.body.appendChild(t); t.select();
            try { document.execCommand("copy"); listo(); } finally { document.body.removeChild(t); }
        }
    });

    // 4. Código de seguimiento en mayúsculas mientras se escribe: <input data-mayusculas>
    document.addEventListener("input", function (e) {
        var el = e.target;
        if (!el.matches("input[data-mayusculas]")) return;
        var pos = el.selectionStart;
        el.value = el.value.toUpperCase().replace(/\s+/g, "");
        if (pos !== null) el.setSelectionRange(pos, pos);
    });

    // 5. Cerrar automáticamente las alertas de éxito después de 6 s.
    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".alert-success[data-autocerrar]").forEach(function (a) {
            setTimeout(function () {
                if (window.bootstrap && bootstrap.Alert) bootstrap.Alert.getOrCreateInstance(a).close();
            }, 6000);
        });

        // 6. Filtros que se envían solos al cambiar el select: <select data-autoenviar>
        document.querySelectorAll("select[data-autoenviar]").forEach(function (s) {
            s.addEventListener("change", function () { s.form && s.form.requestSubmit ? s.form.requestSubmit() : s.form.submit(); });
        });
    });
})();
