using System.Linq.Expressions;

namespace AdminRazer.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Read operations
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
        
        // Write operations
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        
        // Save changes
        Task<int> SaveChangesAsync();
    }
}
