using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdminRazer.Services
{
    public class GeminiInsightService : IAiInsightService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiInsightService> _logger;

        public GeminiInsightService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiInsightService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetDailyInsightAsync(int salesCount, decimal totalRevenue)
        {
            var apiKey = _configuration["Gemini:ApiKey"];

            // Fallback si no hay API Key configurada
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_API_KEY_HERE")
            {
                return GenerateFallbackMessage(salesCount, totalRevenue);
            }

            try
            {
                var prompt = $"Actúa como un asistente de negocios entusiasta y profesional para el administrador de la tienda 'Firmeza'. " +
                             $"Hoy se han realizado {salesCount} ventas con un total de {totalRevenue:C}. " +
                             $"Genera un resumen corto (máximo 2 frases) que sea motivador o informativo sobre el rendimiento de hoy.";

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
                    _logger.LogWarning($"Gemini API returned {response.StatusCode}");
                    return GenerateFallbackMessage(salesCount, totalRevenue);
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseString);

                var text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                return !string.IsNullOrWhiteSpace(text) ? text.Trim() : GenerateFallbackMessage(salesCount, totalRevenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return GenerateFallbackMessage(salesCount, totalRevenue);
            }
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
