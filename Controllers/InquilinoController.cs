using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Models;
using Inmobiliaria10.Data.Repositories;

namespace Inmobiliaria10.Controllers
{
    public class InquilinoController : Controller
    {
        private readonly IInquilinoRepo _repo;

        public InquilinoController(IInquilinoRepo repo)
        {
            _repo = repo;
        }

        // GET: /Inquilino - Mostrar todos pero paginado
        public IActionResult Index(int pagina = 1)
        {
            int cantidadPorPagina = 8; 
            var resultado = _repo.ListarTodosPaginado(pagina, cantidadPorPagina);

            ViewData["TotalPaginas"] = (int)Math.Ceiling((double)resultado.totalRegistros / cantidadPorPagina);
            ViewData["PaginaActual"] = pagina;

            return View(resultado.registros);
        }

        // GET: /Inquilino/Detalle/5 - Muestra un inquilino en particular
        public IActionResult Detalle(int id)
        {
            var inq = _repo.ObtenerPorId(id);
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
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Inquilino i)
        {
            if (!ModelState.IsValid)
                return View(i);

            try
            {
                _repo.Agregar(i);
                TempData["Mensaje"] = "Inquilino creado correctamente";
                return RedirectToAction("Index");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    ModelState.AddModelError("Documento", "El documento ya existe en el sistema.");
                }
                else
                {
                    ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                }
                return View(i);
            }
        }

        // GET: /Inquilino/Editar/5 - Trae los datos para editar
        public IActionResult Editar(int id)
        {
            var inq = _repo.ObtenerPorId(id);
            if (inq == null)
                return NotFound();

            return View(inq);
        }

        // POST: /Inquilino/Editar/5 - Valida y guarda
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Inquilino i)
        {
            if (!ModelState.IsValid)
                return View(i);

            var existente = _repo.ObtenerPorDocumento(i.Documento);
            if (existente != null && existente.IdInquilino != i.IdInquilino)
            {
                ModelState.AddModelError("Documento", "El documento ya está registrado en otro inquilino.");
                return View(i);
            }

            _repo.Actualizar(i);
            TempData["Mensaje"] = "El inquilino ha sido actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // GET: /Inquilino/Eliminar/5 - Muestra confirmación de borrado
        public IActionResult Borrar(int id)
        {
            var inq = _repo.ObtenerPorId(id);
            if (inq == null)
                return NotFound();

            return View(inq); 
        }

        // POST: /Inquilino/Eliminar/5 - Realiza el borrado
        [HttpPost, ActionName("Borrar")]
        [ValidateAntiForgeryToken]
        public IActionResult BorrarConfirmado(int id)
        {
            var inq = _repo.ObtenerPorId(id);
            if (inq == null)
                return NotFound();

            try
            {
                _repo.Borrar(id);
                TempData["Mensaje"] = "Inquilino eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo eliminar el inquilino: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}