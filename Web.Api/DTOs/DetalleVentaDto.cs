using System.ComponentModel.DataAnnotations;

namespace Web.Api.DTOs
{
    /// <summary>
    /// Representa el detalle de una venta, incluyendo producto, cantidad y precios.
    /// </summary>
    public record DetalleVentaDto
    {
        /// <summary>
        /// Identificador único del detalle de venta.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Identificador del producto vendido.
        /// </summary>
        [Required(ErrorMessage = "El campo ProductoId es obligatorio.")]
        public int ProductoId { get; init; }

        /// <summary>
        /// Nombre del producto vendido.
        /// </summary>
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre del producto no puede superar los 200 caracteres.")]
        public string ProductoNombre { get; init; } = null!;

        /// <summary>
        /// Cantidad de unidades vendidas.
        /// </summary>
        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 1.")]
        public int Cantidad { get; init; }

        /// <summary>
        /// Precio unitario del producto.
        /// </summary>
        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Range(0.01, 9999999.99, ErrorMessage = "El precio unitario debe ser mayor que 0.")]
        [DataType(DataType.Currency)]
        public decimal PrecioUnitario { get; init; }

        /// <summary>
        /// Subtotal calculado del detalle (Cantidad * PrecioUnitario).
        /// </summary>
        [Range(0.01, 99999999.99, ErrorMessage = "El subtotal debe ser mayor que 0.")]
        [DataType(DataType.Currency)]
        public decimal Subtotal { get; init; }
    }

    /// <summary>
    /// DTO utilizado para crear un nuevo detalle de venta.
    /// </summary>
    public record DetalleVentaCreateDto
    {
        /// <summary>
        /// Identificador del producto vendido.
        /// </summary>
        [Required(ErrorMessage = "El campo ProductoId es obligatorio.")]
        public int ProductoId { get; init; }

        /// <summary>
        /// Cantidad de unidades vendidas.
        /// </summary>
        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 1.")]
        public int Cantidad { get; init; }

        /// <summary>
        /// Precio unitario del producto.
        /// </summary>
        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Range(0.01, 9999999.99, ErrorMessage = "El precio unitario debe ser mayor que 0.")]
        [DataType(DataType.Currency)]
        public decimal PrecioUnitario { get; init; }
    }
}
