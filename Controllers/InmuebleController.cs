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

            //await CargarSelectsAsync();
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
        
        // CODIGO PARA EL MANEJO DE IMAGENES
        // GET: Inmueble/Imagenes/5
        public async Task<IActionResult> Imagenes(int id)
        {
            var inmueble = await _repoInmueble.ObtenerPorId(id);
            if (inmueble == null)
                return NotFound();

            return View(inmueble);
        }

        // POST: Inmueble/Portada
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Portada(int InmuebleId, IFormFile? Archivo, [FromServices] IWebHostEnvironment environment)
        //HortEnviromet sirve para trabajar con archivos y rutas sin depender de dónde está instalada la app.
        {
            var inmueble = await _repoInmueble.ObtenerPorId(InmuebleId);
            if (inmueble == null)
                return NotFound();

            if (Archivo != null && Archivo.Length > 0)
            {
                //enviroment 
                var ruta = Path.Combine(environment.WebRootPath, "Uploads", "Inmuebles");

                if (!Directory.Exists(ruta))
                    Directory.CreateDirectory(ruta);

                var nombreArchivo = "portada_" + inmueble.IdInmueble + Path.GetExtension(Archivo.FileName);
                var rutaArchivo = Path.Combine(ruta, nombreArchivo);


                using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    await Archivo.CopyToAsync(stream);
                }

                inmueble.Portada = "/uploads/inmuebles/" + nombreArchivo;
                await _repoInmueble.Actualizar(inmueble);

                TempData["Mensaje"] = "Portada actualizada correctamente";
            }

            return RedirectToAction(nameof(Imagenes), new { id = InmuebleId });
        
        }

        // POST: Inmueble/EliminarPortada
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPortada(int InmuebleId, [FromServices] IWebHostEnvironment environment)
        {
            var inmueble = await _repoInmueble.ObtenerPorId(InmuebleId);
            if (inmueble == null)
                return NotFound();

            if (!string.IsNullOrEmpty(inmueble.Portada))
            {
                var ruta = Path.Combine(environment.WebRootPath, inmueble.Portada.TrimStart('/'));

                if (System.IO.File.Exists(ruta))
                {
                    System.IO.File.Delete(ruta);
                }

                inmueble.Portada = null;
                await _repoInmueble.Actualizar(inmueble);

                TempData["Mensaje"] = "Portada eliminada correctamente";
            }

            return RedirectToAction(nameof(Imagenes), new { id = InmuebleId });
        }
        // Endpoints para cargar los combos (select2= con Ajax, como pidió el profe en la segunda entrega
        [HttpGet]
        public IActionResult BuscarTipos(string term)
        {
            var tipos = _repoTipo.BuscarInmuebleTipos(term ?? "");
            var resultados = tipos.Select(t => new
            {
                id = t.IdTipo,
                text = t.DenominacionTipo
            });
            return Json(resultados);
        }

        [HttpGet]
        public IActionResult BuscarUsos(string term)
        {
            var usos = _repoUso.BuscarInmuebleUsos(term ?? "");
            var resultados = usos.Select(u => new
            {
                id = u.IdUso,
                text = u.DenominacionUso
            });
            return Json(resultados);
        }
        
        [HttpGet]
        public async Task<IActionResult> BuscarPropietarios(string term)
        {
            var propietarios = await _repoPropietario.BuscarPropietarioAsync(term ?? "");

            var results = propietarios.Select(p => new
            {
                id = p.IdPropietario,
                text = $"{p.ApellidoNombres} - {p.Documento}"
            });

            return Json(results);
        }

    }
}
