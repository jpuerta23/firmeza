using AdminRazer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml; // ✅ Necesario para EPPlus
using System.Reflection;
using QuestPDF.Infrastructure; // usar la infraestructura para LicenseType

var builder = WebApplication.CreateBuilder(args);

// Configurar la licencia de EPPlus: usar reflection (compatible con las distintas versiones de EPPlus)
try
{
    // Intentar localizar la propiedad estática 'License' (EPPlus 8+)
    var licenseProp = typeof(ExcelPackage).GetProperty("License", BindingFlags.Static | BindingFlags.Public);
    if (licenseProp != null)
    {
        var enumType = licenseProp.PropertyType;
        try
        {
            var enumValue = Enum.Parse(enumType, "NonCommercial");
            licenseProp.SetValue(null, enumValue);
        }
        catch
        {
            // ignorar si no se encuentra el valor exacto
        }
    }
    else
    {
        // Fallback: intentar establecer LicenseContext en versiones antiguas (OfficeOpenXml.LicenseContext)
        var licenseContextType = typeof(ExcelPackage).Assembly.GetType("OfficeOpenXml.LicenseContext");
        if (licenseContextType != null)
        {
            var lcProp = typeof(ExcelPackage).GetProperty("LicenseContext", BindingFlags.Static | BindingFlags.Public);
            var lcField = typeof(ExcelPackage).GetField("LicenseContext", BindingFlags.Static | BindingFlags.Public);
            try
            {
                var nonCommercialVal = Enum.Parse(licenseContextType, "NonCommercial");
                if (lcProp != null && lcProp.CanWrite)
                {
                    lcProp.SetValue(null, nonCommercialVal);
                }
                else if (lcField != null)
                {
                    lcField.SetValue(null, nonCommercialVal);
                }
            }
            catch
            {
                // ignorar
            }
        }
    }
}
catch
{
    // No bloquear el arranque si algo falla aquí; la excepción se lanzará cuando se intente usar EPPlus.
}

// Configurar la licencia de QuestPDF (Community) para entornos de evaluación o cuando aplique
// Usar la asignación simplificada. Ahora que importamos QuestPDF.Infrastructure, LicenseType está disponible.
QuestPDF.Settings.License = LicenseType.Community;

// No forzar URLs aquí: controla el puerto con la variable de entorno ASPNETCORE_URLS
// (p. ej. export ASPNETCORE_URLS="http://localhost:5050") o mediante launchSettings.json del IDE.

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrar HttpClient y Servicio de AI
builder.Services.AddHttpClient<AdminRazer.Services.IAiInsightService, AdminRazer.Services.GeminiInsightService>();
// Registrar Razor Pages para que las páginas de Identity (Login/Register) estén disponibles
builder.Services.AddRazorPages();

if (builder.Environment.IsProduction())
{
    var host = Environment.GetEnvironmentVariable("POSTGRESQL_ADDON_HOST");
    var port = Environment.GetEnvironmentVariable("POSTGRESQL_ADDON_PORT");
    var db = Environment.GetEnvironmentVariable("POSTGRESQL_ADDON_DB");
    var user = Environment.GetEnvironmentVariable("POSTGRESQL_ADDON_USER");
    var password = Environment.GetEnvironmentVariable("POSTGRESQL_ADDON_PASSWORD");

    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(db) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
    {
        var connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
    else
    {
        // Si faltan variables críticas, caer en la cadena por defecto para evitar excepciones en tiempo de arranque.
        var fallback = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(fallback))
        {
            throw new InvalidOperationException("No connection string available for Production environment. Please set POSTGRESQL_* environment variables or provide DefaultConnection in configuration.");
        }
        // Log a warning to help debugging (cuando no haya un logger disponible aún, usar Console)
        Console.WriteLine("[Warning] Production DB environment variables missing or incomplete; falling back to DefaultConnection from configuration.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(fallback));
    }
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<AdminRazer.Services.IExcelImportService, AdminRazer.Services.ExcelImportService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    // Aplica las migraciones para crear la base de datos y las tablas
    await dbContext.Database.MigrateAsync();
    // Crea los roles y el usuario administrador
    await SeedData.Initialize(services, builder.Configuration);
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configurar cultura para Colombia (COP)
var defaultDateCulture = "es-CO";
var ci = new System.Globalization.CultureInfo(defaultDateCulture);
ci.NumberFormat.CurrencySymbol = "$"; // Asegurar símbolo $

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(ci),
    SupportedCultures = new List<System.Globalization.CultureInfo> { ci },
    SupportedUICultures = new List<System.Globalization.CultureInfo> { ci }
};
app.UseRequestLocalization(localizationOptions);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Redirecciones para aceptar URLs en singular y mapearlas a los controladores en plural
app.MapGet("/venta", () => Results.Redirect("/Ventas", permanent: false));
app.MapGet("/venta/{id}", (string id) => Results.Redirect($"/Ventas/{id}", permanent: false));

app.MapGet("/cliente", () => Results.Redirect("/Clientes", permanent: false));
app.MapGet("/cliente/{id}", (string id) => Results.Redirect($"/Clientes/{id}", permanent: false));

app.Run();
