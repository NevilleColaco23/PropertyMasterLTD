using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
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
    private readonly IConfiguration _configuration;

    public UserService(
        UserManager<ApplicationUserIdentity> userManager, 
        SignInManager<ApplicationUserIdentity> signInManager, 
        ITokenService tokenService, 
        IMediator mediator, 
        ILogger<UserService> logger, 
        IEmailQueueService emailQueueService,
        IMongoDatabase mongoDatabase,
        IDemoPropertyService demoPropertyService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _mediator = mediator;
        _logger = logger;
        _emailQueueService = emailQueueService;
        _mongoDatabase = mongoDatabase;
        _demoPropertyService = demoPropertyService;
        _configuration = configuration;
    }

    private string FrontendBaseUrl =>
        (_configuration["FrontendSettings:BaseUrl"]?.Trim('/') is { Length: > 0 } url
            ? url
            : "https://brave-rock-0db8c8503.7.azurestaticapps.net");

    public async Task<(MySignInResult result, SignInData? data)> SignIn(string username, string password)
    {
        try
        {
            _logger.LogInformation("Sign-in attempt for user: {Username}", username);

            // ⭐ HYBRID: Try Identity first
            var user = await _userManager.FindByEmailAsync(username);

            // ⭐ FALLBACK: If Identity can't find user, check MongoDB directly (for old users)
            if (user == null)
            {
                _logger.LogWarning("User not found via Identity, checking MongoDB directly for legacy users...");
                user = await FindLegacyUserAsync(username);

                if (user != null)
                {
                    _logger.LogInformation("Legacy user found: UserId={UserId}. Consider migrating to Identity.", user.Id);

                    // Verify password manually for legacy users
                    var passwordHasher = new PasswordHasher<ApplicationUserIdentity>();
                    var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

                    if (verificationResult == PasswordVerificationResult.Failed)
                    {
                        _logger.LogWarning("Sign-in failed: Invalid password for legacy user {UserId}", user.Id);
                        return (MySignInResult.Failed, null);
                    }

                    // Check email confirmed for legacy users
                    if (!user.EmailConfirmed)
                    {
                        _logger.LogWarning("Sign-in failed: Email not confirmed for legacy user {UserId}", user.Id);
                        return (MySignInResult.NotAllowed, null);
                    }

                    _logger.LogInformation("Legacy user sign-in successful: UserId={UserId}", user.Id);

                    // Generate JWT token
                    var legacyToken = _tokenService.CreateAuthenticationToken(user.Id.ToString(), user.Email ?? user.UserName ?? "");

                    // Extract property access
                    var legacyPropertyAccessList = user.PropertyAccessList?
                        .Where(p => p.IsActive)
                        .Select(p => p.Id)
                        .ToList();

                    return (
                        MySignInResult.Success,
                        data: new SignInData()
                        {
                            Username = user.UserName,
                            Email = user.Email,
                            Token = legacyToken,
                            PropertyAccessList = legacyPropertyAccessList,
                        }
                    );
                }
            }

            if (user == null)
            {
                _logger.LogWarning("Sign-in failed: User not found for email {Username}", username);
                return (MySignInResult.Failed, null);
            }

            _logger.LogInformation("User found via Identity: UserId={UserId}, UserName={UserName}", user.Id, user.UserName);

            // ⭐ HYBRID: Use Identity's SignInManager (handles lockouts, email confirmation, password verification automatically)
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Sign-in failed: Account locked for user {UserId}", user.Id);
                    return (MySignInResult.LockedOut, null);
                }
                if (result.IsNotAllowed)
                {
                    _logger.LogWarning("Sign-in failed: Sign-in not allowed (email not confirmed?) for user {UserId}", user.Id);
                    return (MySignInResult.NotAllowed, null);
                }

                _logger.LogWarning("Sign-in failed: Invalid credentials for user {UserId}", user.Id);
                return (MySignInResult.Failed, null);
            }

            _logger.LogInformation("Sign-in successful for user {UserId}", user.Id);

            // ⭐ Extract PropertyAccessList from your custom field
            var propertyAccessList = user.PropertyAccessList?
                .Where(p => p.IsActive)
                .Select(p => p.Id)
                .ToList();

            if (propertyAccessList != null && propertyAccessList.Any())
            {
                _logger.LogInformation("User {UserId} has access to {PropertyCount} properties", user.Id, propertyAccessList.Count);
            }

            // ⭐ Still use YOUR custom JWT tokens (not Identity cookies)
            var guestEmail = _configuration["GuestSettings:Username"];
            var isGuest = !string.IsNullOrWhiteSpace(guestEmail) &&
                          string.Equals(user.Email, guestEmail, StringComparison.OrdinalIgnoreCase);

            var customClaims = isGuest
                ? new[] { ("role", "Guest") }
                : (IEnumerable<(string, string)>)Array.Empty<(string, string)>();

            var token = _tokenService.CreateAuthenticationToken(user.Id.ToString(), user.Email ?? user.UserName ?? "", customClaims);

            return (
                MySignInResult.Success,
                data: new SignInData()
                {
                    Username = user.UserName,
                    Email = user.Email,
                    Token = token,
                    PropertyAccessList = propertyAccessList,
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
        try
        {
            _logger.LogInformation("Sign-up attempt for email: {Email}", email);

            // ⭐ HYBRID: Use Identity's UserManager to check existing user
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogWarning("Sign-up failed: Email {Email} already exists", email);
                return (SignUpResult.EmailAlreadyExists, null);
            }

            // ⭐ HYBRID: Create user with Identity (it will hash password and store in MongoDB)
            var user = new ApplicationUserIdentity
            {
                UserName = username,
                Email = email,
                PhoneNumber = phoneNumber,
                EmailConfirmed = false,
                PropertyAccessList = new List<PropertyAccess>()
            };

            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Sign-up failed: {Errors}", errors);
                return (SignUpResult.Failed, null);
            }

            _logger.LogInformation("User created successfully: UserId={UserId}, Email={Email}", user.Id, user.Email);

            // ⭐ Generate email confirmation token using Identity
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Make token URL-safe
            var urlSafeToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token))
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            _logger.LogInformation("Generated activation token for user {UserId} with length {TokenLength}", user.Id, urlSafeToken.Length);

            // Queue activation email
            try
            {
                var activationLink = $"{FrontendBaseUrl}/activate?userId={user.Id}&token={urlSafeToken}";
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
                _logger.LogInformation("Activation email queued for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue activation email for user {UserId}", user.Id);
                // Don't fail signup if email fails
            }

            // ✅ Assign property access based on comma-separated property codes
            // Blank = no property assigned (user will see empty property selector)
            try
            {
                if (string.IsNullOrWhiteSpace(propertyCode))
                {
                    _logger.LogInformation("No property code provided for user {UserId} — PropertyAccessList will be empty.", user.Id);
                }
                else
                {
                    var codes = propertyCode
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    _logger.LogInformation("Processing {Count} property code(s) for user {UserId}: {Codes}",
                        codes.Count, user.Id, string.Join(", ", codes));

                    foreach (var code in codes)
                    {
                        var validPropertyId = await _demoPropertyService.ValidateAndGetPropertyIdAsync(code);
                        if (validPropertyId.HasValue)
                        {
                            var granted = await _demoPropertyService.GrantUserAccessToPropertyAsync(user.Id, validPropertyId.Value);
                            if (granted)
                                _logger.LogInformation("Property access granted to user {UserId} for code '{Code}' (ID: {PropertyId})",
                                    user.Id, code, validPropertyId.Value);
                        }
                        else
                        {
                            _logger.LogWarning("Property code '{Code}' not found or inactive — skipping for user {UserId}", code, user.Id);
                        }
                    }
                }

                }
                catch (Exception propertyEx)
                {
                    _logger.LogError(propertyEx, "Error granting property access to user {UserId}", user.Id);
                    // Don't fail signup if property access fails
                }

            return (
                SignUpResult.Success,
                data: new SignUpResultData()
                {
                    UserId = user.Id,
                    Email = user.Email,
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign-up for email: {Email}", email);
            return (SignUpResult.Failed, null);
        }
    }

    public async Task EnsureGuestUserAsync(string email, string password, int demoPropertyId)
    {
        try
        {
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing == null)
            {
                _logger.LogInformation("Auto-provisioning guest user {Email} for demo property {PropertyId}.", email, demoPropertyId);

                var username = email.Split('@')[0];
                var user = new ApplicationUserIdentity
                {
                    UserName       = username,
                    Email          = email,
                    EmailConfirmed = true,
                    PropertyAccessList = new List<PropertyAccess>
                    {
                        new PropertyAccess
                        {
                            Id          = demoPropertyId,
                            IsActive    = true,
                            From        = DateTime.UtcNow,
                            To          = DateTime.UtcNow.AddYears(10),
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy   = 0
                        }
                    }
                };

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to auto-provision guest user {Email}: {Errors}", email, errors);
                    throw new InvalidOperationException($"Guest user provisioning failed: {errors}");
                }

                _logger.LogInformation("Guest user {Email} created successfully (id: {UserId}).", email, user.Id);
            }
            else
            {
                _logger.LogDebug("Guest user {Email} already exists.", email);
            }

            // Always check the demo property — runs whether user is new or existing.
            // This handles the case where the property was deleted from MongoDB manually.
            await EnsureDemoPropertyAsync(demoPropertyId);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while provisioning guest user {Email}.", email);
            throw;
        }
    }

    private async Task EnsureDemoPropertyAsync(int demoPropertyId)
    {
        var propertyCollection = _mongoDatabase.GetCollection<BsonDocument>("Property");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", demoPropertyId);

        var exists = await propertyCollection.Find(filter).AnyAsync();
        if (exists)
        {
            _logger.LogDebug("Demo property {PropertyId} already exists — skipping.", demoPropertyId);
            return;
        }

        _logger.LogInformation("Auto-provisioning demo property {PropertyId}.", demoPropertyId);

        var now = DateTime.UtcNow;
        var propertyDoc = new BsonDocument
        {
            { "_id",            demoPropertyId },
            { "Name",           "The Grand Hotel - Demo" },
            { "Active",         true },
            { "PropertyCode",   "DEMO0001" },
            { "CompanyLogoURL", "https://placehold.co/200x200/4CAF50/white?text=DEMO" },
            { "Rooms", new BsonArray
                {
                    new BsonDocument { { "_id", 101 }, { "RoomCode", "R101" }, { "RoomName", "Standard Single" },    { "Active", true }, { "CompanyLogoURL", "" } },
                    new BsonDocument { { "_id", 102 }, { "RoomCode", "R102" }, { "RoomName", "Standard Double" },    { "Active", true }, { "CompanyLogoURL", "" } },
                    new BsonDocument { { "_id", 201 }, { "RoomCode", "R201" }, { "RoomName", "Deluxe King" },        { "Active", true }, { "CompanyLogoURL", "" } },
                    new BsonDocument { { "_id", 202 }, { "RoomCode", "R202" }, { "RoomName", "Deluxe Twin" },        { "Active", true }, { "CompanyLogoURL", "" } },
                    new BsonDocument { { "_id", 301 }, { "RoomCode", "R301" }, { "RoomName", "Junior Suite" },       { "Active", true }, { "CompanyLogoURL", "" } },
                    new BsonDocument { { "_id", 401 }, { "RoomCode", "R401" }, { "RoomName", "Presidential Suite" }, { "Active", true }, { "CompanyLogoURL", "" } },
                }
            },
            { "IsDemo",    true },
            { "IsDeleted", false },
            { "CreatedAt", now },
            { "CreatedBy", 0 }
        };

        await propertyCollection.InsertOneAsync(propertyDoc);
        _logger.LogInformation("Demo property {PropertyId} inserted successfully.", demoPropertyId);
    }

    public async Task<(bool success, string message)> ConfirmEmail(int userId, string token)
    {
        try
        {
            _logger.LogInformation("Email confirmation attempt for user {UserId}", userId);

            // ⭐ HYBRID: Use Identity's UserManager
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                _logger.LogWarning("Email confirmation failed: User {UserId} not found", userId);
                return (false, "User not found. Invalid activation link.");
            }

            // Check if already confirmed
            if (user.EmailConfirmed)
            {
                _logger.LogInformation("User {UserId} email already confirmed", userId);
                return (true, "Email already confirmed. You can log in now.");
            }

            // Decode URL-safe token back to Identity token.
            // Restore the Base64 padding that was stripped during encoding.
            var padded = token.Replace("-", "+").Replace("_", "/");
            var paddingNeeded = padded.Length % 4;
            if (paddingNeeded > 0) padded += new string('=', 4 - paddingNeeded);

            var decodedToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));

            // ⭐ HYBRID: Use Identity's built-in token verification
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Email confirmation failed for user {UserId}: {Errors}", userId, errors);
                return (false, "Invalid or expired token. Please request a new activation link.");
            }

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

            // ⭐ HYBRID: Use Identity's UserManager
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Resend activation failed: User with email {Email} not found", email);
                return (false, "No account found with this email address. Please sign up first.");
            }

            // Check if already confirmed
            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Resend activation skipped: User {Email} already confirmed", email);
                return (true, "Your email is already confirmed. You can log in now.");
            }

            // ⭐ HYBRID: Generate new token using Identity
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Make token URL-safe
            var urlSafeToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token))
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            // Queue activation email
            var activationLink = $"{FrontendBaseUrl}/activate?userId={user.Id}&token={urlSafeToken}";
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
                                Hi <strong>{user.UserName}</strong>,
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

    /// <summary>
    /// Find legacy users (created before hybrid approach) directly from MongoDB
    /// These users lack Identity fields like NormalizedEmail, SecurityStamp, etc.
    /// </summary>
    private async Task<ApplicationUserIdentity?> FindLegacyUserAsync(string email)
    {
        try
        {
            _logger.LogInformation("Attempting to find legacy user directly from MongoDB for email: {Email}", email);

            // Try both possible collection names that AspNetCore.Identity.MongoDbCore might use
            string[] possibleCollectionNames = { "Users", "ApplicationUser", "AspNetUsers" };

            foreach (var collectionName in possibleCollectionNames)
            {
                try
                {
                    _logger.LogInformation("Trying collection: {CollectionName}", collectionName);
                    var usersCollection = _mongoDatabase.GetCollection<ApplicationUserIdentity>(collectionName);

                    var filter = MongoDB.Driver.Builders<ApplicationUserIdentity>.Filter.Eq(u => u.Email, email);
                    var user = await usersCollection.Find(filter).FirstOrDefaultAsync();

                    if (user != null)
                    {
                        _logger.LogInformation("✅ Found legacy user in collection '{CollectionName}': UserId={UserId}, Email={Email}, EmailConfirmed={EmailConfirmed}", 
                            collectionName, user.Id, user.Email, user.EmailConfirmed);
                        return user;
                    }
                    else
                    {
                        _logger.LogInformation("No user found in collection '{CollectionName}'", collectionName);
                    }
                }
                catch (Exception collEx)
                {
                    _logger.LogWarning(collEx, "Error querying collection '{CollectionName}'", collectionName);
                }
            }

            _logger.LogWarning("Legacy user not found in any collection for email: {Email}", email);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding legacy user for email: {Email}", email);
            return null;
        }
    }
}
