using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria10.Controllers
{
    //[Authorize]
    public class ImagenesController : Controller
    {
        private readonly IImagenRepo _repositorio;

        public ImagenesController(IImagenRepo repositorio)
        {
            _repositorio = repositorio;
        }

        // POST: Imagenes/Alta
        [HttpPost]
        public async Task<IActionResult> Alta(int id, List<IFormFile> imagenes, [FromServices] IWebHostEnvironment environment)
        {
            if (imagenes == null || imagenes.Count == 0)
            {
                TempData["MensajeError"] = "No se recibieron archivos.";
                return RedirectToAction("Imagenes", "Inmueble", new { id });
            }

            string wwwPath = environment.WebRootPath;
            string path = Path.Combine(wwwPath, "Uploads", "Inmuebles", id.ToString());

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (var file in imagenes)
            {
                if (file.Length > 0)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                    var rutaArchivo = Path.Combine(path, nombreArchivo);

                    using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var imagen = new Imagen
                    {
                        IdInmueble = id,
                        Url = $"/Uploads/Inmuebles/{id}/{nombreArchivo}"
                    };

                    await _repositorio.Alta(imagen);
                }
            }

            TempData["Mensaje"] = "Imágenes subidas correctamente.";
            return RedirectToAction("Imagenes", "Inmueble", new { id = id });
        }

        // POST: Imagenes/Eliminar
        [HttpPost]
        //[Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Eliminar(int id, [FromServices] IWebHostEnvironment environment)
        {
            try
            {
                var entidad = await _repositorio.ObtenerPorId(id);
                if (entidad == null)
                {
                    TempData["MensajeError"] = "Imagen no encontrada.";
                    return RedirectToAction("Imagenes", "Inmueble", new { id = id });
                }

                // Borrar archivo físico
                string wwwPath = environment.WebRootPath;
                string rutaArchivo = Path.Combine(wwwPath, entidad.Url.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(rutaArchivo))
                {
                    System.IO.File.Delete(rutaArchivo);
                }

                await _repositorio.Baja(id);

                TempData["Mensaje"] = "Imagen eliminada correctamente.";
                return RedirectToAction("Imagenes", "Inmueble", new { id = entidad.IdInmueble });
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Error al eliminar la imagen: {ex.Message}";
                return RedirectToAction("Imagenes", "Inmueble", new { id });
            }
        }
    }
}
