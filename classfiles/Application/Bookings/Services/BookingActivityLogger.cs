using MongoDB.Driver;
using MyWarehouse.Domain.UserActivity;

namespace MyWarehouse.Application.Bookings.Services
{
    /// <summary>
    /// Service for logging booking-related activities to UserActivityLogs collection
    /// Tracks moves, date changes, cancellations, and other booking operations
    /// </summary>
    public class BookingActivityLogger
    {
        private readonly IMongoDatabase _database;

        public BookingActivityLogger(IMongoDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Logs a booking move operation
        /// </summary>
        public async Task LogBookingMove(
            string bookingId,
            string oldRoomNumber,
            string newRoomNumber,
            int userId,
            string username = "Unknown User",
            bool isSuccess = true,
            string? errorMessage = null)
        {
            var activityLog = new UserActivityLog
            {
                UserId = userId,
                Username = username,
                ActivityType = ActivityType.Update,
                EntityType = "Booking",
                EntityId = null, // BookingId is string, not int
                Action = $"Moved booking {bookingId}",
                DisplayMessage = $"{username} moved booking {bookingId} from Room {oldRoomNumber} to Room {newRoomNumber}",
                Metadata = new Dictionary<string, object>
                {
                    { "BookingId", bookingId },
                    { "OldRoomNumber", oldRoomNumber },
                    { "NewRoomNumber", newRoomNumber },
                    { "OperationType", "BookingMove" }
                },
                Timestamp = DateTime.UtcNow,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage
            };

            await SaveActivityLog(activityLog);
        }

        /// <summary>
        /// Logs booking date update operation
        /// </summary>
        public async Task LogBookingDateUpdate(
            string bookingId,
            DateTime oldCheckIn,
            DateTime oldCheckOut,
            DateTime newCheckIn,
            DateTime newCheckOut,
            decimal oldPrice,
            decimal newPrice,
            int userId,
            string username = "Unknown User",
            bool isSuccess = true,
            string? errorMessage = null)
        {
            var activityLog = new UserActivityLog
            {
                UserId = userId,
                Username = username,
                ActivityType = ActivityType.Update,
                EntityType = "Booking",
                EntityId = null,
                Action = $"Updated dates for booking {bookingId}",
                DisplayMessage = $"{username} changed booking {bookingId} dates from {oldCheckIn:MMM dd} - {oldCheckOut:MMM dd} to {newCheckIn:MMM dd} - {newCheckOut:MMM dd}",
                Metadata = new Dictionary<string, object>
                {
                    { "BookingId", bookingId },
                    { "OldCheckInDate", oldCheckIn.ToString("yyyy-MM-dd") },
                    { "OldCheckOutDate", oldCheckOut.ToString("yyyy-MM-dd") },
                    { "NewCheckInDate", newCheckIn.ToString("yyyy-MM-dd") },
                    { "NewCheckOutDate", newCheckOut.ToString("yyyy-MM-dd") },
                    { "OldPrice", oldPrice },
                    { "NewPrice", newPrice },
                    { "PriceDifference", newPrice - oldPrice },
                    { "OperationType", "BookingDateUpdate" }
                },
                Timestamp = DateTime.UtcNow,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage
            };

            await SaveActivityLog(activityLog);
        }

        /// <summary>
        /// Logs booking cancellation
        /// </summary>
        public async Task LogBookingCancellation(
            string bookingId,
            string roomNumber,
            DateTime checkInDate,
            DateTime checkOutDate,
            string? cancellationReason,
            int userId,
            string username = "Unknown User",
            bool isSuccess = true,
            string? errorMessage = null)
        {
            var activityLog = new UserActivityLog
            {
                UserId = userId,
                Username = username,
                ActivityType = ActivityType.Delete, // Cancellation is a soft delete
                EntityType = "Booking",
                EntityId = null,
                Action = $"Cancelled booking {bookingId}",
                DisplayMessage = $"{username} cancelled booking {bookingId} for Room {roomNumber} ({checkInDate:MMM dd} - {checkOutDate:MMM dd})",
                Metadata = new Dictionary<string, object>
                {
                    { "BookingId", bookingId },
                    { "RoomNumber", roomNumber },
                    { "CheckInDate", checkInDate.ToString("yyyy-MM-dd") },
                    { "CheckOutDate", checkOutDate.ToString("yyyy-MM-dd") },
                    { "CancellationReason", cancellationReason ?? "No reason provided" },
                    { "OperationType", "BookingCancellation" }
                },
                Timestamp = DateTime.UtcNow,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage
            };

            await SaveActivityLog(activityLog);
        }

        /// <summary>
        /// Logs a failed booking operation (for error tracking)
        /// </summary>
        public async Task LogFailedOperation(
            string operationType,
            string bookingId,
            string errorMessage,
            string? stackTrace,
            int userId,
            string username = "Unknown User")
        {
            var activityLog = new UserActivityLog
            {
                UserId = userId,
                Username = username,
                ActivityType = ActivityType.Update,
                EntityType = "Booking",
                EntityId = null,
                Action = $"Failed {operationType}",
                DisplayMessage = $"{username} attempted {operationType} on booking {bookingId} but it failed",
                Metadata = new Dictionary<string, object>
                {
                    { "BookingId", bookingId },
                    { "OperationType", operationType },
                    { "FailureReason", errorMessage }
                },
                Timestamp = DateTime.UtcNow,
                IsSuccess = false,
                ErrorMessage = errorMessage,
                StackTrace = stackTrace
            };

            await SaveActivityLog(activityLog);
        }

        /// <summary>
        /// Saves the activity log to MongoDB
        /// </summary>
        private async Task SaveActivityLog(UserActivityLog activityLog)
        {
            try
            {
                var collection = _database.GetCollection<UserActivityLog>(MongoCollections.UserActivityLogsCollection);
                
                // Get next ID using KeyCounter
                var keyCounterCollection = _database.GetCollection<MongoDB.Bson.BsonDocument>(MongoCollections.KeyCounterCollection);
                var filter = Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", "UserActivityLog");
                var update = Builders<MongoDB.Bson.BsonDocument>.Update.Inc("seq", 1);
                var options = new FindOneAndUpdateOptions<MongoDB.Bson.BsonDocument>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                };
                var result = await keyCounterCollection.FindOneAndUpdateAsync(filter, update, options);
                activityLog.Id = result["seq"].AsInt32;

                await collection.InsertOneAsync(activityLog);
                Console.WriteLine($"✅ Activity logged: {activityLog.Action}");
            }
            catch (Exception ex)
            {
                // Don't throw - logging failures should not break the main operation
                Console.WriteLine($"⚠️ Failed to log activity: {ex.Message}");
            }
        }
    }
}
