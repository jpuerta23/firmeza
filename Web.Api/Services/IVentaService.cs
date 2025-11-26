using AdminRazer.Models;
using Web.Api.DTOs;

namespace Web.Api.Services
{
    public interface IVentaService
    {
        Task<Venta> CreateVentaAsync(VentaCreateDto dto, string userId, string userEmail);
    }
}
