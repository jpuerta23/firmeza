using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AdminRazer.Services
{
    public interface IExcelImportService
    {
        Task<(int count, string message)> ImportarProductosAsync(Stream fileStream);
    }
}
