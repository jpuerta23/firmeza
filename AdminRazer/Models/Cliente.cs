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