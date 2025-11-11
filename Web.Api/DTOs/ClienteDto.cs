using System.ComponentModel.DataAnnotations;

namespace Web.Api.DTOs
{
    /// <summary>
    /// Representa un cliente en el sistema.
    /// </summary>
    public record ClienteDto
    {
        /// <summary>
        /// Identificador único del cliente.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Nombre completo del cliente.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Nombre { get; init; } = null!;

        /// <summary>
        /// Documento de identidad del cliente (DNI, RUC, etc.).
        /// </summary>
        [Required(ErrorMessage = "El documento es obligatorio.")]
        [StringLength(20, ErrorMessage = "El documento no puede superar los 20 caracteres.")]
        public string Documento { get; init; } = null!;

        /// <summary>
        /// Número telefónico del cliente.
        /// </summary>
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
        public string Telefono { get; init; } = null!;

        /// <summary>
        /// Correo electrónico del cliente.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede superar los 100 caracteres.")]
        public string Email { get; init; } = null!;
    }

    /// <summary>
    /// DTO para la creación de un nuevo cliente.
    /// </summary>
    public record ClienteCreateDto
    {
        /// <summary>
        /// Nombre completo del cliente.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Nombre { get; init; } = null!;

        /// <summary>
        /// Documento de identidad del cliente.
        /// </summary>
        [Required(ErrorMessage = "El documento es obligatorio.")]
        [StringLength(20, ErrorMessage = "El documento no puede superar los 20 caracteres.")]
        public string Documento { get; init; } = null!;

        /// <summary>
        /// Número telefónico del cliente.
        /// </summary>
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
        public string Telefono { get; init; } = null!;

        /// <summary>
        /// Correo electrónico del cliente.
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(100, ErrorMessage = "El correo electrónico no puede superar los 100 caracteres.")]
        public string Email { get; init; } = null!;
    }
}
