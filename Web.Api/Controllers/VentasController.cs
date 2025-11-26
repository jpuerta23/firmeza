using AdminRazer.Models;
using AdminRazer.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Api.DTOs;
using Web.Api.Services;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class VentasController : ControllerBase
    {
        private readonly IVentaService _ventaService;
        private readonly IVentaRepository _ventaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;

        public VentasController(
            IVentaService ventaService,
            IVentaRepository ventaRepository,
            IClienteRepository clienteRepository,
            IMapper mapper)
        {
            _ventaService = ventaService;
            _ventaRepository = ventaRepository;
            _clienteRepository = clienteRepository;
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

            IEnumerable<Venta> ventas;

            if (role == "Cliente")
            {
                var cliente = await _clienteRepository.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
                if (cliente == null)
                    return NotFound("Cliente no encontrado.");

                ventas = await _ventaRepository.GetByClienteIdAsync(cliente.Id);
            }
            else
            {
                ventas = await _ventaRepository.GetAllWithClienteAsync();
            }

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
            var venta = await _ventaRepository.GetWithClienteAsync(id);

            if (venta == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Cliente")
            {
                var cliente = await _clienteRepository.FirstOrDefaultAsync(c => c.IdentityUserId == userId);

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
            var userEmail = User.FindFirstValue(ClaimTypes.Name); // Assuming Name is Email

            try
            {
                var venta = await _ventaService.CreateVentaAsync(dto, userId, userEmail);
                
                // Re-fetch to get details populated if CreateVentaAsync didn't populate navigation properties fully for mapping
                // But VentaService assigns navigation properties manually, so it should be fine.
                // However, to be safe and consistent with GetVenta, we might want to map the result directly.
                
                return CreatedAtAction(nameof(GetVenta), new { id = venta.Id },
                    _mapper.Map<VentaDto>(venta));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
            var venta = await _ventaRepository.GetWithClienteAsync(id);

            if (venta == null)
                return NotFound();

            _ventaRepository.Remove(venta);
            await _ventaRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}
