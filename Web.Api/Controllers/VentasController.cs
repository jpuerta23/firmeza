using AdminRazer.Data;
using AdminRazer.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web.Api.DTOs;
using Web.Api.Services;
using System.Text;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class VentasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public VentasController(ApplicationDbContext context, IMapper mapper, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
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

            // Asignar precios reales desde producto y descontar stock
            foreach (var detalle in venta.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                if (producto == null)
                    return BadRequest($"Producto con id {detalle.ProductoId} no existe.");

                if (producto.Stock < detalle.Cantidad)
                    return BadRequest($"No hay suficiente stock para el producto {producto.Nombre}. Stock actual: {producto.Stock}");

                producto.Stock -= detalle.Cantidad; // Descontar stock
                detalle.PrecioUnitario = producto.Precio;
                detalle.Producto = producto; // Asignar navegación para el DTO de respuesta
                detalle.Venta = venta;
            }

            venta.RecalculateTotal();

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            // Enviar correo de confirmación
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"<h1>¡Gracias por tu compra, {cliente.Nombre}!</h1>");
                sb.AppendLine($"<p>Tu pedido #{venta.Id} ha sido confirmado.</p>");
                sb.AppendLine("<h3>Detalles de la compra:</h3>");
                sb.AppendLine("<ul>");
                foreach (var d in venta.Detalles)
                {
                    sb.AppendLine($"<li>{d.Producto.Nombre} x {d.Cantidad} - {d.Subtotal:C}</li>");
                }
                sb.AppendLine("</ul>");
                sb.AppendLine($"<h3>Total: {venta.Total:C}</h3>");
                sb.AppendLine("<p>Esperamos verte pronto.</p>");

                // Usar el email del usuario autenticado (IdentityUser) o del cliente si tuviera campo email
                var userEmail = User.FindFirstValue(ClaimTypes.Name); // Asumiendo que el Name es el email o está disponible
                if (!string.IsNullOrEmpty(userEmail))
                {
                    await _emailService.SendEmailAsync(userEmail, $"Confirmación de Compra #{venta.Id}", sb.ToString());
                }
            }
            catch (Exception ex)
            {
                // Loguear error pero no detener la respuesta de éxito de la venta
                Console.WriteLine($"Error enviando correo: {ex.Message}");
            }

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
