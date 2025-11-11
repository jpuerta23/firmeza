using AdminRazer.Data;
using AdminRazer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using System.Text;
using Web.Api.Seed;
using Web.Api.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Cargar la cadena de conexión desde appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 🔹 Registrar el DbContext con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 🔹 Configurar Identity (usuarios y roles)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 🔹 Registrar AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// 🔹 Configurar JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// 🔹 Configurar comportamiento de validación automática
builder.Services.AddControllers();
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

// 🔹 Configurar Swagger con soporte JWT y descripción personalizada
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Web API - Firmeza",
        Version = "v1",
        Description = "API para gestión de clientes, productos y ventas con autenticación JWT",
        Contact = new OpenApiContact
        {
            Name = "Jhon Puerta Mendoza",
            Email = "PuertaJhon023@gmail.com"
        }
    });

    // 🔸 Autorización JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT (ejemplo: 'Bearer eyJhbGciOi...')"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 🔹 CORS (permitir todo durante desarrollo)
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

// 🔹 Swagger con UI amigable (y acceso en raíz)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API Firmeza v1");
        c.RoutePrefix = string.Empty; // http://localhost:5000/
        c.DocumentTitle = "Documentación de la API - Firmeza";
    });
}

// 🔹 CORS
app.UseCors("AllowAll");

// 🔹 Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Mapear controladores
app.MapControllers();

// 🔹 Crear usuarios y roles "quemados" al iniciar

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await DefaultUserSeeder.SeedAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al ejecutar el seeder: {ex.Message}");
    }
}

// 🔹 Ejecutar la aplicación
app.Run();
