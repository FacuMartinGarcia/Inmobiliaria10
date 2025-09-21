// wwwroot/js/site.js
document.addEventListener("DOMContentLoaded", () => {
    // Mensajes de TempData (éxito/error)
    const mensaje = document.getElementById("mensajeTemp")?.value;
    const error = document.getElementById("errorTemp")?.value;

    if (mensaje) {
        Swal.fire({
            icon: 'success',
            title: 'Éxito',
            text: mensaje
        });
    }

    if (error) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: error
        });
    }
});

// Confirmación genérica (ejemplo para eliminar)
function confirmarEliminacion(url, mensaje = "¿Seguro que desea eliminar este registro?") {
    Swal.fire({
        title: 'Confirmar',
        text: mensaje,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            window.location.href = url;
        }
    });
}
