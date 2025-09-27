$(function () {
    // --- Select2 Usuario
    $('#fUsuario').select2({
        placeholder: 'Buscar usuario…',
        allowClear: true,
        ajax: {
            url: '/Pagos/BuscarUsuarios',
            dataType: 'json',
            delay: 250,
            data: p => ({ term: p.term }),
            processResults: d => ({ results: d.results })
        }
    }).on('change', function () { tabla.ajax.reload(); });


    // --- Select2 Contrato
    $('#fContrato').select2({
        placeholder: 'Buscar contrato…',
        allowClear: true,
        ajax: {
            url: '/Pagos/search-contratos',
            dataType: 'json',
            delay: 250,
            data: p => ({ term: p.term }),
            processResults: d => ({ results: d.results })
        }
    }).on('change', function () { tabla.ajax.reload(); });

    // --- Conceptos (inyectados desde ViewBag.Conceptos en la vista)
    const conceptos = window.Conceptos || {};
        $('#fConcepto').select2({
            placeholder: 'Todos',
            allowClear: true,
            data: Object.entries(conceptos).map(([id, text]) => ({ id, text }))
        }).on('change', function () { tabla.ajax.reload(); });

        // --- DataTable con server-side
        var tabla = $('#tablaAuditoria').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Pagos/data-auditoria',
            data: function (d) {
                d.usuario  = $('#fUsuario').val();
                d.contrato = $('#fContrato').val();
                d.concepto = $('#fConcepto').val();
            }
        },
        columns: [
            { data: 'accion',  title: 'Acción' },
            { data: 'fecha',   title: 'Fecha' },
            { data: 'usuario', title: 'Usuario' },
            { data: 'oldData', title: 'Datos anteriores', orderable: false, searchable: false },
            { data: 'newData', title: 'Datos nuevos',     orderable: false, searchable: false }
        ],
        pageLength: 10,
        language: { url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json' }


    });
});
