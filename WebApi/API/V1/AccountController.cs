using Microsoft.AspNetCore.Identity;
using MyWarehouse.Infrastructure.Authentication.Core.Model;
using MyWarehouse.Infrastructure.Authentication.Core.Services;
using MyWarehouse.Infrastructure.Authentication.External.Services;
using MyWarehouse.Infrastructure.Authentication.Dtos;
using MyWarehouse.Infrastructure.Authentication.Models.Dtos;
using MyWarehouse.WebApi.Authentication.Dtos;
using Azure.Core;

namespace MyWarehouse.Infrastructure.API.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("v{v:apiVersion}/account")]
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
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto login)
        => ProduceLoginResponse(
            await _userService.SignIn(login.Username, login.Password));

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
    public async Task<ActionResult<LoginResponseDto>> ExternalLogin(ExternalLoginDto login)
        => ProduceLoginResponse(
            await _externalSignInService.SignInExternal(login.Provider, login.IdToken));

    private ActionResult<LoginResponseDto> ProduceLoginResponse((MySignInResult result, SignInData? data) loginResults)
    {
        var (result, data) = loginResults;

        return result switch
        {
            MySignInResult.Failed => Unauthorized("Username or password incorrect."),
            MySignInResult.LockedOut => Forbid("User is temporarily locked out."),
            MySignInResult.NotAllowed => Forbid("User is not allowed to sign in."),
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

        var result = await _userService.SignUp(signUpDto.Username, signUpDto.Email,signUpDto.Password,signUpDto.Phone); //check if internal identity function can be used
        
        
        return result.result switch
        {
            SignUpResult.Failed => Unauthorized("Username or password incorrect."),
            SignUpResult.Success when result.data is not null => Ok(new SignUpResponseDto()
            {
                UserId = result.data.UserId,
                Email = result.data.Email,
            }),
            _ => throw new InvalidEnumArgumentException("Sign up failed. Please try again.")
        };
    }

    //[AllowAnonymous]
    //[HttpPost("ConfirmEmail")]
    //public async Task<ActionResult<SignUpResponseDto>> ConfirmEmail([FromQuery] int userId, [FromQuery] string token, CancellationToken ct)
    //{
    //    if (userId <= 0 || string.IsNullOrWhiteSpace(token))
    //        return BadRequest("userId and token are required.");

    //    var ok = await _mediator.Send(new ConfirmEmailCommand
    //    {
    //        UserId = userId,
    //        Token = token
    //    }, ct);

    //    return ok ? Ok("Email activated successfully.") : BadRequest("Invalid or expired activation link.");

    //}
}
