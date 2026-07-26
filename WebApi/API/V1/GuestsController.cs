using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace MyWarehouse.WebApi.API.V1
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiversion}/Guests")]
    public class GuestsController : ControllerBase
    {
        private readonly IMongoDatabase _db;

        public GuestsController(IMongoDatabase db) => _db = db;

        /// <summary>
        /// Search guests by name, email or phone number (case-insensitive)
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Ok(new List<object>());

            try
            {
                var collection = _db.GetCollection<BsonDocument>("guests");
                var pattern = new BsonRegularExpression(Regex.Escape(q.Trim()), "i");

                var filter = Builders<BsonDocument>.Filter.Or(
                    Builders<BsonDocument>.Filter.Regex("firstName", pattern),
                    Builders<BsonDocument>.Filter.Regex("lastName", pattern),
                    Builders<BsonDocument>.Filter.Regex("email", pattern),
                    Builders<BsonDocument>.Filter.Regex("phoneNumber", pattern)
                );

                var docs = await collection.Find(filter).Limit(limit).ToListAsync();

                var results = docs.Select(d => new
                {
                    guestId     = GetLong(d, "guestId"),
                    firstName   = GetString(d, "firstName"),
                    lastName    = GetString(d, "lastName"),
                    email       = GetString(d, "email"),
                    phoneNumber = GetString(d, "phoneNumber")
                }).ToList();

                return Ok(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GuestsController.Search error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a single guest by guestId
        /// </summary>
        [HttpGet("{guestId}")]
        public async Task<IActionResult> GetById(long guestId)
        {
            var collection = _db.GetCollection<BsonDocument>("guests");
            // guestId may be stored as Int32, Int64 or Double - query handles all
            var filter = Builders<BsonDocument>.Filter.Eq("guestId", BsonValue.Create(guestId));
            var doc = await collection.Find(filter).FirstOrDefaultAsync();

            if (doc == null) return NotFound();

            return Ok(new
            {
                guestId     = GetLong(doc, "guestId"),
                firstName   = GetString(doc, "firstName"),
                lastName    = GetString(doc, "lastName"),
                email       = GetString(doc, "email"),
                phoneNumber = GetString(doc, "phoneNumber")
            });
        }

        // --- helpers ---
        private static string GetString(BsonDocument d, string key)
            => d.Contains(key) && !d[key].IsBsonNull ? d[key].AsString : "";

        private static long GetLong(BsonDocument d, string key)
        {
            if (!d.Contains(key) || d[key].IsBsonNull) return 0;
            var v = d[key];
            return v.BsonType switch
            {
                BsonType.Int32  => v.AsInt32,
                BsonType.Int64  => v.AsInt64,
                BsonType.Double => (long)v.AsDouble,
                _               => 0
            };
        }
    }
}

