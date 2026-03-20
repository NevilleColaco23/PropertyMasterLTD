namespace MyWarehouse.Application.Dashboard.DTOs
{
    /// <summary>
    /// Guest details for booking information
    /// </summary>
    public class GuestDetailsDTO
    {
        public string GuestId { get; set; } = string.Empty;
        public string FirstName { get; set; } = "Unavailable";
        public string LastName { get; set; } = "Unavailable";
        public string Email { get; set; } = "Unavailable";
        public string PhoneNumber { get; set; } = "Unavailable";
        public string Nationality { get; set; } = "Unavailable";
    }
}
