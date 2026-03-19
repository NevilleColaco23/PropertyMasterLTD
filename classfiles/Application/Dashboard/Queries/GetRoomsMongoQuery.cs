using MongoDB.Bson;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using System.Data;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// MongoDB aggregation query to fetch rooms for room planner
    /// </summary>
    public class GetRoomsMongoQuery : INamedQuery
    {
        private readonly int _userId;
        private readonly List<int> _propertyIds;
        private readonly bool _activeOnly;

        public GetRoomsMongoQuery(int userId, List<int> propertyIds, bool activeOnly = true)
        {
            _userId = userId;
            _propertyIds = propertyIds ?? new List<int>();
            _activeOnly = activeOnly;
        }

        public BsonArray? BsonPipeline => GetRoomsPipeline();

        public string QueryStr => string.Empty; // Not used for MongoDB aggregation pipelines

        public CommandType CommandType => CommandType.Text; // Standard for MongoDB queries

        public IReadOnlyList<NamedQueryParameter> Parameters => null; // No parameters for aggregation pipeline

        private BsonArray GetRoomsPipeline()
        {
            var pipeline = new BsonArray();

            // Stage 1: Match filters - combine all conditions with $and
            var andConditions = new BsonArray();

            // Active rooms only (if requested) - support both PascalCase and camelCase
            if (_activeOnly)
            {
                andConditions.Add(new BsonDocument("$or", new BsonArray
                {
                    new BsonDocument("Active", true),
                    new BsonDocument("isActive", true)
                }));
            }

            // Filter by property IDs (if specified) - support both PropertyId (PascalCase) and propertyId (camelCase)
            if (_propertyIds != null && _propertyIds.Any())
            {
                var propertyIdsArray = new BsonArray(_propertyIds.Select(id => new BsonInt32(id)));
                andConditions.Add(new BsonDocument("$or", new BsonArray
                {
                    new BsonDocument("PropertyId", new BsonDocument("$in", propertyIdsArray)),
                    new BsonDocument("propertyId", new BsonDocument("$in", propertyIdsArray))
                }));
            }

            // Exclude soft-deleted rooms
            andConditions.Add(new BsonDocument("$or", new BsonArray
            {
                new BsonDocument("IsDeleted", new BsonDocument("$ne", true)),
                new BsonDocument("IsDeleted", new BsonDocument("$exists", false))
            }));

            // Build match document with $and to combine all conditions
            if (andConditions.Count > 0)
            {
                var matchDocument = new BsonDocument("$and", andConditions);
                pipeline.Add(new BsonDocument("$match", matchDocument));
            }

            // Stage 2: Lookup property details (Property collection uses _id as PropertyId)
            pipeline.Add(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Property" },
                { "localField", "PropertyId" },
                { "foreignField", "_id" },
                { "as", "propertyInfo" }
            }));

            // Stage 3: Project fields - map both PascalCase and camelCase to consistent output
            pipeline.Add(new BsonDocument("$project", new BsonDocument
            {
                { "_id", "$_id" },
                // RoomId: try RoomId first, fallback to _id numeric value
                { "roomId", new BsonDocument("$ifNull", new BsonArray { "$RoomId", "$_id" }) },
                // RoomNumber: try roomNumber (camelCase), then RoomCode (PascalCase)
                { "roomNumber", new BsonDocument("$ifNull", new BsonArray { "$roomNumber", "$RoomCode" }) },
                // RoomName: try multiple sources
                { "roomName", new BsonDocument("$ifNull", new BsonArray 
                    { 
                        "$RoomName", 
                        "$roomName",
                        "$RoomCode",
                        "$roomNumber" 
                    }) 
                },
                // RoomType: try roomType, fallback to "Standard"
                { "roomType", new BsonDocument("$ifNull", new BsonArray { "$roomType", "Standard" }) },
                // PropertyId: try PropertyId (PascalCase) first, then propertyId (camelCase)
                { "propertyId", new BsonDocument("$ifNull", new BsonArray { "$PropertyId", "$propertyId" }) },
                // PropertyName: extract from lookup (Property collection uses "Name" field)
                { "propertyName", new BsonDocument("$ifNull", new BsonArray 
                    { 
                        new BsonDocument("$arrayElemAt", new BsonArray { "$propertyInfo.Name", 0 }),
                        new BsonDocument("$arrayElemAt", new BsonArray { "$propertyInfo.name", 0 }),
                        "Unknown Property" 
                    }) 
                },
                { "floor", "$floor" },
                { "capacity", "$capacity" },
                // Status: try status field, fallback to "Available"
                { "status", new BsonDocument("$ifNull", new BsonArray { "$status", "Available" }) },
                { "amenities", new BsonDocument("$ifNull", new BsonArray { "$amenities", new BsonArray() }) },
                { "pricePerNight", "$pricePerNight" },
                // isActive: try both Active (PascalCase) and isActive (camelCase)
                { "isActive", new BsonDocument("$ifNull", new BsonArray 
                    { 
                        "$isActive", 
                        "$Active", 
                        true 
                    }) 
                }
            }));

            // Stage 4: Sort by property name, then floor, then room number
            pipeline.Add(new BsonDocument("$sort", new BsonDocument
            {
                { "propertyName", 1 },
                { "floor", 1 },
                { "roomNumber", 1 }
            }));

            return pipeline;
        }

        // Debug methods
        public string GetDebugInfo()
        {
            return $"GetRoomsMongoQuery - UserId: {_userId}, PropertyIds: [{string.Join(", ", _propertyIds)}], ActiveOnly: {_activeOnly}";
        }

        public string GetPipelineAsJsonString()
        {
            return BsonPipeline?.ToJson() ?? "[]";
        }
    }
}
