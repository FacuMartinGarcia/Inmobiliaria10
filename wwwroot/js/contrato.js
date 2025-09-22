$(function () {

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
  // Cálculo de multa (solo en Editar - cuando existe #Rescision)
  // =======================
  if ($("#Rescision").length) {
    async function calcularMulta() {
      const idContrato = $("#IdContrato").val();
      const fechaRescision = $("#Rescision").val();
      const inputMontoMultaEl = $("#MontoMulta")[0];
      const spanValidacion = inputMontoMultaEl ? inputMontoMultaEl.nextElementSibling : null;

      if (!inputMontoMultaEl) return;
      inputMontoMultaEl.value = '';
      if (spanValidacion) spanValidacion.textContent = '';

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
          inputMontoMultaEl.value = Number(data.multa).toFixed(2);
        } else {
          if (spanValidacion) spanValidacion.textContent = data.mensaje ?? "No se pudo calcular la multa.";
        }
      } catch (ex) {
        if (spanValidacion) spanValidacion.textContent = "Error al comunicarse con el servidor.";
      }
    }

    $("#Rescision").on("change", calcularMulta);
  }

  // =======================
  // Confirmación al enviar (crear o editar)
  // =======================
  // Nota: si la página de Crear NO tiene #Rescision, no afectamos el submit.
  $('form').on('submit', async function (e) {
    // en Crear: si no existe #Rescision devolvemos (no interferimos)
    if (!$("#Rescision").length) return;

    e.preventDefault();
    const form = this;
    const fechaRescision = $("#Rescision").val();
    const mensaje = fechaRescision
      ? "El contrato se va a rescindir. ¿Está seguro?"
      : "Se actualizarán los datos del contrato.";

    // Primera confirmación (guardar/ rescindir)
    const r1 = await Swal.fire({
      title: 'Confirmación',
      text: mensaje,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, guardar',
      cancelButtonText: 'Cancelar',
      reverseButtons: true
    });

    if (!r1.isConfirmed) return;

    // Si hay rescisión, comprobamos multa y preguntamos si desea generar pago
    if (fechaRescision) {
      const $montoMulta = $("#MontoMulta");

      // Si aún no se calculó el monto, forzamos cálculo antes de preguntar
      if ($montoMulta.length && !$montoMulta.val()) {
        // calcularMulta puede existir solo si #Rescision está presente (ver arriba)
        if (typeof calcularMulta === "function") {
          await calcularMulta();
        }
      }

      // Parseo seguro del monto (reemplazamos coma por punto por las dudas)
      const montoRaw = ($montoMulta.val() || '0').toString().replace(',', '.');
      const montoMulta = parseFloat(montoRaw) || 0;

      if (montoMulta > 0) {
        // Segunda confirmación: generar pago de multa
        const r2 = await Swal.fire({
          title: 'Generar pago por multa',
          text: `Se detectó una multa de $${montoMulta.toFixed(2)}. ¿Desea generar el pago ahora?`,
          icon: 'question',
          showCancelButton: true,
          confirmButtonText: 'Sí, generar pago',
          cancelButtonText: 'No',
          reverseButtons: true
        });

        if (r2.isConfirmed) {
          // Añadimos campo oculto si no existe
          if (!$('#GenerarPagoMulta').length) {
            $('<input>').attr({
              type: 'hidden',
              id: 'GenerarPagoMulta',
              name: 'GenerarPagoMulta',
              value: 'true'
            }).appendTo(form);
          } else {
            $('#GenerarPagoMulta').val('true');
          }
        } else {
          // Si no quiere generar pago, nos aseguramos de que no exista el campo
          $('#GenerarPagoMulta').remove();
        }
      } else {
        // No hay multa: nos aseguramos de limpiar el campo oculto si existe
        $('#GenerarPagoMulta').remove();
      }
    } else {
      // No hay rescisión: limpiamos por si acaso
      $('#GenerarPagoMulta').remove();
    }

    // Validación del formulario y envío final
    if ($(form).valid()) {
      form.submit();
    }
  });
});
