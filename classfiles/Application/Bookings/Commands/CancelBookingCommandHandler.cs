using MediatR;
using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Bookings.Services;
using BookingEntity = MyWarehouse.Domain.Bookings.Bookings;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Handler for canceling a booking
    /// </summary>
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, BookingEntity>
    {
        private readonly IBookingsRepository _bookingsRepository;
        private readonly IMongoDatabase _database;
        private readonly BookingActivityLogger _activityLogger;

        public CancelBookingCommandHandler(
            IBookingsRepository bookingsRepository,
            IMongoDatabase database,
            BookingActivityLogger activityLogger)
        {
            _bookingsRepository = bookingsRepository;
            _database = database;
            _activityLogger = activityLogger;
        }

        public async Task<BookingEntity> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"🚫 ===== CANCEL BOOKING COMMAND =====");
                Console.WriteLine($"📋 Booking ID: {request.BookingId}");
                Console.WriteLine($"👤 User ID: {request.UserId}");
                Console.WriteLine($"📝 Reason: {request.CancellationReason ?? "Not provided"}");

                // 1. Get the booking (query with projection to avoid _id deserialization issues)
                var bookingsCollection = _database.GetCollection<BookingEntity>(MongoCollections.BookingsCollection);
                var filter = Builders<BookingEntity>.Filter.Eq(b => b.BookingId, request.BookingId);

                // Project to exclude _id field from deserialization
                var projection = Builders<BookingEntity>.Projection.Exclude("_id");
                var booking = await bookingsCollection.Find(filter)
                    .Project<BookingEntity>(projection)
                    .FirstOrDefaultAsync(cancellationToken);

                if (booking == null)
                {
                    Console.WriteLine($"❌ Booking not found: {request.BookingId}");
                    throw new InvalidOperationException($"Booking with ID '{request.BookingId}' not found");
                }

                Console.WriteLine($"✅ Found booking:");
                Console.WriteLine($"   - Room: {booking.RoomNumber}");
                Console.WriteLine($"   - Dates: {booking.CheckInDate:yyyy-MM-dd} to {booking.CheckOutDate:yyyy-MM-dd}");
                Console.WriteLine($"   - Current Status: {(booking.IsConfirmed ? "Confirmed" : "Not Confirmed")}");

                // 2. Check if already cancelled (using isConfirmed as cancellation indicator)
                if (!booking.IsConfirmed)
                {
                    Console.WriteLine($"⚠️ Booking is already cancelled");
                    throw new InvalidOperationException($"Booking '{request.BookingId}' is already cancelled");
                }

                // 3. Update booking status to cancelled
                var update = Builders<BookingEntity>.Update
                    .Set(b => b.IsConfirmed, false) // Set to false to indicate cancelled
                    .Set(b => b.LastModified, DateTime.UtcNow);

                // Add cancellation reason to special requests if provided
                if (!string.IsNullOrWhiteSpace(request.CancellationReason))
                {
                    var cancellationNote = $"[CANCELLED] {request.CancellationReason}";
                    update = update.Push(b => b.SpecialRequests, cancellationNote);
                    Console.WriteLine($"📝 Added cancellation note to special requests");
                }

                var updateResult = await bookingsCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

                if (updateResult.ModifiedCount == 0)
                {
                    Console.WriteLine($"❌ Failed to cancel booking in database");
                    throw new InvalidOperationException("Failed to cancel booking");
                }

                // Update local object
                booking.IsConfirmed = false;
                booking.LastModified = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(request.CancellationReason))
                {
                    booking.SpecialRequests ??= new List<string>();
                    booking.SpecialRequests.Add($"[CANCELLED] {request.CancellationReason}");
                }

                Console.WriteLine($"✅ Booking cancelled successfully");
                Console.WriteLine($"✅ Modified {updateResult.ModifiedCount} document(s)");

                // Log activity
                await _activityLogger.LogBookingCancellation(
                    bookingId: request.BookingId,
                    roomNumber: booking.RoomNumber,
                    checkInDate: booking.CheckInDate,
                    checkOutDate: booking.CheckOutDate,
                    cancellationReason: request.CancellationReason,
                    userId: request.UserId,
                    username: "User",
                    isSuccess: true
                );

                // TODO: Send cancellation notification to guest
                // await _emailService.SendCancellationEmailAsync(booking.GuestId, booking.BookingId);

                return booking;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ===== CANCEL BOOKING ERROR =====");
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");

                // Log failed operation
                await _activityLogger.LogFailedOperation(
                    operationType: "BookingCancellation",
                    bookingId: request.BookingId,
                    errorMessage: ex.Message,
                    stackTrace: ex.StackTrace,
                    userId: request.UserId,
                    username: "User"
                );

                throw;
            }
        }
    }
}
