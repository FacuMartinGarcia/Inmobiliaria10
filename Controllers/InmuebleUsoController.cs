using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Models;
using Inmobiliaria10.Repositories;

namespace Inmobiliaria10.Controllers
{
    public class InmuebleUsoController : Controller
    {
        private readonly InmuebleUsoRepo repo;

        public InmuebleUsoController(IConfiguration config)
        {
            var conn = config.GetConnectionString("Inmogenial");
            repo = new InmuebleUsoRepo(conn!);
        }

        public IActionResult Index()
        {
            var lista = repo.MostrarTodosInmuebleUsos();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(InmuebleUso u)
        {
            if (ModelState.IsValid)
            {
                repo.Agregar(u);
                return RedirectToAction(nameof(Index));
            }
            return View(u);
        }
    }
}
