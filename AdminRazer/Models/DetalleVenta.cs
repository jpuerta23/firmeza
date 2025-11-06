#pragma warning disable CS8019 // Using directive is not required

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdminRazer.Models
{
    public class DetalleVenta : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        public int VentaId { get; set; }

        [Required]
        public Venta Venta { get; set; } = null!;

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public Producto Producto { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "El precio unitario debe ser >= 0")]
        public decimal PrecioUnitario { get; set; }

        // Propiedad calculada que será usada por Venta.RecalculateTotal
        public decimal Subtotal => Cantidad * PrecioUnitario;

        // Implementación de la validación de la interfaz
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Cantidad <= 0)
            {
                yield return new ValidationResult("Cantidad debe ser mayor que cero", new[] { nameof(Cantidad) });
            }

            if (PrecioUnitario < 0)
            {
                yield return new ValidationResult("PrecioUnitario no puede ser negativo", new[] { nameof(PrecioUnitario) });
            }
        }
    }
}

#pragma warning restore CS8019
