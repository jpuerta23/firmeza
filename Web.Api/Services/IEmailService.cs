using System.Threading.Tasks;

namespace Web.Api.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
