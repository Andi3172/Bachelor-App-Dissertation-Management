using Licenta_app.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.TypeMapping;
using System.Security.Claims;

namespace Licenta_app.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationSessionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RegistrationSessionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // get all registration sessions
        // GET: api/registrationsessions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistrationSession>>> GetRegistrationSessions()
        {
            return await _context.RegistrationSessions
                .Include(rs => rs.Professor)
                .ToListAsync();
        }

        // get registration session by id
        // GET: api/registrationsessions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistrationSession>> GetRegistrationSessionById(int id)
        {
            var registrationSession = await _context.RegistrationSessions
                .Include(rs => rs.Professor)
                .FirstOrDefaultAsync(rs => rs.Id == id);
            if (registrationSession == null)
            {
                return NotFound();
            }
            return registrationSession;
        }

        // create new registration session
        // POST: api/registrationsessions
        [Authorize(Roles = "Professor")]
        [HttpPost]
        public async Task<ActionResult<RegistrationSession>> CreateSession(RegistrationSession session)
        {
            var professor = await _context.Professors.FindAsync(session.ProfessorId);
            if (professor == null)
            {
                return BadRequest("Invalid professor ID");
            }
            _context.RegistrationSessions.Add(session);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetRegistrationSessionById", new { id = session.Id }, session);
        }

        // update registration session
        // PUT: api/registrationsessions/5
        [Authorize(Roles = "Professor, Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSession(int id, [FromBody] RegistrationSession session)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User Id not found in token");
            }
            
            var prof = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == int.Parse(userIdClaim));
            if (prof == null || (prof.UserId != session.ProfessorId && !User.IsInRole("Admin")))
            {
                return Unauthorized("You are not authorized to modify this session.");
            }

            var existingSession = await _context.RegistrationSessions.FindAsync(id);
            if (existingSession == null)
            {
                return NotFound("Registration session not found");
            }
            var depart = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == prof.DepartmentId);
            if (depart == null)
            {
                return NotFound("Department not found");
            }

            existingSession.StartDate = session.StartDate;
            existingSession.EndDate = session.EndDate;

            //if the user is admin or the professor is head of department, allow changing max students
            if (User.IsInRole("Admin") || depart.HeadOfDepartmentId == prof.UserId)
            {
                existingSession.MaxStudents = session.MaxStudents;
            }


            _context.Entry(existingSession).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!_context.RegistrationSessions.Any(rs => rs.Id == id))
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

        // delete registration session
        // DELETE: api/registrationsessions/5
        [Authorize(Roles = "Professor, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var session = await _context.RegistrationSessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            _context.RegistrationSessions.Remove(session);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //get all registration sessions for a professor
        //see Professor Controller for this

        // get all active registration sessions
        // GET: api/registrationsessions/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<RegistrationSession>>> GetActiveRegistrationSessions()
        {
            var now = DateTime.UtcNow;
            var activeSessions = await _context.RegistrationSessions
                .Where(rs => rs.StartDate <= now && rs.EndDate >= now)
                .Include(rs => rs.Professor).ThenInclude(p=> p.User)
                .ToListAsync();

            return activeSessions;
        }



    }


}
