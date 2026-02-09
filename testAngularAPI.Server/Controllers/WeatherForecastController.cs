using Microsoft.AspNetCore.Mvc;
using testAngularAPI.Server.Mongo;
using MongoDB.Driver;
using testAngularAPI.Server.Extensions;

namespace testAngularAPI.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly MongoDbContext _context;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching","Neville"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, MongoDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            // EXAMPLE: How to get user ID in an API endpoint
            // This demonstrates accessing the authenticated user's ID
            var userId = User.GetUserId();
            var username = User.GetUsername();

            // Log the user information for debugging
            _logger.LogInformation("Weather forecast requested by UserId: {UserId}, Username: {Username}", userId, username);

            // Fetch all users from MongoDB
            var users = await _context.Properties.Find(_ => true).ToListAsync();

            // Use user names as summaries
            var summaries = users.Select(u => u.Name).ToArray();

            // Handle case where MongoDB is empty
            if (summaries.Length == 0)
            {
                summaries = new[] { "No Users Found" };
            }

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            })
            .ToArray();
        }
    }
}
