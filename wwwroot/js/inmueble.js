document.addEventListener("DOMContentLoaded", function () {

    // Si no existe window.inmuebleData (Nuevo), lo inicializamos vacío
    window.inmuebleData = window.inmuebleData || {
        propietario: null,
        tipo: null,
        uso: null
    };

    function initSelect2(selector, url, placeholder, selected = null) {
        const $el = $(selector);

        // Aseguramos que exista opción vacía para que aparezca el placeholder
        if ($el.find("option[value='']").length === 0) {
            $el.prepend(new Option("", "", true, true)).val("").trigger("change");
        }

        $el.select2({
            theme: 'bootstrap4',
            placeholder: placeholder,
            allowClear: true,
            width: '100%',
            language: {
                inputTooShort: function (args) {
                    return `Ingrese ${args.minimum - args.input.length} o más caracteres`;
                },
                noResults: function () {
                    return "No se encontraron resultados";
                },
                searching: function () {
                    return "Buscando...";
                }
            },
            ajax: {
                url: url,
                dataType: 'json',
                delay: 250,
                data: params => ({ term: params.term }),
                processResults: data => ({ results: data })
            },
            minimumInputLength: 0
        });

        // Solo si hay un valor seleccionado (ej: Editar), lo cargamos
        if (selected && selected.id && selected.text) {
            const option = new Option(selected.text, selected.id, true, true);
            $el.append(option).trigger('change');
        }
    }

    // Inicializar selects
    initSelect2("#propietario", "/Inmueble/BuscarPropietarios", "Buscar propietario...", window.inmuebleData.propietario);
    initSelect2("#tipo", "/Inmueble/BuscarTipos", "Buscar tipo de inmueble...", window.inmuebleData.tipo);
    initSelect2("#uso", "/Inmueble/BuscarUsos", "Buscar uso de inmueble...", window.inmuebleData.uso);

});
