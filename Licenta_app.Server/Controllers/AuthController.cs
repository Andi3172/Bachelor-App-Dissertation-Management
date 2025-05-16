using Licenta_app.Server.Data;
using Licenta_app.Server.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using Google.Apis.Auth;


//using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text;


namespace Licenta_app.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // login
        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Models.LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateJwtToken(user);
            Console.WriteLine("JWT Key in AuthController: " + _configuration["Jwt:Key"]);

            return Ok(new { token });
        }

        //register
        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Models.RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Email already in use");
            }

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username already in use");
            }

            //hash pw
            request.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            //decide role
            if (request.Email.Contains("stud.ase.ro"))
            {
                request.Role = "Student";
            }
            else if (request.Email.Contains("admin"))
            {
                request.Role = "Admin";
            }
            else
            {
                request.Role = "Professor";
            }

            var newUser = new User
            {
                Email = request.Email,
                Username = request.Username,
                Password = request.Password,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow

            };

            switch (request.Role)
            {
                case "Student":
                    var student = new Student
                    {
                        User = newUser,
                    };
                    _context.Students.Add(student);
                    break;
                case "Professor":
                    var professor = new Professor
                    {
                        User = newUser
                    };
                    _context.Professors.Add(professor);
                    break;
                case "Admin":
                    break;
            }

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            //generate token as well
            var token = GenerateJwtToken(newUser);
            return Ok(new { message = "User registered succsessfully", token });

        }


        //google login
        // POST: api/auth/google-login
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.idToken);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
                if (user == null)
                {
                    user = new User
                    {
                        Email = payload.Email,
                        Username = payload.Name ?? payload.Email,
                        Password = "", // No password for Google users
                        Role = payload.Email.Contains("stud.ase.ro") ? "Student" :
                               payload.Email.Contains("admin") ? "Admin" : "Professor",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Users.Add(user);

                    // Add to Student or Professor table as needed
                    if (user.Role == "Student")
                        _context.Students.Add(new Student { User = user });
                    else if (user.Role == "Professor")
                        _context.Professors.Add(new Professor { User = user });

                    await _context.SaveChangesAsync();
                }

                // Generate a JWT token
                var token = GenerateJwtToken(user);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized("Invalid Google token: " + ex.Message);
            }
        }



        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JWT");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!.Trim());

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("role", user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            return new JwtSecurityTokenHandler().CreateEncodedJwt(tokenDescriptor);
        }
    }
};