using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace AdminRazer.ViewModels
{
    public class ClienteCreateViewModel : IValidatableObject
    {
        [Required]
        [StringLength(200, ErrorMessage = "El nombre no puede tener más de 200 caracteres")]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El documento no puede tener más de 50 caracteres")]
        [RegularExpression("^[a-zA-Z0-9- ]*$", ErrorMessage = "El documento sólo puede contener letras, números, guiones y espacios")]
        public string Documento { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El teléfono no puede tener más de 50 caracteres")]
        [RegularExpression(@"^[0-9+()\- ]*$", ErrorMessage = "El teléfono sólo puede contener dígitos, espacios, paréntesis, + y -")]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "El email no puede tener más de 200 caracteres")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        // Nuevos campos para contraseña
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
                yield return new ValidationResult("Nombre es obligatorio", new[] { nameof(Nombre) });

            if (string.IsNullOrWhiteSpace(Documento) && string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult("Se requiere al menos Documento o Email", new[] { nameof(Documento), nameof(Email) });
            }

            // Validar que el email si existe, no tenga scripts (protección extra)
            if (!string.IsNullOrWhiteSpace(Email))
            {
                if (Regex.IsMatch(Email, "<\\/?script", RegexOptions.IgnoreCase))
                    yield return new ValidationResult("Email inválido", new[] { nameof(Email) });
            }

            // Validar teléfono complementario (ej. mínimo 6 dígitos si se proporciona)
            if (!string.IsNullOrWhiteSpace(Telefono))
            {
                var digitsOnly = Regex.Replace(Telefono, "[^0-9]", "");
                if (digitsOnly.Length < 6)
                    yield return new ValidationResult("El teléfono parece corto; verifique el número", new[] { nameof(Telefono) });
            }

            // Validación de contraseña:
            // - En el flujo de creación (este tipo) la contraseña es obligatoria.
            // - En edición la contraseña es opcional; si se proporciona debe cumplir longitud y coincidencia.
            var isCreate = this.GetType() == typeof(ClienteCreateViewModel);

            if (isCreate)
            {
                if (string.IsNullOrWhiteSpace(Password))
                    yield return new ValidationResult("La contraseña es obligatoria", new[] { nameof(Password) });
            }

            if (!string.IsNullOrWhiteSpace(Password))
            {
                if (Password.Length < 6)
                    yield return new ValidationResult("La contraseña debe tener al menos 6 caracteres", new[] { nameof(Password) });

                if (Password != ConfirmPassword)
                    yield return new ValidationResult("Las contraseñas no coinciden", new[] { nameof(Password), nameof(ConfirmPassword) });
            }
        }
    }

    // Clase de edición separada a nivel de espacio de nombres (las vistas la esperan como AdminRazer.ViewModels.ClienteEditViewModel)
    public class ClienteEditViewModel : ClienteCreateViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}
