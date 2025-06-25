namespace MyWarehouse.Infrastructure.Authentication.Dtos
{
    public class SignUpResponseDto
    {
        public int UserId { get; init; }
        public string? Email { get; init; } = null!;
    }
}
