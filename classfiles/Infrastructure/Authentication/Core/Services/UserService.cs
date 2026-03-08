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

    public UserService(
        UserManager<ApplicationUserIdentity> userManager, 
        SignInManager<ApplicationUserIdentity> signInManager, 
        ITokenService tokenService, 
        IMediator mediator, 
        ILogger<UserService> logger, 
        IEmailQueueService emailQueueService,
        IMongoDatabase mongoDatabase)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _mediator = mediator;
        _logger = logger;
        _emailQueueService = emailQueueService;
        _mongoDatabase = mongoDatabase;
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
                    PropertyAccessList = user.PropertyAccessList?.Select(p => p.PropertyID).ToList(),
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the sign-in process for username: {Username}.", username);
            throw new Exception("An error occurred while signing in.", ex);
        }
    }

    public async Task<(SignUpResult result, SignUpResultData? data)> SignUp(string username, string email, string password, string phoneNumber)
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

            // Hash the token for storage (security best practice)
            var tokenHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(token)));

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
            var htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        <h2 style='color: #333; border-bottom: 3px solid #4CAF50; padding-bottom: 10px;'>Welcome to Property Master!</h2>
                        <p style='color: #555; font-size: 16px;'>Hi <strong>{username}</strong>,</p>
                        <p style='color: #555; font-size: 14px;'>Thank you for signing up for Property Master. To complete your registration, please activate your account by clicking the button below:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{activationLink}' style='background-color: #4CAF50; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-size: 16px; display: inline-block;'>Activate Account</a>
                        </div>
                        <p style='color: #888; font-size: 12px;'>Or copy and paste this link into your browser:</p>
                        <p style='color: #4CAF50; font-size: 12px; word-break: break-all;'>{activationLink}</p>
                        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                        <p style='color: #888; font-size: 12px;'><strong>Note:</strong> This activation link will expire in 24 hours.</p>
                        <p style='color: #888; font-size: 12px;'>If you did not sign up for this account, please ignore this email.</p>
                        <p style='color: #555; font-size: 14px; margin-top: 20px;'>Best regards,<br/><strong>Property Master Team</strong></p>
                    </div>
                </body>
                </html>";

            await _emailQueueService.QueueEmailAsync(email, "Activate Your Account", htmlBody, "activation");

            _logger.LogInformation("Activation email queued for user {UserId} with token expiration in 24 hours", userId);
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
            // Get user from MongoDB
            var usersCollection = _mongoDatabase.GetCollection<MongoDB.Bson.BsonDocument>("Users");
            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", userId);
            var userDoc = await usersCollection.Find(filter).FirstOrDefaultAsync();

            if (userDoc == null)
            {
                _logger.LogWarning("Email confirmation failed: User {UserId} not found", userId);
                return (false, "Invalid activation link.");
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
                return (false, "Invalid activation link.");
            }

            var storedTokenHash = userDoc["EmailConfirmationTokenHash"].AsString;

            // Check token expiration
            if (userDoc.Contains("EmailConfirmationTokenExpiresAtUtc"))
            {
                var expiresAt = userDoc["EmailConfirmationTokenExpiresAtUtc"].ToUniversalTime();
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

            if (storedTokenHash != providedTokenHash)
            {
                _logger.LogWarning("Email confirmation failed: Invalid token for user {UserId}", userId);
                return (false, "Invalid activation link.");
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
}
