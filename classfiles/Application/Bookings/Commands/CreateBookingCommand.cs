using MediatR;

namespace MyWarehouse.Application.Bookings.Commands
{
    /// <summary>
    /// Command to create a new booking
    /// </summary>
    public class CreateBookingCommand : IRequest<Domain.Bookings.Bookings>
    {
        public int PropertyId { get; set; }
        public long GuestId { get; set; }
        public string RoomNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public double TotalPrice { get; set; }
        public long PaymentStatusId { get; set; }
        public long BookingSourceId { get; set; }
        public string? SpecialRequests { get; set; }
        public bool IsConfirmed { get; set; }
        public int UserId { get; set; }

        public CreateBookingCommand(
            int propertyId,
            long guestId,
            string roomNumber,
            DateTime checkInDate,
            DateTime checkOutDate,
            int numberOfGuests,
            double totalPrice,
            long paymentStatusId,
            long bookingSourceId,
            string? specialRequests,
            bool isConfirmed,
            int userId)
        {
            PropertyId = propertyId;
            GuestId = guestId;
            RoomNumber = roomNumber;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            NumberOfGuests = numberOfGuests;
            TotalPrice = totalPrice;
            PaymentStatusId = paymentStatusId;
            BookingSourceId = bookingSourceId;
            SpecialRequests = specialRequests;
            IsConfirmed = isConfirmed;
            UserId = userId;
        }
    }
}
