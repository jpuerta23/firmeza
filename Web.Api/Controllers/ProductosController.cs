using AdminRazer.Models;
using AdminRazer.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.DTOs;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;

        public ProductosController(IProductoRepository productoRepository, IMapper mapper)
        {
            _productoRepository = productoRepository;
            _mapper = mapper;
        }

        // CLIENTE Y ADMIN → PUEDEN VER PRODUCTOS
       

        // GET: api/Productos
        [HttpGet]
        [Authorize(Roles = "Cliente,Administrador")]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
        {
            var productos = await _productoRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<ProductoDto>>(productos));
        }

        // GET: api/Productos/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Cliente,Administrador")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await _productoRepository.GetByIdAsync(id);
            if (producto == null)
                return NotFound();

            return Ok(_mapper.Map<ProductoDto>(producto));
        }

      
        // SOLO ADMINISTRADOR → CRUD COMPLETO
        
        // POST: api/Productos
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<ProductoDto>> PostProducto([FromBody] ProductoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var producto = _mapper.Map<Producto>(dto);
            await _productoRepository.AddAsync(producto);
            await _productoRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id },
                _mapper.Map<ProductoDto>(producto));
        }

        // PUT: api/Productos/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> PutProducto(int id, [FromBody] ProductoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var producto = await _productoRepository.GetByIdAsync(id);
            if (producto == null)
                return NotFound();

            _mapper.Map(dto, producto);
            _productoRepository.Update(producto);
            await _productoRepository.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Productos/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _productoRepository.GetByIdAsync(id);
            if (producto == null)
                return NotFound();

            _productoRepository.Remove(producto);
            await _productoRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}
