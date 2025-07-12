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
                    .ThenInclude(s => s.User)
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
                    .ThenInclude(s => s.User)
                .Include(r => r.RegistrationSession)
                    .ThenInclude(rs => rs.Professor)
                        .ThenInclude(p => p.User)
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

            var userId = int.Parse(userIdClaim);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            var professor = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == userId);

            if (student == null && professor == null && !User.IsInRole("Admin"))
            {
                return Unauthorized("User is not a student or professor");
            }

            // Only allow students to update their own requests, professors to update requests for their sessions, or admins
            if (student != null && student.UserId != request.StudentId && !User.IsInRole("Admin"))
            {
                return Unauthorized("Student can only update their own requests");
            }
            if (professor != null)
            {
                // Load the session to check professor ownership
                var session = await _context.RegistrationSessions
                    .FirstOrDefaultAsync(rs => rs.Id == request.RegistrationSessionId);
                if (session == null || (session.ProfessorId != professor.UserId && !User.IsInRole("Admin")))
                {
                    return Unauthorized("Professor can only update requests for their own sessions");
                }
            }

            var existingRequest = await _context.RegistrationRequests.FindAsync(id);
            if (existingRequest == null)
            {
                return NotFound();
            }

            //track approval
            var wasPrevioslyApproved = existingRequest.Status == RequestStatus.Approved;

            existingRequest.Status = request.Status;
            existingRequest.ProposedTheme = request.ProposedTheme;
            existingRequest.StatusJustification = request.StatusJustification;
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

            // === Academic Default File utomation ===
            // Only adds the academic default file i the request is now approved and wasn't before
            if(!wasPrevioslyApproved && existingRequest.Status == RequestStatus.Approved)
            {
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "academic-default.docx");
                if (System.IO.File.Exists(templatePath))
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_academic-default.docx";
                    var destPath = Path.Combine(uploadsFolder, uniqueFileName);

                    // copy the template to the uploads folder
                    System.IO.File.Copy(templatePath, destPath, overwrite: true);

                    // remove any existing academic-default file for this request
                    var oldFile = await _context.Files
                        .FirstOrDefaultAsync(f => f.RequestId == existingRequest.Id && f.FileType == "academic-default");
                    if (oldFile != null)
                    {
                        if (System.IO.File.Exists(oldFile.FilePath))
                            System.IO.File.Delete(oldFile.FilePath);
                        _context.Files.Remove(oldFile);
                        await _context.SaveChangesAsync();
                    }

                    //add new FileUpload entry
                    var academicDefaultFile = new FileUpload
                    {
                        RequestId = existingRequest.Id,
                        UploadedBy = "system",
                        FileName = "academic-default.docx",
                        FilePath = destPath,
                        FileType = "academic-default",
                        UploadedDate = DateTime.UtcNow,
                        RegistrationRequest = existingRequest
                    };
                    _context.Files.Add(academicDefaultFile);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    //log or handle missing template file
                    Console.WriteLine("Academic default template file not found at: " + templatePath);
                }

            }
            // === End of Academic Default File Automation ===

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

           

            return Ok(requests);
        }

        // get all registration requests by session
        // GET: api/registrationrequests/by-session/5
        [HttpGet("by-session/{sessionId}")]
        public async Task<ActionResult<IEnumerable<RegistrationRequest>>> GetRequestsBySession(int sessionId)
        {
            var requests = await _context.RegistrationRequests
                .Where(r => r.RegistrationSessionId == sessionId)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .ToListAsync();
            
            return Ok(requests);
        }


        // get all approved registration requests for a professor
        [Authorize(Roles = "Professor, Admin")]
        [HttpGet("approved-by-professor/{professorId}")]
        public async Task<ActionResult<IEnumerable<RegistrationRequest>>> GetApprovedRequestsByProfessor(int professorId)
        {
            var requests = await _context.RegistrationRequests
                .Include(r => r.Student).ThenInclude(s => s.User)
                .Include(r => r.RegistrationSession)
                .Where(r => r.RegistrationSession.ProfessorId == professorId && r.Status == RequestStatus.Approved)
                .ToListAsync();

            return Ok(requests);
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
