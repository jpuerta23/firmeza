using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace AdminRazer.ViewModels
{
    public class ProductoCreateViewModel : IValidatableObject
    {
        [Required]
        [StringLength(200, ErrorMessage = "El nombre no puede tener más de 200 caracteres")]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La categoría no puede tener más de 200 caracteres")]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string Categoria { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser >= 0")]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser >= 0")]
        public int Stock { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                yield return new ValidationResult("Nombre es obligatorio", new[] { nameof(Nombre) });
            }

            // Validar máximo de decimales en Precio (por ejemplo 2 decimales)
            var precioString = Precio.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (precioString.Contains('.') && precioString.Split('.')[1].Length > 2)
            {
                yield return new ValidationResult("El precio no puede tener más de 2 decimales", new[] { nameof(Precio) });
            }

            if (Stock < 0)
            {
                yield return new ValidationResult("Stock no puede ser negativo", new[] { nameof(Stock) });
            }
        }
    }

    public class ProductoEditViewModel : IValidatableObject
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "El nombre no puede tener más de 200 caracteres")]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La categoría no puede tener más de 200 caracteres")]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string Categoria { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser >= 0")]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser >= 0")]
        public int Stock { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Id <= 0)
            {
                yield return new ValidationResult("Id inválido", new[] { nameof(Id) });
            }

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                yield return new ValidationResult("Nombre es obligatorio", new[] { nameof(Nombre) });
            }

            var precioString = Precio.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (precioString.Contains('.') && precioString.Split('.')[1].Length > 2)
            {
                yield return new ValidationResult("El precio no puede tener más de 2 decimales", new[] { nameof(Precio) });
            }

            if (Stock < 0)
            {
                yield return new ValidationResult("Stock no puede ser negativo", new[] { nameof(Stock) });
            }
        }
    }
}
