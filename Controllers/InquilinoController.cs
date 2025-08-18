using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Models;
using Inmobiliaria10.Repositories;

namespace Inmobiliaria10.Controllers
{
    public class InquilinoController : Controller
    {
        private readonly InquilinoRepo repo;

        public InquilinoController(IConfiguration config)
        {
            //Usar la cadena de conexion de appsetting
            var conn = config.GetConnectionString("Inmogenial");
            repo = new InquilinoRepo(conn!);
        }

        // GET: /Inquilino - Muestra todos los inquilinos (método listarTodos)
        public IActionResult Index()
        {
            var lista = repo.ListarTodos();
            return View(lista);
        }

        // GET: /Inquilino/Detalle/5 - Muestra un inquilino en particular
        public IActionResult Detalle(int id)
        {
            var inq = repo.ObtenerPorId(id);
            if (inq == null)
                return NotFound();

            return View(inq);
        }

        // GET: /Inquilino/Crear - Muestra el formulario vacío
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /Inquilino/Crear - Recibe los datos y los guarda
        [HttpPost]
        public IActionResult Crear(Inquilino i)
        {
            if (!ModelState.IsValid) // Usa las validaciones del modelo
                return View(i);

            int idGenerado = repo.Agregar(i); 
            return RedirectToAction("Detalle", new { id = idGenerado });
        }

        // GET: /Inquilino/Editar/5 - Trae los datos para editar
        public IActionResult Editar(int id)
        {
            var inq = repo.ObtenerPorId(id);
            if (inq == null)
                return NotFound();

            return View(inq);
        }

        // POST: /Inquilino/Editar/5 - Valida y guarda
        [HttpPost]
        public IActionResult Editar(Inquilino i)
        {
            if (!ModelState.IsValid)
                return View(i);

            repo.Actualizar(i);
            return RedirectToAction("Detalle", new { id = i.IdInquilino });
        }
    }
}
