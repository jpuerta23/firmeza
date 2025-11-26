using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminRazer.Repositories.Interfaces;

namespace AdminRazer.Controllers
{
    // Solo usuarios en el rol "Administrador" pueden acceder al panel.
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly AdminRazer.Services.IAiInsightService _aiService;

        public AdminController(
            IVentaRepository ventaRepository,
            IClienteRepository clienteRepository,
            IProductoRepository productoRepository,
            AdminRazer.Services.IAiInsightService aiService)
        {
            _ventaRepository = ventaRepository;
            _clienteRepository = clienteRepository;
            _productoRepository = productoRepository;
            _aiService = aiService;
        }

        public async Task<IActionResult> Index()
        {
            // Calcular ventas de hoy (UTC para consistencia con la BD)
            var today = DateTime.UtcNow.Date;
            
            var ventasHoy = await _ventaRepository.GetByFechaAsync(today);
            var ventasHoyList = ventasHoy.ToList();

            var count = ventasHoyList.Count;
            var total = ventasHoyList.Sum(v => v.Total);

            // Obtener conteos totales
            var clientsCount = await _clienteRepository.CountAsync();
            var productsCount = await _productoRepository.CountAsync();

            // Obtener historial de ventas (últimos 7 días)
            var last7Days = DateTime.UtcNow.Date.AddDays(-6);
            var salesHistoryDict = await _ventaRepository.GetSalesHistoryAsync(last7Days);

            var salesHistory = new List<decimal>();
            var salesDates = new List<string>();

            for (int i = 0; i < 7; i++)
            {
                var date = last7Days.AddDays(i);
                salesHistoryDict.TryGetValue(date, out var totalForDay);
                salesHistory.Add(totalForDay);
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

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] AdminRazer.ViewModels.ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest("La pregunta no puede estar vacía.");
            }

            var answer = await _aiService.ChatAsync(request.Question);
            return Ok(new AdminRazer.ViewModels.ChatResponse { Answer = answer });
        }
    }
}