using Licenta_app.Server.Data;
using Licenta_app.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace Licenta_app.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // get all users
        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.OrderBy(u => u.Id).ToListAsync();
        }

        // get user by id
        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Student)
                .Include(u => u.Professor)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        //Creating a new user is done in the AuthController.cs file for security reasons.

        // update existing user
        // PUT: api/users
        //[HttpPut("{id}")]
        [HttpPut("{id}")]
        

        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }

            if (userIdClaim != id.ToString())
            {
                return Forbid("You can only update your own user");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            // Validate current password using BCrypt
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, existingUser.Password);
            if (!isPasswordCorrect)
            {
                return Unauthorized("Incorrect current password.");
            }

            // Apply updates
            existingUser.Username = dto.Username;
            existingUser.Email = dto.Email;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Update password only if a new one is provided
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            }

            _context.Entry(existingUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "An error occurred while saving changes.");
            }

            return NoContent();
        }


        // delete user
        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if(user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == id);
            if (student != null)
            {
                _context.Students.Remove(student);
            }
            var professor = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == id);
            if (professor != null)
            {
                _context.Professors.Remove(professor);
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the user.");
            }

            return NoContent();
        }

    }
}
