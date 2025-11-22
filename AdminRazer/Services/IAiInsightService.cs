namespace AdminRazer.Services
{
    public interface IAiInsightService
    {
        Task<string> GetDailyInsightAsync(int salesCount, decimal totalRevenue);
    }
}
