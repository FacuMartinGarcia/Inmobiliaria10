document.addEventListener("DOMContentLoaded", function () {

    function initSelect2(selector, url, placeholder, selectedId, selectedText) {
        const $el = $(selector);

        $el.select2({
            theme: 'bootstrap4',
            placeholder: placeholder,
            allowClear: true,
            width: '100%',
            ajax: {
                url: url,
                dataType: 'json',
                delay: 250,
                data: params => ({ term: params.term }),
                processResults: data => ({ results: data })
            },
            minimumInputLength: 1
        });

        // Si estoy en Editar y ya hay un valor cargado, lo inyecto
        if (selectedId && selectedText) {
            const option = new Option(selectedText, selectedId, true, true);
            $el.append(option).trigger('change');
        }
    }

    // 🔹 Inicializar Propietario
    initSelect2(
        "#propietario",
        "/Inmueble/BuscarPropietarios",
        "Buscar propietario...",
        $("#propietario").data("selected-id"),
        $("#propietario").data("selected-text")
    );

    // 🔹 Inicializar Tipo
    initSelect2(
        "#tipo",
        "/Inmueble/BuscarTipos",
        "Buscar tipo de inmueble...",
        $("#tipo").data("selected-id"),
        $("#tipo").data("selected-text")
    );

    // 🔹 Inicializar Uso
    initSelect2(
        "#uso",
        "/Inmueble/BuscarUsos",
        "Buscar uso de inmueble...",
        $("#uso").data("selected-id"),
        $("#uso").data("selected-text")
    );
});
