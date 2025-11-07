using AdminRazer.Data;
using AdminRazer.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.DTOs;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetallesVentaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DetallesVentaController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/DetallesVenta
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetalleVentaDto>>> GetDetalles()
        {
            var detalles = await _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<DetalleVentaDto>>(detalles));
        }

        // GET: api/DetallesVenta/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleVentaDto>> GetDetalle(int id)
        {
            var detalle = await _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (detalle == null)
                return NotFound();

            return Ok(_mapper.Map<DetalleVentaDto>(detalle));
        }

        // POST: api/DetallesVenta
        [HttpPost]
        public async Task<ActionResult<DetalleVentaDto>> PostDetalle(DetalleVentaCreateDto dto)
        {
            var detalle = _mapper.Map<DetalleVenta>(dto);
            _context.DetallesVenta.Add(detalle);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<DetalleVentaDto>(detalle);
            return CreatedAtAction(nameof(GetDetalle), new { id = detalle.Id }, result);
        }

        // PUT: api/DetallesVenta/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetalle(int id, DetalleVentaCreateDto dto)
        {
            var detalle = await _context.DetallesVenta.FindAsync(id);
            if (detalle == null)
                return NotFound();

            _mapper.Map(dto, detalle);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/DetallesVenta/5
        [HttpDelete("{id}")]
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
