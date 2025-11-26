namespace AdminRazer.Services
{
    public interface IAiInsightService
    {
        Task<string> GetDailyInsightAsync(int salesCount, decimal totalRevenue);
        Task<string> ChatAsync(string question);
    }
}
