using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AdminRazer.Repositories.Interfaces;
using AdminRazer.Models;

namespace AdminRazer.Services
{
    public class GeminiInsightService : IAiInsightService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiInsightService> _logger;
        private readonly IProductoRepository _productoRepository;
        private readonly IVentaRepository _ventaRepository;

        // MODELO ACTUALIZADO Y VÁLIDO
        private const string GeminiModel = "gemini-2.0-flash";

        public GeminiInsightService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiInsightService> logger,
            IProductoRepository productoRepository,
            IVentaRepository ventaRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _productoRepository = productoRepository;
            _ventaRepository = ventaRepository;
        }

        // =====================================
        // RESUMEN DIARIO
        // =====================================
        public async Task<string> GetDailyInsightAsync(int salesCount, decimal totalRevenue)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return GenerateFallbackMessage(salesCount, totalRevenue);

            try
            {
                var prompt = $"Actúa como un asistente de negocios para la tienda 'Firmeza'. " +
                             $"Hoy se realizaron {salesCount} ventas con un total de {totalRevenue:C}. " +
                             $"Crea un resumen motivador de máximo 2 frases.";

                return await CallGeminiApi(prompt, apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetDailyInsightAsync");
                return GenerateFallbackMessage(salesCount, totalRevenue);
            }
        }

        // =====================================
        // CHAT GENERAL
        // =====================================
        public async Task<string> ChatAsync(string question)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return "La clave API de Gemini no está configurada.";

            try
            {
                var context = await BuildContextAsync();

                var prompt =
                    $"Eres un asistente inteligente para el administrador de 'Firmeza'. " +
                    $"Aquí tienes el contexto completo y actualizado de la tienda (productos, inventario y ventas recientes):\n\n" +
                    $"{context}\n\n" +
                    $"Pregunta del usuario: {question}\n" +
                    "Responde usando únicamente los datos del contexto. " +
                    "No digas nunca que no tienes acceso a información; si algo no aparece en el contexto responde: 'esa información no está en el registro'. " +
                    "Sé profesional, claro y analítico.";

                return await CallGeminiApi(prompt, apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ChatAsync");
                return "Ocurrió un error procesando tu solicitud.";
            }
        }

        // =====================================
        // CONSTRUIR CONTEXTO COMPLETO
        // =====================================
        private async Task<string> BuildContextAsync()
        {
            var sb = new StringBuilder();

            // ==============================
            // PRODUCTOS
            // ==============================
            var productos = await _productoRepository.GetAllAsync();
            sb.AppendLine("=== PRODUCTOS EN INVENTARIO ===");

            foreach (var p in productos)
            {
                sb.AppendLine(
                    $"- ID: {p.Id} | Nombre: {p.Nombre} | Precio: {p.Precio:C} | Stock: {p.Stock} | Categoría: {p.Categoria}");
            }

            // ==============================
            // TODAS LAS VENTAS
            // ==============================
            sb.AppendLine("\n=== ÚLTIMAS 50 VENTAS REGISTRADAS ===");

            var ventas = await _ventaRepository.GetRecentAsync(50); // <- LIMITADO A 50 RECIENTES

            if (!ventas.Any())
            {
                sb.AppendLine("No hay ventas registradas.");
            }
            else
            {
                foreach (var venta in ventas.OrderByDescending(v => v.Fecha))
                {
                    sb.AppendLine($"\n> Venta #{venta.Id} | Fecha: {venta.Fecha} | Total: {venta.Total:C}");

                    foreach (var d in venta.Detalles)
                    {
                        var prod = productos.FirstOrDefault(x => x.Id == d.ProductoId);
                        string nombre = prod?.Nombre ?? "Producto desconocido";

                        sb.AppendLine($"   - {nombre} | Cantidad: {d.Cantidad} | Subtotal: {d.Subtotal:C}");
                    }
                }
            }

            // ==============================
            // ESTADÍSTICAS GLOBALES
            // ==============================
            var totalRevenue = ventas.Sum(v => v.Total);
            var totalSales = ventas.Count();

            sb.AppendLine("\n=== ESTADÍSTICAS GLOBALES ===");
            sb.AppendLine($"Total histórico de ingresos: {totalRevenue:C}");
            sb.AppendLine($"Total de ventas registradas: {totalSales}");

            // Producto más vendido global
            var best = ventas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.ProductoId)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    Cant = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.Cant)
                .FirstOrDefault();

            if (best != null)
            {
                var p = productos.FirstOrDefault(x => x.Id == best.ProductoId);
                sb.AppendLine($"Producto más vendido: {p?.Nombre ?? "Desconocido"} ({best.Cant} unidades)");
            }

            return sb.ToString();
        }

        // =====================================
        // LLAMAR API GEMINI
        // =====================================
        private async Task<string> CallGeminiApi(string prompt, string apiKey)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={apiKey}";

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Gemini error {response.StatusCode}: {responseContent}");
                return "Hubo un problema al comunicarse con el servicio de IA.";
            }

            try
            {
                var json = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var text = json
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "La IA no devolvió contenido.";
            }
            catch
            {
                return "La IA respondió en un formato inesperado.";
            }
        }

        // =====================================
        // FALLBACK
        // =====================================
        private string GenerateFallbackMessage(int count, decimal total)
        {
            if (count == 0)
                return "Hoy aún no se registran ventas.";

            return $"Hoy se han registrado {count} ventas por un total de {total:C}.";
        }
    }
}
