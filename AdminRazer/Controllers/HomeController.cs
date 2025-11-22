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

        // Lógica para el Dashboard público o landing page (si aplica)
        // En este caso, parece que la vista Index es la landing page pública.
        // Si el requerimiento es para el dashboard de ADMIN, debería estar en AdminController o similar.
        // Re-leyendo el requerimiento: "quiero que en adminrazer en el dasboard le salga..."
        // El código actual redirige a AdminController si está autenticado.
        // Voy a asumir que el usuario se refiere al Dashboard del Administrador.
        
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