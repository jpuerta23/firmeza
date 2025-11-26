using Microsoft.EntityFrameworkCore;
using AdminRazer.Data;
using AdminRazer.Models;
using AdminRazer.Repositories.Interfaces;

namespace AdminRazer.Repositories
{
    public class ProductoRepository : Repository<Producto>, IProductoRepository
    {
        public ProductoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Producto>> GetByCategoriaAsync(string categoria)
        {
            return await _dbSet.Where(p => p.Categoria == categoria).ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetByStockBelowAsync(int minStock)
        {
            return await _dbSet.Where(p => p.Stock < minStock).ToListAsync();
        }

        public async Task<bool> NombreExistsAsync(string nombre, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _dbSet.AnyAsync(p => p.Nombre == nombre && p.Id != excludeId.Value);
            }
            return await _dbSet.AnyAsync(p => p.Nombre == nombre);
        }
    }
}
