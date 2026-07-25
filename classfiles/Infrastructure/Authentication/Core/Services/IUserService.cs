using MyWarehouse.Infrastructure.Authentication.Core.Model;

namespace MyWarehouse.Infrastructure.Authentication.Core.Services;

public interface IUserService
{
    Task<(MySignInResult result, SignInData? data)> SignIn(string username, string password);
    Task<(SignUpResult result, SignUpResultData? data)> SignUp(string username, string email, string password, string phoneNumber, string? propertyCode = null);
    Task<(bool success, string message)> ConfirmEmail(int userId, string token);
    Task<(bool success, string message)> ResendActivationEmail(string email);

    /// <summary>
    /// Ensures the guest demo user exists with a confirmed email and demo property access.
    /// Creates the user automatically on first call — idempotent, safe to call every login.
    /// </summary>
    Task EnsureGuestUserAsync(string email, string password, int demoPropertyId);
}
