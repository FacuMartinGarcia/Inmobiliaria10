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

            repo.Agregar(tipo);
            return RedirectToAction(nameof(Index));
        }
    }
}
