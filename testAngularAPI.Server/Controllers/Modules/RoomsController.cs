using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using testAngularAPI.Server.Mongo;
using testAngularAPI.Server.Model;

namespace MyWarehouse.Infrastructure.API.V1
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{v:apiVersion}/rooms")]
    public class RoomsController : ControllerBase
    {
        private readonly MongoDbContext _db;

        public RoomsController(MongoDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetByProperty([FromQuery] int propertyId)
        {
            if (propertyId <= 0) return BadRequest("propertyId is required");

            var filter = Builders<RoomEntity>.Filter.Eq("PropertyId", propertyId);
            var rooms = await _db.Rooms.Find(filter).ToListAsync();

            var dto = rooms.Select(r => new {
                id = r.Id.ToString(),
                propertyId = r.PropertyId,
                roomCode = r.RoomCode,
                roomName = r.RoomName,
                active = r.Active
            }).ToList();

            return Ok(dto);
        }
    }
}
