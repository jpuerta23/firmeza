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
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class VentasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public VentasController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ============================================================
        // ADMIN → ve todas las ventas
        // CLIENTE → solo ve sus ventas
        // ============================================================

        [HttpGet]
        [Authorize(Roles = "Cliente,Administrador")]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            IQueryable<Venta> query = _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto);

            if (role == "Cliente")
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
                if (cliente == null)
                    return NotFound("Cliente no encontrado.");

                query = query.Where(v => v.ClienteId == cliente.Id);
            }

            var ventas = await query.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<VentaDto>>(ventas));
        }

        // ============================================================
        // ADMIN → puede ver cualquier venta
        // CLIENTE → solo su venta
        // ============================================================

        [HttpGet("{id}")]
        [Authorize(Roles = "Cliente,Administrador")]
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
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Cliente")
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);

                if (cliente == null)
                    return NotFound("Cliente no encontrado.");

                if (venta.ClienteId != cliente.Id)
                {
                    return StatusCode(403, new
                    {
                        Codigo = 403,
                        Mensaje = "Acceso denegado.",
                        Detalle = "No puedes acceder a una venta que no te pertenece."
                    });
                }
            }

            return Ok(_mapper.Map<VentaDto>(venta));
        }

        // ============================================================
        // CLIENTE → puede crear ventas
        // ADMIN → NO crea ventas
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<VentaDto>> PostVenta([FromBody] VentaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdentityUserId == userId);

            if (cliente == null)
                return BadRequest("Cliente no válido o no vinculado con usuario.");

            var venta = _mapper.Map<Venta>(dto);
            venta.ClienteId = cliente.Id;

            // Asignar precios reales desde producto
            foreach (var detalle in venta.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                if (producto == null)
                    return BadRequest($"Producto con id {detalle.ProductoId} no existe.");

                detalle.PrecioUnitario = producto.Precio;
                detalle.Venta = venta;
            }

            venta.RecalculateTotal();

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVenta), new { id = venta.Id },
                _mapper.Map<VentaDto>(venta));
        }

        // ============================================================
        // CLIENTE → NO puede editar su venta
        // ADMIN → NO edita ventas
        // ============================================================

        [HttpPut("{id}")]
        public IActionResult PutVenta(int id)
        {
            return StatusCode(405, new
            {
                Codigo = 405,
                Mensaje = "Método no permitido.",
                Detalle = "Las ventas no se pueden modificar."
            });
        }

        // ============================================================
        // ADMIN → puede eliminar cualquier venta
        // CLIENTE → NO puede eliminar ventas
        // ============================================================

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteVenta(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound();

            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
