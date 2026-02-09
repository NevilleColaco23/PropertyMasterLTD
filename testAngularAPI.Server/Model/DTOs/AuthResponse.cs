namespace testAngularAPI.Server.Model.DTOs
{
    public class AuthResponse
    {
        public required string AccessToken { get; set; }
        public required string TokenType { get; set; }
        public required int ExpiresIn { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string IsExternalLogin { get; set; } = "false";
        public string ExternalAuthenticationProvider { get; set; } = string.Empty;
    }
}
