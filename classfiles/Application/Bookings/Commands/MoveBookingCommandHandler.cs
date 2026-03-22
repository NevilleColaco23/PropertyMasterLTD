using MediatR;
using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Bookings.Services;
using BookingEntity = MyWarehouse.Domain.Bookings.Bookings;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Handler for moving a booking to a different room
    /// </summary>
    public class MoveBookingCommandHandler : IRequestHandler<MoveBookingCommand, BookingEntity>
    {
        private readonly IBookingsRepository _bookingsRepository;
        private readonly IMongoDatabase _database;
        private readonly BookingActivityLogger _activityLogger;

        public MoveBookingCommandHandler(
            IBookingsRepository bookingsRepository,
            IMongoDatabase database,
            BookingActivityLogger activityLogger)
        {
            _bookingsRepository = bookingsRepository;
            _database = database;
            _activityLogger = activityLogger;
        }

        public async Task<BookingEntity> Handle(MoveBookingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"🚀 ===== MOVE BOOKING COMMAND =====");
                Console.WriteLine($"📋 Booking ID: {request.BookingId}");
                Console.WriteLine($"🏠 New Room: {request.NewRoomNumber}");
                Console.WriteLine($"👤 User ID: {request.UserId}");

                // 1. Get the booking (query as BsonDocument to avoid _id deserialization issues)
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

                Console.WriteLine($"✅ Found booking - Current Room: {booking.RoomNumber}");

                // 2. Validate new room exists and is available
                var roomsCollection = _database.GetCollection<MongoDB.Bson.BsonDocument>(MongoCollections.RoomCollection);
                var roomFilter = Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("RoomCode", request.NewRoomNumber);
                var room = await roomsCollection.Find(roomFilter).FirstOrDefaultAsync(cancellationToken);

                if (room == null)
                {
                    Console.WriteLine($"❌ Target room not found: {request.NewRoomNumber}");
                    throw new InvalidOperationException($"Room '{request.NewRoomNumber}' not found");
                }

                Console.WriteLine($"✅ Target room found and validated");

                // 3. Check if target room is available for the booking dates
                var conflictFilter = Builders<BookingEntity>.Filter.And(
                    Builders<BookingEntity>.Filter.Eq(b => b.RoomNumber, request.NewRoomNumber),
                    Builders<BookingEntity>.Filter.Ne(b => b.BookingId, request.BookingId), // Exclude current booking
                    Builders<BookingEntity>.Filter.Or(
                        // Booking overlaps with existing booking
                        Builders<BookingEntity>.Filter.And(
                            Builders<BookingEntity>.Filter.Lte(b => b.CheckInDate, booking.CheckOutDate),
                            Builders<BookingEntity>.Filter.Gte(b => b.CheckOutDate, booking.CheckInDate)
                        )
                    )
                );

                var conflictingBooking = await bookingsCollection.Find(conflictFilter).FirstOrDefaultAsync(cancellationToken);

                if (conflictingBooking != null)
                {
                    Console.WriteLine($"❌ Room {request.NewRoomNumber} is not available for dates {booking.CheckInDate:yyyy-MM-dd} to {booking.CheckOutDate:yyyy-MM-dd}");
                    Console.WriteLine($"❌ Conflicting booking: {conflictingBooking.BookingId}");
                    throw new InvalidOperationException($"Room '{request.NewRoomNumber}' is not available for the booking dates");
                }

                Console.WriteLine($"✅ No conflicts found - Room is available");

                // 4. Update the booking
                var oldRoomNumber = booking.RoomNumber;
                booking.RoomNumber = request.NewRoomNumber;
                booking.LastModified = DateTime.UtcNow;

                var update = Builders<BookingEntity>.Update
                    .Set(b => b.RoomNumber, request.NewRoomNumber)
                    .Set(b => b.LastModified, DateTime.UtcNow);

                var updateResult = await bookingsCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

                if (updateResult.ModifiedCount == 0)
                {
                    Console.WriteLine($"❌ Failed to update booking in database");
                    throw new InvalidOperationException("Failed to update booking");
                }

                Console.WriteLine($"✅ Booking moved successfully from Room {oldRoomNumber} to Room {request.NewRoomNumber}");
                Console.WriteLine($"✅ Modified {updateResult.ModifiedCount} document(s)");

                // Log activity
                await _activityLogger.LogBookingMove(
                    bookingId: request.BookingId,
                    oldRoomNumber: oldRoomNumber,
                    newRoomNumber: request.NewRoomNumber,
                    userId: request.UserId,
                    username: "User", // TODO: Get actual username from user service
                    isSuccess: true
                );

                return booking;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ===== MOVE BOOKING ERROR =====");
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");

                // Log failed operation
                await _activityLogger.LogFailedOperation(
                    operationType: "BookingMove",
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
