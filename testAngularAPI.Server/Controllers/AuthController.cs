using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace testAngularAPI.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Generates a test JWT token for development/testing purposes ONLY.
        /// WARNING: This endpoint should NOT be available in production!
        /// </summary>
        [HttpPost("generate-test-token")]
        public IActionResult GenerateTestToken([FromBody] TestTokenRequest request)
        {
            // Security: Only allow in development environment
            if (!_configuration.GetValue<bool>("IsDevelopment", false))
            {
                return NotFound(); // Hide endpoint in production
            }

            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new { message = "UserId is required" });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.UserId),
                new Claim(ClaimTypes.Name, request.Username ?? "test-user"),
                new Claim(ClaimTypes.Email, request.Email ?? "test@example.com"),
                new Claim("sub", request.UserId) // JWT standard claim for user ID
            };

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Test JWT token generated for UserId: {UserId}", request.UserId);

            return Ok(new
            {
                token = tokenString,
                expiresAt = token.ValidTo,
                tokenType = "Bearer",
                message = "Use this token in the Authorization header: Bearer <token>"
            });
        }
    }

    public class TestTokenRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
}
