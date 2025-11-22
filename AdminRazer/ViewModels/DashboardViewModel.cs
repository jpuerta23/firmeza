namespace AdminRazer.ViewModels
{
    public class DashboardViewModel
    {
        public int SalesCountToday { get; set; }
        public decimal SalesTotalToday { get; set; }
        public string AiInsightMessage { get; set; } = string.Empty;
    }
}
