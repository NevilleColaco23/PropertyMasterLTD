using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using testAngularAPI.Server.Model;
using testAngularAPI.Server.Model.DTOs;
using testAngularAPI.Server.Mongo;

namespace testAngularAPI.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(MongoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            // Find user by username
            var user = await _context.Users
                .Find(u => u.Username == request.Username)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Verify password (in production, use proper password hashing like BCrypt)
            // SECURITY NOTE: This simple hash comparison is vulnerable to timing attacks.
            // Consider using BCrypt.Net-Next or similar library with built-in secure comparison.
            if (user.Password != HashPassword(request.Password))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Generate JWT token with userId claim
            var token = GenerateJwtToken(user);

            var response = new AuthResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = 3600, // 1 hour
                Username = user.Username,
                Email = user.Email,
                UserId = user.Id ?? string.Empty
            };

            return Ok(response);
        }

        [HttpPost("SignUp")]
        public async Task<ActionResult<AuthResponse>> SignUp([FromBody] SignUpRequest request)
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .Find(u => u.Username == request.Username || u.Email == request.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return BadRequest(new { message = "Username or email already exists" });
            }

            // Create new user
            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.InsertOneAsync(newUser);

            // Generate JWT token with userId claim
            var token = GenerateJwtToken(newUser);

            var response = new AuthResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = 3600, // 1 hour
                Username = newUser.Username,
                Email = newUser.Email,
                UserId = newUser.Id ?? string.Empty
            };

            return Ok(response);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "ThisIsADefaultSecretKeyForDevelopmentPurposesOnly123456789";
            var issuer = jwtSettings["Issuer"] ?? "testAngularAPI";
            var audience = jwtSettings["Audience"] ?? "testAngularAPIUsers";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("userId", user.Id ?? string.Empty),
                new Claim("username", user.Username),
                new Claim("email", user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            // SECURITY WARNING: SHA256 is NOT suitable for password hashing!
            // This is a simple implementation for demonstration purposes only.
            // 
            // For production, use a proper password hashing algorithm such as:
            // - BCrypt (recommended): Install BCrypt.Net-Next package
            // - Argon2 (recommended): Install Konscious.Security.Cryptography.Argon2 package
            // - PBKDF2: Use Rfc2898DeriveBytes from System.Security.Cryptography
            //
            // These algorithms automatically handle salting and are designed to be slow
            // to prevent brute force attacks.
            //
            // Example with BCrypt:
            // return BCrypt.Net.BCrypt.HashPassword(password);
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
