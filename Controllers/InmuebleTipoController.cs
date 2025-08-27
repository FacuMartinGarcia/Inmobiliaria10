using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Models;
using Inmobiliaria10.Repositories;

namespace Inmobiliaria10.Controllers
{
    public class InmuebleTipoController : Controller
    {
        private readonly InmuebleTipoRepo repo;

        public InmuebleTipoController(IConfiguration config)
        {
            var conn = config.GetConnectionString("Inmogenial");
            repo = new InmuebleTipoRepo(conn!);
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
