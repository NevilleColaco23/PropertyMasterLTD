using MediatR;
using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Bookings.Services;
using BookingEntity = MyWarehouse.Domain.Bookings.Bookings;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Handler for updating booking check-in and check-out dates
    /// </summary>
    public class UpdateBookingDatesCommandHandler : IRequestHandler<UpdateBookingDatesCommand, BookingEntity>
    {
        private readonly IBookingsRepository _bookingsRepository;
        private readonly IMongoDatabase _database;
        private readonly BookingActivityLogger _activityLogger;

        public UpdateBookingDatesCommandHandler(
            IBookingsRepository bookingsRepository,
            IMongoDatabase database,
            BookingActivityLogger activityLogger)
        {
            _bookingsRepository = bookingsRepository;
            _database = database;
            _activityLogger = activityLogger;
        }

        public async Task<BookingEntity> Handle(UpdateBookingDatesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"📏 ===== UPDATE BOOKING DATES COMMAND =====");
                Console.WriteLine($"📋 Booking ID: {request.BookingId}");
                Console.WriteLine($"📅 New Check-In: {request.NewCheckInDate:yyyy-MM-dd}");
                Console.WriteLine($"📅 New Check-Out: {request.NewCheckOutDate:yyyy-MM-dd}");
                Console.WriteLine($"👤 User ID: {request.UserId}");

                // 1. Validate dates
                if (request.NewCheckInDate >= request.NewCheckOutDate)
                {
                    Console.WriteLine($"❌ Invalid dates: Check-in must be before check-out");
                    throw new InvalidOperationException("Check-in date must be before check-out date");
                }

                // 2. Get the booking (query with projection to avoid _id deserialization issues)
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

                Console.WriteLine($"✅ Found booking - Current dates: {booking.CheckInDate:yyyy-MM-dd} to {booking.CheckOutDate:yyyy-MM-dd}");
                Console.WriteLine($"✅ Room: {booking.RoomNumber}");

                // 3. Check for conflicts with other bookings in the same room
                var conflictFilter = Builders<BookingEntity>.Filter.And(
                    Builders<BookingEntity>.Filter.Eq(b => b.RoomNumber, booking.RoomNumber),
                    Builders<BookingEntity>.Filter.Ne(b => b.BookingId, request.BookingId), // Exclude current booking
                    Builders<BookingEntity>.Filter.Or(
                        // New dates overlap with existing booking
                        Builders<BookingEntity>.Filter.And(
                            Builders<BookingEntity>.Filter.Lte(b => b.CheckInDate, request.NewCheckOutDate),
                            Builders<BookingEntity>.Filter.Gte(b => b.CheckOutDate, request.NewCheckInDate)
                        )
                    )
                );

                var conflictingBooking = await bookingsCollection.Find(conflictFilter).FirstOrDefaultAsync(cancellationToken);

                if (conflictingBooking != null)
                {
                    Console.WriteLine($"❌ Date conflict detected with booking: {conflictingBooking.BookingId}");
                    Console.WriteLine($"❌ Conflicting dates: {conflictingBooking.CheckInDate:yyyy-MM-dd} to {conflictingBooking.CheckOutDate:yyyy-MM-dd}");
                    throw new InvalidOperationException($"The new dates conflict with another booking (ID: {conflictingBooking.BookingId})");
                }

                Console.WriteLine($"✅ No conflicts found - Dates are available");

                // 4. Calculate new price based on new duration (optional - implement if price calculation logic exists)
                var oldCheckInDate = booking.CheckInDate;
                var oldCheckOutDate = booking.CheckOutDate;
                var oldTotalPrice = booking.TotalPrice;
                var oldDuration = (booking.CheckOutDate - booking.CheckInDate).Days;
                var newDuration = (request.NewCheckOutDate - request.NewCheckInDate).Days;
                var pricePerNight = booking.TotalPrice / oldDuration;
                var newTotalPrice = pricePerNight * newDuration;

                Console.WriteLine($"💰 Price Calculation:");
                Console.WriteLine($"   - Old Duration: {oldDuration} nights, Price: ${booking.TotalPrice:F2}");
                Console.WriteLine($"   - New Duration: {newDuration} nights, Price: ${newTotalPrice:F2}");

                // 5. Update the booking
                var update = Builders<BookingEntity>.Update
                    .Set(b => b.CheckInDate, request.NewCheckInDate)
                    .Set(b => b.CheckOutDate, request.NewCheckOutDate)
                    .Set(b => b.TotalPrice, newTotalPrice)
                    .Set(b => b.LastModified, DateTime.UtcNow);

                var updateResult = await bookingsCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

                if (updateResult.ModifiedCount == 0)
                {
                    Console.WriteLine($"❌ Failed to update booking in database");
                    throw new InvalidOperationException("Failed to update booking dates");
                }

                // Update local object
                booking.CheckInDate = request.NewCheckInDate;
                booking.CheckOutDate = request.NewCheckOutDate;
                booking.TotalPrice = newTotalPrice;
                booking.LastModified = DateTime.UtcNow;

                var action = newDuration > oldDuration ? "extended" : "shortened";
                Console.WriteLine($"✅ Booking dates {action} successfully");
                Console.WriteLine($"✅ Modified {updateResult.ModifiedCount} document(s)");

                // Log activity
                await _activityLogger.LogBookingDateUpdate(
                    bookingId: request.BookingId,
                    oldCheckIn: oldCheckInDate,
                    oldCheckOut: oldCheckOutDate,
                    newCheckIn: request.NewCheckInDate,
                    newCheckOut: request.NewCheckOutDate,
                    oldPrice: (decimal)oldTotalPrice,
                    newPrice: (decimal)newTotalPrice,
                    userId: request.UserId,
                    username: "User",
                    isSuccess: true
                );

                return booking;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ===== UPDATE BOOKING DATES ERROR =====");
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");

                // Log failed operation
                await _activityLogger.LogFailedOperation(
                    operationType: "BookingDateUpdate",
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
