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
            
            var ventasHoy = await _context.Ventas
                .Where(v => v.Fecha.Date == today)
                .ToListAsync();

            var count = ventasHoy.Count;
            var total = ventasHoy.Sum(v => v.Total);

            // Obtener conteos totales
            var clientsCount = await _context.Clientes.CountAsync();
            var productsCount = await _context.Productos.CountAsync();

            // Obtener historial de ventas (últimos 7 días)
            var last7Days = DateTime.UtcNow.Date.AddDays(-6);
            var salesHistoryData = await _context.Ventas
                .Where(v => v.Fecha.Date >= last7Days)
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Total) })
                .ToListAsync();

            var salesHistory = new List<decimal>();
            var salesDates = new List<string>();

            for (int i = 0; i < 7; i++)
            {
                var date = last7Days.AddDays(i);
                var salesForDay = salesHistoryData.FirstOrDefault(s => s.Date == date);
                salesHistory.Add(salesForDay?.Total ?? 0);
                salesDates.Add(date.ToString("dd MMM"));
            }

            // Usar el servicio de AI para generar el mensaje
            string mensaje = await _aiService.GetDailyInsightAsync(count, total);

            var model = new AdminRazer.ViewModels.DashboardViewModel
            {
                SalesCountToday = count,
                SalesTotalToday = total,
                AiInsightMessage = mensaje,
                ClientsCount = clientsCount,
                ProductsCount = productsCount,
                SalesHistory = salesHistory,
                SalesDates = salesDates
            };

            return View(model);
        }
    }
}