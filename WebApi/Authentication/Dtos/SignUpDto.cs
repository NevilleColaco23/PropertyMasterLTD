namespace MyWarehouse.Infrastructure.Authentication.Dtos
{
    public class SignUpDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Phone { get; set; }
        public string? PropertyCode { get; set; }  // Optional: If provided and valid, user joins that property; if empty/invalid, assigns demo
    }
}
