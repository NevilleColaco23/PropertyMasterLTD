using Microsoft.AspNetCore.Identity;
using MyWarehouse.Infrastructure.Authentication.Core.Model;
using MyWarehouse.Infrastructure.Authentication.Core.Services;
using MyWarehouse.Infrastructure.Authentication.External.Services;
using MyWarehouse.Infrastructure.Authentication.Dtos;
using MyWarehouse.Infrastructure.Authentication.Models.Dtos;
using MyWarehouse.WebApi.Authentication.Dtos;
using Azure.Core;
using MyWarehouse.Application.UserActivity.Attributes;

namespace MyWarehouse.Infrastructure.API.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{v:apiVersion}/account")]
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IExternalSignInService _externalSignInService;

    public AccountController(IUserService userService, IExternalSignInService externalSignInService)
    {
        _userService = userService;
        _externalSignInService = externalSignInService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [LogCreate("User Session", Description = "User logged in")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto login)
    {
        var result = await _userService.SignIn(login.Username, login.Password);

        return ProduceLoginResponse(result);
    }

    /// <summary>
    /// OAuth2.0 compliant login endpoint. Used for Swagger login.
    /// </summary>
    [AllowAnonymous]
    [ApiVersionNeutral]
    [HttpPost("/account/oauth2/access_token")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponseDto>> LoginForm([FromForm] LoginDto login)
    {
        var (result, model) = await _userService.SignIn(login.Username, login.Password);

        return result switch
        {
            MySignInResult.Success => Ok(new OAuth2TokenResponseDto
            {
                AccessToken = model!.Token.AccessToken,
                TokenType = model.Token.TokenType,
                ExpiresIn = model.Token.GetRemainingLifetimeSeconds()
            }),
            _ => Unauthorized()
        };
    }

    [AllowAnonymous]
    [HttpPost("loginExternal")]
    [LogCreate("User Session", Description = "User logged in via external provider")]
    public async Task<ActionResult<LoginResponseDto>> ExternalLogin(ExternalLoginDto login)
    {
        var result = await _externalSignInService.SignInExternal(login.Provider, login.IdToken);

        return ProduceLoginResponse(result);
    }

    private ActionResult<LoginResponseDto> ProduceLoginResponse((MySignInResult result, SignInData? data) loginResults)
    {
        var (result, data) = loginResults;

        return result switch
        {
            MySignInResult.Failed => Unauthorized("Username or password incorrect."),
            MySignInResult.LockedOut => Forbid("User is temporarily locked out."),
            MySignInResult.NotAllowed => Unauthorized(new { message = "Please activate your account. Check your email for the activation link." }),
            MySignInResult.Success when data is not null => Ok(new LoginResponseDto()
            {
                AccessToken = data.Token.AccessToken,
                TokenType = data.Token.TokenType,
                ExpiresIn = data.Token.GetRemainingLifetimeSeconds(),
                Username = data.Username,
                Email = data.Email,
                IsExternalLogin = data.IsExternalLogin,
                ExternalAuthenticationProvider = data.ExternalAuthenticationProvider,
                PropertyAccessList = data.PropertyAccessList?.Select(p => p).ToList()
            }),
            _ => throw new InvalidEnumArgumentException("Unknown sign-in result or sign-in data missing.")
        };
    }


    [AllowAnonymous]
    [HttpPost("SignUp")]
    [LogCreate("User Account", Description = "New user registered")]
    public async Task<ActionResult<SignUpResponseDto>> SignUp([FromBody] SignUpDto signUpDto)
    {
        if (signUpDto == null)
        {
            return BadRequest("Sign-up data is required.");
        }

        if (string.IsNullOrEmpty(signUpDto.Username) || string.IsNullOrEmpty(signUpDto.Email) || string.IsNullOrEmpty(signUpDto.Password)
            || string.IsNullOrEmpty(signUpDto.Phone))
        {
            return BadRequest("Username, email, and password and phone are required.");
        }

        var user = new IdentityUser
        {
            UserName = signUpDto.Username,
            Email = signUpDto.Email,
            PasswordHash = signUpDto.Password,
            PhoneNumber = signUpDto.Phone
        };

        var result = await _userService.SignUp(
            signUpDto.Username, 
            signUpDto.Email,
            signUpDto.Password,
            signUpDto.Phone,
            signUpDto.PropertyCode); // Pass property code to determine demo vs real property assignment

        return result.result switch
        {
            SignUpResult.Failed => Unauthorized("Username or password incorrect."),
            SignUpResult.EmailAlreadyExists => BadRequest("Email already exists. Please use a different email or try logging in."),
            SignUpResult.NotAllowed => BadRequest("Signup is not allowed. Please contact support."),
            SignUpResult.Success when result.data is not null => Ok(new SignUpResponseDto()
            {
                UserId = result.data.UserId,
                Email = result.data.Email,
            }),
            _ => throw new InvalidEnumArgumentException("Sign up failed. Please try again.")
        };
    }

    [AllowAnonymous]
    [HttpPost("ConfirmEmail")]
    [LogUpdate("User Account", Description = "User confirmed email")]
    public async Task<ActionResult> ConfirmEmail([FromQuery] int userId, [FromQuery] string token)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(token))
            return BadRequest("userId and token are required.");

        var (success, message) = await _userService.ConfirmEmail(userId, token);

        if (success)
            return Ok(new { message, success = true });
        else
            return BadRequest(new { message, success = false });
    }

    [AllowAnonymous]
    [HttpPost("ResendActivationEmail")]
    [LogCreate("User Account", Description = "Resent activation email")]
    public async Task<ActionResult> ResendActivationEmail([FromBody] ResendActivationEmailDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.Email))
            return BadRequest(new { message = "Email is required.", success = false });

        var (success, message) = await _userService.ResendActivationEmail(dto.Email);

        if (success)
            return Ok(new { message, success = true });
        else
            return BadRequest(new { message, success = false });
    }
}
