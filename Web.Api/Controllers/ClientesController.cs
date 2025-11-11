using AdminRazer.Data;
using AdminRazer.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web.Api.DTOs;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Cliente")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ClientesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ✅ GET: api/Clientes/me — obtiene los datos del cliente autenticado
        [HttpGet("me")]
        public async Task<ActionResult<ClienteDto>> GetMiPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token no válido o sin usuario asociado.");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            return Ok(_mapper.Map<ClienteDto>(cliente));
        }

        // ✅ PUT: api/Clientes/me — actualiza los datos del cliente autenticado
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMiPerfil([FromBody] ClienteCreateDto clienteDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token no válido o sin usuario asociado.");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            _mapper.Map(clienteDto, cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ GET: api/Clientes — permite listar todos (si deseas mantenerlo libre)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            var clientes = await _context.Clientes.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ClienteDto>>(clientes));
        }

        // ✅ GET: api/Clientes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            return Ok(_mapper.Map<ClienteDto>(cliente));
        }

        // ✅ POST: api/Clientes — el cliente puede crear (si lo deseas)
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> PostCliente([FromBody] ClienteCreateDto clienteDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token no válido o sin usuario asociado.");

            var clienteExistente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            if (clienteExistente != null)
                return BadRequest("Ya existe un cliente asociado a este usuario.");

            var cliente = _mapper.Map<Cliente>(clienteDto);
            cliente.IdentityUserId = userId;

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<ClienteDto>(cliente);
            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, result);
        }

        // ✅ DELETE: api/Clientes/me — elimina su propio registro
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMiCuenta()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token no válido o sin usuario asociado.");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
