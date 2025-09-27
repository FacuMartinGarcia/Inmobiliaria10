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
    }).on('change', function () {
        tabla.ajax.reload();
    });

    // --- DataTable Auditoría de Contratos
    var tabla = $('#tablaAuditoriaContratos').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Contrato/DataAuditoria', 
            data: function (d) {
                d.usuario = $('#fUsuario').val();
            }
        },
        columns: [
            { data: 'accion',  title: 'Acción' },
            { data: 'fecha',   title: 'Fecha' },
            { data: 'usuario', title: 'Usuario' },
            {
                data: 'oldData',
                title: 'Datos anteriores',
                orderable: false,
                searchable: false
            },
            {
                data: 'newData',
                title: 'Datos nuevos',
                orderable: false,
                searchable: false
            }
        ],
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100],
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
        }
    });
});
