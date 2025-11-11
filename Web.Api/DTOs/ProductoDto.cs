using System.ComponentModel.DataAnnotations;

namespace Web.Api.DTOs
{
    /// <summary>
    /// Representa un producto disponible en el sistema.
    /// </summary>
    public record ProductoDto
    {
        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Nombre del producto.
        /// </summary>
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre del producto no puede superar los 150 caracteres.")]
        public string Nombre { get; init; } = null!;

        /// <summary>
        /// Precio del producto.
        /// </summary>
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 9999999.99, ErrorMessage = "El precio debe ser mayor que 0.")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; init; }

        /// <summary>
        /// Stock disponible del producto.
        /// </summary>
        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; init; }
    }

    /// <summary>
    /// DTO utilizado para la creación de un nuevo producto.
    /// </summary>
    public record ProductoCreateDto
    {
        /// <summary>
        /// Nombre del producto.
        /// </summary>
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre del producto no puede superar los 150 caracteres.")]
        public string Nombre { get; init; } = null!;

        /// <summary>
        /// Categoría del producto.
        /// </summary>
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [StringLength(100, ErrorMessage = "La categoría no puede superar los 100 caracteres.")]
        public string Categoria { get; init; } = null!;

        /// <summary>
        /// Precio del producto.
        /// </summary>
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 9999999.99, ErrorMessage = "El precio debe ser mayor que 0.")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; init; }

        /// <summary>
        /// Stock disponible del producto.
        /// </summary>
        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; init; }
    }

    /// <summary>
    /// DTO utilizado para la actualización de un producto existente.
    /// </summary>
    public record ProductoUpdateDto
    {
        /// <summary>
        /// Identificador único del producto a actualizar.
        /// </summary>
        [Required(ErrorMessage = "El Id del producto es obligatorio.")]
        public int Id { get; init; }

        /// <summary>
        /// Nombre actualizado del producto.
        /// </summary>
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre del producto no puede superar los 150 caracteres.")]
        public string Nombre { get; init; } = null!;

        /// <summary>
        /// Categoría del producto.
        /// </summary>
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [StringLength(100, ErrorMessage = "La categoría no puede superar los 100 caracteres.")]
        public string Categoria { get; init; } = null!;

        /// <summary>
        /// Precio actualizado del producto.
        /// </summary>
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 9999999.99, ErrorMessage = "El precio debe ser mayor que 0.")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; init; }

        /// <summary>
        /// Stock disponible actualizado.
        /// </summary>
        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; init; }
    }
}
