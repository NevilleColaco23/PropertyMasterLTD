using MongoDB.Driver;
using MongoDB.Bson;
using MyWarehouse.Application.Common.Dependencies.DataAccess;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// Handler for GetRecentActivityQuery
    /// </summary>
    public class GetRecentActivityQueryHandler : IRequestHandler<GetRecentActivityQuery, List<ActivityItemResponse>>
    {
        private readonly IMongoDatabase _database;

        public GetRecentActivityQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<List<ActivityItemResponse>> Handle(GetRecentActivityQuery request, CancellationToken cancellationToken)
        {
            var activities = new List<ActivityItemResponse>();

            // Assuming you have an ActivityLog collection
            // If not, we'll aggregate from various collections
            var activityCollection = _database.GetCollection<BsonDocument>("ActivityLog");

            try
            {
                // Try to get from ActivityLog collection
                var activityDocs = await activityCollection
                    .Find(Builders<BsonDocument>.Filter.Eq("UserId", request.UserId))
                    .SortByDescending(a => a["Timestamp"])
                    .Limit(request.Limit)
                    .ToListAsync(cancellationToken);

                foreach (var doc in activityDocs)
                {
                    activities.Add(new ActivityItemResponse
                    {
                        Id = doc["_id"].ToString() ?? Guid.NewGuid().ToString(),
                        Icon = doc.GetValue("Icon", "circle").AsString,
                        IconColor = doc.GetValue("IconColor", "#1976d2").AsString,
                        Title = doc.GetValue("Title", "Activity").AsString,
                        Subtitle = doc.GetValue("Subtitle", "").AsString,
                        Timestamp = doc.GetValue("Timestamp", DateTime.UtcNow).ToUniversalTime(),
                        Metadata = doc.GetValue("Metadata", "").AsString,
                        ActivityType = doc.GetValue("ActivityType", "general").AsString
                    });
                }
            }
            catch
            {
                // If ActivityLog doesn't exist, create sample activities from recent changes
                activities = await GetActivitiesFromRecentChanges(request.UserId, request.Limit, cancellationToken);
            }

            return activities;
        }

        private async Task<List<ActivityItemResponse>> GetActivitiesFromRecentChanges(int userId, int limit, CancellationToken cancellationToken)
        {
            var activities = new List<ActivityItemResponse>();

            // Get recent property changes
            var propertiesCollection = _database.GetCollection<BsonDocument>(MongoCollections.PropertyCollection);
            var recentProperties = await propertiesCollection
                .Find(Builders<BsonDocument>.Filter.Eq("UserId", userId))
                .SortByDescending(p => p["UpdatedAt"])
                .Limit(limit / 2)
                .ToListAsync(cancellationToken);

            foreach (var prop in recentProperties)
            {
                activities.Add(new ActivityItemResponse
                {
                    Id = prop["_id"].ToString() ?? Guid.NewGuid().ToString(),
                    Icon = "edit",
                    IconColor = "#ff9800",
                    Title = "Property updated",
                    Subtitle = $"{prop.GetValue("PropertyName", "Property")} - Details modified",
                    Timestamp = prop.GetValue("UpdatedAt", DateTime.UtcNow).ToUniversalTime(),
                    Metadata = "Update",
                    ActivityType = "property_update"
                });
            }

            // Get recent bookings if collection exists
            try
            {
                var bookingsCollection = _database.GetCollection<BsonDocument>("Bookings");
                var recentBookings = await bookingsCollection
                    .Find(Builders<BsonDocument>.Filter.Empty)
                    .SortByDescending(b => b["CreatedAt"])
                    .Limit(limit / 2)
                    .ToListAsync(cancellationToken);

                foreach (var booking in recentBookings)
                {
                    var status = booking.GetValue("Status", "").AsString;
                    string icon, iconColor, title, metadata;

                    switch (status.ToLower())
                    {
                        case "confirmed":
                            icon = "check_circle";
                            iconColor = "#4caf50";
                            title = "New booking confirmed";
                            metadata = "Booking";
                            break;
                        case "cancelled":
                            icon = "event_busy";
                            iconColor = "#f44336";
                            title = "Booking cancelled";
                            metadata = "Cancellation";
                            break;
                        default:
                            icon = "event";
                            iconColor = "#2196f3";
                            title = "Booking created";
                            metadata = "Booking";
                            break;
                    }

                    activities.Add(new ActivityItemResponse
                    {
                        Id = booking["_id"].ToString() ?? Guid.NewGuid().ToString(),
                        Icon = icon,
                        IconColor = iconColor,
                        Title = title,
                        Subtitle = $"Room {booking.GetValue("RoomNumber", "N/A")} - {booking.GetValue("GuestName", "Guest")}",
                        Timestamp = booking.GetValue("CreatedAt", DateTime.UtcNow).ToUniversalTime(),
                        Metadata = metadata,
                        ActivityType = "booking"
                    });
                }
            }
            catch
            {
                // Bookings collection doesn't exist, skip
            }

            // Sort by timestamp and take top items
            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// Handler for GetCalendarEventsQuery
    /// </summary>
    public class GetCalendarEventsQueryHandler : IRequestHandler<GetCalendarEventsQuery, List<CalendarEventResponse>>
    {
        private readonly IMongoDatabase _database;

        public GetCalendarEventsQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<List<CalendarEventResponse>> Handle(GetCalendarEventsQuery request, CancellationToken cancellationToken)
        {
            var events = new List<CalendarEventResponse>();

            try
            {
                // Get bookings within date range
                var bookingsCollection = _database.GetCollection<BsonDocument>("Bookings");
                
                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Gte("CheckInDate", request.StartDate),
                    Builders<BsonDocument>.Filter.Lte("CheckInDate", request.EndDate)
                );

                var bookings = await bookingsCollection
                    .Find(filter)
                    .ToListAsync(cancellationToken);

                foreach (var booking in bookings)
                {
                    var checkInDate = booking.GetValue("CheckInDate", DateTime.UtcNow).ToUniversalTime();
                    var checkOutDate = booking.GetValue("CheckOutDate", DateTime.UtcNow).ToUniversalTime();
                    var guestName = booking.GetValue("GuestName", "Guest").AsString;
                    var roomNumber = booking.GetValue("RoomNumber", "N/A").AsString;
                    var status = booking.GetValue("Status", "pending").AsString.ToLower();

                    // Check-in event
                    events.Add(new CalendarEventResponse
                    {
                        Id = $"{booking["_id"]}-checkin",
                        Title = $"Check-in: {guestName}",
                        Start = checkInDate,
                        Color = status == "confirmed" ? "#4caf50" : "#ff9800",
                        Type = "check-in",
                        Description = $"Room {roomNumber}"
                    });

                    // Check-out event
                    events.Add(new CalendarEventResponse
                    {
                        Id = $"{booking["_id"]}-checkout",
                        Title = $"Check-out: {guestName}",
                        Start = checkOutDate,
                        Color = "#f44336",
                        Type = "check-out",
                        Description = $"Room {roomNumber}"
                    });
                }
            }
            catch
            {
                // If Bookings collection doesn't exist, return empty list
                // Or create sample events
                events = GetSampleCalendarEvents(request.StartDate, request.EndDate);
            }

            return events.OrderBy(e => e.Start).ToList();
        }

        private List<CalendarEventResponse> GetSampleCalendarEvents(DateTime startDate, DateTime endDate)
        {
            // Return some sample events if no real data exists
            var today = DateTime.UtcNow.Date;
            
            return new List<CalendarEventResponse>
            {
                new CalendarEventResponse
                {
                    Id = "sample-1",
                    Title = "Sample Check-in",
                    Start = today,
                    Color = "#4caf50",
                    Type = "check-in",
                    Description = "Sample booking"
                },
                new CalendarEventResponse
                {
                    Id = "sample-2",
                    Title = "Sample Check-out",
                    Start = today.AddDays(1),
                    Color = "#f44336",
                    Type = "check-out",
                    Description = "Sample booking"
                }
            };
        }
    }
}
