using AdminRazer.Models;

namespace AdminRazer.Repositories.Interfaces
{
    public interface IProductoRepository : IRepository<Producto>
    {
        Task<IEnumerable<Producto>> GetByCategoriaAsync(string categoria);
        Task<IEnumerable<Producto>> GetByStockBelowAsync(int minStock);
        Task<bool> NombreExistsAsync(string nombre, int? excludeId = null);
    }
}
