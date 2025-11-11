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
    public class VentasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public VentasController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ✅ GET: api/Ventas
        // El cliente autenticado puede ver todas las ventas (propias o todas si lo deseas)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);

            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            // Si solo quieres que vea sus propias ventas, descomenta esta línea:
            // var ventas = await _context.Ventas.Where(v => v.ClienteId == cliente.Id)
            //     .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            //     .ToListAsync();

            // Si puede ver todas las ventas:
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<VentaDto>>(ventas));
        }

        // ✅ GET: api/Ventas/{id}
        // El cliente autenticado puede ver su venta o cualquier venta
        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDto>> GetVenta(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);

            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            // Si quieres restringir que solo pueda ver sus ventas:
            // if (venta.ClienteId != cliente.Id)
            //     return Forbid("No puedes acceder a una venta que no es tuya.");

            return Ok(_mapper.Map<VentaDto>(venta));
        }

        // ✅ POST: api/Ventas
        // El cliente autenticado puede registrar nuevas ventas
        [HttpPost]
        public async Task<ActionResult<VentaDto>> PostVenta(VentaCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);

            if (cliente == null)
                return BadRequest("Cliente no válido o no vinculado con el usuario autenticado.");

            var venta = _mapper.Map<Venta>(dto);
            venta.ClienteId = cliente.Id;
            venta.RecalculateTotal();

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<VentaDto>(venta);
            return CreatedAtAction(nameof(GetVenta), new { id = venta.Id }, result);
        }

        // ✅ PUT: api/Ventas/{id}
        // El cliente autenticado puede modificar sus ventas
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenta(int id, [FromBody] VentaCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);

            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound();

            if (venta.ClienteId != cliente.Id)
                return Forbid("No puedes modificar una venta que no es tuya.");

            _mapper.Map(dto, venta);
            venta.RecalculateTotal();
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ DELETE: api/Ventas/{id}
        // El cliente autenticado puede eliminar sus ventas
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenta(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);

            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
                return NotFound();

            if (venta.ClienteId != cliente.Id)
                return Forbid("No puedes eliminar una venta que no es tuya.");

            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
