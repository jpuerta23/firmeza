using Microsoft.AspNetCore.Identity;
using AdminRazer.Models;

namespace AdminRazer.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Queremos asegurarnos que existe el rol "Cliente"; si existe un rol antiguo "Usuario" moveremos sus usuarios.
        string[] roleNames = { "Administrador", "Cliente" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Si existe el rol antiguo "Usuario", migrar sus usuarios al nuevo rol "Cliente" y eliminar el rol antiguo.
        if (await roleManager.RoleExistsAsync("Usuario"))
        {
            var usuariosEnRol = await userManager.GetUsersInRoleAsync("Usuario");
            foreach (var u in usuariosEnRol)
            {
                // agregar a Cliente si no está
                if (!await userManager.IsInRoleAsync(u, "Cliente"))
                {
                    await userManager.AddToRoleAsync(u, "Cliente");
                }
                // remover de Usuario
                await userManager.RemoveFromRoleAsync(u, "Usuario");
            }

            // eliminar el rol Usuario (si ya no tiene usuarios)
            var rolUsuario = await roleManager.FindByNameAsync("Usuario");
            if (rolUsuario != null)
            {
                await roleManager.DeleteAsync(rolUsuario);
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

        // Seed Clientes: ahora vinculamos cada cliente con un IdentityUser y guardamos el PasswordHash
        if (!db.Clientes.Any())
        {
            // definir contraseñas por defecto (en producción obtener de config o proceso separado)
            var defaultPasswordCliente = configuration["DefaultClientePassword"] ?? "Cliente123!";

            var clientesSeed = new List<Cliente>
            {
                new Cliente { Nombre = "jhon", Documento = "1000", Telefono = "3000003", Email = "Puertajhon@gmail.com" },
                new Cliente { Nombre = "María", Documento = "2000", Telefono = "3000004", Email = "maria@example.com" }
            };

            foreach (var cliente in clientesSeed)
            {
                // crear usuario Identity si no existe
                var existingUser = await userManager.FindByEmailAsync(cliente.Email);
                if (existingUser == null)
                {
                    var identityUser = new IdentityUser
                    {
                        UserName = cliente.Email,
                        Email = cliente.Email,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(identityUser, defaultPasswordCliente);
                    if (createResult.Succeeded)
                    {
                        // asignar rol Cliente
                        await userManager.AddToRoleAsync(identityUser, "Cliente");

                        // actualizar entidad cliente con IdentityUserId y PasswordHash
                        cliente.IdentityUserId = identityUser.Id;
                        cliente.PasswordHash = identityUser.PasswordHash; // Identity setea el PasswordHash al crear el usuario
                    }
                    else
                    {
                        // Si falla la creación del usuario, tomar el primer error y lanzar o registrar según convenga.
                        // Aquí lo registramos en consola para no interrumpir el seed completo.
                        var errors = string.Join(';', createResult.Errors.Select(e => e.Description));
                        Console.WriteLine($"No se pudo crear el usuario Identity para {cliente.Email}: {errors}");
                    }
                }
                else
                {
                    // usuario ya existe: enlazar su Id y hash
                    cliente.IdentityUserId = existingUser.Id;
                    cliente.PasswordHash = existingUser.PasswordHash;

                    // asegurar que el usuario esté en el rol Cliente
                    if (!await userManager.IsInRoleAsync(existingUser, "Cliente"))
                    {
                        await userManager.AddToRoleAsync(existingUser, "Cliente");
                    }
                }

                db.Clientes.Add(cliente);
            }

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