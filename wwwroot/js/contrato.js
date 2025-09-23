// contrato.js
$(function () {

  // =======================
  // Select2 Inquilino
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

    // --- Precarga en Editar con window.contratoData ---
    if (window.contratoData?.inquilino?.id) {
      const d = window.contratoData.inquilino;
      const opt = new Option(d.text, d.id, true, true);
      $('#IdInquilino').append(opt).trigger('change');
    } else {
      // --- Precarga vía AJAX si existe value en el select ---
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
  }

  // =======================
  // Select2 Inmueble
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

    $('#IdInmueble').on('select2:select', function (e) {
      const data = e.params.data;
      if (data && data.precio) {
        $('#MontoMensual').val(data.precio);
      }
    });

    if (window.contratoData?.inmueble?.id) {
      const d = window.contratoData.inmueble;
      const opt = new Option(d.text, d.id, true, true);
      $('#IdInmueble').append(opt).trigger('change');
      if (d.precio) {
        $('#MontoMensual').val(d.precio);
      }
    } else {
      // AJAX si exsite valor
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
  }

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

  $('form').on('submit', async function (e) {

    if (!$("#Rescision").length) return;

    e.preventDefault();
    const form = this;
    const fechaRescision = $("#Rescision").val();
    const mensaje = fechaRescision
      ? "El contrato se va a rescindir. ¿Está seguro?"
      : "Se actualizarán los datos del contrato.";

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

      if ($montoMulta.length && !$montoMulta.val()) {
        if (typeof calcularMulta === "function") {
          await calcularMulta();
        }
      }

      const montoRaw = ($montoMulta.val() || '0').toString().replace(',', '.');
      const montoMulta = parseFloat(montoRaw) || 0;

      if (montoMulta > 0) {
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
          $('#GenerarPagoMulta').remove();
        }
      } else {
        $('#GenerarPagoMulta').remove();
      }
    } else {
      $('#GenerarPagoMulta').remove();
    }

    if ($(form).valid()) {
      form.submit();
    }
  });
});
