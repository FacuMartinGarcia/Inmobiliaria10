// wwwroot/js/pagos.js
$(function () {
    const pageId = $("body").attr("id");

    // ============================================================
    // INDEX PAGOS
    // ============================================================
    if (pageId === "page-pagos-index") {
        // --- Filtro contrato con Select2 + AJAX ---
        $('#fContrato').select2({
            placeholder: 'Buscar contrato…',
            allowClear: true,
            ajax: {
                url: '/Pagos/search-contratos',
                dataType: 'json',
                delay: 250,
                data: p => ({ term: p.term || '', page: p.page || 1 }),
                processResults: d => ({
                    results: (d.results || []).map(x => ({ id: x.id, text: x.text }))
                })
            }
        });

        // --- Preselección desde ViewBag ---
        const preContrato = $('#fContrato').data('pre');
        if (preContrato && preContrato > 0) {
            $.getJSON('/Pagos/search-contratos', { id: preContrato }, function (res) {
                if (res && res.item) {
                    const opt = new Option(res.item.text, res.item.id, true, true);
                    $('#fContrato').append(opt).trigger('change');
                }
            });
        }

        // --- DataTable server-side ---
        const dt = $('#tblPagos').DataTable({
            processing: true,
            serverSide: true,
            searching: false,
            lengthMenu: [10, 20, 50, 100],
            pageLength: 20,
            order: [[0, 'desc']], // Ordenar por fecha desc
            ajax: {
                url: '/Pagos/data',
                data: d => {
                    d.contrato = $('#fContrato').val() || '';
                }
            },
            columns: [
                { data: 'fechaPago' },
                { data: 'detalle' },
                { data: 'conceptoDenominacion' },
                {
                    data: 'importe',
                    className: 'text-end',
                    render: v => {
                        try {
                            return v != null
                                ? new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(v)
                                : '-';
                        } catch { return v; }
                    }
                },
                { data: 'contratoTexto' },
                {
                    data: 'estado',
                    render: v => v === 'Eliminado'
                        ? '<span class="badge bg-warning text-dark">Eliminado</span>'
                        : '<span class="badge bg-success">Activo</span>'
                },
                {
                    data: 'idPago',
                    orderable: false,
                    className: 'text-end text-nowrap',
                    render: id => !id ? '' : `
                        <a href="/Pagos/Detalles/${id}" class="btn btn-sm btn-secondary"><i class="fa-solid fa-eye"></i></a>
                        <a href="/Pagos/Editar/${id}" class="btn btn-sm btn-primary"><i class="fa-solid fa-pen"></i></a>

                        <!-- 🔹 Ahora usamos form-delete en lugar de onclick -->
                        <form action="/Pagos/Eliminar/${id}" method="post" class="d-inline form-delete"
                              data-msg="¿Confirmás eliminar el pago #${id}?">
                            <input type="hidden" name="__RequestVerificationToken" value="${$('input[name="__RequestVerificationToken"]').val()}"/>
                            <button type="submit" class="btn btn-sm btn-danger">
                                <i class="fa-solid fa-trash-can"></i>
                            </button>
                        </form>
                    `
                }
            ],
            language: { url: 'https://cdn.datatables.net/plug-ins/2.0.8/i18n/es-ES.json' }
        });

        // --- Actualizar botón "Nuevo Pago" con contrato seleccionado ---
        function updateNuevoHref() {
            const c = $('#fContrato').val();
            const url = c ? `/Pagos/Crear?contrato=${encodeURIComponent(c)}` : '/Pagos/Crear';
            $('#btnNuevo').attr('href', url);
        }

        $('#frmFiltros').on('submit', function (e) {
            e.preventDefault();
            updateNuevoHref();
            dt.ajax.reload();
        });

        $('#fContrato').on('change', function () {
            updateNuevoHref();
            dt.ajax.reload();
        });

        updateNuevoHref();
    }

    // ============================================================
    // CREAR / EDITAR PAGO
    // ============================================================
    if (pageId === "page-create-pago" || pageId === "page-edit-pago") {
        // --- Contrato con Select2 + AJAX ---
        const $orig = $('#IdContrato');
        if ($orig.length) {
            const $sel = $('<select class="form-select" style="width:100%"></select>');

            // Copiar validaciones unobtrusive
            $.each($orig[0].attributes, function (_, a) {
                if (a && a.name && a.name.startsWith('data-val')) {
                    $sel.attr(a.name, a.value);
                }
            });

            $orig.after($sel).attr('id', 'IdContrato_original').attr('name', '').hide();
            $sel.attr('id', 'IdContrato').attr('name', 'IdContrato');

            $('#IdContrato').select2({
                placeholder: 'Buscar contrato (Contrato + Inmueble)…',
                allowClear: true,
                width: '100%',
                ajax: {
                    url: '/Pagos/search-contratos',
                    dataType: 'json',
                    delay: 250,
                    data: params => ({ term: params.term || '', page: params.page || 1 }),
                    processResults: data => ({
                        results: (data.results || []).map(x => ({ id: x.id, text: x.text }))
                    })
                }
            });

            // --- Preselección de contrato ---
            const pre = $('#IdContrato_original').val();
            if (pre && pre > 0) {
                $.getJSON('/Pagos/search-contratos', { id: pre }, function (res) {
                    if (res && res.item) {
                        const opt = new Option(res.item.text, res.item.id, true, true);
                        $('#IdContrato').append(opt).trigger('change');
                    }
                });
            }
        }

        // --- Concepto con Select2 ---
        $('#IdConcepto').select2({
            placeholder: "Seleccione un concepto",
            allowClear: true,
            width: '100%'
        });
    }
});
