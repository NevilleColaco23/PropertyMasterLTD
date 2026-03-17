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

                // Filter by check-in date within the requested range
                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Gte("checkInDate", request.StartDate),
                    Builders<BsonDocument>.Filter.Lte("checkInDate", request.EndDate),
                    Builders<BsonDocument>.Filter.Eq("Status", "Active") // Only active bookings
                );

                var bookings = await bookingsCollection
                    .Find(filter)
                    .SortBy(b => b["checkInDate"])
                    .ToListAsync(cancellationToken);

                foreach (var booking in bookings)
                {
                    var checkInDate = booking.GetValue("checkInDate", DateTime.UtcNow).ToUniversalTime();
                    var checkOutDate = booking.GetValue("checkOutDate", DateTime.UtcNow).ToUniversalTime();
                    var roomNumber = booking.GetValue("roomNumber", "N/A").AsString;
                    var bookingId = booking.GetValue("bookingId", "N/A").AsString;
                    var numberOfGuests = booking.GetValue("numberOfGuests", 1).ToInt32();
                    var isConfirmed = booking.GetValue("isConfirmed", false).ToBoolean();
                    var totalPrice = booking.GetValue("totalPrice", 0.0).ToDouble();

                    // Create check-in event
                    events.Add(new CalendarEventResponse
                    {
                        Id = $"{booking["_id"]}-checkin",
                        Title = $"Check-in: Room {roomNumber}",
                        Start = checkInDate,
                        End = checkInDate.AddHours(1), // Check-in window
                        Color = isConfirmed ? "#4caf50" : "#ff9800",
                        Type = "check-in",
                        Description = $"Booking {bookingId} - {numberOfGuests} guest(s) - ${totalPrice:F2}"
                    });

                    // Create check-out event
                    events.Add(new CalendarEventResponse
                    {
                        Id = $"{booking["_id"]}-checkout",
                        Title = $"Check-out: Room {roomNumber}",
                        Start = checkOutDate,
                        End = checkOutDate.AddHours(1), // Check-out window
                        Color = "#2196f3",
                        Type = "check-out",
                        Description = $"Booking {bookingId} - Room {roomNumber}"
                    });

                    // Create booking span event (optional - shows entire booking period)
                    events.Add(new CalendarEventResponse
                    {
                        Id = $"{booking["_id"]}-stay",
                        Title = $"Occupied: Room {roomNumber}",
                        Start = checkInDate,
                        End = checkOutDate,
                        Color = "#e0e0e0",
                        Type = "booking",
                        Description = $"Booking {bookingId} - {numberOfGuests} guest(s)"
                    });
                }

                Console.WriteLine($"📅 Calendar widget: Found {bookings.Count} bookings, created {events.Count} events");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Calendar widget error: {ex.Message}");
                // If Bookings collection doesn't exist or query fails, return empty list
                events = new List<CalendarEventResponse>();
            }

            return events.OrderBy(e => e.Start).ToList();
        }
    }

    /// <summary>
    /// Handler for GetBookingTrendsQuery
    /// </summary>
    public class GetBookingTrendsQueryHandler : IRequestHandler<GetBookingTrendsQuery, BookingTrendsResponse>
    {
        private readonly IMongoDatabase _database;

        public GetBookingTrendsQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<BookingTrendsResponse> Handle(GetBookingTrendsQuery request, CancellationToken cancellationToken)
        {
            var response = new BookingTrendsResponse
            {
                Title = "Booking Trends",
                ChartType = "line"
            };

            try
            {
                var bookingsCollection = _database.GetCollection<BsonDocument>("Bookings");

                // Calculate date range
                var endDate = DateTime.UtcNow.Date.AddDays(1); // Include today
                var startDate = endDate.AddDays(-request.DaysBack);

                // Get bookings in date range
                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Gte("CreatedAt", startDate),
                    Builders<BsonDocument>.Filter.Lt("CreatedAt", endDate),
                    Builders<BsonDocument>.Filter.Eq("Status", "Active")
                );

                var bookings = await bookingsCollection
                    .Find(filter)
                    .ToListAsync(cancellationToken);

                // Group bookings by date
                var bookingsByDate = new Dictionary<DateTime, int>();

                // Initialize all dates in range with 0
                for (var date = startDate; date < endDate; date = date.AddDays(1))
                {
                    bookingsByDate[date] = 0;
                }

                // Count bookings per date
                foreach (var booking in bookings)
                {
                    var createdAt = booking.GetValue("CreatedAt", DateTime.UtcNow).ToUniversalTime().Date;
                    if (bookingsByDate.ContainsKey(createdAt))
                    {
                        bookingsByDate[createdAt]++;
                    }
                }

                // Format based on groupBy parameter
                if (request.GroupBy == "week")
                {
                    response = GroupByWeek(bookingsByDate, startDate, endDate);
                }
                else if (request.GroupBy == "month")
                {
                    response = GroupByMonth(bookingsByDate, startDate, endDate);
                }
                else // default: day
                {
                    // Generate labels and data
                    var sortedDates = bookingsByDate.Keys.OrderBy(d => d).ToList();

                    response.Labels = sortedDates.Select(d => d.ToString("MMM dd")).ToList();

                    response.Datasets = new List<ChartDataset>
                    {
                        new ChartDataset
                        {
                            Label = "Bookings",
                            Data = sortedDates.Select(d => bookingsByDate[d]).ToList(),
                            BackgroundColor = "#1976d2",
                            BorderColor = "#1976d2",
                            BorderWidth = 2
                        }
                    };
                }

                Console.WriteLine($"📊 Chart widget: Generated trends for {request.DaysBack} days, total bookings: {bookings.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Chart widget error: {ex.Message}");

                // Return sample data if error
                response.Labels = new List<string> { "No Data" };
                response.Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Bookings",
                        Data = new List<int> { 0 },
                        BackgroundColor = "#cccccc",
                        BorderColor = "#cccccc"
                    }
                };
            }

            return response;
        }

        private BookingTrendsResponse GroupByWeek(Dictionary<DateTime, int> bookingsByDate, DateTime startDate, DateTime endDate)
        {
            var weeklyData = new Dictionary<string, int>();

            foreach (var kvp in bookingsByDate)
            {
                var weekStart = kvp.Key.AddDays(-(int)kvp.Key.DayOfWeek);
                var weekLabel = $"Week of {weekStart:MMM dd}";

                if (!weeklyData.ContainsKey(weekLabel))
                    weeklyData[weekLabel] = 0;

                weeklyData[weekLabel] += kvp.Value;
            }

            return new BookingTrendsResponse
            {
                Title = "Weekly Booking Trends",
                ChartType = "bar",
                Labels = weeklyData.Keys.ToList(),
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Bookings per Week",
                        Data = weeklyData.Values.ToList(),
                        BackgroundColor = "#4caf50",
                        BorderColor = "#4caf50",
                        BorderWidth = 2
                    }
                }
            };
        }

        private BookingTrendsResponse GroupByMonth(Dictionary<DateTime, int> bookingsByDate, DateTime startDate, DateTime endDate)
        {
            var monthlyData = new Dictionary<string, int>();

            foreach (var kvp in bookingsByDate)
            {
                var monthLabel = kvp.Key.ToString("MMM yyyy");

                if (!monthlyData.ContainsKey(monthLabel))
                    monthlyData[monthLabel] = 0;

                monthlyData[monthLabel] += kvp.Value;
            }

            return new BookingTrendsResponse
            {
                Title = "Monthly Booking Trends",
                ChartType = "bar",
                Labels = monthlyData.Keys.ToList(),
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset
                    {
                        Label = "Bookings per Month",
                        Data = monthlyData.Values.ToList(),
                        BackgroundColor = "#ff9800",
                        BorderColor = "#ff9800",
                        BorderWidth = 2
                    }
                }
            };
        }
    }
}
