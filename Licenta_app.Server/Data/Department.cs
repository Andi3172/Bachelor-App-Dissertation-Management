using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Licenta_app.Server.Data
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? HeadOfDepartmentId { get; set; }

        [NotMapped]
        public Professor? HeadOfDepartment { get; set; }
    }
}
