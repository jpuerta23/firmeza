using System.ComponentModel.DataAnnotations;

namespace Web.Api.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Nombre { get; set; } = null!;

        [Required]
        public string Documento { get; set; } = null!;

        [Required]
        public string Telefono { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;
    }
}
