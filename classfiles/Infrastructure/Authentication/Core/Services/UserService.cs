using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MyWarehouse.Application.Users.CreateUser;
using MyWarehouse.Infrastructure.Authentication.Core.Model;
using MyWarehouse.Infrastructure.Models;
using MyWarehouse.Infrastructure.Services;
using MyWarehouse.WebApi.MailTemplates.MailTemplateModel;

namespace MyWarehouse.Infrastructure.Authentication.Core.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUserIdentity> _userManager;
    private readonly SignInManager<ApplicationUserIdentity> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IMediator _mediator;
    private readonly ILogger<UserService> _logger;
    private readonly IEmailQueueService _emailQueueService;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IDemoPropertyService _demoPropertyService;

    public UserService(
        UserManager<ApplicationUserIdentity> userManager, 
        SignInManager<ApplicationUserIdentity> signInManager, 
        ITokenService tokenService, 
        IMediator mediator, 
        ILogger<UserService> logger, 
        IEmailQueueService emailQueueService,
        IMongoDatabase mongoDatabase,
        IDemoPropertyService demoPropertyService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _mediator = mediator;
        _logger = logger;
        _emailQueueService = emailQueueService;
        _mongoDatabase = mongoDatabase;
        _demoPropertyService = demoPropertyService;
    }

    public async Task<(MySignInResult result, SignInData? data)> SignIn(string username, string password)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(username);

            if (user == null)
            {
                return (MySignInResult.Failed, null);
            }

            // Don't use SignInManager.PasswordSignInAsync(), because that sets useless cookies.
            // But 'CheckPasswordSignInAsync' doesn't. Yep, it's confusing. Good thing we have access to the source code
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    return (MySignInResult.LockedOut, null);
                if (result.IsNotAllowed)
                    return (MySignInResult.NotAllowed, null);
                throw new System.Exception("Unhandled sign-in outcome.");
            }

            var token = _tokenService.CreateAuthenticationToken(user.Id.ToString(), username);

            return (
                MySignInResult.Success,
                data: new SignInData()
                {
                    Username = user.UserName,
                    Email = user.Email,
                    Token = token,
                    PropertyAccessList = user.PropertyAccessList?.Select(p => p.Id).ToList(), // Fixed: Use "Id" not "PropertyID"
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the sign-in process for username: {Username}.", username);
            throw new Exception("An error occurred while signing in.", ex);
        }
    }

    public async Task<(SignUpResult result, SignUpResultData? data)> SignUp(string username, string email, string password, string phoneNumber, string? propertyCode = null)
    {
        var emailFound = await _userManager.FindByEmailAsync(email);

        if (emailFound is not null)
        {
            return (SignUpResult.EmailAlreadyExists, null);
        }

        var userObj = new ApplicationUserIdentity
        {
            UserName = username,
            Email = email
        };
        var passwordHasher = new PasswordHasher<ApplicationUserIdentity>();
        var pass = passwordHasher.HashPassword(userObj, password);


        CreateUserCommand createUserCommand = new()
        {
            UserName = username,
            Email = email,
            Password = pass,
            PhoneNumber = phoneNumber
        };

        var userId = await _mediator.Send(createUserCommand);

        if (userId == 0)
            return (SignUpResult.Failed, null);

        // Generate email confirmation token
        try
        {
            // Generate secure token (URL-safe)
            var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            _logger.LogInformation("Generated activation token for user {UserId} with length {TokenLength}", userId, token.Length);

            // Hash the token for storage (security best practice)
            var tokenHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(token)));

            _logger.LogDebug("Token hash for user {UserId}: {TokenHashPrefix}... (length: {HashLength})", 
                userId, tokenHash.Substring(0, 10), tokenHash.Length);

            // Store token hash directly in MongoDB Users collection
            var usersCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Users");
            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", userId);
            var update = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Update
                .Set("EmailConfirmationTokenHash", tokenHash)
                .Set("EmailConfirmationTokenExpiresAtUtc", DateTime.UtcNow.AddHours(24))
                .Set("EmailConfirmationTokenCreatedAtUtc", DateTime.UtcNow)
                .Set("EmailConfirmed", false);

            await usersCollection.UpdateOneAsync(filter, update);

            // Queue activation email with real token
            var activationLink = $"https://property-master-silk.vercel.app/activate?userId={userId}&token={token}";
            var htmlBody = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Activate Your Account</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h1 style=""color: #333333; margin: 0 0 20px 0; font-size: 24px; border-bottom: 3px solid #4CAF50; padding-bottom: 15px;"">
                                Welcome to Property Master!
                            </h1>
                            <p style=""color: #555555; font-size: 16px; line-height: 1.6; margin: 20px 0;"">
                                Hi <strong>{username}</strong>,
                            </p>
                            <p style=""color: #555555; font-size: 14px; line-height: 1.6; margin: 20px 0;"">
                                Thank you for signing up for Property Master. To complete your registration, please activate your account by clicking the button below:
                            </p>
                            <table role=""presentation"" style=""margin: 30px auto;"">
                                <tr>
                                    <td align=""center"" style=""border-radius: 5px; background-color: #4CAF50;"">
                                        <a href=""{activationLink}"" target=""_blank"" style=""display: inline-block; padding: 15px 30px; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 5px; font-weight: bold;"">
                                            Activate Account
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #888888; font-size: 12px; line-height: 1.6; margin: 20px 0;"">
                                Or copy and paste this link into your browser:
                            </p>
                            <p style=""color: #4CAF50; font-size: 12px; word-break: break-all; background-color: #f9f9f9; padding: 10px; border-radius: 4px;"">
                                {activationLink}
                            </p>
                            <hr style=""border: none; border-top: 1px solid #eeeeee; margin: 30px 0;"">
                            <p style=""color: #888888; font-size: 12px; line-height: 1.6; margin: 10px 0;"">
                                <strong>Note:</strong> This activation link will expire in 24 hours.
                            </p>
                            <p style=""color: #888888; font-size: 12px; line-height: 1.6; margin: 10px 0;"">
                                If you did not sign up for this account, please ignore this email.
                            </p>
                            <p style=""color: #555555; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0;"">
                                Best regards,<br>
                                <strong>Property Master Team</strong>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            await _emailQueueService.QueueEmailAsync(email, "Activate Your Account", htmlBody, "activation");

            _logger.LogInformation("Activation email queued for user {UserId} with token expiration in 24 hours", userId);

            // ✅ NEW LOGIC: Check property code and assign property accordingly
            try
            {
                var shouldAssignDemo = await _demoPropertyService.ShouldAssignDemoPropertyAsync(propertyCode);

                if (shouldAssignDemo)
                {
                    // No property code or invalid code - assign demo property
                    var demoAccessGranted = await _demoPropertyService.GrantUserAccessToDemoPropertyAsync(userId);
                    if (demoAccessGranted)
                    {
                        _logger.LogInformation("Demo property access granted to user {UserId} (propertyCode: {PropertyCode})", 
                            userId, string.IsNullOrWhiteSpace(propertyCode) ? "empty" : propertyCode);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to grant demo property access to user {UserId}", userId);
                    }
                }
                else
                {
                    // Valid property code provided - assign real property
                    var validPropertyId = await _demoPropertyService.ValidateAndGetPropertyIdAsync(propertyCode!);
                    if (validPropertyId.HasValue)
                    {
                        var propertyAccessGranted = await _demoPropertyService.GrantUserAccessToPropertyAsync(userId, validPropertyId.Value);
                        if (propertyAccessGranted)
                        {
                            _logger.LogInformation("Property access granted to user {UserId} for property {PropertyId} (code: {PropertyCode})", 
                                userId, validPropertyId.Value, propertyCode);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to grant property access to user {UserId} for property {PropertyId}", 
                                userId, validPropertyId.Value);
                        }
                    }
                }
            }
            catch (Exception propertyEx)
            {
                _logger.LogError(propertyEx, "Error granting property access to user {UserId}", userId);
                // Don't fail signup if property access fails
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue activation email for user {UserId}", userId);
            // Don't fail the signup if email queueing fails
        }

        return (
            SignUpResult.Success,
            data: new SignUpResultData()
            {
                UserId = Convert.ToInt32(userId),
                Email = email,
            }
        );
    }

    public async Task<(bool success, string message)> ConfirmEmail(int userId, string token)
    {
        try
        {
            _logger.LogInformation("Email confirmation attempt for user {UserId} with token length {TokenLength}", userId, token?.Length ?? 0);

            // Get user from MongoDB
            var usersCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Users");
            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", userId);
            var userDoc = await usersCollection.Find(filter).FirstOrDefaultAsync();

            if (userDoc == null)
            {
                _logger.LogWarning("Email confirmation failed: User {UserId} not found", userId);
                return (false, "User not found. Invalid activation link.");
            }

            // Check if already confirmed
            if (userDoc.Contains("EmailConfirmed") && userDoc["EmailConfirmed"].AsBoolean)
            {
                _logger.LogInformation("User {UserId} email already confirmed", userId);
                return (true, "Email already confirmed. You can log in now.");
            }

            // Get stored token hash
            if (!userDoc.Contains("EmailConfirmationTokenHash") || string.IsNullOrEmpty(userDoc["EmailConfirmationTokenHash"].AsString))
            {
                _logger.LogWarning("Email confirmation failed: No token found for user {UserId}", userId);
                return (false, "No activation token found. Please request a new activation link.");
            }

            var storedTokenHash = userDoc["EmailConfirmationTokenHash"].AsString;
            _logger.LogDebug("Stored token hash length: {HashLength}", storedTokenHash.Length);

            // Check token expiration
            if (userDoc.Contains("EmailConfirmationTokenExpiresAtUtc"))
            {
                var expiresAt = userDoc["EmailConfirmationTokenExpiresAtUtc"].ToUniversalTime();
                _logger.LogDebug("Token expires at {ExpiresAt}, current time {CurrentTime}", expiresAt, DateTime.UtcNow);
                if (DateTime.UtcNow > expiresAt)
                {
                    _logger.LogWarning("Email confirmation failed: Token expired for user {UserId}", userId);
                    return (false, "Activation link has expired. Please request a new one.");
                }
            }

            // Hash the provided token and compare
            var providedTokenHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(token)));

            _logger.LogDebug("Provided token hash length: {HashLength}", providedTokenHash.Length);
            _logger.LogDebug("Token hashes match: {Match}", storedTokenHash == providedTokenHash);

            if (storedTokenHash != providedTokenHash)
            {
                _logger.LogWarning("Email confirmation failed: Invalid token for user {UserId}. Stored hash: {StoredHash}, Provided hash: {ProvidedHash}", 
                    userId, storedTokenHash.Substring(0, 10) + "...", providedTokenHash.Substring(0, 10) + "...");
                return (false, "Invalid token. The activation link is incorrect.");
            }

            // Activate the account
            var update = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Update
                .Set("EmailConfirmed", true)
                .Set("EmailConfirmationTokenHash", "")  // Clear the token
                .Set("LockoutEnabled", false);           // Ensure account is not locked

            await usersCollection.UpdateOneAsync(filter, update);

            _logger.LogInformation("Email confirmed successfully for user {UserId}", userId);
            return (true, "Email confirmed successfully! You can now log in.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for user {UserId}", userId);
            return (false, "An error occurred while confirming your email. Please try again.");
        }
    }

    public async Task<(bool success, string message)> ResendActivationEmail(string email)
    {
        try
        {
            _logger.LogInformation("Resend activation email requested for {Email}", email);

            // Find user by email
            var usersCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Users");
            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("Email", email);
            var userDoc = await usersCollection.Find(filter).FirstOrDefaultAsync();

            if (userDoc == null)
            {
                _logger.LogWarning("Resend activation failed: User with email {Email} not found", email);
                return (false, "No account found with this email address. Please sign up first.");
            }

            var userId = userDoc["_id"].AsInt32;
            var username = userDoc["UserName"].AsString;

            // Check if already confirmed
            if (userDoc.Contains("EmailConfirmed") && userDoc["EmailConfirmed"].AsBoolean)
            {
                _logger.LogInformation("Resend activation skipped: User {Email} already confirmed", email);
                return (true, "Your email is already confirmed. You can log in now.");
            }

            // Generate new token
            var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            var tokenHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(token)));

            // Update token in database
            var update = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Update
                .Set("EmailConfirmationTokenHash", tokenHash)
                .Set("EmailConfirmationTokenExpiresAtUtc", DateTime.UtcNow.AddHours(24))
                .Set("EmailConfirmationTokenCreatedAtUtc", DateTime.UtcNow);

            await usersCollection.UpdateOneAsync(filter, update);

            // Queue activation email
            var activationLink = $"https://property-master-silk.vercel.app/activate?userId={userId}&token={token}";
            var htmlBody = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Activate Your Account</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h1 style=""color: #333333; margin: 0 0 20px 0; font-size: 24px; border-bottom: 3px solid #4CAF50; padding-bottom: 15px;"">
                                Activation Link Requested
                            </h1>
                            <p style=""color: #555555; font-size: 16px; line-height: 1.6; margin: 20px 0;"">
                                Hi <strong>{username}</strong>,
                            </p>
                            <p style=""color: #555555; font-size: 14px; line-height: 1.6; margin: 20px 0;"">
                                You requested a new activation link for your Property Master account. Click the button below to activate your account:
                            </p>
                            <table role=""presentation"" style=""margin: 30px auto;"">
                                <tr>
                                    <td align=""center"" style=""border-radius: 5px; background-color: #4CAF50;"">
                                        <a href=""{activationLink}"" target=""_blank"" style=""display: inline-block; padding: 15px 30px; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 5px; font-weight: bold;"">
                                            Activate Account
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #888888; font-size: 12px; line-height: 1.6; margin: 20px 0;"">
                                Or copy and paste this link into your browser:
                            </p>
                            <p style=""color: #4CAF50; font-size: 12px; word-break: break-all; background-color: #f9f9f9; padding: 10px; border-radius: 4px;"">
                                {activationLink}
                            </p>
                            <hr style=""border: none; border-top: 1px solid #eeeeee; margin: 30px 0;"">
                            <p style=""color: #888888; font-size: 12px; line-height: 1.6; margin: 10px 0;"">
                                <strong>Note:</strong> This activation link will expire in 24 hours.
                            </p>
                            <p style=""color: #888888; font-size: 12px; line-height: 1.6; margin: 10px 0;"">
                                If you did not request this email, please ignore it.
                            </p>
                            <p style=""color: #555555; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0;"">
                                Best regards,<br>
                                <strong>Property Master Team</strong>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            await _emailQueueService.QueueEmailAsync(email, "Activate Your Account", htmlBody, "activation");

            _logger.LogInformation("Activation email resent successfully for user {Email}", email);
            return (true, "Activation email sent! Please check your inbox.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending activation email for {Email}", email);
            return (false, "Failed to send activation email. Please try again later.");
        }
    }
}
