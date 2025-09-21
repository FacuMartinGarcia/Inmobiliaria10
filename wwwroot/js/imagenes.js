// wwwroot/js/imagenes.js
document.addEventListener("DOMContentLoaded", function () {

    // 🔹 Mensajes de éxito o error inyectados desde la vista
    if (window.mensajeOk) {
        Swal.fire({
            icon: 'success',
            title: 'Éxito',
            text: window.mensajeOk,
            timer: 1500,
            timerProgressBar: true,
            showConfirmButton: false
        });
    }

    if (window.mensajeErr) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: window.mensajeErr,
            timer: 1500,
            timerProgressBar: true,
            showConfirmButton: true
        });
    }

    // 🔹 Confirmación al eliminar portada
    document.querySelectorAll('.eliminar-portada').forEach(form => {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            Swal.fire({
                title: '¿Estás seguro?',
                text: "¡No podrás deshacer esta acción!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Sí, eliminar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });
    });

    // 🔹 Confirmación al eliminar imágenes
    document.querySelectorAll('.eliminar-imagen').forEach(form => {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            Swal.fire({
                title: '¿Estás seguro?',
                text: "¡No podrás deshacer esta acción!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Sí, eliminar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });
    });

    // 🔹 Mostrar nombre de archivo portada
    const inputArchivo = document.getElementById('Archivo');
    const nombreArchivoSpan = document.getElementById('nombreArchivoSeleccionado');
    if (inputArchivo) {
        inputArchivo.addEventListener('change', function () {
            nombreArchivoSpan.textContent =
                inputArchivo.files.length > 0
                    ? inputArchivo.files[0].name
                    : 'Ningún archivo seleccionado';
        });
    }

    // 🔹 Mostrar nombres de archivos de interior
    const inputImagenes = document.getElementById('imagenes');
    const archivosSeleccionados = document.getElementById('archivosSeleccionados');
    if (inputImagenes) {
        inputImagenes.addEventListener('change', function () {
            archivosSeleccionados.textContent =
                inputImagenes.files.length > 0
                    ? Array.from(inputImagenes.files).map(f => f.name).join(', ')
                    : 'Ningún archivo seleccionado';
        });
    }
});
