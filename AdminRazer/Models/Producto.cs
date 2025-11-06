namespace AdminRazer.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Categoria { get; set; } = null!;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }
}