// Controllers/PropietariosController.cs
using Inmobiliaria10.Models;
using Inmobiliaria10.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria10.Controllers;

[Authorize]
public class PropietarioController : Controller
{
    private readonly IPropietarioRepo _repo;
    private readonly IInmuebleRepo _repoInmueble;

    public PropietarioController(
        IPropietarioRepo repo,
        IInmuebleRepo repoInmueble
        )
    {
        _repo = repo;
        _repoInmueble = repoInmueble;
    }

 
    // GET: /Propietarios
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        var data = await _repo.BuscarPaginado(q, page, pageSize);
        var vm = new PropietarioIndexVm { Data = data };
        return View(vm);
    }

    // GET: /Propietarios/Details/5
    public async Task<IActionResult> Detalles(int id)
    {
        var p = await _repo.ObtenerPorId(id);
        if (p == null) return NotFound();
        return View(p);
    }

    // GET: /Propietarios/Create
    public IActionResult Crear() => View(new Propietario());

    // POST: /Propietarios/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Propietario p)
    {
        if (await _repo.ExistsDocumento(p.Documento))
            ModelState.AddModelError(nameof(p.Documento), "Ya existe un propietario con ese documento.");

        if (!ModelState.IsValid) return View(p);

        p.IdPropietario = await _repo.Crear(p);
        TempData["Mensaje"] = "Propietario creado.";
        return RedirectToAction(nameof(Detalles), new { id = p.IdPropietario });
    }

    // GET: /Propietarios/Edit/5
    public async Task<IActionResult> Editar(int id)
    {
        var p = await _repo.ObtenerPorId(id);
        if (p == null) return NotFound();
        return View(p);
    }

    // POST: /Propietarios/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Propietario p)
    {
        if (id != p.IdPropietario) return BadRequest();

        if (await _repo.ExistsDocumento(p.Documento, id))
            ModelState.AddModelError(nameof(p.Documento), "Ya existe otro propietario con ese documento.");

        if (!ModelState.IsValid) return View(p);

        var ok = await _repo.Modificar(p);
        if (!ok) return NotFound();

        TempData["Mensaje"] = "Propietario actualizado.";
        return RedirectToAction(nameof(Detalles), new { id });
    }

    // GET: /Propietarios/Delete/5
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Borrar(int id)
    {
        var p = await _repo.ObtenerPorId(id);
        if (p == null) 
            return NotFound();

        var tieneInmuebles = await _repoInmueble
            .ListarPorPropietario(id, 1, 1);

        if (tieneInmuebles.registros.Any())
        {
            TempData["Error"] = "No es posible eliminar al propietario, ya que posee al menos un inmueble asociado.";
            return RedirectToAction(nameof(Index));
        }

        return View(p);
    }


    // POST: /Propietarios/DeleteConfirmed/5
    [Authorize(Roles = "Administrador")]
    [HttpPost, ActionName("Borrar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {

        var tieneInmuebles = await _repoInmueble.ListarPorPropietario(id, 1, 1);
        if (tieneInmuebles.registros.Any())
        {
            TempData["Error"] = "No es posible eliminar al propietario, ya que posee al menos un inmueble asociado.";
            return RedirectToAction(nameof(Index));
        }

        var ok = await _repo.Borrar(id);
        if (!ok) return NotFound();

        TempData["Mensaje"] = "Propietario eliminado.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> VerInmuebles(int id, int pagina = 1, string? search = null)
    {
        int cantidadPorPagina = 10; 
        var propietario = await _repo.ObtenerPorId(id);
        if (propietario == null) return NotFound();

        var (inmuebles, total) = await _repoInmueble.ListarPorPropietario(id, pagina, cantidadPorPagina, search);

        ViewBag.Propietario = propietario;
        ViewBag.TotalInmuebles = total;
        ViewBag.PaginaActual = pagina;
        ViewBag.CantidadPorPagina = cantidadPorPagina;

        return View(inmuebles);
    }

}
