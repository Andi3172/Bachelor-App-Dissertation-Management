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
        public async Task<ActionResult<IEnumerable<RegistrationSession>>> GetRegistrationSessions(int page = 1, int pageSize = 10)
        {
            return await _context.RegistrationSessions
                .Include(rs => rs.Professor)
                .Skip((page -1) * pageSize)
                .Take(pageSize)
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

            if (session.MaxStudents <=0)
            {
                session.MaxStudents = 5; // default value
            }

            var isHeadOfDepartment = professor.Department?.HeadOfDepartmentId == professor.UserId;

            if (!isHeadOfDepartment && session.MaxStudents != 5)
            {
                return BadRequest("Only the Head of Department or Admin can modify the maximum number of students.");
            }

            session.StartDate = session.StartDate.ToUniversalTime();
            session.EndDate = session.EndDate.ToUniversalTime();

            var overlappingSession = await _context.RegistrationSessions
                .Where(rs => rs.ProfessorId == session.ProfessorId)
                .Where(rs => rs.StartDate < session.EndDate && rs.EndDate > session.StartDate)
                .FirstOrDefaultAsync();

            if (overlappingSession != null)
            {
                return BadRequest("Overlapping session exists for this professor.");
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

            var prof = await _context.Professors.Include(p => p.Department).FirstOrDefaultAsync(p => p.UserId == int.Parse(userIdClaim));
            if (prof == null || (prof.UserId != session.ProfessorId && !User.IsInRole("Admin")))
            {
                return Unauthorized("You are not authorized to modify this session.");
            }

            var existingSession = await _context.RegistrationSessions.FindAsync(id);
            if (existingSession == null)
            {
                return NotFound("Registration session not found");
            }

            existingSession.StartDate = session.StartDate.ToUniversalTime();
            existingSession.EndDate = session.EndDate.ToUniversalTime();

            var isHeadOfDepartment = prof.Department?.HeadOfDepartmentId == prof.UserId;
            if (User.IsInRole("Admin") || isHeadOfDepartment)
            {
                existingSession.MaxStudents = session.MaxStudents;
            }
            else if (session.MaxStudents != existingSession.MaxStudents)
            {
                return BadRequest("Only the Head of Department or Admin can modify the maximum number of students.");
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
            //reset indexes
            var sessions = await _context.RegistrationSessions
                .Where(rs => rs.ProfessorId == session.ProfessorId)
                .ToListAsync();
            for (int i = 0; i < sessions.Count; i++)
            {
                sessions[i].Id = i + 1;
                _context.Entry(sessions[i]).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //get all registration sessions for a professor
        //see Professor Controller for this
        // GET: api/registrationsessions/by-professor/5
        [HttpGet("by-professor/{professorId}")]
        public async Task<ActionResult<IEnumerable<RegistrationSession>>> GetSessionsByProfessor(int professorId)
        {
            var sessions = await _context.RegistrationSessions
                .Where(rs => rs.ProfessorId == professorId)
                .Include(rs => rs.Professor)
                .ToListAsync();

            return Ok(sessions);
        }

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


        // get all registration sessions with pending requests
        // GET: api/registrationsessions/pending-requests
        [HttpGet("pending-requests")]
        public async Task<ActionResult<IEnumerable<RegistrationSession>>> GetSessionsWithPendingRequests()
        {
            var sessions = await _context.RegistrationSessions
                .Where(rs => _context.RegistrationRequests.Any(r => r.RegistrationSessionId == rs.Id && r.Status == RequestStatus.Pending))
                .Include(rs => rs.Professor)
                .ToListAsync();

            return Ok(sessions);
        }


        // get all active registration sessions for a professor
        // GET: api/registrationsessions/active/by-professor/5
        [HttpGet("active/by-professor/{professorId}")]
        public async Task<ActionResult<IEnumerable<RegistrationSession>>> GetActiveSessionsByProfessor(int professorId)
        {
            var now = DateTime.UtcNow;
            var activeSessions = await _context.RegistrationSessions
                .Where(rs => rs.ProfessorId == professorId && rs.StartDate <= now && rs.EndDate >= now)
                .Include(rs => rs.Professor)
                .ToListAsync();

            return Ok(activeSessions);
        }

    }

}
