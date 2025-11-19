using System.Net;
using System.Text.Json;

namespace Web.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado");

                // Evitar romper Swagger
                if (context.Request.Path.StartsWithSegments("/swagger"))
                {
                    throw;
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new Dictionary<string, object>
                {
                    ["Codigo"] = 500,
                    ["Mensaje"] = "Ocurrió un error interno.",
                };

                if (_env.IsDevelopment())
                {
                    response["Detalle"] = ex.Message;
                    response["StackTrace"] = ex.StackTrace ?? string.Empty;

                    var innerList = new List<string>();
                    var inner = ex.InnerException;
                    while (inner != null)
                    {
                        innerList.Add(inner.Message);
                        inner = inner.InnerException;
                    }
                    response["InnerExceptions"] = innerList;
                }

                var result = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(result);
            }
        }
    }
}
