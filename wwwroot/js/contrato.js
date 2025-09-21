$(function () {
  // =======================
  // Select2 Inquilinos
  // =======================
  if ($("#IdInquilino").length) {
    $('#IdInquilino').select2({
      placeholder: 'Buscar inquilino…',
      allowClear: true,
      ajax: {
        url: '/Contrato/SearchInquilinos',
        dataType: 'json',
        delay: 250,
        data: params => ({ term: params.term }),
        processResults: data => ({ results: data.results })
      }
    });

    // Precarga inquilino en Editar
    const preInquilino = $("#IdInquilino").val();
    if (preInquilino && parseInt(preInquilino) > 0) {
      $.getJSON('/Contrato/SearchInquilinos', { id: preInquilino }, function (res) {
        if (res && res.item) {
          const opt = new Option(res.item.text, res.item.id, true, true);
          $('#IdInquilino').append(opt).trigger('change');
        }
      });
    }
  }

  // =======================
  // Select2 Inmuebles
  // =======================
  if ($("#IdInmueble").length) {
    $('#IdInmueble').select2({
      placeholder: 'Buscar inmueble…',
      allowClear: true,
      ajax: {
        url: '/Contrato/SearchInmuebles',
        dataType: 'json',
        delay: 250,
        data: params => ({ term: params.term }),
        processResults: data => ({ results: data.results })
      }
    });

    // Autocompletar precio mensual al seleccionar inmueble
    $('#IdInmueble').on('select2:select', function (e) {
      const data = e.params.data;
      if (data && data.precio) {
        $('#MontoMensual').val(data.precio);
      }
    });

    // Precarga inmueble en Editar
    const preInmueble = $("#IdInmueble").val();
    if (preInmueble && parseInt(preInmueble) > 0) {
      $.getJSON('/Contrato/SearchInmuebles', { id: preInmueble }, function (res) {
        if (res && res.item) {
          const opt = new Option(res.item.text, res.item.id, true, true);
          $('#IdInmueble').append(opt).trigger('change');
          if (res.item.precio) {
            $('#MontoMensual').val(res.item.precio);
          }
        }
      });
    }
  }

  // =======================
  // Cálculo de multa (solo en Editar)
  // =======================
  if ($("#Rescision").length) {
    async function calcularMulta() {
      const idContrato = $("#IdContrato").val();
      const fechaRescision = $("#Rescision").val();
      const inputMontoMulta = $("#MontoMulta")[0];
      const spanValidacion = inputMontoMulta.nextElementSibling;

      inputMontoMulta.value = '';
      spanValidacion.textContent = '';

      if (!fechaRescision) return;

      const params = new URLSearchParams();
      params.append("idContrato", idContrato);
      params.append("fechaRescision", fechaRescision);

      try {
        const resp = await fetch('/Contrato/CalcularMulta', {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: params.toString()
        });

        const data = await resp.json();
        if (data.ok && data.multa != null) {
          inputMontoMulta.value = data.multa.toFixed(2);
        } else {
          spanValidacion.textContent = data.mensaje ?? "No se pudo calcular la multa.";
        }
      } catch {
        spanValidacion.textContent = "Error al comunicarse con el servidor.";
      }
    }
    $("#Rescision").on("change", calcularMulta);
  }

  // =======================
  // Confirmación al enviar (crear o editar)
  // =======================
  $('form').on('submit', function (e) {
    if (!$("#Rescision").length) return; // en Crear no hay rescisión

    e.preventDefault();
    const form = this;
    const fechaRescision = $("#Rescision").val();
    const mensaje = fechaRescision
      ? "El contrato se va a rescindir. ¿Está seguro?"
      : "Se actualizarán los datos del contrato.";

    Swal.fire({
      title: 'Confirmación',
      text: mensaje,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, guardar',
      cancelButtonText: 'Cancelar',
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed && $(form).valid()) {
        form.submit();
      }
    });
  });
});
