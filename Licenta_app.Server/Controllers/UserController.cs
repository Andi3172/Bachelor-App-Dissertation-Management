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
        //from query is used to get the id from the url. Can i get the id from the token?
        
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token");
            }
            Console.WriteLine("User ID from token: " + userIdClaim);
            if (userIdClaim != id.ToString())
            {
                return Forbid("You can only update your own user");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            //change password and hash it
            if (user.Password != null)
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }


            existingUser.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existingUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!_context.Users.Any(u => u.Id == id))
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
