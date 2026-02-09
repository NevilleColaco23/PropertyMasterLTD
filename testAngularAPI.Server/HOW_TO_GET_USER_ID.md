# How to Get User ID in API

This document explains how to retrieve the authenticated user's ID in API endpoints when using JWT authentication.

## Overview

The application uses **JWT (JSON Web Token) authentication** configured in `Program.cs`. User information (including user ID) is extracted from JWT claims that are automatically populated by ASP.NET Core when a valid JWT token is provided in the request.

## JWT Authentication Setup

The API is configured with JWT Bearer authentication:

```csharp
// In Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true
        };
    });
```

JWT settings are configured in `appsettings.json`:
- **Key**: Secret key for signing tokens
- **Issuer**: Token issuer (testAngularAPI)
- **Audience**: Token audience (testAngularAPIUsers)
- **ExpiryInMinutes**: Token lifetime (60 minutes)

## How JWT Tokens Work

When a client authenticates, they receive a JWT token containing claims (user information). This token is included in subsequent requests via the Authorization header:

```
Authorization: Bearer <jwt-token>
```

ASP.NET Core automatically validates the token and populates the `User` ClaimsPrincipal with the claims from the JWT token.

## Methods to Get User ID

### Method 1: Using Extension Methods (Recommended)

The project includes `ClaimsPrincipalExtensions` that provide convenient methods to extract user information from JWT claims:

```csharp
using testAngularAPI.Server.Extensions;

[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult GetData()
    {
        // Get user ID from the authenticated user
        var userId = User.GetUserId();
        
        // Get username
        var username = User.GetUsername();
        
        // Get email
        var email = User.GetEmail();
        
        // Use the user ID to fetch user-specific data
        // ... your business logic here
        
        return Ok(new { userId, username, email });
    }
}
```

### Method 2: Direct Access to Claims

You can also access claims directly from the `User` property (which is a `ClaimsPrincipal`):

```csharp
[HttpGet]
public IActionResult GetData()
{
    // Access specific claim types
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email = User.FindFirst(ClaimTypes.Email)?.Value;
    var name = User.FindFirst(ClaimTypes.Name)?.Value;
    
    return Ok(new { userId, email, name });
}
```

### Method 3: Using User.Identity

For basic authentication scenarios:

```csharp
[HttpGet]
public IActionResult GetData()
{
    // Check if user is authenticated
    if (User.Identity?.IsAuthenticated != true)
    {
        return Unauthorized();
    }
    
    // Get username from identity
    var username = User.Identity.Name;
    
    return Ok(new { username });
}
```

## Key Components

### 1. JWT Authentication (Program.cs)

JWT Bearer authentication is configured to validate tokens and extract claims automatically.

### 2. ClaimsPrincipalExtensions.cs

Located in `testAngularAPI.Server/Extensions/ClaimsPrincipalExtensions.cs`

This file contains extension methods for `ClaimsPrincipal` that make it easy to extract user information from JWT claims:
- User ID (from NameIdentifier, sub, userId, or id claims)
- Username (from Name or username claims)
- Email (from Email claim)

These methods check multiple common JWT claim types to ensure compatibility with different JWT token formats.

### 2. User Model

Located in `testAngularAPI.Server/Model/User.cs`

Defines the User entity with:
- `Id` - Unique identifier (MongoDB ObjectId)
- `Username` - User's username
- `Email` - User's email address

### 3. UserController

Located in `testAngularAPI.Server/Controllers/UserController.cs`

Provides example endpoints demonstrating how to:
- Get current user information from JWT (`GET /api/user/me`)
- Get user profile with authentication check (`GET /api/user/profile`)
- Perform actions with user ID from JWT (`POST /api/user/action`)

## Example Usage

### Get Current User Information (from JWT token)

```bash
GET /api/user/me
Authorization: Bearer <your-jwt-token>
```

