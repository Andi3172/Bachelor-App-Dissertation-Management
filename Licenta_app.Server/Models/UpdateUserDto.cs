namespace Licenta_app.Server.Models
{
    public class UpdateUserDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public string? NewPassword { get; set; }

        public string? Department { get; set; }
    }
}
