using Microsoft.AspNetCore.Mvc;
using testAngularAPI.Server.Extensions;
using testAngularAPI.Server.Model;

namespace testAngularAPI.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Example endpoint that demonstrates how to get the current user's ID from the request.
        /// This endpoint shows multiple ways to access user information.
        /// </summary>
        /// <returns>User information including ID, username, and email if available</returns>
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            // Method 1: Using the extension method (recommended)
            var userId = User.GetUserId();
            var username = User.GetUsername();
            var email = User.GetEmail();

            // Method 2: Direct access to User.Identity
            var identityName = User.Identity?.Name;
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

            // Method 3: Access specific claims directly
            var nameIdentifierClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var userInfo = new
            {
                UserId = userId,
                Username = username,
                Email = email,
                IdentityName = identityName,
                IsAuthenticated = isAuthenticated,
                NameIdentifierClaim = nameIdentifierClaim,
                Message = "This demonstrates how to get user ID and information from the API request"
            };

            _logger.LogInformation("User info requested: UserId={UserId}, Username={Username}", userId, username);

            return Ok(userInfo);
        }

        /// <summary>
        /// Example endpoint that requires user ID to be present
        /// </summary>
        [HttpGet("profile")]
        public IActionResult GetUserProfile()
        {
            // Get user ID from the authenticated user
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found. Please ensure you are authenticated." });
            }

            // In a real application, you would fetch user data from database using the userId
            // For demonstration purposes, we'll return mock data
            var userProfile = new User
            {
                Id = userId,
                Username = User.GetUsername() ?? "Unknown",
                Email = User.GetEmail() ?? "noemail@example.com"
            };

            return Ok(userProfile);
        }

        /// <summary>
        /// Example of a POST endpoint that uses user ID
        /// </summary>
        [HttpPost("action")]
        public IActionResult PerformUserAction([FromBody] UserActionRequest request)
        {
            // Get the authenticated user's ID
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            _logger.LogInformation("User {UserId} performed action: {Action}", userId, request.Action);

            return Ok(new
            {
                Success = true,
                Message = $"Action '{request.Action}' performed by user {userId}",
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public class UserActionRequest
    {
        public string? Action { get; set; }
    }
}
