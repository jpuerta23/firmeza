using Microsoft.AspNetCore.Identity;

namespace Web.Api.Seed
{
    public static class DefaultUserSeeder
    {
        public static async Task SeedAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // 🔹 Crear roles por defecto
            string[] roles = new[] { "Admin", "Cliente" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 🔹 Crear usuario Admin por defecto
            var adminUser = await userManager.FindByNameAsync("admin_demo");
            if (adminUser == null)
            {
                var newAdmin = new IdentityUser
                {
                    UserName = "admin_demo",
                    Email = "admin@demo.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
            }

            // 🔹 Crear usuario Cliente por defecto
            var clientUser = await userManager.FindByNameAsync("cliente_demo");
            if (clientUser == null)
            {
                var newClient = new IdentityUser
                {
                    UserName = "cliente_demo",
                    Email = "cliente@demo.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newClient, "Cliente123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(newClient, "Cliente");
            }

            Console.WriteLine("✅ Usuarios y roles de prueba creados correctamente.");
        }
    }
}