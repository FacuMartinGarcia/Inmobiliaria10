using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;


namespace Inmobiliaria10.Controllers
{
    [Authorize]
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

        [HttpPost]
        public async Task<IActionResult> CambiarImagenPerfil(IFormFile AvatarFile, [FromServices] IWebHostEnvironment env)
        {
            if (AvatarFile == null || AvatarFile.Length == 0)
            {
                TempData["MensajeError"] = "No se recibió ninguna imagen.";
                return RedirectToAction("Perfil", "Usuario");
            }

            // Id del usuario desde las claims (evita hidden manipulables)
            var idUsuario = int.Parse(User.FindFirst("IdUsuario")!.Value);
            Console.WriteLine($"🟡 idUsuario detectado: {idUsuario}");

            // Buscar imagen anterior (puede ser null)
            var anterior = await _repositorio.ObtenerPerfilPorUsuario(idUsuario);

            // Carpeta física por usuario (usa 'uploads' en minúscula)
            var www = env.WebRootPath;
            var carpetaUsuario = Path.Combine(www, "uploads", "Usuarios", idUsuario.ToString());
            if (!Directory.Exists(carpetaUsuario))
                Directory.CreateDirectory(carpetaUsuario);

            // Guardar archivo nuevo
            var ext = Path.GetExtension(AvatarFile.FileName);
            var nombre = $"{Guid.NewGuid()}{ext}";
            var rutaFisica = Path.Combine(carpetaUsuario, nombre);

            using (var fs = new FileStream(rutaFisica, FileMode.Create))
                await AvatarFile.CopyToAsync(fs);

            // Ruta relativa que se guarda en DB (sin slash inicial, normalizada en minúscula)
            var nuevaRuta = Path.Combine("uploads", "Usuarios", idUsuario.ToString(), nombre)
                            .Replace("\\", "/");

            await _repositorio.AltaPerfil(idUsuario, nuevaRuta);

            // Eliminar imagen anterior si existía
            if (anterior != null && !string.IsNullOrEmpty(anterior.Url))
            {
                var rutaAnterior = Path.Combine(www, anterior.Url.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(rutaAnterior))
                {
                    System.IO.File.Delete(rutaAnterior);
                }
            }

            // 🔄 Actualizar claim "Foto" para reflejar la nueva imagen sin re-loguear
            var identity = (ClaimsIdentity)User.Identity!;
            var fotoClaim = identity.FindFirst("Foto");
            if (fotoClaim != null)
                identity.RemoveClaim(fotoClaim);

            identity.AddClaim(new Claim("Foto", "/" + nuevaRuta));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            TempData["Mensaje"] = "Imagen de perfil actualizada correctamente.";
            return RedirectToAction("Perfil", "Usuario");
        }
    }
}
