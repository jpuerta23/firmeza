using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AdminRazer.Models;

namespace AdminRazer.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AdminRazer.Data.ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, AdminRazer.Data.ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            // Si el usuario está autenticado, mostrar el panel de administrador solo para roles autorizados.
            return RedirectToAction("Index", "Admin");
        }

        // Landing Page
        
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
}