using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using MyWarehouse.Infrastructure.API.V1;
using MyWarehouse.Infrastructure.Authentication.Core.Model;
using MyWarehouse.Infrastructure.Authentication.Core.Services;
using MyWarehouse.Infrastructure.Authentication.Dtos;
using MyWarehouse.Infrastructure.Authentication.External.Model;
using MyWarehouse.Infrastructure.Authentication.External.Services;
using MyWarehouse.Infrastructure.Authentication.Models.Dtos;
using MyWarehouse.WebApi.Authentication.Dtos;
using System.ComponentModel;

namespace APIUnitProject.Tests.ControllersTest
{
    public class AccountControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IExternalSignInService> _mockExternalSignInService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            // Arrange (common setup for all tests in this class)
            _mockUserService = new Mock<IUserService>();
            _mockExternalSignInService = new Mock<IExternalSignInService>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Provide guest credentials so GuestLogin endpoint behaves predictably in tests
            _mockConfiguration.Setup(c => c["GuestSettings:Username"]).Returns("guest@propertymaster.demo");
            _mockConfiguration.Setup(c => c["GuestSettings:Password"]).Returns("Guest@Demo2024!");

            _controller = new AccountController(_mockUserService.Object, _mockExternalSignInService.Object, _mockConfiguration.Object);
        }

        #region Login Endpoint Tests (/v{v:apiVersion}/account/login)

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithLoginResponseDto()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "testuser", Password = "Password123!" };
            var expectedToken = new TokenModel(
            tokenType: "Bearer",
            accessToken: "some_access_token",
            expiresAt: DateTime.UtcNow.AddHours(1)
            );
            var signInData = new SignInData
            {
                Token = expectedToken,
                Username = loginDto.Username,
                Email = "test@example.com",
                PropertyAccessList = new List<int> { 100 }
            };

            _mockUserService.Setup(s => s.SignIn(loginDto.Username, loginDto.Password))
                            .ReturnsAsync((MySignInResult.Success, signInData));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var responseDto = Assert.IsType<LoginResponseDto>(okResult.Value);

            Assert.Equal(signInData.Username, responseDto.Username);
            Assert.Equal(signInData.Email, responseDto.Email);
            Assert.Equal(signInData.Token.AccessToken, responseDto.AccessToken);
            Assert.Equal(signInData.Token.TokenType, responseDto.TokenType);
            Assert.True(responseDto.ExpiresIn > 0); // Check that it's a positive value
            Assert.False(responseDto.IsExternalLogin);
            Assert.Null(responseDto.ExternalAuthenticationProvider);
            Assert.Contains(100, collection: responseDto.PropertyAccessList);

            // Verify that SignIn was called exactly once with the correct parameters
            _mockUserService.Verify(s => s.SignIn(loginDto.Username, loginDto.Password), Times.Once);
        }

        [Theory]
        [InlineData(MySignInResult.Failed, "Username or password incorrect.")]
        [InlineData(MySignInResult.LockedOut, "User is temporarily locked out.")]
        [InlineData(MySignInResult.NotAllowed, "User is not allowed to sign in.")]
        public async Task Login_SignInFailure_ReturnsUnauthorizedOrForbid(MySignInResult signInResult, string expectedMessage)
        {
            // Arrange
            var loginDto = new LoginDto { Username = "testuser", Password = "wrongpassword" };

            _mockUserService.Setup(s => s.SignIn(loginDto.Username, loginDto.Password))
                            .ReturnsAsync((signInResult, null)); // No SignIn data on failure

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            if (signInResult == MySignInResult.LockedOut)
            {
                Assert.IsType<ForbidResult>(result.Result);
                // ForbidResult does not typically carry a message body by default
            }
            else // Failed, NotAllowed
            {
                var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
                if (signInResult == MySignInResult.Failed)
                {
                    Assert.Equal(expectedMessage, unauthorizedResult.Value);
                }
            }

            _mockUserService.Verify(s => s.SignIn(loginDto.Username, loginDto.Password), Times.Once);
        }

        [Fact]
        public async Task Login_UnknownSignInResult_ThrowsInvalidEnumArgumentException()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "testuser", Password = "somepassword" };
            var unknownResult = (MySignInResult)999; // Simulate an unexpected enum value

            _mockUserService.Setup(s => s.SignIn(loginDto.Username, loginDto.Password))
                            .ReturnsAsync((unknownResult, null));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidEnumArgumentException>(() => _controller.Login(loginDto));

            _mockUserService.Verify(s => s.SignIn(loginDto.Username, loginDto.Password), Times.Once);
        }

        #endregion

        #region LoginForm Endpoint Tests (/account/oauth2/access_token)

        [Fact]
        public async Task LoginForm_ValidCredentials_ReturnsOkWithOAuth2Response()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "oauthuser", Password = "oauthpassword" };
            var expectedToken = new TokenModel(
                tokenType: "Bearer",
                accessToken: "oauth_access_token",
                expiresAt: DateTime.UtcNow.AddHours(2)
            );
            var signInData = new SignInData { Token = expectedToken };

            _mockUserService.Setup(s => s.SignIn(loginDto.Username, loginDto.Password))
                            .ReturnsAsync((MySignInResult.Success, signInData));

            // Act
            var result = await _controller.LoginForm(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            // *** CHANGE THIS LINE ***
            // Cast directly to the public DTO type
            var responseDto = Assert.IsType<OAuth2TokenResponseDto>(okResult.Value);

            Assert.NotNull(responseDto);
            // *** Access properties using their PascalCase names as defined in OAuth2TokenResponseDto ***
            Assert.Equal(expectedToken.AccessToken, responseDto.AccessToken);
            Assert.Equal(expectedToken.TokenType, responseDto.TokenType);
            Assert.True(responseDto.ExpiresIn > 0);

            _mockUserService.Verify(s => s.SignIn(loginDto.Username, loginDto.Password), Times.Once);

        }

        [Theory]
        [InlineData(MySignInResult.Failed)]
        [InlineData(MySignInResult.LockedOut)]
        [InlineData(MySignInResult.NotAllowed)]
        public async Task LoginForm_SignInFailure_ReturnsUnauthorized(MySignInResult signInResult)
        {
            // Arrange
            var loginDto = new LoginDto { Username = "oauthuser", Password = "wrongoauthpassword" };

            _mockUserService.Setup(s => s.SignIn(loginDto.Username, loginDto.Password))
                            .ReturnsAsync((signInResult, null));

            // Act
            var result = await _controller.LoginForm(loginDto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result); // This endpoint returns Unauthorized for any non-success
            _mockUserService.Verify(s => s.SignIn(loginDto.Username, loginDto.Password), Times.Once);
        }

        #endregion

        #region ExternalLogin Endpoint Tests

        [Fact]
        public async Task ExternalLogin_ValidCredentials_ReturnsOkWithLoginResponseDto()
        {

            // Arrange
            var externalLoginDto = new ExternalLoginDto { Provider = ExternalAuthenticationProvider.Google, IdToken = "some_google_id_token" };
            var expectedToken = new TokenModel(
            tokenType: "Bearer",
            accessToken: "some_access_token",
            expiresAt: DateTime.UtcNow.AddHours(1)
            );
            var signInData = new SignInData
            {
                Token = expectedToken,
                Username = "externaluser",
                Email = "external@example.com",
                ExternalAuthenticationProvider = "Google"
            };

            _mockExternalSignInService.Setup(s => s.SignInExternal(externalLoginDto.Provider, externalLoginDto.IdToken))
                                      .ReturnsAsync((MySignInResult.Success, signInData));

            // Act
            var result = await _controller.ExternalLogin(externalLoginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var responseDto = Assert.IsType<LoginResponseDto>(okResult.Value);

            Assert.Equal(signInData.Username, responseDto.Username);
            Assert.Equal(signInData.ExternalAuthenticationProvider, responseDto.ExternalAuthenticationProvider);
            Assert.True(responseDto.IsExternalLogin);
            Assert.Equal(expectedToken.AccessToken, responseDto.AccessToken);

            _mockExternalSignInService.Verify(s => s.SignInExternal(externalLoginDto.Provider, externalLoginDto.IdToken), Times.Once);
        }

        [Theory]
        [InlineData(MySignInResult.Failed, typeof(UnauthorizedObjectResult))]
        [InlineData(MySignInResult.LockedOut, typeof(ForbidResult))]
        [InlineData(MySignInResult.NotAllowed, typeof(UnauthorizedObjectResult))]
        public async Task ExternalLogin_SignInFailure_ReturnsCorrectActionResult(MySignInResult signInResult, Type expectedType)
        {
            // Arrange
            var externalLoginDto = new ExternalLoginDto { Provider = ExternalAuthenticationProvider.Google, IdToken = "invalid_id_token" };

            _mockExternalSignInService.Setup(s => s.SignInExternal(externalLoginDto.Provider, externalLoginDto.IdToken))
                                      .ReturnsAsync((signInResult, null));

            // Act
            var result = await _controller.ExternalLogin(externalLoginDto);

            // Assert
            Assert.IsType(expectedType, result.Result);

            _mockExternalSignInService.Verify(s => s.SignInExternal(externalLoginDto.Provider, externalLoginDto.IdToken), Times.Once);
        }

        #endregion

        #region SignUp Endpoint Tests

        [Fact]
        public async Task SignUp_ValidDto_ReturnsOkWithSignUpResponseDto()
        {
            // Arrange
            var signUpDto = new SignUpDto
            {
                Username = "newuser",
                Email = "new@example.com",
                Password = "SecurePassword123!",
                Phone = "1234567890",
            };
            // *** CHANGE THIS LINE ***
            // Use SignUpResultData, not SignUpData
            var signUpResultData = new SignUpResultData
            {
                UserId = 123, // Use a realistic mocked ID
                Email = signUpDto.Email
            };

            _mockUserService.Setup(s => s.SignUp(
                signUpDto.Username,
                signUpDto.Email,
                signUpDto.Password,
                It.IsAny<string>(),  // phoneNumber parameter
                It.IsAny<string?>())) // propertyCode optional parameter (must be explicit for Moq)
                .ReturnsAsync((SignUpResult.Success, signUpResultData));

            // Act
            var result = await _controller.SignUp(signUpDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var responseDto = Assert.IsType<SignUpResponseDto>(okResult.Value);

            Assert.Equal(signUpResultData.UserId, responseDto.UserId);
            Assert.Equal(signUpResultData.Email, responseDto.Email);

            _mockUserService.Verify(s => s.SignUp(
                signUpDto.Username,
                signUpDto.Email,
                signUpDto.Password,
                It.IsAny<string>(),
                It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task SignUp_NullDto_ReturnsBadRequest()
        {
            // Arrange
            SignUpDto signUpDto = null;

            // Act
            var result = await _controller.SignUp(signUpDto!); // Cast to avoid compiler warning for null

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Sign-up data is required.", badRequestResult.Value);

            // Verify that no service methods were called
            _mockUserService.Verify(s => s.SignUp(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
        }

        [Theory]
        [InlineData("user", "", "pass", "1234567890", "Username, email, and password and phone are required.")]
        [InlineData("", "email@test.com", "pass", "1234567890", "Username, email, and password and phone are required.")]
        [InlineData("user", "email@test.com", "", "1234567890", "Username, email, and password and phone are required.")]
        [InlineData("user", "email@test.com", "pass", "", "Username, email, and password and phone are required.")]
        [InlineData("", "", "", "", "Username, email, and password and phone are required.")]
        public async Task SignUp_MissingRequiredFields_ReturnsBadRequest(string username, string email, string password, string phone, string expectedMessage)
        {
            // Arrange
            var signUpDto = new SignUpDto
            {
                Username = username,
                Email = email,
                Password = password,
                Phone = phone
            };

            // Act
            var result = await _controller.SignUp(signUpDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(expectedMessage, badRequestResult.Value);

            _mockUserService.Verify(s => s.SignUp(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task SignUp_UserCreationFailed_ReturnsUnauthorized()
        {
            // Arrange
            var signUpDto = new SignUpDto
            {
                Username = "existinguser",
                Email = "existing@example.com",
                Password = "Password123!",
                Phone = "1234567890"
            };

            _mockUserService.Setup(s => s.SignUp(
                signUpDto.Username,
                signUpDto.Email,
                signUpDto.Password,
                It.IsAny<string>(),
                It.IsAny<string?>()))
                .ReturnsAsync((SignUpResult.Failed, null)); // Simulate failure

            // Act
            var result = await _controller.SignUp(signUpDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Username or password incorrect.", unauthorizedResult.Value);

            _mockUserService.Verify(s => s.SignUp(
                signUpDto.Username,
                signUpDto.Email,
                signUpDto.Password,
                It.IsAny<string>(),
                It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task SignUp_UnknownSignUpResult_ThrowsInvalidEnumArgumentException()
        {
            // Arrange
            var signUpDto = new SignUpDto
            {
                Username = "user",
                Email = "email@test.com",
                Password = "pass",
                Phone = "1234567890"
            };
            var unknownResult = (SignUpResult)999;

            _mockUserService.Setup(s => s.SignUp(
                signUpDto.Username,
                signUpDto.Email,
                signUpDto.Password,
                It.IsAny<string>(),
                It.IsAny<string?>()))
                .ReturnsAsync((unknownResult, null));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidEnumArgumentException>(() => _controller.SignUp(signUpDto));

            _mockUserService.Verify(s => s.SignUp(
                signUpDto.Username,
                signUpDto.Email,
                signUpDto.Password,
                It.IsAny<string>(),
                It.IsAny<string?>()), Times.Once);
        }

        #endregion
    }
}

