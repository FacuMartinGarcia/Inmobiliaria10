$(document).ready(function () {
    // Siempre inicializamos Select2 si existe
    if ($("#rol").length) {
        $('#rol').select2({
            theme: 'bootstrap4',
            placeholder: '-- Seleccione Rol --',
            allowClear: true,
            width: '100%'
        });
    }

    // Detectar qué vista se cargó por id del body
    const pageId = $("body").attr("id");

    if (pageId === "page-create-user") {
        // Lógica exclusiva de Crear Usuario
        console.log("Vista: Crear Usuario");
        // Ejemplo: limpiar form al iniciar
        $("#UsuarioForm")[0]?.reset();
    }

    if (pageId === "page-edit-user") {
        // Lógica exclusiva de Editar Usuario
        console.log("Vista: Editar Usuario");
        // Ejemplo: habilitar campos adicionales
        $("#ExtraFields").show();
    }

    if (pageId === "page-change-password") {
        // Lógica exclusiva de Cambiar Contraseña
        console.log("Vista: Cambiar Contraseña");
        $("#Password").removeAttr("data-val");
        $("#Password").removeAttr("data-val-required");

        var passwordInput = document.getElementById('Password');
        if (passwordInput) {
            new bootstrap.Tooltip(passwordInput, {
                title: "Ingrese su nueva contraseña",
                placement: "right"
            });
        }
    }
});
