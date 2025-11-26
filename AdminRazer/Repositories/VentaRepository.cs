using Microsoft.EntityFrameworkCore;
using AdminRazer.Data;
using AdminRazer.Models;
using AdminRazer.Repositories.Interfaces;

namespace AdminRazer.Repositories
{
    public class VentaRepository : Repository<Venta>, IVentaRepository
    {
        public VentaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Venta>> GetByClienteIdAsync(int clienteId)
        {
            return await _dbSet
                .Where(v => v.ClienteId == clienteId)
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetByFechaAsync(DateTime fecha)
        {
            var startOfDay = fecha.Date;
            var endOfDay = startOfDay.AddDays(1);
            
            return await _dbSet
                .Where(v => v.Fecha >= startOfDay && v.Fecha < endOfDay)
                .Include(v => v.Cliente)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetByFechaRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(v => v.Fecha >= startDate && v.Fecha <= endDate)
                .Include(v => v.Cliente)
                .ToListAsync();
        }

        public async Task<Venta?> GetWithClienteAsync(int id)
        {
            return await _dbSet
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Venta>> GetAllWithClienteAsync()
        {
            return await _dbSet
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalByFechaAsync(DateTime fecha)
        {
            var startOfDay = fecha.Date;
            var endOfDay = startOfDay.AddDays(1);
            
            var ventas = await _dbSet
                .Where(v => v.Fecha >= startOfDay && v.Fecha < endOfDay)
                .ToListAsync();
            
            return ventas.Sum(v => v.Total);
        }

        public async Task<Dictionary<DateTime, decimal>> GetSalesHistoryAsync(DateTime startDate)
        {
            var ventas = await _dbSet
                .Where(v => v.Fecha >= startDate)
                .ToListAsync();
            
            return ventas
                .GroupBy(v => v.Fecha.Date)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Total));
        }
    }
}
