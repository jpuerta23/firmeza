using System.ComponentModel.DataAnnotations;

namespace Web.Api.DTOs
{
    /// <summary>
    /// Representa una venta realizada, incluyendo cliente, método de pago y detalles.
    /// </summary>
    public record VentaDto
    {
        /// <summary>
        /// Identificador único de la venta.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Fecha en la que se realizó la venta.
        /// </summary>
        [Required(ErrorMessage = "La fecha de la venta es obligatoria.")]
        public DateTime Fecha { get; init; }

        /// <summary>
        /// Identificador del cliente asociado a la venta.
        /// </summary>
        [Required(ErrorMessage = "El Id del cliente es obligatorio.")]
        public int ClienteId { get; init; }

        /// <summary>
        /// Nombre del cliente asociado a la venta.
        /// </summary>
        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre del cliente no puede superar los 150 caracteres.")]
        public string ClienteNombre { get; init; } = null!;

        /// <summary>
        /// Método de pago utilizado (efectivo, tarjeta, transferencia, etc.).
        /// </summary>
        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "El método de pago no puede superar los 50 caracteres.")]
        public string MetodoPago { get; init; } = null!;

        /// <summary>
        /// Total de la venta (suma de subtotales de los detalles).
        /// </summary>
        [Range(0.01, 99999999.99, ErrorMessage = "El total debe ser mayor que 0.")]
        [DataType(DataType.Currency)]
        public decimal Total { get; init; }

        /// <summary>
        /// Lista de detalles que componen la venta.
        /// </summary>
        [Required(ErrorMessage = "Debe incluir al menos un detalle de venta.")]
        public List<DetalleVentaDto> Detalles { get; init; } = new();
    }

    /// <summary>
    /// DTO utilizado para la creación de una nueva venta.
    /// </summary>
    public record VentaCreateDto
    {
        /// <summary>
        /// Identificador del cliente que realiza la compra.
        /// </summary>
        [Required(ErrorMessage = "El Id del cliente es obligatorio.")]
        public int ClienteId { get; init; }

        /// <summary>
        /// Método de pago utilizado.
        /// </summary>
        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "El método de pago no puede superar los 50 caracteres.")]
        public string MetodoPago { get; init; } = null!;

        /// <summary>
        /// Lista de detalles de productos incluidos en la venta.
        /// </summary>
        [Required(ErrorMessage = "Debe incluir al menos un detalle de venta.")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto en la venta.")]
        public List<DetalleVentaCreateDto> Detalles { get; init; } = new();
    }

    /// <summary>
    /// DTO utilizado para actualizar una venta existente.
    /// </summary>
    public record VentaUpdateDto
    {
        /// <summary>
        /// Identificador único de la venta a actualizar.
        /// </summary>
        [Required(ErrorMessage = "El Id de la venta es obligatorio.")]
        public int Id { get; init; }

        /// <summary>
        /// Identificador del cliente asociado.
        /// </summary>
        [Required(ErrorMessage = "El Id del cliente es obligatorio.")]
        public int ClienteId { get; init; }

        /// <summary>
        /// Método de pago actualizado.
        /// </summary>
        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "El método de pago no puede superar los 50 caracteres.")]
        public string MetodoPago { get; init; } = null!;

        /// <summary>
        /// Lista actualizada de detalles de venta.
        /// </summary>
        [Required(ErrorMessage = "Debe incluir al menos un detalle de venta.")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto en la venta.")]
        public List<DetalleVentaCreateDto> Detalles { get; init; } = new();
    }
}
