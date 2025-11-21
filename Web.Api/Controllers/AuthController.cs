using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Web.Api.DTOs;
using AdminRazer.Data;
using AdminRazer.Models;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public AuthController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _context = context;
        }


        // ✅ Login con generación de token JWT
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            if (model == null)
                return BadRequest(new { Error = "Cuerpo vacío. Enviar { \"email\":\"...\", \"password\":\"...\" }" });

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { Error = "Email y password son requeridos." });

            // Ahora el login se realiza por email (email + password)
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Credenciales inválidas.");

            var roles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id), // ✅ IMPORTANTE: identificador único del usuario
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Agregar roles al token
            // Normalizar roles: si la base de datos contiene 'Admin' (Web.Api antiguo), mapear a 'Administrador'
            var normalizedRoles = roles.Select(role => role == "Admin" ? "Administrador" : role).ToList();
            authClaims.AddRange(normalizedRoles.Select(r => new Claim(ClaimTypes.Role, r)));

            var jwtSettings = _config.GetSection("Jwt");
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                user = new
                {
                    id = user.Id,
                    username = user.UserName,
                    roles = normalizedRoles
                }
            });
        }
        // ✅ Register: Crea usuario Identity + Cliente
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Verificar si el usuario ya existe
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { Error = "El correo ya está registrado." });

            // 2. Crear usuario Identity
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { Error = "Error al crear usuario", Details = result.Errors });
            }

            // 3. Asignar rol "Cliente"
            if (!await _roleManager.RoleExistsAsync("Cliente"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Cliente"));
            }
            await _userManager.AddToRoleAsync(user, "Cliente");

            // 4. Crear entidad Cliente vinculada
            var cliente = new Cliente
            {
                Nombre = model.Nombre,
                Documento = model.Documento,
                Telefono = model.Telefono,
                Email = model.Email,
                IdentityUserId = user.Id
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Registro exitoso. Ahora puedes iniciar sesión." });
        }
    }
}
