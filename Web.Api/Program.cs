using AdminRazer.Data;
using AdminRazer.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. BASE DE DATOS
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 2. AUTOMAPPER
builder.Services.AddAutoMapper(typeof(Program));

// 3. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. JWT AUTHENTICATION
var jwtKey = builder.Configuration["Jwt:Key"];

// Validar que Jwt:Key esté presente para evitar pasar null a Encoding.GetBytes
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("La clave JWT (Jwt:Key) no está configurada. Agrega Jwt:Key en appsettings.json o variables de entorno.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });


// 5. AUTORIZACIÓN

builder.Services.AddAuthorization();


// 6. CONTROLADORES

builder.Services.AddControllers();


// 7. IDENTITY

var identityBuilder = builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
});

identityBuilder.AddEntityFrameworkStores<ApplicationDbContext>();
identityBuilder.AddDefaultTokenProviders();
identityBuilder.AddSignInManager();


// 7.5. EMAIL SERVICE

builder.Services.Configure<Web.Api.Services.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<Web.Api.Services.IEmailService, Web.Api.Services.SmtpEmailService>();


// 7.6. REPOSITORIES & SERVICES

builder.Services.AddScoped<AdminRazer.Repositories.Interfaces.IClienteRepository, AdminRazer.Repositories.ClienteRepository>();
builder.Services.AddScoped<AdminRazer.Repositories.Interfaces.IProductoRepository, AdminRazer.Repositories.ProductoRepository>();
builder.Services.AddScoped<AdminRazer.Repositories.Interfaces.IVentaRepository, AdminRazer.Repositories.VentaRepository>();
builder.Services.AddScoped<Web.Api.Services.IVentaService, Web.Api.Services.VentaService>();

// 8. SWAGGER + JWT

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Ingresa: Bearer TU_TOKEN",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 9. CONSTRUIR APP

var app = builder.Build();

// 10. MIDDLEWARE

// Habilitar Swagger en todos los entornos
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Firmeza API v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz (http://localhost:5000)
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
