using MediatR;
using MongoDB.Driver;
using MongoDB.Bson;
using MyWarehouse.Application.Dashboard.DTOs;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// Query handler to get bookings with guest details for room planner
    /// </summary>
    public class GetBookingsWithGuestsQueryHandler : IRequestHandler<GetBookingsWithGuestsQuery, List<BookingWithGuestDTO>>
    {
        private readonly IMongoDatabase _database;

        public GetBookingsWithGuestsQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<List<BookingWithGuestDTO>> Handle(GetBookingsWithGuestsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var bookingsCollection = _database.GetCollection<BsonDocument>(MongoCollections.BookingsCollection);

                // 🔍 DEBUG: Log request parameters
                Console.WriteLine($"🔧 ===== BOOKINGS WITH GUESTS QUERY DEBUG =====");
                Console.WriteLine($"🔧 Request - UserId: {request.UserId}, PropertyIds: [{string.Join(", ", request.PropertyIds)}]");
                Console.WriteLine($"🔧 Date Range: {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}");

                // Create MongoDB query with guest join
                var mongoQuery = new GetBookingsWithGuestsMongoQuery(
                    request.UserId,
                    request.StartDate,
                    request.EndDate,
                    request.PropertyIds
                );

                // 🔍 DEBUG: Get query info
                var debugInfo = mongoQuery.GetDebugInfo();
                var debugPipeline = mongoQuery.GetPipelineAsJsonString();
                Console.WriteLine($"🔧 Query Info: {debugInfo}");
                Console.WriteLine($"🔧 Pipeline JSON: {debugPipeline}");

                // Execute aggregation pipeline
                var pipelineStages = mongoQuery.BsonPipeline.Select(stage => (BsonDocument)stage).ToArray();

                Console.WriteLine($"🔧 Executing aggregation with {pipelineStages.Length} stages");

                var bookingDocs = await bookingsCollection
                    .Aggregate<BsonDocument>(pipelineStages)
                    .ToListAsync(cancellationToken);

                Console.WriteLine($"✅ AFTER AGGREGATION - Returned {bookingDocs.Count} booking documents");

                if (bookingDocs.Count == 0)
                {
                    Console.WriteLine("⚠️ ===== AGGREGATION RETURNED 0 RESULTS =====");
                    Console.WriteLine("⚠️ Check if:");
                    Console.WriteLine("⚠️   1. Bookings exist in the date range");
                    Console.WriteLine("⚠️   2. PropertyIds match");
                    Console.WriteLine("⚠️   3. Guests collection exists and has matching guestIds");
                }
                else
                {
                    Console.WriteLine($"✅ Sample of first booking document: {bookingDocs[0].ToJson()}");
                }

                // Map BsonDocument to DTO
                var bookings = new List<BookingWithGuestDTO>();
                foreach (var doc in bookingDocs)
                {
                    // Helper function to safely get string
                    string GetString(BsonDocument document, string fieldName, string defaultValue = "")
                    {
                        if (!document.Contains(fieldName)) return defaultValue;
                        var value = document.GetValue(fieldName, BsonNull.Value);
                        if (value.IsBsonNull) return defaultValue;

                        // Handle both string and numeric values
                        try
                        {
                            return value.ToString();
                        }
                        catch
                        {
                            return defaultValue;
                        }
                    }

                    // Helper function to safely get int
                    int GetInt(BsonDocument document, string fieldName, int defaultValue = 0)
                    {
                        if (!document.Contains(fieldName)) return defaultValue;
                        var value = document.GetValue(fieldName, BsonNull.Value);
                        if (value.IsBsonNull) return defaultValue;

                        try
                        {
                            return value.AsInt32;
                        }
                        catch
                        {
                            return defaultValue;
                        }
                    }

                    // Helper function to safely get DateTime
                    DateTime GetDateTime(BsonDocument document, string fieldName)
                    {
                        if (!document.Contains(fieldName)) return DateTime.MinValue;
                        var value = document.GetValue(fieldName, BsonNull.Value);
                        if (value.IsBsonNull) return DateTime.MinValue;

                        try
                        {
                            return value.ToUniversalTime();
                        }
                        catch
                        {
                            return DateTime.MinValue;
                        }
                    }

                    bookings.Add(new BookingWithGuestDTO
                    {
                        Id = doc["_id"].ToString(),
                        BookingId = GetString(doc, "bookingId", doc["_id"].ToString()),
                        RoomNumber = GetString(doc, "roomNumber", ""),
                        PropertyId = GetInt(doc, "propertyId", 0),
                        PropertyName = GetString(doc, "propertyName", "Unknown Property"),
                        CheckInDate = GetDateTime(doc, "checkInDate"),
                        CheckOutDate = GetDateTime(doc, "checkOutDate"),
                        Status = GetString(doc, "status", "confirmed"),
                        GuestId = GetString(doc, "guestId", ""),
                        GuestFirstName = GetString(doc, "guestFirstName", "Unavailable"),
                        GuestLastName = GetString(doc, "guestLastName", "Unavailable"),
                        GuestEmail = GetString(doc, "guestEmail", "Unavailable"),
                        GuestPhoneNumber = GetString(doc, "guestPhoneNumber", "Unavailable"),
                        GuestNationality = GetString(doc, "guestNationality", "Unavailable")
                    });
                }

                Console.WriteLine($"✅ Mapped {bookings.Count} bookings with guest details");
                Console.WriteLine($"🔧 ===== END BOOKINGS WITH GUESTS QUERY DEBUG =====");

                return bookings;
            }
            catch (Exception ex)
            {
                // Log error and return empty list
                Console.WriteLine($"❌ ===== BOOKINGS WITH GUESTS QUERY ERROR =====");
                Console.WriteLine($"❌ Error fetching bookings: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                Console.WriteLine($"❌ ===== END ERROR =====");
                return new List<BookingWithGuestDTO>();
            }
        }
    }
}
