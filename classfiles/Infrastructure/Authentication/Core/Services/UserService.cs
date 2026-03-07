using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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

    public UserService(UserManager<ApplicationUserIdentity> userManager, SignInManager<ApplicationUserIdentity> signInManager, ITokenService tokenService, IMediator mediator, ILogger<UserService> logger, IEmailQueueService emailQueueService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _mediator = mediator;
        _logger = logger;
        _emailQueueService = emailQueueService;
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

        // Queue activation email
        try
        {
            var activationLink = $"https://property-master-silk.vercel.app/activate?userId={userId}&token=placeholder";
            var htmlBody = $@"
                <html>
                <body>
                    <h2>Welcome to Property Master!</h2>
                    <p>Hi {username},</p>
                    <p>Thank you for signing up. Please activate your account by clicking the link below:</p>
                    <p><a href='{activationLink}'>Activate Account</a></p>
                    <p>If you did not sign up for this account, please ignore this email.</p>
                    <p>Best regards,<br/>Property Master Team</p>
                </body>
                </html>";

            await _emailQueueService.QueueEmailAsync(email, "Activate Your Account", htmlBody, "activation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue activation email for user {UserId}", userId);
            // Don't fail the signup if email queueing fails
        }

        if (userId == 0)
            return (SignUpResult.Failed, null);

        return (
            SignUpResult.Success,
            data: new SignUpResultData()
            {
                UserId = Convert.ToInt32(userId),
                Email = email,
            }
        );
    }
}
