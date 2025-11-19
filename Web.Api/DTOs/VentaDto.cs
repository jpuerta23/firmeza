using System.ComponentModel.DataAnnotations;

namespace Web.Api.DTOs
{
    /// <summary>
    /// Representa una venta con información del cliente, método de pago y detalles.
    /// </summary>
    public record VentaDto
    {
        public int Id { get; init; }

        [Required(ErrorMessage = "La fecha de la venta es obligatoria.")]
        public DateTime Fecha { get; init; }

        [Required(ErrorMessage = "El Id del cliente es obligatorio.")]
        public int ClienteId { get; init; }

        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre del cliente no puede superar los 150 caracteres.")]
        public string ClienteNombre { get; init; } = null!;

        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "El método de pago no puede superar los 50 caracteres.")]
        public string MetodoPago { get; init; } = null!;

        [Range(0.01, 99999999.99, ErrorMessage = "El total debe ser mayor que 0.")]
        [DataType(DataType.Currency)]
        public decimal Total { get; init; }

        [Required(ErrorMessage = "Debe incluir al menos un detalle de venta.")]
        public List<DetalleVentaDto> Detalles { get; init; } = new();
    }

    /// <summary>
    /// DTO utilizado para crear una nueva venta.
    /// </summary>
    public record VentaCreateDto
    {
        [Required(ErrorMessage = "El Id del cliente es obligatorio.")]
        public int ClienteId { get; init; }

        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "El método de pago no puede superar los 50 caracteres.")]
        public string MetodoPago { get; init; } = null!;

        [Required(ErrorMessage = "Debe incluir al menos un detalle de venta.")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto en la venta.")]
        public List<DetalleVentaCreateSimpleDto> Detalles { get; init; } = new();
    }

    /// <summary>
    /// DTO utilizado para actualizar una venta existente.
    /// </summary>


    /// <summary>
    /// Representa un detalle dentro de una venta (respuesta al cliente).
    /// </summary>
    public record DetalleVentaDto
    {
        public int ProductoId { get; init; }

        public string ProductoNombre { get; init; } = null!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0.")]
        public int Cantidad { get; init; }

        [Required]
        [Range(0.01, 99999999.99, ErrorMessage = "El precio unitario debe ser mayor que 0.")]
        public decimal PrecioUnitario { get; init; }

        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    /// <summary>
    /// DTO mínimo usado para crear un detalle de venta.
    /// </summary>
    public record DetalleVentaCreateSimpleDto
    {
        [Required(ErrorMessage = "El Id del producto es obligatorio.")]
        public int ProductoId { get; init; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0.")]
        public int Cantidad { get; init; }
    }
}
