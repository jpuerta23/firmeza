using AdminRazer.Models;

namespace AdminRazer.Repositories.Interfaces
{
    public interface IVentaRepository : IRepository<Venta>
    {
        Task<IEnumerable<Venta>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<Venta>> GetByFechaAsync(DateTime fecha);
        Task<IEnumerable<Venta>> GetByFechaRangeAsync(DateTime startDate, DateTime endDate);
        Task<Venta?> GetWithClienteAsync(int id);
        Task<IEnumerable<Venta>> GetAllWithClienteAsync();
        Task<decimal> GetTotalByFechaAsync(DateTime fecha);
        Task<Dictionary<DateTime, decimal>> GetSalesHistoryAsync(DateTime startDate);
    }
}
