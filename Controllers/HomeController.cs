using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria10.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria10.Controllers;
[Authorize] 
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult MostrarCodigo(int code)
    {
        if (code == 404)
        {
            return View("EnConstruccion");
        }

        return View("Error");
    }
}
