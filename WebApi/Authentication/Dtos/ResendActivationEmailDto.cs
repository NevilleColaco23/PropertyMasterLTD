namespace MyWarehouse.Infrastructure.Authentication.Models.Dtos;

public record ResendActivationEmailDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = null!;
}
