using System.Text.Json;

namespace Web.Api.Middleware
{
    public class SuccessResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public SuccessResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            // ⛔ EXCLUIR SWAGGER COMPLETO
            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // ⛔ EXCLUIR ARCHIVOS ESTÁTICOS
            if (path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".png") ||
                path.EndsWith(".jpg") || path.EndsWith(".ico") || path.EndsWith(".html"))
            {
                await _next(context);
                return;
            }

            // Guardar el body original
            var originalBody = context.Response.Body;
            await using var tempBody = new MemoryStream();
            context.Response.Body = tempBody;

            await _next(context);

            // Leer la respuesta generada por el controller
            tempBody.Seek(0, SeekOrigin.Begin);
            var rawBody = await new StreamReader(tempBody).ReadToEndAsync();

            // ⛔ Si no es respuesta OK, copiar tal cual
            if (context.Response.StatusCode < 200 || context.Response.StatusCode >= 300)
            {
                tempBody.Seek(0, SeekOrigin.Begin);
                await tempBody.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
                return;
            }

            // ⛔ Si el Content-Type no es JSON, no tocar
            var contentType = context.Response.ContentType ?? "";
            if (!contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                tempBody.Seek(0, SeekOrigin.Begin);
                await tempBody.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
                return;
            }

            // Intentar deserializar
            object? data;
            try
            {
                data = JsonSerializer.Deserialize<object>(rawBody);
            }
            catch
            {
                data = rawBody;
            }

            // Crear respuesta final
            var wrapped = new
            {
                exito = true,
                codigo = context.Response.StatusCode,
                data
            };

            var json = JsonSerializer.Serialize(wrapped, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            context.Response.Body = originalBody;
            context.Response.ContentType = "application/json";
            // ❌ NO SETEAMOS CONTENT-LENGTH para evitar bugs
            await context.Response.WriteAsync(json);
        }
    }
}
