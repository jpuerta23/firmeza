using AdminRazer.Data;
using AdminRazer.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.DTOs;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class DetallesVentaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DetallesVentaController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ✅ GET: api/DetallesVenta
        // Solo los administradores pueden ver todos los detalles
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<DetalleVentaDto>>> GetDetalles()
        {
            var detalles = await _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .ToListAsync();

            var result = _mapper.Map<IEnumerable<DetalleVentaDto>>(detalles);
            return Ok(result);
        }

        // ✅ GET: api/DetallesVenta/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Cliente")]
        public async Task<ActionResult<DetalleVentaDto>> GetDetalle(int id)
        {
            var detalle = await _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (detalle == null)
                return NotFound();

            // 🔸 Si es Cliente, solo puede ver sus propias ventas
            if (User.IsInRole("Cliente"))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (detalle.Venta.Cliente?.IdentityUserId != userId)
                    return Forbid("No tienes permiso para acceder a este detalle de venta.");
            }

            return Ok(_mapper.Map<DetalleVentaDto>(detalle));
        }

        // ✅ POST: api/DetallesVenta
        // Solo administradores pueden crear detalles de venta
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DetalleVentaDto>> PostDetalle([FromBody] DetalleVentaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var detalle = _mapper.Map<DetalleVenta>(dto);
            _context.DetallesVenta.Add(detalle);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<DetalleVentaDto>(detalle);
            return CreatedAtAction(nameof(GetDetalle), new { id = detalle.Id }, result);
        }

        // ✅ PUT: api/DetallesVenta/{id}
        // Solo administradores pueden actualizar
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutDetalle(int id, [FromBody] DetalleVentaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var detalle = await _context.DetallesVenta.FindAsync(id);
            if (detalle == null)
                return NotFound();

            _mapper.Map(dto, detalle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ DELETE: api/DetallesVenta/{id}
        // Solo administradores pueden eliminar
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDetalle(int id)
        {
            var detalle = await _context.DetallesVenta.FindAsync(id);
            if (detalle == null)
                return NotFound();

            _context.DetallesVenta.Remove(detalle);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