Response:
```json
{
  "userId": "507f1f77bcf86cd799439011",
  "username": "john.doe",
  "email": "john@example.com",
  "identityName": "john.doe",
  "isAuthenticated": true,
  "message": "This demonstrates how to get user ID and information from the API request"
}
```

### Get User Profile (from JWT token)

```bash
GET /api/user/profile
Authorization: Bearer <your-jwt-token>
```

Response:
```json
{
  "id": "507f1f77bcf86cd799439011",
  "username": "john.doe",
  "email": "john@example.com"
}
```

## JWT Token Structure

When creating JWT tokens (typically in your authentication/login endpoint), include these standard claims:

```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id),  // User ID
    new Claim(ClaimTypes.Name, user.Username),       // Username
    new Claim(ClaimTypes.Email, user.Email),         // Email
    // Add other claims as needed
};

var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Key"]));
var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

var token = new JwtSecurityToken(
    issuer: jwtSettings["Issuer"],
    audience: jwtSettings["Audience"],
    claims: claims,
    expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtSettings["ExpiryInMinutes"])),
    signingCredentials: credentials
);

var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
```

## Authentication Setup

The JWT authentication is already configured in your repository:

1. **JWT Bearer Authentication** is configured in `Program.cs`:
   ```csharp
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options => { ... });
   
   app.UseAuthentication();  // This must come before UseAuthorization
   app.UseAuthorization();
   ```

2. **JWT settings** are in `appsettings.json`:
   ```json
   {
     "Jwt": {
       "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256",
       "Issuer": "testAngularAPI",
       "Audience": "testAngularAPIUsers",
       "ExpiryInMinutes": 60
     }
   }
   ```

3. **Requests must include JWT token** in the Authorization header:
   ```
   Authorization: Bearer <jwt-token>
   ```

## Common Claim Types

When working with user claims, here are the standard claim types:

- `ClaimTypes.NameIdentifier` - User's unique identifier (user ID)
- `ClaimTypes.Name` - User's name/username
- `ClaimTypes.Email` - User's email address
- `ClaimTypes.Role` - User's role(s)
- `sub` - Subject claim (often used in JWT for user ID)

## Best Practices

1. **Always check if user is authenticated** before accessing user information
2. **Use extension methods** for cleaner, more maintainable code
3. **Log user actions** with user ID for audit trails
4. **Handle missing claims gracefully** (claims may not always be present)
5. **Validate user authorization** for sensitive operations
6. **Protect sensitive JWT keys** - Store the JWT Key in environment variables or Azure Key Vault in production

## Testing with JWT Authentication

### Without Authentication

Without a JWT token, the endpoints will return empty/null user information or 401 Unauthorized (depending on the endpoint):

```bash
curl http://localhost:5000/api/user/me
# Returns: { "userId": null, "username": null, ..., "isAuthenticated": false }

curl http://localhost:5000/api/user/profile
# Returns: { "message": "User ID not found. Please ensure you are authenticated." }
```

### With Authentication

To test with JWT authentication:

1. **Create a login/authentication endpoint** that generates JWT tokens with user claims
2. **Get a JWT token** from your authentication endpoint
3. **Include the token in requests**:

```bash
curl -H "Authorization: Bearer <your-jwt-token>" http://localhost:5000/api/user/me
```

### Example: Creating JWT Tokens for Testing

You can create a simple endpoint to generate test tokens (for development only):

```csharp
[HttpPost("test-token")]
public IActionResult GenerateTestToken([FromBody] string userId)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Name, "test-user"),
        new Claim(ClaimTypes.Email, "test@example.com")
    };
    
    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: credentials
    );
    
    return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
}
```

## See Also

- `Program.cs` - JWT authentication configuration
- `appsettings.json` - JWT settings (Key, Issuer, Audience)
- `WeatherForecastController.cs` - Shows user ID usage in an existing controller
- `ClaimsPrincipalExtensions.cs` - Extension methods source code
- `UserController.cs` - Complete examples of user ID usage
