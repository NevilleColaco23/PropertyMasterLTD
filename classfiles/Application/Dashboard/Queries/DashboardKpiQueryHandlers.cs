using MongoDB.Driver;
using MongoDB.Bson;
using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// Handler for GetTotalPropertiesQuery
    /// </summary>
    public class GetTotalPropertiesQueryHandler : IRequestHandler<GetTotalPropertiesQuery, KpiValueResponse>
    {
        private readonly IMongoDatabase _database;

        public GetTotalPropertiesQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<KpiValueResponse> Handle(GetTotalPropertiesQuery request, CancellationToken cancellationToken)
        {
            var collection = _database.GetCollection<BsonDocument>(MongoCollections.PropertyCollection);

            // Count total properties (not deleted)
            var totalProperties = await collection.CountDocumentsAsync(
                Builders<BsonDocument>.Filter.Ne("IsDeleted", true),
                cancellationToken: cancellationToken
            );

            // Calculate trend (compare with last month)
            var lastMonthStart = DateTime.UtcNow.AddMonths(-1);
            var propertiesLastMonth = await collection.CountDocumentsAsync(
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Ne("IsDeleted", true),
                    Builders<BsonDocument>.Filter.Lt("CreatedAt", lastMonthStart)
                ),
                cancellationToken: cancellationToken
            );

            var propertiesAddedThisMonth = totalProperties - propertiesLastMonth;
            var trendPercentage = propertiesLastMonth > 0 
                ? (double)propertiesAddedThisMonth / propertiesLastMonth * 100 
                : 0;

            return new KpiValueResponse
            {
                WidgetId = "total-properties",
                Value = (int)totalProperties,
                ShowTrend = true,
                TrendValue = Math.Round(Math.Abs(trendPercentage), 1),
                TrendDirection = trendPercentage >= 0 ? "up" : "down",
                CalculatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Handler for GetTotalRoomsQuery
    /// </summary>
    public class GetTotalRoomsQueryHandler : IRequestHandler<GetTotalRoomsQuery, KpiValueResponse>
    {
        private readonly IMongoDatabase _database;

        public GetTotalRoomsQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<KpiValueResponse> Handle(GetTotalRoomsQuery request, CancellationToken cancellationToken)
        {
            var collection = _database.GetCollection<BsonDocument>(MongoCollections.PropertyCollection);

            // Get all active properties
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Ne("IsDeleted", true),
                Builders<BsonDocument>.Filter.Eq("Active", true)
            );

            var properties = await collection.Find(filter).ToListAsync(cancellationToken);

            // Sum up all active rooms across properties
            var totalRooms = 0;
            foreach (var property in properties)
            {
                if (property.Contains("Rooms") && property["Rooms"].IsBsonArray)
                {
                    var rooms = property["Rooms"].AsBsonArray;
                    totalRooms += rooms.Count(r => r.AsBsonDocument.GetValue("Active", false).AsBoolean);
                }
            }

            return new KpiValueResponse
            {
                WidgetId = "total-rooms",
                Value = totalRooms,
                ShowTrend = false,
                CalculatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Handler for GetBookingsTodayQuery
    /// </summary>
    public class GetBookingsTodayQueryHandler : IRequestHandler<GetBookingsTodayQuery, KpiValueResponse>
    {
        private readonly IMongoDatabase _database;

        public GetBookingsTodayQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<KpiValueResponse> Handle(GetBookingsTodayQuery request, CancellationToken cancellationToken)
        {
            var collection = _database.GetCollection<BsonDocument>("Bookings");

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Count bookings created today
            var bookingsToday = await collection.CountDocumentsAsync(
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Gte("CreatedAt", today),
                    Builders<BsonDocument>.Filter.Lt("CreatedAt", tomorrow)
                ),
                cancellationToken: cancellationToken
            );

            // Calculate trend (compare with yesterday)
            var yesterday = today.AddDays(-1);
            var bookingsYesterday = await collection.CountDocumentsAsync(
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Gte("CreatedAt", yesterday),
                    Builders<BsonDocument>.Filter.Lt("CreatedAt", today)
                ),
                cancellationToken: cancellationToken
            );

            var trendPercentage = bookingsYesterday > 0 
                ? (double)(bookingsToday - bookingsYesterday) / bookingsYesterday * 100 
                : 0;

            return new KpiValueResponse
            {
                WidgetId = "bookings-today",
                Value = (int)bookingsToday,
                ShowTrend = true,
                TrendValue = Math.Round(Math.Abs(trendPercentage), 1),
                TrendDirection = trendPercentage >= 0 ? "up" : "down",
                CalculatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Handler for GetOccupancyRateQuery
    /// </summary>
    public class GetOccupancyRateQueryHandler : IRequestHandler<GetOccupancyRateQuery, KpiValueResponse>
    {
        private readonly IMongoDatabase _database;

        public GetOccupancyRateQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<KpiValueResponse> Handle(GetOccupancyRateQuery request, CancellationToken cancellationToken)
        {
            // Get total rooms
            var propertiesCollection = _database.GetCollection<BsonDocument>(MongoCollections.PropertyCollection);

            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Ne("IsDeleted", true),
                Builders<BsonDocument>.Filter.Eq("Active", true)
            );

            var properties = await propertiesCollection.Find(filter).ToListAsync(cancellationToken);

            // Sum up all active rooms
            var totalRooms = 0;
            foreach (var property in properties)
            {
                if (property.Contains("Rooms") && property["Rooms"].IsBsonArray)
                {
                    var rooms = property["Rooms"].AsBsonArray;
                    totalRooms += rooms.Count(r => r.AsBsonDocument.GetValue("Active", false).AsBoolean);
                }
            }

            if (totalRooms == 0)
            {
                return new KpiValueResponse
                {
                    WidgetId = "occupancy-rate",
                    Value = "0%",
                    ShowTrend = false,
                    CalculatedAt = DateTime.UtcNow
                };
            }

            // Count occupied rooms (active bookings for today)
            var bookingsCollection = _database.GetCollection<BsonDocument>("Bookings");
            var today = DateTime.UtcNow.Date;

            var occupiedRooms = await bookingsCollection.CountDocumentsAsync(
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Lte("CheckInDate", today),
                    Builders<BsonDocument>.Filter.Gte("CheckOutDate", today),
                    Builders<BsonDocument>.Filter.Eq("Status", "Active")
                ),
                cancellationToken: cancellationToken
            );

            var occupancyRate = (double)occupiedRooms / totalRooms * 100;

            // Calculate trend (compare with last week)
            var lastWeek = today.AddDays(-7);
            var occupiedRoomsLastWeek = await bookingsCollection.CountDocumentsAsync(
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Lte("CheckInDate", lastWeek),
                    Builders<BsonDocument>.Filter.Gte("CheckOutDate", lastWeek),
                    Builders<BsonDocument>.Filter.Eq("Status", "Active")
                ),
                cancellationToken: cancellationToken
            );

            var occupancyRateLastWeek = (double)occupiedRoomsLastWeek / totalRooms * 100;
            var trendPercentage = occupancyRate - occupancyRateLastWeek;

            return new KpiValueResponse
            {
                WidgetId = "occupancy-rate",
                Value = $"{Math.Round(occupancyRate, 1)}%",
                ShowTrend = true,
                TrendValue = Math.Round(Math.Abs(trendPercentage), 1),
                TrendDirection = trendPercentage >= 0 ? "up" : "down",
                CalculatedAt = DateTime.UtcNow
            };
        }
    }
}
