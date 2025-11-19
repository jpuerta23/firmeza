using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace AdminRazer.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public class AccountLoginDto
        {
            public string? Email { get; set; }
            public string? Password { get; set; }
            public bool RememberMe { get; set; }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AccountLoginDto dto)
        {
            if (dto == null)
                return BadRequest(new { Error = "Cuerpo de la petición vacío. Envíe { \"email\": \"...\", \"password\": \"...\" }" });

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { Error = "Email y password son requeridos." });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new { Error = "Usuario no encontrado." });

            // Solo permitir login si el usuario pertenece al rol Administrador (este proyecto es el panel admin)
            if (!await _userManager.IsInRoleAsync(user, "Administrador"))
            {
                return Forbid();
            }

            if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
                return StatusCode(423, new { Error = "Cuenta bloqueada por intentos fallidos. Contacta al administrador." });

            if (_userManager.Options.SignIn.RequireConfirmedAccount && !user.EmailConfirmed)
                return StatusCode(403, new { Error = "Email no confirmado. Revisa tu correo." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                // Realizar el sign-in para establecer la cookie de autenticación
                await _signInManager.SignInAsync(user, dto.RememberMe);
                return Ok(new { Succeeded = true, Message = "Login exitoso." });
            }

            if (result.IsLockedOut)
                return StatusCode(423, new { Error = "Cuenta bloqueada por intentos fallidos." });

            if (result.IsNotAllowed)
                return StatusCode(403, new { Error = "No permitido. Posible verificación pendiente o restricciones." });

            return Unauthorized(new { Error = "Credenciales incorrectas." });
        }
    }
}
