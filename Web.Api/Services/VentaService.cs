using AdminRazer.Models;
using AdminRazer.Repositories.Interfaces;
using AutoMapper;
using System.Text;
using Web.Api.DTOs;

namespace Web.Api.Services
{
    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public VentaService(
            IVentaRepository ventaRepository,
            IProductoRepository productoRepository,
            IClienteRepository clienteRepository,
            IMapper mapper,
            IEmailService emailService)
        {
            _ventaRepository = ventaRepository;
            _productoRepository = productoRepository;
            _clienteRepository = clienteRepository;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<Venta> CreateVentaAsync(VentaCreateDto dto, string userId, string userEmail)
        {
            var cliente = await _clienteRepository.FirstOrDefaultAsync(c => c.IdentityUserId == userId);

            if (cliente == null)
                throw new Exception("Cliente no válido o no vinculado con usuario.");

            var venta = _mapper.Map<Venta>(dto);
            venta.ClienteId = cliente.Id;

            // Asignar precios reales desde producto y descontar stock
            foreach (var detalle in venta.Detalles)
            {
                var producto = await _productoRepository.GetByIdAsync(detalle.ProductoId);
                if (producto == null)
                    throw new Exception($"Producto con id {detalle.ProductoId} no existe.");

                if (producto.Stock < detalle.Cantidad)
                    throw new Exception($"No hay suficiente stock para el producto {producto.Nombre}. Stock actual: {producto.Stock}");

                producto.Stock -= detalle.Cantidad; // Descontar stock
                _productoRepository.Update(producto); // Marcar para actualización

                detalle.PrecioUnitario = producto.Precio;
                detalle.Producto = producto; // Asignar navegación para el DTO de respuesta
                detalle.Venta = venta;
            }

            venta.RecalculateTotal();

            await _ventaRepository.AddAsync(venta);
            await _ventaRepository.SaveChangesAsync();

            // Enviar correo de confirmación
            await SendConfirmationEmail(cliente, venta, userEmail);

            return venta;
        }

        private async Task SendConfirmationEmail(Cliente cliente, Venta venta, string userEmail)
        {
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
        }
    }
}
