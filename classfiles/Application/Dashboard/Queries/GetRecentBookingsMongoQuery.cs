using MongoDB.Bson;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using System.Data;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// MongoDB aggregation query for fetching recent bookings with guest information via $lookup
    /// </summary>
    public class GetRecentBookingsMongoQuery : INamedQuery
    {
        private readonly int _limit;
        private readonly int _userId;
        private readonly List<int> _propertyIds;

        public GetRecentBookingsMongoQuery(int userId, int limit, List<int> propertyIds = null)
        {
            _userId = userId;
            _limit = limit;
            _propertyIds = propertyIds ?? new List<int>();
        }

        public BsonArray? BsonPipeline => GetRecentBookingsPipeline();

        private BsonArray GetRecentBookingsPipeline()
        {
            var pipeline = new BsonArray();

            // Stage 1: Match by property IDs if provided
            if (_propertyIds != null && _propertyIds.Any())
            {
                var propertyIdsArray = new BsonArray(_propertyIds.Select(id => new BsonInt32(id)));
                pipeline.Add(new BsonDocument("$match", new BsonDocument("propertyId", new BsonDocument("$in", propertyIdsArray))));
            }

            // Stage 2: Sort by CreatedAt descending (most recent first)
            pipeline.Add(new BsonDocument("$sort", new BsonDocument("bookingDate", -1)));

            // Stage 3: Limit results
            pipeline.Add(new BsonDocument("$limit", _limit));

            pipeline.Add(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "guests" },
                { "localField", "guestId" },
                { "foreignField", "_id" },
                { "as", "guestInfo" }
            }));

            // Stage 5: Unwind guestInfo array (converts array to object, preserveNullAndEmptyArrays keeps bookings without guests)
            pipeline.Add(new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$guestInfo" },
                { "preserveNullAndEmptyArrays", true }
            }));

            // Stage 6: Project final shape with guest name extracted and concatenated  
            pipeline.Add(new BsonDocument("$project", new BsonDocument
            {
                { "_id", 1 },
                { "bookingId", 1 },
                { "roomNumber", 1 },
                { "checkInDate", 1 },
                { "checkOutDate", 1 },
                { "numberOfGuests", 1 },
                { "totalPrice", 1 },
                { "isConfirmed", 1 },
                { "bookingDate", 1 },
                { "guestId", 1 },
                // Build guest name: check if guestInfo exists first, then concatenate firstName + lastName
                { "guestName", new BsonDocument("$cond", new BsonArray
                    {
                        // If: guestInfo exists (not null)
                        new BsonDocument("$ne", new BsonArray { "$guestInfo", BsonNull.Value }),
                        // Then: concatenate firstName + space + lastName
                        new BsonDocument("$concat", new BsonArray
                        {
                            new BsonDocument("$ifNull", new BsonArray { "$guestInfo.firstName", "" }),
                            " ",
                            new BsonDocument("$ifNull", new BsonArray { "$guestInfo.lastName", "" })
                        }),
                        // Else: show "Unavailable" instead of blank
                        "Unavailable"
                    })
                }
            }));

            return pipeline;
        }

        public string QueryStr => string.Empty;

        public CommandType CommandType => CommandType.Text;

        public IReadOnlyList<NamedQueryParameter> Parameters => null;
    }
}
