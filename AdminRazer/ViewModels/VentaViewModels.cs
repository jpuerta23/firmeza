// ...existing code...
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace AdminRazer.ViewModels
{
    public class DetalleVentaViewModel : IValidatableObject
    {
        [Required]
        public int ProductoId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductoNombre { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser >= 0")]
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Cantidad <= 0)
                yield return new ValidationResult("Cantidad debe ser mayor que cero", new[] { nameof(Cantidad) });

            if (PrecioUnitario < 0)
                yield return new ValidationResult("PrecioUnitario no puede ser negativo", new[] { nameof(PrecioUnitario) });

            if (Subtotal != Cantidad * PrecioUnitario)
                yield return new ValidationResult("Subtotal no coincide con Cantidad * PrecioUnitario", new[] { nameof(Subtotal) });
        }
    }

    public class VentaViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(200)]
        public string ClienteNombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string MetodoPago { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Total { get; set; }

        public List<DetalleVentaViewModel> Detalles { get; set; } = new List<DetalleVentaViewModel>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Fecha > DateTime.UtcNow.AddHours(1)) // permitir un pequeño desfase
                yield return new ValidationResult("Fecha no puede ser en el futuro", new[] { nameof(Fecha) });

            if (Detalles == null || Detalles.Count == 0)
                yield return new ValidationResult("La venta debe contener al menos un detalle", new[] { nameof(Detalles) });

            foreach (var (detalle, index) in Detalles.Select((d, i) => (d, i)))
            {
                var ctx = new ValidationContext(detalle);
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(detalle, ctx, results, true))
                {
                    foreach (var r in results)
                    {
                        var memberNames = r.MemberNames.Select(n => $"Detalles[{index}].{n}");
                        yield return new ValidationResult(r.ErrorMessage, memberNames);
                    }
                }
            }

            var expected = Detalles.Sum(d => d.Subtotal);
            if (Total != expected)
                yield return new ValidationResult("El Total no coincide con la suma de los subtotales", new[] { nameof(Total) });
        }
    }
}
