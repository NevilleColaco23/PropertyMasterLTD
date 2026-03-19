using MediatR;
using MongoDB.Driver;
using MongoDB.Bson;
using MyWarehouse.Application.Dashboard.DTOs;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// Query handler to get rooms for room planner
    /// </summary>
    public class GetRoomsByPropertyQueryHandler : IRequestHandler<GetRoomsByPropertyQuery, List<GetRoomListDTO>>
    {
        private readonly IMongoDatabase _database;

        public GetRoomsByPropertyQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<List<GetRoomListDTO>> Handle(GetRoomsByPropertyQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var roomsCollection = _database.GetCollection<BsonDocument>(MongoCollections.RoomCollection);

                // 🔍 DEBUG: Log request parameters
                Console.WriteLine($"🔧 ===== ROOM PLANNER QUERY DEBUG =====");
                Console.WriteLine($"🔧 Request - UserId: {request.UserId}, PropertyIds: [{string.Join(", ", request.PropertyIds)}], ActiveOnly: {request.ActiveOnly}");

                // Create MongoDB query
                var mongoQuery = new GetRoomsMongoQuery(
                    request.UserId,
                    request.PropertyIds,
                    request.ActiveOnly
                );

                // 🔍 DEBUG: Get query info (set breakpoint to inspect)
                var debugInfo = mongoQuery.GetDebugInfo();
                var debugPipeline = mongoQuery.GetPipelineAsJsonString();
                Console.WriteLine($"🔧 Query Info: {debugInfo}");
                Console.WriteLine($"🔧 Pipeline JSON: {debugPipeline}");

                // Execute aggregation pipeline
                var pipelineStages = mongoQuery.BsonPipeline.Select(stage => (BsonDocument)stage).ToArray();

                Console.WriteLine($"🔧 Executing aggregation with {pipelineStages.Length} stages");

                // 🔍 DEBUG: Before executing aggregation, check collection directly
                var totalRooms = await roomsCollection.CountDocumentsAsync(new BsonDocument());
                Console.WriteLine($"🔧 BEFORE AGGREGATION - Total rooms in collection: {totalRooms}");

                // Check rooms for PropertyId=1 with exact match
                var prop1Rooms = await roomsCollection.CountDocumentsAsync(
                    new BsonDocument("PropertyId", 1));
                Console.WriteLine($"🔧 BEFORE AGGREGATION - Rooms with PropertyId=1: {prop1Rooms}");

                // Check active rooms
                var activeRooms = await roomsCollection.CountDocumentsAsync(
                    new BsonDocument("Active", true));
                Console.WriteLine($"🔧 BEFORE AGGREGATION - Rooms with Active=true: {activeRooms}");

                // Check non-deleted rooms
                var nonDeletedRooms = await roomsCollection.CountDocumentsAsync(
                    new BsonDocument("$or", new BsonArray
                    {
                        new BsonDocument("IsDeleted", new BsonDocument("$ne", true)),
                        new BsonDocument("IsDeleted", new BsonDocument("$exists", false))
                    }));
                Console.WriteLine($"🔧 BEFORE AGGREGATION - Non-deleted rooms: {nonDeletedRooms}");

                // Now execute the aggregation
                var roomDocs = await roomsCollection
                    .Aggregate<BsonDocument>(pipelineStages)
                    .ToListAsync(cancellationToken);

                Console.WriteLine($"✅ AFTER AGGREGATION - Returned {roomDocs.Count} room documents");

                if (roomDocs.Count == 0)
                {
                    Console.WriteLine("⚠️ ===== AGGREGATION RETURNED 0 RESULTS =====");
                    Console.WriteLine("⚠️ This means the $match stage filtered out all rooms OR the $lookup failed.");
                    Console.WriteLine("⚠️ Check if:");
                    Console.WriteLine("⚠️   1. PropertyId field exists and matches the filter");
                    Console.WriteLine("⚠️   2. Active field is true");
                    Console.WriteLine("⚠️   3. IsDeleted is false or doesn't exist");
                    Console.WriteLine("⚠️   4. Property collection has matching PropertyId documents");
                }
                else
                {
                    Console.WriteLine($"✅ Sample of first room document: {roomDocs[0].ToJson()}");
                }

                // Map BsonDocument to DTO
                var rooms = new List<GetRoomListDTO>();
                foreach (var doc in roomDocs)
                {
                    // Helper function to safely get nullable int
                    int? GetNullableInt(BsonDocument document, string fieldName)
                    {
                        if (!document.Contains(fieldName)) return null;
                        var value = document.GetValue(fieldName, BsonNull.Value);
                        return value.IsBsonNull ? null : (int?)value.AsInt32;
                    }

                    // Helper function to safely get nullable decimal
                    decimal? GetNullableDecimal(BsonDocument document, string fieldName)
                    {
                        if (!document.Contains(fieldName)) return null;
                        var value = document.GetValue(fieldName, BsonNull.Value);
                        return value.IsBsonNull ? null : (decimal?)value.ToDecimal();
                    }

                    rooms.Add(new GetRoomListDTO
                    {
                        Id = doc["_id"].ToString(),
                        RoomId = doc.GetValue("roomId", 0).AsInt32,
                        RoomNumber = doc.GetValue("roomNumber", "").AsString,
                        RoomName = doc.GetValue("roomName", "").AsString,
                        RoomType = doc.GetValue("roomType", "Standard").AsString,
                        PropertyId = doc.GetValue("propertyId", 0).AsInt32,
                        PropertyName = doc.GetValue("propertyName", "Unknown").AsString,
                        Floor = GetNullableInt(doc, "floor"),
                        Capacity = GetNullableInt(doc, "capacity"),
                        Status = doc.GetValue("status", "Available").AsString,
                        Amenities = doc.GetValue("amenities", new BsonArray()).AsBsonArray
                            .Select(a => a.AsString)
                            .ToList(),
                        PricePerNight = GetNullableDecimal(doc, "pricePerNight"),
                        IsActive = doc.GetValue("isActive", true).ToBoolean()
                    });
                }

                Console.WriteLine($"✅ Mapped {rooms.Count} rooms for room planner");
                Console.WriteLine($"🔧 ===== END ROOM PLANNER QUERY DEBUG =====");

                return rooms;
            }
            catch (Exception ex)
            {
                // Log error and return empty list
                Console.WriteLine($"❌ ===== ROOM PLANNER QUERY ERROR =====");
                Console.WriteLine($"❌ Error fetching rooms: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                Console.WriteLine($"❌ ===== END ERROR =====");
                return new List<GetRoomListDTO>();
            }
        }
    }
}
