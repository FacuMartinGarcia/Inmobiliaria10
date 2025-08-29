using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Models;
using Inmobiliaria10.Data.Repositories;

namespace Inmobiliaria10.Controllers
{
    public class InmuebleUsoController : Controller
    {
        private readonly IInmuebleUsoRepo repo;

        public InmuebleUsoController(IInmuebleUsoRepo repo)
        {
            this.repo = repo;
        }

        public IActionResult Index()
        {
            var usos = repo.MostrarTodosInmuebleUsos();
            return View(usos);
        }

        public IActionResult Detalle(int id)
        {
            var uso = repo.ObtenerPorId(id);
            if (uso == null)
                return NotFound();

            return View(uso);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(InmuebleUso uso)
        {
            if (!ModelState.IsValid)
                return View(uso);

            try
            {
                repo.Agregar(uso);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DenominacionUso", ex.Message);
                return View(uso);
            }
        }

        public IActionResult Editar(int id)
        {
            var uso = repo.ObtenerPorId(id);
            if (uso == null)
                return NotFound();

            return View(uso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(InmuebleUso uso)
        {
            if (!ModelState.IsValid)
                return View(uso);

            try
            {
                repo.Editar(uso);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DenominacionUso", ex.Message);
                return View(uso);
            }
        }

        public IActionResult Eliminar(int id)
        {
            var uso = repo.ObtenerPorId(id);
            if (uso == null)
                return NotFound();

            return View(uso);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            repo.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
