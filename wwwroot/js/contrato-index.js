$(function () {
  // =======================
  // Select2 en Inquilino e Inmueble
  // =======================
  $('#inquilinoSelect').select2({
    width: '100%',
    placeholder: 'Todos',
    allowClear: true,
    language: {
      noResults: () => 'Sin resultados',
      searching: () => 'Buscando…'
    }
  });

  $('#inmuebleSelect').select2({
    width: '100%',
    placeholder: 'Todos',
    allowClear: true,
    language: {
      noResults: () => 'Sin resultados',
      searching: () => 'Buscando…'
    }
  });

  // =======================
  // Cascada: Tipo → Inmueble
  // =======================
  $('#tipoSelect').on('change', async function () {
    const idTipo = this.value;
    const $sel = $('#inmuebleSelect');

    // limpiar y deshabilitar mientras carga
    $sel.prop('disabled', true);
    $sel.empty().append(new Option('Todos', '', true, false));

    try {
      if (idTipo) {
        const resp = await fetch(`/Contrato/InmueblesPorTipo?idTipo=${encodeURIComponent(idTipo)}`);
        if (!resp.ok) throw new Error('HTTP ' + resp.status);
        const data = await resp.json(); // [{id, texto}]
        data.forEach(x => $sel.append(new Option(x.texto, x.id)));
      }
    } catch (e) {
      console.error('Error cargando inmuebles:', e);
    }

    // re-habilitar y refrescar Select2
    $sel.prop('disabled', false).trigger('change.select2');
  });
});
