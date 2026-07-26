using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using MongoDB.Driver;
using testAngularAPI.Server.Mongo;
using testAngularAPI.Server.Model;
using System.Text.RegularExpressions;

namespace MyWarehouse.Infrastructure.API.V1
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{v:apiVersion}/guests")]
    public class GuestsController : ControllerBase
    {
        private readonly MongoDbContext _db;

        public GuestsController(MongoDbContext db) => _db = db;

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(new List<object>());

            // Build case-insensitive regex filter for common guest fields
            var pattern = new BsonRegularExpression(Regex.Escape(q), "i");
            var filter = Builders<Guest>.Filter.Or(
                Builders<Guest>.Filter.Regex("firstName", pattern),
                Builders<Guest>.Filter.Regex("lastName", pattern),
                Builders<Guest>.Filter.Regex("email", pattern),
                Builders<Guest>.Filter.Regex("phoneNumber", pattern)
            );

            var results = await _db.Guests.Find(filter).Limit(limit).ToListAsync();

            var dto = results.Select(g => new {
                guestId = g.GuestId,
                firstName = g.FirstName,
                lastName = g.LastName,
                email = g.Email,
                phoneNumber = g.PhoneNumber
            }).ToList();

            return Ok(dto);
        }

        [HttpGet("{guestId}")]
        public async Task<IActionResult> Get(long guestId)
        {
            var g = await _db.Guests.Find(Builders<Guest>.Filter.Eq("guestId", guestId)).FirstOrDefaultAsync();
            if (g == null) return NotFound();
            return Ok(g);
        }
    }
}
