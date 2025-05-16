using Licenta_app.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Licenta_app.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // get all departments
        // GET: api/departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            return await _context.Departments
                .Include(d => d.HeadOfDepartment)
                .ThenInclude(h => h.User)
                .ToListAsync();
        }

        // get department by id
        // GET: api/departments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            var department = await _context.Departments
                .Include(d => d.HeadOfDepartment)
                .ThenInclude(h => h.User)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }
            return department;
        }

        // create new department
        // POST: api/departments
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Department>> CreateDepartment(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDepartment", new { id = department.DepartmentId }, department);
        }

        // update department
        // PUT: api/departments/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] Department department)
        {
            var existingDepartment = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (existingDepartment == null)
            {
                return NotFound("Department not found");
            }

            existingDepartment.DepartmentName = department.DepartmentName;

            if (department.HeadOfDepartmentId != null)
            {
                var headOfDepartment = await _context.Professors
                    .Include(p => p.User) // Include the User details for frontend display
                    .FirstOrDefaultAsync(p => p.UserId == department.HeadOfDepartmentId);

                if (headOfDepartment == null)
                {
                    return BadRequest("Invalid HeadOfDepartmentId. No professor found with the given ID.");
                }

                existingDepartment.HeadOfDepartmentId = department.HeadOfDepartmentId;
                existingDepartment.HeadOfDepartment = headOfDepartment;
            }
            else
            {
                // If no HeadOfDepartmentId is provided, clear the existing head
                existingDepartment.HeadOfDepartmentId = null;
                existingDepartment.HeadOfDepartment = null;
            }

            _context.Entry(existingDepartment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "An error occurred while saving changes.");
            }
            // After SaveChangesAsync in UpdateDepartment and CreateDepartment
            var updatedDepartment = await _context.Departments
                .Include(d => d.HeadOfDepartment)
                .ThenInclude(h => h.User)
                .FirstOrDefaultAsync(d => d.DepartmentId == existingDepartment.DepartmentId);

            return Ok(updatedDepartment);


           
        }


        //delete department
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var depart = await _context.Departments.FindAsync(id);
            if (depart == null)
            {
                return NotFound("Department not found");
            }

            var hasProfessors = await _context.Professors.AnyAsync(p => p.DepartmentId == id);
            var hasStudents = await _context.Students.AnyAsync(s => s.Department == depart.DepartmentName);

            if (hasProfessors || hasStudents)
            {
                return BadRequest("Cannot delete department. It has associated professors or students.");
            }

            _context.Departments.Remove(depart);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the department.");
            }

            return Ok(depart);
        }


        // get all professors in a department
        //see Professor Controller for this

        // get all students in a department
        //see Student Controller for this


    }
}
