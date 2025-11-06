#pragma warning disable CS8019 // Using directive is not required
#pragma warning disable IDE0005 // Remove unnecessary usings


using System.ComponentModel.DataAnnotations;


namespace AdminRazer.Models
{
    public class Venta : IValidatableObject
    {
        // Constructor parameterless requerido por EF
        public Venta()
        {
            Fecha = DateTime.UtcNow;
            MetodoPago = string.Empty;
            Detalles = new List<DetalleVenta>();
        }

        // Constructor opcional para conveniencia (acepta null)
        public Venta(string? metodoPago) : this()
        {
            MetodoPago = metodoPago ?? string.Empty;
        }

        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public Cliente Cliente { get; set; } = null!;

        // Detalles con validación en cada DetalleVenta
        public List<DetalleVenta> Detalles { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Total { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string MetodoPago { get; set; }

        // Método helper para recalcular el total a partir de los detalles
        public void RecalculateTotal()
        {
            if (Detalles.Count == 0)
            {
                Total = 0m;
                return;
            }

            Total = Detalles.Sum(d => d.Subtotal);
        }

        // Validaciones de modelo adicionales
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(MetodoPago))
            {
                yield return new ValidationResult("MetodoPago es obligatorio", new[] { nameof(MetodoPago) });
            }

            if (Detalles.Count == 0)
            {
                yield return new ValidationResult("La venta debe contener al menos un detalle", new[] { nameof(Detalles) });
                yield break;
            }

            for (int i = 0; i < Detalles.Count; i++)
            {
                var detalle = Detalles[i];

                var ctx = new ValidationContext(detalle);
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(detalle, ctx, results, validateAllProperties: true))
                {
                    foreach (var r in results)
                    {
                        var memberNames = r.MemberNames.Select(n => $"Detalles[{i}].{n}").ToArray();
                        yield return new ValidationResult($"Detalle[{i}]: {r.ErrorMessage}", memberNames);
                    }
                }
            }

            decimal expected = Detalles.Sum(d => d.Subtotal);
            if (Total != expected)
            {
                yield return new ValidationResult("El Total no coincide con la suma de los subtotales", new[] { nameof(Total) });
            }
        }
    }
}

#pragma warning restore IDE0005
#pragma warning restore CS8019
