namespace Web.Api.DTOs
{
    public class ClienteLinkDto
    {
        // Si ya existe un IdentityUser y quieres vincularlo por id
        public string? ExistingUserId { get; set; }

        // Si quieres crear un nuevo IdentityUser, proporciona email y password
        public string? Email { get; set; }
        public string? Password { get; set; }

        // Opcional username si quieres que sea distinto del email
        public string? Username { get; set; }
    }
}

