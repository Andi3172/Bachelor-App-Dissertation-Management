using Licenta_app.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Licenta_app.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // get all students
        // GET: api/students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students
                .ToListAsync();
        }

        // get student by id
        // GET: api/students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == id);
            if (student == null)
            {
                return NotFound();
            }
            return student;
        }

        //get student by department
        // GET: api/students/by-department/departmentName
        [HttpGet("by-department/{departmentName}")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudentsByDepartment(string departmentName)
        {
            var students = await _context.Students
                .Where(s => s.Department == departmentName)
                .ToListAsync();

            if (students == null || !students.Any())
            {
                return NotFound("No students found in department " + departmentName);
            }

            return students;
        }

        //get student by student number
        // GET: api/students/by-student-number/123456
        [HttpGet("by-student-number/{studentNumber}")]
        public async Task<ActionResult<Student>> GetStudentByStudentNumber(string studentNumber)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
            if (student == null)
            {
                return NotFound("Student not found");
            }
            return student;
        }

        // create new student
        // POST: api/students
        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent(Student student)
        {
            var user = await _context.Users.FindAsync(student.UserId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            if (_context.Students.Any(s => s.UserId == student.UserId))
            {
                return BadRequest("Student already exists");
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudent), new { id = student.UserId }, student);
        }

        // update existing student
        // PUT: api/students/5
        [Authorize(Roles = "Student, Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, Student student)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("user ID not found in token");
            }

            if (userIdClaim != id.ToString())
            {
                return Unauthorized("You are not authorized to modify this student.");
            }

            var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.UserId == id);
            if (existingStudent == null)
            {
                return NotFound("Student not found");
            }

            existingStudent.Department = student.Department;
            existingStudent.StudentNumber = student.StudentNumber;
            

            _context.Entry(existingStudent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!_context.Students.Any(s => s.UserId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        //admin specific update student
        // PUT: api/students/admin-update/5
        [Authorize(Roles= "Admin")]
        [HttpPut("admin-update/{id}")]
        public async Task<ActionResult<Student>> AdminUpdateStuent(int id, [FromBody] Student student)
        {
            var stud = await _context.Students.FirstOrDefaultAsync(s => s.UserId == id);
            if (stud == null)
            {
                return NotFound("Student not found");
            }
            stud.Department = student.Department;
            stud.StudentNumber = student.StudentNumber;

            _context.Entry(stud).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!_context.Students.Any(s => s.UserId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }


        // delete student
        // DELETE: api/students/5
        [Authorize(Roles = "Student")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent([FromQuery] int id, [FromBody] Student student)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }
            if (userIdClaim != id.ToString())
            {
                return Unauthorized("You are not authorized to delete this student.");
            }

            var studToDelete = await _context.Students.FirstOrDefaultAsync(s => s.UserId == id);
            if (studToDelete == null)
            {
                return NotFound("Student not found");
            }

            _context.Students.Remove(studToDelete);

            var userToDelete = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (userToDelete != null)
            {
                _context.Users.Remove(userToDelete);
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the student.");
            }
            return NoContent();
        }
        //delete all students in a department
        // DELETE: api/students/by-department/departmentName
        [HttpDelete("by-department/{departmentName}")]
        public async Task<IActionResult> DeleteStudentsByDepartment(string departmentName)
        {
            var students = await _context.Students
                .Where(s => s.Department == departmentName)
                .ToListAsync();
            if (students == null || !students.Any())
            {
                return NotFound("No students found in department " + departmentName);
            }
            _context.Students.RemoveRange(students);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
