// ...existing code...
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AdminRazer.Data;

namespace AdminRazer.Controllers
{
    [ApiController]
    [Route("/debug")]
    public class DebugController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public DebugController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        [HttpGet("admin-info")]
        public async Task<IActionResult> AdminInfo()
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var admin = await _userManager.FindByEmailAsync("admin@admin.com");
            if (admin == null)
                return Ok(new { Exists = false });

            var roles = await _userManager.GetRolesAsync(admin);
            return Ok(new
            {
                Exists = true,
                Id = admin.Id,
                Email = admin.Email,
                UserName = admin.UserName,
                EmailConfirmed = admin.EmailConfirmed,
                Roles = roles
            });
        }

        [HttpPost("reset-admin-password")]
        public async Task<IActionResult> ResetAdminPassword([FromBody] ResetPasswordDto dto)
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var admin = await _userManager.FindByEmailAsync("admin@admin.com");
            if (admin == null)
                return NotFound(new { Exists = false, Mensaje = "Usuario admin no encontrado." });

            var newPassword = dto?.NewPassword;
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                // tomar de configuration falla aquí (no hay acceso), usar password por defecto conocido
                newPassword = "Admin123!";
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(admin);
            var result = await _userManager.ResetPasswordAsync(admin, token, newPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { Succeeded = false, Errores = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { Succeeded = true, Mensaje = "Contraseña admin reseteada. Usa la nueva contraseña para hacer login.", NewPassword = newPassword });
        }

        // DTO para reset
        public class ResetPasswordDto { public string? NewPassword { get; set; } }
    }
}
// ...existing code...
