using Microsoft.AspNetCore.Mvc;
using testAngularAPI.Server.Mongo;
using testAngularAPI.Server.Model;
using MongoDB.Driver;

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
