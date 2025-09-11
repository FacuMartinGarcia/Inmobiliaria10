using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Models;
using Inmobiliaria10.Data.Repositories;

namespace Inmobiliaria10.Controllers
{
    public class InmuebleTipoController : Controller
    {
        private readonly IInmuebleTipoRepo repo;

        public InmuebleTipoController(IInmuebleTipoRepo repo)
        {
            this.repo = repo;
        }

        public IActionResult Index()
        {
            var tipos = repo.MostrarTodosInmuebleTipos();
            return View(tipos);
        }
        public IActionResult Detalle(int id)
        {
            var tipo = repo.ObtenerPorId(id);
            if (tipo == null)
                return NotFound();

            return View(tipo);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(InmuebleTipo tipo)
        {
            if (!ModelState.IsValid)
                return View(tipo);

            try
            {
                repo.Agregar(tipo);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DenominacionTipo", ex.Message);
                return View(tipo);
            }
        }

        public IActionResult Editar(int id)
        {
            var tipo = repo.ObtenerPorId(id);
            if (tipo == null)
                return NotFound();

            return View(tipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(InmuebleTipo tipo)
        {
            if (!ModelState.IsValid)
                return View(tipo);

            try
            {
                repo.Editar(tipo);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DenominacionTipo", ex.Message);
                return View(tipo);
            }
        }

        public IActionResult Eliminar(int id)
        {
            var tipo = repo.ObtenerPorId(id);
            if (tipo == null)
                return NotFound();

            return View(tipo);
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
