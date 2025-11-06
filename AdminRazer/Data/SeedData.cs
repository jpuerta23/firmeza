using Microsoft.AspNetCore.Identity;
using AdminRazer.Models;

namespace AdminRazer.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        string[] roleNames = { "Administrador", "Usuario" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminUser = await userManager.FindByEmailAsync("admin@admin.com");
        if (adminUser == null)
        {
            var newAdminUser = new IdentityUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                EmailConfirmed = true
            };

            var password = configuration["AdminPassword"] ?? "Admin123!";
            var result = await userManager.CreateAsync(newAdminUser, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdminUser, "Administrador");
            }
        }

        // -----------------------
        // Seed application data
        // -----------------------
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Seed Clientes
        if (!db.Clientes.Any())
        {
            db.Clientes.Add(new Cliente { Nombre = "jhon", Documento = "1000", Telefono = "3000003", Email = "Puertajhon@gmail.com" });
            db.Clientes.Add(new Cliente { Nombre = "María", Documento = "2000", Telefono = "3000004", Email = "maria@example.com" });
            await db.SaveChangesAsync();
        }

        // Seed Productos
        if (!db.Productos.Any())
        {
            db.Productos.Add(new Producto { Nombre = "Producto A", Precio = 10.5m });
            db.Productos.Add(new Producto { Nombre = "Producto B", Precio = 25.0m });
            await db.SaveChangesAsync();
        }

        // Seed Ventas (una muestra)
        if (!db.Ventas.Any())
        {
            var cliente = db.Clientes.First();
            var producto = db.Productos.First();

            var venta = new Venta
            {
                ClienteId = cliente.Id,
                Cliente = cliente,
                MetodoPago = "Efectivo",
                Fecha = DateTime.UtcNow,
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Producto = producto,
                        Cantidad = 2,
                        PrecioUnitario = producto.Precio
                    }
                }
            };

            venta.RecalculateTotal();
            db.Ventas.Add(venta);
            await db.SaveChangesAsync();
        }
    }
}