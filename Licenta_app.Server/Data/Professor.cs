using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Licenta_app.Server.Data
{
    public class Professor
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}