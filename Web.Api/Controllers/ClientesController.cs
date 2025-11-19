using AdminRazer.Data;
using AdminRazer.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Web.Api.DTOs;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ClientesController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ClientesController(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<ClientesController> logger,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ============================================================
        //  PERFIL DEL USUARIO AUTENTICADO (ADMIN Y CLIENTE)
        // ============================================================

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("me")]
        public async Task<ActionResult<ClienteDto>> GetMiPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            _logger.LogInformation("GET /api/Clientes/me invoked by userId={userId} role={role}", userId, role);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token inválido.");

            // Si es ADMIN → puede ver su perfil aunque no exista en Clientes
            if (role == "Administrador")
            {
                return Ok(new
                {
                    Mensaje = "Eres administrador. No tienes perfil de cliente.",
                    UserId = userId,
                    Rol = role
                });
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            if (cliente == null)
                return NotFound("No existe un perfil de cliente vinculado al usuario.");

            return Ok(_mapper.Map<ClienteDto>(cliente));
        }

        // ============================================================
        //  CLIENTE – ACTUALIZAR SU PERFIL
        // ============================================================

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Cliente")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMiPerfil([FromBody] ClienteCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            _mapper.Map(dto, cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  CLIENTE – BORRAR SU CUENTA
        // ============================================================

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Cliente")]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMiCuenta()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  CRUD COMPLETO PARA ADMINISTRADOR
        // ============================================================

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Administrador")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            var clientes = await _context.Clientes.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ClienteDto>>(clientes));
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Administrador")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            return Ok(_mapper.Map<ClienteDto>(cliente));
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Administrador")]
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> PostCliente([FromBody] ClienteCreateDto dto)
        {
            var cliente = _mapper.Map<Cliente>(dto);

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id },
                _mapper.Map<ClienteDto>(cliente));
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, [FromBody] ClienteCreateDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            _mapper.Map(dto, cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        //  ADMIN: VINCULAR CLIENTE A UN USUARIO DE IDENTITY
        // ============================================================

        // DTO local para mantener la compatibilidad si el DTO global fue eliminado
        public class ClienteLinkDto
        {
            public string? ExistingUserId { get; set; }
            public string? Email { get; set; }
            public string? Password { get; set; }
            public string? Username { get; set; }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Administrador")]
        [HttpPost("{id}/link-user")]
        public async Task<IActionResult> LinkClienteToUser(int id, [FromBody] ClienteLinkDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { Codigo = 404, Mensaje = "Cliente no encontrado." });

            IdentityUser? user = null;

            // Si viene un id de usuario existente
            if (!string.IsNullOrWhiteSpace(dto.ExistingUserId))
            {
                user = await _userManager.FindByIdAsync(dto.ExistingUserId);
                if (user == null)
                    return NotFound(new { Codigo = 404, Mensaje = "Usuario Identity no encontrado." });
            }
            else
            {
                // Crear usuario nuevo
                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    return BadRequest(new
                    {
                        Codigo = 400,
                        Mensaje = "Email y Password requeridos para crear usuario."
                    });
                }

                var existing = await _userManager.FindByEmailAsync(dto.Email);
                if (existing != null)
                    return BadRequest(new { Codigo = 400, Mensaje = "El usuario ya existe." });

                user = new IdentityUser
                {
                    UserName = dto.Username ?? dto.Email,
                    Email = dto.Email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                    return BadRequest(createResult.Errors);

                // Asignar rol Cliente si no lo tiene
                if (!await _roleManager.RoleExistsAsync("Cliente"))
                    await _roleManager.CreateAsync(new IdentityRole("Cliente"));

                if (!await _userManager.IsInRoleAsync(user, "Cliente"))
                    await _userManager.AddToRoleAsync(user, "Cliente");
            }

            // Vincular
            cliente.IdentityUserId = user.Id;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Codigo = 200,
                Mensaje = "Cliente vinculado correctamente.",
                ClienteId = cliente.Id,
                IdentityUserId = user.Id
            });
        }
    }
}
