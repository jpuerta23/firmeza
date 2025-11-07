namespace Web.Api.DTOs
{
    public class VentaDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = null!;
        public string MetodoPago { get; set; } = null!;
        public decimal Total { get; set; }
        public List<DetalleVentaDto> Detalles { get; set; } = new();
    }

    public class VentaCreateDto
    {
        public int ClienteId { get; set; }
        public string MetodoPago { get; set; } = null!;
        public List<DetalleVentaCreateDto> Detalles { get; set; } = new();
    }
}