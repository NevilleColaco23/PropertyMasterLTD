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

    public UserService(UserManager<ApplicationUserIdentity> userManager, SignInManager<ApplicationUserIdentity> signInManager, ITokenService tokenService, IMediator mediator, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _mediator = mediator;
        _logger = logger;
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

            // Don't use SignInManager.PasswordSignInAsync(), because that sets usele
            //
            //
            // ss cookies.
            // But 'CheckPasswordSignInAsync' doesn't. Yep, it's confusing. Good thing we have access to the source code. :D
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

        //UserActivationMailTemplateModel model = new();
        //model.Message = "Test Email Body";
        //model.Dated = DateTime.Now;
        //var mailService = ResourceLocator.Get<IMailService>();
        //mailService.SendMail(   model, "BidInviteTemplate", "nevillecolaco94@gmail.com", null, null, //pass model here
        //                            "Test", null, null);
        
        
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
