// wwwroot/js/site.js
document.addEventListener("DOMContentLoaded", () => {
    // =======================
    // Alertas desde TempData
    // =======================
    const mensaje = document.getElementById("mensajeTemp")?.value;
    const error   = document.getElementById("errorTemp")?.value;
    const info    = document.getElementById("infoTemp")?.value;

    if (mensaje) {
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'success',
            title: mensaje,
            showConfirmButton: false,
            timer: 2500,
            timerProgressBar: true
        });
    }

    if (info) {
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'info',
            title: info,
            showConfirmButton: false,
            timer: 2500,
            timerProgressBar: true
        });
    }

    if (error) {
        Swal.fire({
            icon: 'error',
            title: 'Ups…',
            text: error,
            confirmButtonText: 'Cerrar'
        });
    }

    // =======================
    // Confirmación genérica de eliminación
    // =======================
    document.querySelectorAll("form.form-delete").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();

            Swal.fire({
                title: "¿Eliminar?",
                text: form.dataset.msg || "Esta acción no se puede deshacer.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Sí, eliminar",
                cancelButtonText: "Cancelar",
                reverseButtons: true
            }).then(result => {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });
    });
});
