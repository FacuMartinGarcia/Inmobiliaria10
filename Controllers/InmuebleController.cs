using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inmobiliaria10.Models;
using Inmobiliaria10.Data.Repositories;

namespace Inmobiliaria10.Controllers
{
    public class InmuebleController : Controller
    {
        private readonly IInmuebleRepo _repoInmueble;
        private readonly IPropietarioRepo _repoPropietario;
        private readonly IInmuebleUsoRepo _repoUso;
        private readonly IInmuebleTipoRepo _repoTipo;

        public InmuebleController(
            IInmuebleRepo repoInmueble,
            IPropietarioRepo repoPropietario,
            IInmuebleUsoRepo repoUso,
            IInmuebleTipoRepo repoTipo)
        {
            _repoInmueble = repoInmueble;
            _repoPropietario = repoPropietario;
            _repoUso = repoUso;
            _repoTipo = repoTipo;
        }

        public async Task<IActionResult> Index(int pagina = 1, string? searchString = null)
        {
            int cantidadPorPagina = 8;

            var resultado = await _repoInmueble.ListarTodosPaginado(pagina, cantidadPorPagina, searchString);

            ViewData["TotalPaginas"] = (int)Math.Ceiling((double)resultado.totalRegistros / cantidadPorPagina);
            ViewData["PaginaActual"] = pagina;
            ViewData["SearchString"] = searchString;

            return View(resultado.registros);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var inmueble = await _repoInmueble.ObtenerPorId(id);
            if (inmueble == null)
                return NotFound();

            return View(inmueble);
        }

        public async Task<IActionResult> Crear()
        {
            await CargarSelectsAsync();
            var inmueble = new Inmueble
            {
                Activo = true
            };
            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                await _repoInmueble.Agregar(inmueble);
                TempData["Mensaje"] = "Inmueble creado correctamente";
                return RedirectToAction("Index");
            }

            await CargarSelectsAsync();
            return View(inmueble);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var inmueble = await _repoInmueble.ObtenerPorId(id);
            if (inmueble == null)
                return NotFound();

            await CargarSelectsAsync();
            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                await _repoInmueble.Actualizar(inmueble);
                TempData["Mensaje"] = "Inmueble actualizado correctamente";
                return RedirectToAction("Index");
            }

            await CargarSelectsAsync();
            return View(inmueble);
        }

        public async Task<IActionResult> Eliminar(int id)
        {
            var inmueble = await _repoInmueble.ObtenerPorId(id);
            if (inmueble == null)
                return NotFound();

            return View(inmueble);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            await _repoInmueble.Eliminar(id);
            TempData["Mensaje"] = "Inmueble eliminado correctamente";
            return RedirectToAction("Index");
        }

        private async Task CargarSelectsAsync()
        {
            var propietarios = await _repoPropietario.ObtenerTodo();
            ViewBag.Propietarios = propietarios
                .Select(p => new SelectListItem
                {
                    Value = p.IdPropietario.ToString(),
                    Text = $"{p.ApellidoNombres} - {p.Documento}"
                })
                .ToList();

            ViewBag.Usos = _repoUso.MostrarTodosInmuebleUsos()
                .Select(u => new SelectListItem
                {
                    Value = u.IdUso.ToString(),
                    Text = u.DenominacionUso
                }).ToList();

            ViewBag.Tipos = _repoTipo.MostrarTodosInmuebleTipos()
                .Select(t => new SelectListItem
                {
                    Value = t.IdTipo.ToString(),
                    Text = t.DenominacionTipo
                }).ToList();
        }
    }
}
