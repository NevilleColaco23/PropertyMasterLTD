using MongoDB.Bson;
using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// MongoDB aggregation query to fetch bookings with guest details for room planner
    /// </summary>
    public class GetBookingsWithGuestsMongoQuery : INamedQuery
    {
        private readonly int _userId;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly List<int> _propertyIds;

        public GetBookingsWithGuestsMongoQuery(
            int userId,
            DateTime startDate,
            DateTime endDate,
            List<int> propertyIds)
        {
            _userId = userId;
            _startDate = startDate;
            _endDate = endDate;
            _propertyIds = propertyIds ?? new List<int>();
        }

        public BsonArray? BsonPipeline => GetBookingsPipeline();

        public string QueryStr => string.Empty;

        public System.Data.CommandType CommandType => System.Data.CommandType.Text;

        public IReadOnlyList<NamedQueryParameter> Parameters => null;

        private BsonArray GetBookingsPipeline()
        {
            var pipeline = new BsonArray();

            // Stage 1: $match - Filter bookings by date range and property
            var matchConditions = new BsonArray
            {
                // Date range filter (check if booking overlaps with the requested period)
                // Support all three casing variants: checkInDate, CheckInDate, checkinDate
                new BsonDocument("$or", new BsonArray
                {
                    // camelCase with capital I and D: checkInDate, checkOutDate
                    new BsonDocument("$and", new BsonArray
                    {
                        new BsonDocument("checkInDate", new BsonDocument("$lte", _endDate)),
                        new BsonDocument("checkOutDate", new BsonDocument("$gte", _startDate))
                    }),
                    // Pascal case: CheckInDate, CheckOutDate
                    new BsonDocument("$and", new BsonArray
                    {
                        new BsonDocument("CheckInDate", new BsonDocument("$lte", _endDate)),
                        new BsonDocument("CheckOutDate", new BsonDocument("$gte", _startDate))
                    }),
                    // all lowercase: checkinDate, checkoutDate
                    new BsonDocument("$and", new BsonArray
                    {
                        new BsonDocument("checkinDate", new BsonDocument("$lte", _endDate)),
                        new BsonDocument("checkoutDate", new BsonDocument("$gte", _startDate))
                    })
                })
            };

            // Property filter (support both PascalCase and camelCase)
            if (_propertyIds != null && _propertyIds.Any())
            {
                var propertyIdsArray = new BsonArray(_propertyIds.Select(id => new BsonInt32(id)));
                matchConditions.Add(new BsonDocument("$or", new BsonArray
                {
                    new BsonDocument("propertyId", new BsonDocument("$in", propertyIdsArray)),
                    new BsonDocument("PropertyId", new BsonDocument("$in", propertyIdsArray))
                }));
            }

            var matchDocument = new BsonDocument("$and", matchConditions);
            pipeline.Add(new BsonDocument("$match", matchDocument));

            // Stage 2: $lookup - Join with Guests collection
            pipeline.Add(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Guests" },
                { "localField", "guestId" },
                { "foreignField", "guestId" },
                { "as", "guestInfo" }
            }));

            // Stage 3: $lookup - Join with Property collection for property name
            pipeline.Add(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Property" },
                { "let", new BsonDocument("propId", "$propertyId") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$eq", new BsonArray { "$_id", "$$propId" })))
                    }
                },
                { "as", "propertyInfo" }
            }));

            // Stage 4: $project - Map fields with guest details
            pipeline.Add(new BsonDocument("$project", new BsonDocument
            {
                { "_id", "$_id" },
                { "bookingId", new BsonDocument("$ifNull", new BsonArray { "$bookingId", "$_id" }) },
                { "roomNumber", new BsonDocument("$ifNull", new BsonArray { "$roomNumber", "" }) },
                { "propertyId", new BsonDocument("$ifNull", new BsonArray { "$propertyId", "$PropertyId" }) },
                { "propertyName", new BsonDocument("$ifNull", new BsonArray
                    {
                        new BsonDocument("$arrayElemAt", new BsonArray { "$propertyInfo.Name", 0 }),
                        "Unknown Property"
                    })
                },
                { "checkInDate", new BsonDocument("$ifNull", new BsonArray { "$checkInDate", "$checkinDate" }) },
                { "checkOutDate", new BsonDocument("$ifNull", new BsonArray { "$checkOutDate", "$checkoutDate" }) },
                { "status", new BsonDocument("$ifNull", new BsonArray { "$Status", "$status", "confirmed" }) },
                { "guestId", new BsonDocument("$ifNull", new BsonArray { "$guestId", "" }) },
                
                // Guest details with fallback
                { "guestFirstName", new BsonDocument("$ifNull", new BsonArray
                    {
                        new BsonDocument("$arrayElemAt", new BsonArray { "$guestInfo.firstName", 0 }),
                        new BsonDocument("$arrayElemAt", new BsonArray { "$guestInfo.FirstName", 0 }),
                        "Unavailable"
                    })
                },
                { "guestLastName", new BsonDocument("$ifNull", new BsonArray
                    {
                        new BsonDocument("$arrayElemAt", new BsonArray { "$guestInfo.lastName", 0 }),
                        new BsonDocument("$arrayElemAt", new BsonArray { "$guestInfo.LastName", 0 }),
                        "Unavailable"
                    })
                },
                { "guestEmail", new BsonDocument("$ifNull", new BsonArray
                    {
                        new BsonDocument("$arrayElemAt", new BsonArray { "$guestInfo.email", 0 }),
                        new BsonDocument("$arrayElemAt", new BsonArray { "$guestInfo.Email", 0 }),
                        "Unavailable"
                    })
                },
                { "guestPhoneNumber", new BsonDocument("$ifNull", new BsonArray
                    {
                        new BsonDocument("$arrayElemAt", new BsonArray { "$guestInfo.phoneNumber", 0 }),
                        new BsonDocument("$arrayElemAt", new BsonArray { "$guestInfo.PhoneNumber", 0 }),
                        "Unavailable"
                    })
                },
                { "guestNationality", new BsonDocument("$ifNull", new BsonArray
                    {
                        new BsonDocument("$arrayElemAt", new BsonArray { "$guestInfo.nationality", 0 }),
                        new BsonDocument("$arrayElemAt", new BsonArray { "$guestInfo.Nationality", 0 }),
                        "Unavailable"
                    })
                }
            }));

            // Stage 5: $sort
            pipeline.Add(new BsonDocument("$sort", new BsonDocument
            {
                { "checkInDate", 1 },
                { "roomNumber", 1 }
            }));

            return pipeline;
        }

        public string GetDebugInfo()
        {
            return $"GetBookingsWithGuestsMongoQuery - UserId: {_userId}, DateRange: {_startDate:yyyy-MM-dd} to {_endDate:yyyy-MM-dd}, PropertyIds: [{string.Join(", ", _propertyIds)}]";
        }

        public string GetPipelineAsJsonString()
        {
            return BsonPipeline?.ToJson() ?? "[]";
        }
    }
}
