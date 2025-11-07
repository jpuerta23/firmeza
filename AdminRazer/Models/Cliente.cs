using System.ComponentModel.DataAnnotations;

namespace AdminRazer.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string Nombre { get; set; } = null!;

        [MaxLength(50)]
        public string Documento { get; set; } = null!;

        [MaxLength(50)]
        public string Telefono { get; set; } = null!;

        [MaxLength(200)]
        public string Email { get; set; } = null!;

        // Nuevo: campo para almacenar el Id del usuario de Identity asociado (nullable para registros anteriores)
        [MaxLength(450)]
        public string? IdentityUserId { get; set; }

        // Nuevo: almacenar el hash de la contraseña (NO almacenar contraseñas en texto plano)
        [MaxLength(500)]
        public string? PasswordHash { get; set; }

        // Constructor sin parámetros necesario para EF
        public Cliente() { }

        // Constructor opcional para comodidad al crear instancias
        public Cliente(string nombre, string documento, string telefono, string email)
        {
            Nombre = nombre;
            Documento = documento;
            Telefono = telefono;
            Email = email;
        }
    }
}