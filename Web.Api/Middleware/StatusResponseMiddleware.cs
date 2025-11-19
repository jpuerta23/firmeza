using System.Text.Json;

namespace Web.Api.Middleware
{
    public class StatusResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<StatusResponseMiddleware> _logger;

        public StatusResponseMiddleware(RequestDelegate next, ILogger<StatusResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Evaluar la ruta y headers antes de crear buffers: si es swagger o recurso estático, no interceptar
            var path = context.Request.Path.ToString().ToLower();
            var accept = context.Request.Headers["Accept"].ToString().ToLower();

            bool skip =
                path.StartsWith("/swagger") ||
                accept.Contains("text/html") ||
                path.EndsWith(".css") ||
                path.EndsWith(".js") ||
                path.EndsWith(".png") ||
                path.EndsWith(".ico");

            if (skip)
            {
                // No interceptamos: dejamos que el pipeline procese normalmente
                await _next(context);
                return;
            }

            // Solo bufferizamos para rutas que NO son swagger ni recursos estáticos
            var originalBody = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            await _next(context);

            buffer.Seek(0, SeekOrigin.Begin);

            var status = context.Response.StatusCode;
            object? payload = null;

            switch (status)
            {
                case StatusCodes.Status401Unauthorized:
                    payload = new
                    {
                        Codigo = 401,
                        Mensaje = "No autenticado.",
                        Detalle = "Falta el token o es inválido."
                    };
                    break;

                case StatusCodes.Status403Forbidden:
                    payload = new
                    {
                        Codigo = 403,
                        Mensaje = "Acceso denegado.",
                        Detalle = "El usuario no tiene permisos suficientes."
                    };
                    break;

                case StatusCodes.Status404NotFound:
                    payload = new
                    {
                        Codigo = 404,
                        Mensaje = "Recurso no encontrado.",
                        Detalle = "La ruta o el recurso no existe."
                    };
                    break;
            }

            buffer.SetLength(0);

            if (payload != null)
            {
                context.Response.ContentType = "application/json";
                var json = JsonSerializer.Serialize(payload);
                await buffer.WriteAsync(System.Text.Encoding.UTF8.GetBytes(json));
            }
            else
            {
                // Si no hay payload custom, devolver la respuesta original
                buffer.Seek(0, SeekOrigin.Begin);
            }

            // Copia final al response real
            buffer.Seek(0, SeekOrigin.Begin);
            await buffer.CopyToAsync(originalBody);

            context.Response.Body = originalBody;
        }
    }
}
