document.addEventListener('DOMContentLoaded', function () {
    const plazo = document.querySelector('[name="PlazoAnios"]');
    const inicio = document.querySelector('[name="FechaInicio"]');
    const fin = document.querySelector('[name="FechaFin"]');

    function actualizarFin() {
        const años = parseInt(plazo.value) || 0;
        const fi = new Date(inicio.value);
        if (!isNaN(fi.getTime()) && años > 0) {
            const ff = new Date(fi);
            ff.setFullYear(ff.getFullYear() + años);
            // mantengo la misma lógica que servidor (AddYears)
            fin.value = ff.toISOString().slice(0, 10);
        }
    }

    plazo.addEventListener('change', actualizarFin);
    actualizarFin();
});