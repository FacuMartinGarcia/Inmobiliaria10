using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using Inmobiliaria10.Data.Repositories;
using Inmobiliaria10.Models;
using Inmobiliaria10.Models.ViewModels;
using Inmobiliaria10.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net; // 🔹 BCrypt.Net-Next

namespace Inmobiliaria10.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioRepo _repo;
        private readonly IRolRepo _rolRepo;
        private readonly IEmailService _emailService;
        private readonly IImagenRepo _imagenRepo;
        private readonly IWebHostEnvironment _env;

        public UsuarioController(IUsuarioRepo repo, IRolRepo rolRepo, IEmailService emailService, IImagenRepo imagenRepo, IWebHostEnvironment env)
        {
            _repo = repo;
            _rolRepo = rolRepo;
            _emailService = emailService;
            _imagenRepo = imagenRepo;
            _env = env;
        }

        // LISTADO
        public async Task<IActionResult> Index(int pagina = 1, string? search = null)
        {
            int cantidadPorPagina = 10;
            var (usuarios, totalRegistros) = await _repo.ListarTodosPaginado(pagina, cantidadPorPagina, search);

            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.PaginaActual = pagina;
            ViewBag.CantidadPorPagina = cantidadPorPagina;
            ViewBag.Search = search;

            return View(usuarios);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Detalle(int id, CancellationToken ct = default)
        {
            var usu = await _repo.ObtenerPorId(id, ct);
            if (usu == null)
                return NotFound();

            return View(usu);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear()
        {
            ViewBag.Roles = await ObtenerRolesSelectList();
            return View();
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario u)
        {
            var existente = await _repo.ObtenerPorAlias(u.Alias);
            if (existente != null)
            {
                ModelState.AddModelError("Alias", "El alias ya está en uso por otro usuario.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await ObtenerRolesSelectList();
                return View(u);
            }

            u.Password = HashPassword(u.Password);
            await _repo.Agregar(u);

            TempData["Mensaje"] = "Usuario creado correctamente.";
            return RedirectToAction("Index");
        }

        // EDITAR (GET)
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _repo.ObtenerPorId(id);
            if (usuario == null)
                return NotFound();

            var vm = new UsuarioEditViewModel
            {
                IdUsuario = usuario.IdUsuario,
                ApellidoNombres = usuario.ApellidoNombres,
                Alias = usuario.Alias,
                Email = usuario.Email,
                IdRol = usuario.IdRol
            };

            ViewBag.Roles = await ObtenerRolesSelectList();
            return View(vm);
        }

        // EDITAR (POST)
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(UsuarioEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await ObtenerRolesSelectList();
                return View(vm);
            }

            var usuarioActual = await _repo.ObtenerPorId(vm.IdUsuario);
            if (usuarioActual == null)
                return NotFound();

            // Validar alias duplicado
            var existente = await _repo.ObtenerPorAlias(vm.Alias);
            if (existente != null && existente.IdUsuario != vm.IdUsuario)
            {
                ModelState.AddModelError("Alias", "El alias ya está en uso por otro usuario.");
                ViewBag.Roles = await ObtenerRolesSelectList();
                return View(vm);
            }

            usuarioActual.ApellidoNombres = vm.ApellidoNombres;
            usuarioActual.Alias = vm.Alias;
            usuarioActual.Email = vm.Email;
            usuarioActual.IdRol = vm.IdRol;

            if (!string.IsNullOrEmpty(vm.Password))
            {
                usuarioActual.Password = HashPassword(vm.Password);
            }

            await _repo.Actualizar(usuarioActual);

            TempData["Mensaje"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // ELIMINAR
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var u = await _repo.ObtenerPorId(id);
            if (u == null)
                return NotFound();

            return View(u);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            await _repo.Eliminar(id);
            TempData["Mensaje"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Index");
        }

        // CAMBIAR PASSWORD
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CambiarPassword()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null)
            {
                TempData["Error"] = "Debes iniciar sesión para cambiar tu contraseña.";
                return RedirectToAction("Login");
            }

            int idUsuario = int.Parse(idClaim.Value);
            var usuario = await _repo.ObtenerPorId(idUsuario);
            if (usuario == null)
                return NotFound();

            var vm = new CambiarPasswordViewModel { IdUsuario = usuario.IdUsuario };
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var usuarioActual = await _repo.ObtenerPorId(vm.IdUsuario);
            if (usuarioActual == null)
                return NotFound();

            // Verificar contraseña actual
            if (!VerifyPassword(vm.PasswordActual, usuarioActual.Password))
            {
                ModelState.AddModelError("PasswordActual", "La contraseña actual es incorrecta.");
                return View(vm);
            }

            // Guardar nueva contraseña
            usuarioActual.Password = HashPassword(vm.NuevaPassword);
            await _repo.Actualizar(usuarioActual);

            TempData["Mensaje"] = "Contraseña cambiada correctamente.";
            return RedirectToAction("Perfil");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPassword(RecuperarPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var usuario = await _repo.ObtenerPorEmail(vm.Email);
            if (usuario == null)
            {
                TempData["Error"] = "No existe ningún usuario con ese correo.";
                return View(vm);
            }
            if (string.IsNullOrWhiteSpace(vm.Email))
            {
                TempData["Error"] = "Debe ingresar un correo válido.";
                return View(vm);
            }
            var token = Guid.NewGuid().ToString("N");
            await _repo.GuardarTokenReset(usuario.IdUsuario, token, DateTime.UtcNow.AddHours(1));

            var link = Url.Action("ResetPassword", "Usuario",
                new { token }, protocol: HttpContext.Request.Scheme);

            await _emailService.Enviar(vm.Email, "Recuperar contraseña",
                $"<p>Hacé click en el siguiente enlace para restablecer tu contraseña:</p><p><a href='{link}'>Restablecer Contraseña</a></p>");

            TempData["Mensaje"] ="Si el correo existe en nuestra base, recibirás un enlace de recuperación.";
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "El enlace de recuperación no es válido.";
                return RedirectToAction("RecuperarPassword");
            }

            var vm = new ResetPasswordViewModel { Token = token };
            return View(vm);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var usuario = await _repo.ObtenerPorToken(vm.Token);
            if (usuario == null || usuario.ResetTokenExpira <= DateTime.UtcNow)
            {
                TempData["Error"] = "El enlace de recuperación es inválido o ha expirado.";
                return RedirectToAction("RecuperarPassword");
            }

            await _repo.ActualizarPassword(usuario.IdUsuario, HashPassword(vm.NuevaPassword));
            await _repo.LimpiarTokenReset(usuario.IdUsuario);

            TempData["Mensaje"] = "Tu contraseña ha sido restablecida correctamente. Ahora puedes iniciar sesión.";
            return RedirectToAction("Login", "Usuario");
        }

        // LOGIN
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var input = (vm.Email ?? "").Trim();
            var emailNorm = input.ToLowerInvariant();
            var aliasNorm = input.ToUpperInvariant();

            var usuarios = await _repo.ListarTodos();
            var usuario = usuarios.FirstOrDefault(u =>
                (!string.IsNullOrEmpty(u.Email) && u.Email.ToLowerInvariant() == emailNorm) ||
                (!string.IsNullOrEmpty(u.Alias) && u.Alias.ToUpperInvariant() == aliasNorm));

            if (usuario == null || !VerifyPassword(vm.Password, usuario.Password))
            {
                ModelState.AddModelError("", "Credenciales incorrectas.");
                return View(vm);
            }

            // 🔎 Buscar foto en filesystem
            var fotoPerfil = "/img/user.png"; // fallback
            var userFolder = Path.Combine(_env.WebRootPath, "uploads", "usuarios", usuario.IdUsuario.ToString());

            if (Directory.Exists(userFolder))
            {
                var file = Directory.GetFiles(userFolder).FirstOrDefault();
                if (file != null)
                {
                    fotoPerfil = $"/uploads/usuarios/{usuario.IdUsuario}/{Path.GetFileName(file)}";
                }
            }

            var rolDb = usuario.Rol?.DenominacionRol;
            var rol = !string.IsNullOrWhiteSpace(rolDb)
                ? char.ToUpper(rolDb[0]) + rolDb.Substring(1).ToLower()
                : "Empleado";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Alias),
                new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty),
                new Claim("FullName", usuario.ApellidoNombres ?? string.Empty),
                new Claim(ClaimTypes.Role, rol),
                new Claim("Foto", fotoPerfil)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Usuario");
        }

        // Helpers
        private async Task<List<SelectListItem>> ObtenerRolesSelectList()
        {
            var roles = await _rolRepo.ListarTodos();
            return roles.Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.DenominacionRol
            }).ToList();
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null)
            {
                TempData["Error"] = "Debes iniciar sesión para ver tu perfil.";
                return RedirectToAction("Login");
            }

            int idUsuario = int.Parse(idClaim.Value);
            var usuario = await _repo.ObtenerPorId(idUsuario);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(UsuarioPerfilViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Tomar el Id desde el claim si no vino en hidden
            if (vm.IdUsuario <= 0)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out var idFromClaim))
                    vm.IdUsuario = idFromClaim;
            }

            var usuarioActual = await _repo.ObtenerPorId(vm.IdUsuario);
            if (usuarioActual == null)
                return NotFound();

            // Actualizar solo campos editables
            usuarioActual.ApellidoNombres = (vm.ApellidoNombres ?? "").Trim();
            usuarioActual.Alias = (vm.Alias ?? "").Trim();
            usuarioActual.Email = (vm.Email ?? "").Trim();

            try
            {
                var filas = await _repo.Actualizar(usuarioActual);

                if (filas == 0)
                {
                    TempData["Error"] = "No se guardaron cambios (0 filas afectadas).";
                    return View(vm);
                }

                // Refrescar claim del alias en sesión
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var aliasClaim = identity.FindFirst(ClaimTypes.Name);
                    if (aliasClaim != null) identity.RemoveClaim(aliasClaim);
                    identity.AddClaim(new Claim(ClaimTypes.Name, usuarioActual.Alias ?? ""));
                    await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
                }

                TempData["Mensaje"] = "Perfil actualizado correctamente.";
                return RedirectToAction("Perfil");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR UPDATE PERFIL: " + ex);
                TempData["Error"] = ex.Message;
                return View(vm);
            }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PerfilImagen()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null)
            {
                TempData["Error"] = "Debes iniciar sesión para cambiar la foto de perfil.";
                return RedirectToAction("Login");
            }

            int idUsuario = int.Parse(idClaim.Value);
            var usuario = await _repo.ObtenerPorId(idUsuario);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PerfilImagen(int idUsuario, IFormFile AvatarFile)
        {
            if (AvatarFile == null || AvatarFile.Length == 0)
            {
                TempData["Error"] = "Debe seleccionar una imagen válida.";
                return RedirectToAction(nameof(Perfil));
            }

            var wwwPath = _env.WebRootPath;
            var userFolder = Path.Combine(wwwPath, "uploads", "usuarios", idUsuario.ToString());
            if (!Directory.Exists(userFolder))
                Directory.CreateDirectory(userFolder);

            // Borrar foto anterior si existía
            foreach (var oldFile in Directory.GetFiles(userFolder))
            {
                System.IO.File.Delete(oldFile);
            }

            // Guardar nueva imagen
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(AvatarFile.FileName)}";
            var filePath = Path.Combine(userFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await AvatarFile.CopyToAsync(stream);
            }

            var relativeUrl = $"/uploads/usuarios/{idUsuario}/{fileName}";

            // 🔄 Actualizar claim "Foto"
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var fotoClaim = identity.FindFirst("Foto");
                if (fotoClaim != null)
                    identity.RemoveClaim(fotoClaim);

                identity.AddClaim(new Claim("Foto", relativeUrl));
                await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
            }

            TempData["Mensaje"] = "Foto de perfil actualizada correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PerfilDatos()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null)
                return RedirectToAction("Login");

            int idUsuario = int.Parse(idClaim.Value);
            var usuario = await _repo.ObtenerPorId(idUsuario);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }
    }
}
