namespace AdminRazer.ViewModels
{
    public class DashboardViewModel
    {
        public int SalesCountToday { get; set; }
        public decimal SalesTotalToday { get; set; }
        public string AiInsightMessage { get; set; } = string.Empty;

        // Nuevas propiedades para el dashboard rediseñado
        public int ClientsCount { get; set; }
        public int ProductsCount { get; set; }
        public List<decimal> SalesHistory { get; set; } = new List<decimal>();
        public List<string> SalesDates { get; set; } = new List<string>();
    }
}
