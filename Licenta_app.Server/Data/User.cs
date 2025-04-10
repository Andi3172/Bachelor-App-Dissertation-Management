using System.ComponentModel.DataAnnotations;

namespace Licenta_app.Server.Data
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        //for navigation
        public Student? Student { get; set; }
        public Professor? Professor { get; set; }
    }
}