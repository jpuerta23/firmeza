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
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Cliente")]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProductosController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ✅ GET: api/Productos
        // Todos los clientes autenticados pueden ver los productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
        {
            var productos = await _context.Productos.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ProductoDto>>(productos));
        }

        // ✅ GET: api/Productos/{id}
        // El cliente autenticado puede ver un producto específico
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            return Ok(_mapper.Map<ProductoDto>(producto));
        }

        // ✅ POST: api/Productos
        // El cliente autenticado puede crear productos
        [HttpPost]
        public async Task<ActionResult<ProductoDto>> PostProducto([FromBody] ProductoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var producto = _mapper.Map<Producto>(dto);
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<ProductoDto>(producto);
            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, result);
        }

        // ✅ PUT: api/Productos/{id}
        // El cliente autenticado puede actualizar productos
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, [FromBody] ProductoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            _mapper.Map(dto, producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ DELETE: api/Productos/{id}
        // El cliente autenticado puede eliminar productos
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
