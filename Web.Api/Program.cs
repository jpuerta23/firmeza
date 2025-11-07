using AdminRazer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Web.Api.Middleware; // middleware

var builder = WebApplication.CreateBuilder(args);

// 🔹 Cargar la cadena de conexión desde appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 🔹 Registrar el DbContext con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 🔹 Registrar AutoMapper (buscará perfiles en todos los ensamblados)
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// 🔹 Agregar controladores
builder.Services.AddControllers();

// 🔹 Configurar validación automática de modelos
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errores = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .Select(e => new
            {
                Campo = e.Key,
                Errores = e.Value?.Errors.Select(er => er.ErrorMessage)
            });

        return new BadRequestObjectResult(new
        {
            Mensaje = "Los datos enviados no son válidos.",
            Errores = errores
        });
    };
});

// 🔹 Configurar Swagger (documentación interactiva de la API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 (Opcional) Habilitar CORS si el frontend (AdminRazer) va a consumir la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 🔹 Middleware global de manejo de errores
app.UseMiddleware<ErrorHandlingMiddleware>();

// 🔹 Activar Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Usar CORS
app.UseCors("AllowAll");

// 🔹 Mapear controladores
app.MapControllers();

// 🔹 Ejecutar aplicación
app.Run();
