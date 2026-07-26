using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MyWarehouse.WebApi.API.V1
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{v:apiversion}/Rooms")]
    public class RoomsController : ControllerBase
    {
        private readonly IMongoDatabase _db;

        public RoomsController(IMongoDatabase db) => _db = db;

        /// <summary>
        /// Get all active rooms for a given property
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetByProperty([FromQuery] int propertyId)
        {
            if (propertyId <= 0)
                return BadRequest("propertyId is required");

            var collection = _db.GetCollection<BsonDocument>("Room");

            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("PropertyId", propertyId),
                Builders<BsonDocument>.Filter.Ne("IsDeleted", true)
            );

            var docs = await collection.Find(filter).ToListAsync();

            var results = docs.Select(d => new
            {
                id         = d["_id"].ToString(),
                propertyId = d.Contains("PropertyId") ? d["PropertyId"].AsInt32   : 0,
                roomCode   = d.Contains("RoomCode")   ? d["RoomCode"].AsString    : "",
                roomName   = d.Contains("RoomName")   ? d["RoomName"].AsString    : "",
                active     = d.Contains("Active")     ? d["Active"].AsBoolean     : false
            }).ToList();

            return Ok(results);
        }
    }
}
