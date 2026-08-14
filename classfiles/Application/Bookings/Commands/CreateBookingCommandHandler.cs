using MediatR;
using MongoDB.Driver;
using MyWarehouse.Application.Services;
using MyWarehouse.Application.Bookings.Services;
using BookingEntity = MyWarehouse.Domain.Bookings.Bookings;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Handler for creating a new booking
    /// </summary>
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingEntity>
    {
        private const string BookingIdChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly IMongoDatabase _database;
        private readonly ICounterService _counterService;
        private readonly BookingActivityLogger _activityLogger;

        public CreateBookingCommandHandler(
            IMongoDatabase database,
            ICounterService counterService,
            BookingActivityLogger activityLogger)
        {
            _database = database;
            _counterService = counterService;
            _activityLogger = activityLogger;
        }

        public async Task<BookingEntity> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CheckOutDate <= request.CheckInDate)
                {
                    throw new InvalidOperationException("Check-out date must be after check-in date");
                }

                var bookingsCollection = _database.GetCollection<BookingEntity>(MongoCollections.BookingsCollection);

                // Validate the room exists
                var roomsCollection = _database.GetCollection<MongoDB.Bson.BsonDocument>(MongoCollections.RoomCollection);
                var roomFilter = Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("RoomCode", request.RoomNumber);
                var room = await roomsCollection.Find(roomFilter).FirstOrDefaultAsync(cancellationToken);

                if (room == null)
                {
                    throw new InvalidOperationException($"Room '{request.RoomNumber}' not found");
                }

                // Check for overlapping bookings on the same room
                var conflictFilter = Builders<BookingEntity>.Filter.And(
                    Builders<BookingEntity>.Filter.Eq(b => b.RoomNumber, request.RoomNumber),
                    Builders<BookingEntity>.Filter.Lt(b => b.CheckInDate, request.CheckOutDate),
                    Builders<BookingEntity>.Filter.Gt(b => b.CheckOutDate, request.CheckInDate)
                );

                var conflictingBooking = await bookingsCollection.Find(conflictFilter).FirstOrDefaultAsync(cancellationToken);
                if (conflictingBooking != null)
                {
                    throw new InvalidOperationException($"Room '{request.RoomNumber}' is not available for the selected dates");
                }

                var nextId = await _counterService.GetNextSequenceValue(nameof(BookingEntity));

                var booking = new BookingEntity
                {
                    Id = nextId,
                    BookingId = GenerateBookingId(),
                    PropertyId = request.PropertyId,
                    GuestId = request.GuestId,
                    StaffId = request.UserId,
                    RoomNumber = request.RoomNumber,
                    BookingDate = DateTime.UtcNow,
                    CheckInDate = request.CheckInDate,
                    CheckOutDate = request.CheckOutDate,
                    NumberOfGuests = request.NumberOfGuests,
                    TotalPrice = request.TotalPrice,
                    PaymentStatusId = request.PaymentStatusId,
                    BookingSourceId = request.BookingSourceId,
                    SpecialRequests = string.IsNullOrWhiteSpace(request.SpecialRequests)
                        ? new List<string>()
                        : new List<string> { request.SpecialRequests },
                    IsConfirmed = request.IsConfirmed,
                    LastModified = DateTime.UtcNow
                };

                await bookingsCollection.InsertOneAsync(booking, cancellationToken: cancellationToken);

                await _activityLogger.LogBookingCreation(
                    bookingId: booking.BookingId,
                    roomNumber: booking.RoomNumber,
                    checkInDate: booking.CheckInDate,
                    checkOutDate: booking.CheckOutDate,
                    userId: request.UserId,
                    username: "User",
                    isSuccess: true
                );

                return booking;
            }
            catch (Exception ex)
            {
                await _activityLogger.LogFailedOperation(
                    operationType: "BookingCreate",
                    bookingId: string.Empty,
                    errorMessage: ex.Message,
                    stackTrace: ex.StackTrace,
                    userId: request.UserId,
                    username: "User"
                );

                throw;
            }
        }

        private static string GenerateBookingId()
        {
            var random = Random.Shared;
            var chars = new char[10];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = BookingIdChars[random.Next(BookingIdChars.Length)];
            }
            return new string(chars);
        }
    }
}
