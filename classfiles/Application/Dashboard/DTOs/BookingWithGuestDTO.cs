namespace MyWarehouse.Application.Dashboard.DTOs
{
    /// <summary>
    /// DTO for booking with guest details
    /// </summary>
    public class BookingWithGuestDTO
    {
        public string Id { get; set; } = string.Empty;
        public string BookingId { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = "Unknown Property";
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = "confirmed";
        public string GuestId { get; set; } = string.Empty;
        
        // Guest details
        public string GuestFirstName { get; set; } = "Unavailable";
        public string GuestLastName { get; set; } = "Unavailable";
        public string GuestEmail { get; set; } = "Unavailable";
        public string GuestPhoneNumber { get; set; } = "Unavailable";
        public string GuestNationality { get; set; } = "Unavailable";
    }
}
