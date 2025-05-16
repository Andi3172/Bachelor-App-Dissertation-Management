using Microsoft.AspNetCore.Mvc;
using Licenta_app.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;


namespace Licenta_app.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfessorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // get all professors
        // GET: api/professors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Professor>>> GetProfessors()
        {
            return await _context.Professors
                .ToListAsync();
        }

        // get professor by id
        // GET: api/professors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Professor>> GetProfessor(int id)
        {
            var professor = await _context.Professors
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (professor == null)
            {
                return NotFound();
            }

            return professor;
        }

        // get professors by department name
        // GET: api/professors/by-department/departmentName
        [HttpGet("by-department/{departmentName}")]
        public async Task<ActionResult<IEnumerable<Professor>>> GetProfessorsByDepartment(string departmentName)
        {
            var professors = await _context.Professors
                .Where(p => p.Department.DepartmentName == departmentName)
                .Include(p => p.Department)
                .ToListAsync();
            if (professors == null || !professors.Any())
            {
                return NotFound("No professors found in department " + departmentName);
            }
            return professors;
        }

        /*
        
        MAYBE get professors by department id, but it's not really necessary
         
        */

        // create new professor
        // POST: api/professors
        [HttpPost]
        public async Task<ActionResult<Professor>> CreateProfessor(Professor professor)
        {
            var user = await _context.Users.FindAsync(professor.UserId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            _context.Professors.Add(professor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProfessor), new { id = professor.UserId }, professor);
        }

        // self update existing professor
        // PUT: api/professors/{5}
        [Authorize(Roles = "Professor, Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfessor(int id, Professor professor)
        {
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }

            if (userIdClaim != id.ToString())
            {
                return Forbid("You are not authorized to modify this professor's data.");
            }

            var prof = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == id);
            if (prof == null)
            {
                return NotFound("Professor not found");
            }
            prof.DepartmentId = professor.DepartmentId;

            _context.Entry(prof).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!_context.Professors.Any(p => p.UserId == prof.UserId))
                {
                    return NotFound("Professor not found during save.");
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        //admin specific endpoint for editing other Professors
        // PUT: api/professors/admin-update/5
        [Authorize(Roles = "Admin")]
        [HttpPut("admin-update/{id}")]
        public async Task<ActionResult<Professor>> AdminUpdateProfessor(int id, [FromBody] Professor professor)
        {
            var prof = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == id);
            if (prof == null)
            {
                return NotFound("Professor not found");
            }

            prof.DepartmentId = professor.DepartmentId;

            _context.Entry(prof).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while saving the professor.");
            }

            return NoContent();
        }

        // delete professor
        // DELETE: api/professors/5
        [Authorize(Roles = "Professor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfessor([FromQuery] int id, [FromBody] Professor professor)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }

            if (userIdClaim != id.ToString())
            {
                return Forbid("You are not authorized to delete this professor's data.");
            }

            var prof = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == id);
            if (prof == null)
            {
                return NotFound("Professor not found");
            }

            _context.Professors.Remove(prof);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the professor.");
            }

            return NoContent();
        }

        // get professor's department
        // GET: api/professors/5/department
        [HttpGet("{id}/department")]
        public async Task<ActionResult<Department>> GetProfessorDepartment(int id)
        {
            var professor = await _context.Professors
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.UserId == id);
            if (professor == null)
            {
                return NotFound("Professor not found");
            }
            return professor.Department;
        }

        // get a professor's registration sessions
        // GET: api/professors/{id}/registration-sessions
        [HttpGet("{id}/registration-sessions")]
        public async Task<ActionResult<IEnumerable<RegistrationSession>>> GetProfessorRegistrationSessions(int id)
        {
            var sessions = await _context.RegistrationSessions
                .Where(rs => rs.ProfessorId == id)
                .ToListAsync();

            if (sessions == null || !sessions.Any())
            {
                return NotFound("No registration sessions found for professor with id " + id);
            }

            return sessions;
        }

    }
}
