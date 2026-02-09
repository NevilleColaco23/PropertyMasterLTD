using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using testAngularAPI.Server.Mongo;
using testAngularAPI.Server.Model;
using MongoDB.Driver;
using System.Security.Claims;

namespace testAngularAPI.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger, MongoDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Get the currently authenticated user's information
        /// </summary>
        [Authorize]
        [HttpGet("me", Name = "GetCurrentUser")]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            try
            {
                // Get the user ID from the JWT token claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var user = await _context.Users.Find(filter).FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {userId} not found" });
                }

                return Ok(new
                {
                    userId = user.Id,
                    name = user.Name,
                    email = user.Email,
                    message = "This is the active user accessing the API"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current user");
                return StatusCode(500, "An error occurred while fetching user information");
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet(Name = "GetAllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            try
            {
                var users = await _context.Users.Find(_ => true).ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all users");
                return StatusCode(500, "An error occurred while fetching users");
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}", Name = "GetUserById")]
        public async Task<ActionResult<User>> GetById(string id)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, id);
                var user = await _context.Users.Find(filter).FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while fetching the user");
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost(Name = "CreateUser")]
        public async Task<ActionResult<User>> Create([FromBody] User user)
        {
            try
            {
                await _context.Users.InsertOneAsync(user);
                return CreatedAtRoute("GetUserById", new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "An error occurred while creating the user");
            }
        }
    }
}
