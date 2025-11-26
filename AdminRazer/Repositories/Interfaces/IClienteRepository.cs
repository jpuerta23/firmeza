using AdminRazer.Models;

namespace AdminRazer.Repositories.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> GetByEmailAsync(string email);
        Task<Cliente?> GetByDocumentoAsync(string documento);
        Task<Cliente?> GetByIdentityUserIdAsync(string identityUserId);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<bool> DocumentoExistsAsync(string documento, int? excludeId = null);
    }
}
