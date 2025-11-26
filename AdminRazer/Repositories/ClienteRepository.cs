using Microsoft.EntityFrameworkCore;
using AdminRazer.Data;
using AdminRazer.Models;
using AdminRazer.Repositories.Interfaces;

namespace AdminRazer.Repositories
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cliente?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Cliente?> GetByDocumentoAsync(string documento)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Documento == documento);
        }

        public async Task<Cliente?> GetByIdentityUserIdAsync(string identityUserId)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _dbSet.AnyAsync(c => c.Email == email && c.Id != excludeId.Value);
            }
            return await _dbSet.AnyAsync(c => c.Email == email);
        }

        public async Task<bool> DocumentoExistsAsync(string documento, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _dbSet.AnyAsync(c => c.Documento == documento && c.Id != excludeId.Value);
            }
            return await _dbSet.AnyAsync(c => c.Documento == documento);
        }
    }
}
