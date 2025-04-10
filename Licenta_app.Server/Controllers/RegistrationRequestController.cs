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
    public class RegistrationRequestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RegistrationRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // get all registration requests
        // GET: api/registrationrequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistrationRequest>>> GetRegistrationRequests()
        {
            return await _context.RegistrationRequests
                .Include(r => r.Student)
                .Include(r => r.RegistrationSession)
                .ToListAsync();
        }

        // get registration request by id
        // GET: api/registrationrequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistrationRequest>> GetRegistrationRequestById(int id)
        {
            var registrationRequest = await _context.RegistrationRequests
                .Include(r => r.Student)
                .Include(r => r.RegistrationSession).ThenInclude(rs => rs.Professor)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (registrationRequest == null)
            {
                return NotFound();
            }
            return registrationRequest;
        }

        // create new registration request
        // POST: api/registrationrequests
        [Authorize(Roles = "Student, Admin")]
        [HttpPost]
        public async Task<ActionResult<RegistrationRequest>> CreateRequest(RegistrationRequest request)
        {
            var student = await _context.Students.FindAsync(request.StudentId);
            var session = await _context.RegistrationSessions.FindAsync(request.RegistrationSessionId);

            if (student == null || session == null)
            {
                return BadRequest("Invalid student or registration session ID");
            }

            request.Status = RequestStatus.Pending; // default status
            _context.RegistrationRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRegistrationRequestById), new { id = request.Id }, request);
        }

        // update existing registration request
        // PUT: api/registrationrequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, [FromBody] RegistrationRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == int.Parse(userIdClaim));
            var professor = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == int.Parse(userIdClaim));

            if (student == null || professor == null || (student.UserId != request.StudentId && !User.IsInRole("Admin")) || (professor.UserId != request.RegistrationSession.Professor.UserId && !User.IsInRole("Admin")))
            {
                return Unauthorized("User is not a student or professor");
            }

            var existingRequest = await _context.RegistrationRequests.FindAsync(id);
            if (existingRequest == null)
            {
                return NotFound();
            }

            existingRequest.Status = request.Status;
            existingRequest.ProposedTheme = request.ProposedTheme;
            if (User.IsInRole("Admin"))
            {
                existingRequest.StatusJustification = request.StatusJustification;
            }
            
            existingRequest.RegistrationSessionId = request.RegistrationSessionId;




            _context.Entry(existingRequest).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!_context.RegistrationRequests.Any(r => r.Id == id))
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

        // delete registration request
        // DELETE: api/registrationrequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.RegistrationRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            _context.RegistrationRequests.Remove(request);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // get all registration requests for a student
        // GET: api/registrationrequests/by-student/5
        [HttpGet("by-student/{studentId}")]
        public async Task<ActionResult<IEnumerable<RegistrationRequest>>> GetRequestsByStudent(int studentId)
        {
            var requests = await _context.RegistrationRequests
                .Where(r=> r.StudentId == studentId)
                .Include(r => r.RegistrationSession).ThenInclude(rs=> rs.Professor)
                .ToListAsync();

            if (!requests.Any())
            {
                return NotFound("No requests found for student with ID " + studentId);
            }

            return requests;
        }

        // get all registration requests by session
        // GET: api/registrationrequests/by-session/5
        [HttpGet("by-session/{sessionId}")]
        public async Task<ActionResult<IEnumerable<RegistrationRequest>>> GetRequestsBySession(int sessionId)
        {
            var requests = await _context.RegistrationRequests
                .Where(r => r.RegistrationSessionId == sessionId)
                .Include(r => r.Student)
                .ToListAsync();
            if (!requests.Any())
            {
                return NotFound("No requests found for session with ID " + sessionId);
            }
            return requests;
        }

        // update request status
        // PATCH: api/registrationrequests/5/status
        /*[HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] RequestStatus status)
        {
            var request = await _context.RegistrationRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            request.Status = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        */
    }
}
