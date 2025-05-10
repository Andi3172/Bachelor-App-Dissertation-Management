using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Licenta_app.Server.Data
{
    public class Student
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }
        public string? StudentNumber { get; set; }
        public string? Department { get; set; }
    }
}