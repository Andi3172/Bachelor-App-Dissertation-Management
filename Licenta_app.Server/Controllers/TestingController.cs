using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Licenta_app.Server.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TestingController : Controller
    {
        [HttpGet("validate")]
        public IActionResult ValidateToken([FromHeader(Name = "Authorization")] string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader))
            {
                return BadRequest("No Authorization header provided");
            }

            var token = authHeader.Replace("Bearer ", "").Trim();

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    return BadRequest("Invalid token format.");
                }
                return Ok(new
                {
                    Header = jsonToken.Header,
                    Payload = jsonToken.Payload,
                    Signature = tokenParts[2], // Extracted signature
                    DotCount = token.Count(c => c == '.')
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
