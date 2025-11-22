using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminRazer.Controllers
{
    // Solo usuarios en el rol "Administrador" pueden acceder al panel.
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly AdminRazer.Data.ApplicationDbContext _context;
        private readonly AdminRazer.Services.IAiInsightService _aiService;

        public AdminController(AdminRazer.Data.ApplicationDbContext context, AdminRazer.Services.IAiInsightService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<IActionResult> Index()
        {
            // Calcular ventas de hoy (UTC para consistencia con la BD)
            var today = DateTime.UtcNow.Date;
            
            // Nota: En la BD las fechas se guardan como UTC (por convención en este proyecto).
            // Si se requiere zona horaria local, habría que ajustar.
            // Asumimos que la fecha en BD es UTC.
            
            var ventasHoy = await _context.Ventas
                .Where(v => v.Fecha.Date == today)
                .ToListAsync();

            var count = ventasHoy.Count;
            var total = ventasHoy.Sum(v => v.Total);

            // Usar el servicio de AI para generar el mensaje
            string mensaje = await _aiService.GetDailyInsightAsync(count, total);

            var model = new AdminRazer.ViewModels.DashboardViewModel
            {
                SalesCountToday = count,
                SalesTotalToday = total,
                AiInsightMessage = mensaje
            };

            return View(model);
        }
    }
}