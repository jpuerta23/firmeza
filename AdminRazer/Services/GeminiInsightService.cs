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

        public async Task<string> GetDailyInsightAsync(int salesCount, decimal totalRevenue)
        {
            // Reusing the chat logic for daily insight could be an option, but keeping it simple for now as per original logic
            // but we can enhance it later.
            var apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_API_KEY_HERE")
            {
                return GenerateFallbackMessage(salesCount, totalRevenue);
            }

            try
            {
                var prompt = $"Actúa como un asistente de negocios entusiasta y profesional para el administrador de la tienda 'Firmeza'. " +
                             $"Hoy se han realizado {salesCount} ventas con un total de {totalRevenue:C}. " +
                             $"Genera un resumen corto (máximo 2 frases) que sea motivador o informativo sobre el rendimiento de hoy.";

                return await CallGeminiApi(prompt, apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API for Daily Insight");
                return GenerateFallbackMessage(salesCount, totalRevenue);
            }
        }

        public async Task<string> ChatAsync(string question)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_API_KEY_HERE")
            {
                return "Lo siento, no puedo responder en este momento porque mi clave de API no está configurada.";
            }

            try
            {
                // 1. Gather Context
                var context = await BuildContextAsync();

                // 2. Construct Prompt
                var prompt = $"Eres un asistente inteligente para el administrador de la tienda 'Firmeza'. " +
                             $"Tienes acceso a los siguientes datos de la tienda:\n\n{context}\n\n" +
                             $"Pregunta del usuario: {question}\n" +
                             $"Responde de manera concisa, útil y profesional. Si te preguntan por el producto más vendido, usa los datos proporcionados.";

                // 3. Call API
                return await CallGeminiApi(prompt, apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API for Chat");
                return "Lo siento, ocurrió un error al procesar tu pregunta.";
            }
        }

        private async Task<string> BuildContextAsync()
        {
            var sb = new StringBuilder();

            // Products
            var productos = await _productoRepository.GetAllAsync();
            sb.AppendLine("--- PRODUCTOS ---");
            foreach (var p in productos)
            {
                sb.AppendLine($"- ID: {p.Id}, Nombre: {p.Nombre}, Precio: {p.Precio:C}, Stock: {p.Stock}, Categoría: {p.Categoria}");
            }

            // Recent Sales (Last 30 days)
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;
            var ventas = await _ventaRepository.GetByFechaRangeAsync(startDate, endDate);
            
            // Calculate stats
            var totalRevenue = ventas.Sum(v => v.Total);
            var totalSalesCount = ventas.Count();

            // Best selling product
            var bestSelling = ventas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.ProductoId)
                .Select(g => new 
                { 
                    ProductoId = g.Key, 
                    TotalQuantity = g.Sum(d => d.Cantidad),
                    TotalRevenue = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .FirstOrDefault();

            string bestSellingProductName = "N/A";
            if (bestSelling != null)
            {
                var p = productos.FirstOrDefault(x => x.Id == bestSelling.ProductoId);
                bestSellingProductName = p?.Nombre ?? $"ID {bestSelling.ProductoId}";
            }

            sb.AppendLine("\n--- VENTAS (Últimos 30 días) ---");
            sb.AppendLine($"Total Ingresos: {totalRevenue:C}");
            sb.AppendLine($"Total Transacciones: {totalSalesCount}");
            sb.AppendLine($"Producto Más Vendido (Unidades): {bestSellingProductName} ({(bestSelling?.TotalQuantity ?? 0)} unidades)");

            return sb.ToString();
        }

        private async Task<string> CallGeminiApi(string prompt, string apiKey)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Gemini API returned {response.StatusCode}: {error}");
                return "Lo siento, hubo un problema al comunicarse con el servicio de IA.";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseString);

            var text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            return !string.IsNullOrWhiteSpace(text) ? text.Trim() : "No pude generar una respuesta.";
        }

        private string GenerateFallbackMessage(int count, decimal total)
        {
            if (count == 0) return "Hoy no se han registrado ventas todavía.";
            return $"Hoy se han vendido {count} productos, generando un total de {total:C}.";
        }

        // Clases para deserializar la respuesta de Gemini
        private class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate
        {
            [JsonPropertyName("content")]
            public Content? Content { get; set; }
        }

        private class Content
        {
            [JsonPropertyName("parts")]
            public List<Part>? Parts { get; set; }
        }

        private class Part
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}
