# How to Get User ID in API

This document explains how to retrieve the authenticated user's ID in API endpoints.

## Overview

The application provides multiple ways to access user information (including user ID) in API controllers through the ASP.NET Core authentication system.

## Methods to Get User ID

### Method 1: Using Extension Methods (Recommended)

The project includes `ClaimsPrincipalExtensions` that provide convenient methods to extract user information:

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

### 1. ClaimsPrincipalExtensions.cs

Located in `testAngularAPI.Server/Extensions/ClaimsPrincipalExtensions.cs`

This file contains extension methods for `ClaimsPrincipal` that make it easy to extract:
- User ID (from NameIdentifier, sub, userId, or id claims)
- Username (from Name or username claims)
- Email (from Email claim)

### 2. User Model

Located in `testAngularAPI.Server/Model/User.cs`

Defines the User entity with:
- `Id` - Unique identifier (MongoDB ObjectId)
- `Username` - User's username
- `Email` - User's email address

### 3. UserController

Located in `testAngularAPI.Server/Controllers/UserController.cs`

Provides example endpoints demonstrating how to:
- Get current user information (`GET /api/user/me`)
- Get user profile (`GET /api/user/profile`)
- Perform actions with user ID (`POST /api/user/action`)

## Example Usage

### Get Current User Information

```bash
GET /api/user/me
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

### Get User Profile

```bash
GET /api/user/profile
```

Response:
```json
{
  "id": "507f1f77bcf86cd799439011",
  "username": "john.doe",
  "email": "john@example.com"
}
```

## Authentication Setup

For the user ID to be available, ensure that:

1. **Authentication is configured** in `Program.cs`:
   ```csharp
   builder.Services.AddAuthentication(...);
   app.UseAuthentication();
   app.UseAuthorization();
   ```

2. **JWT tokens or cookies include user claims**:
   - NameIdentifier (standard claim for user ID)
   - Name (for username)
   - Email (for email address)

3. **Requests include authentication headers**:
   ```
   Authorization: Bearer <your-jwt-token>
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

## Testing Without Authentication

For development/testing without a full authentication system:

You can test the endpoints using tools like Swagger, Postman, or curl. Without authentication, the user ID will be null or empty, which is expected behavior.

To properly test with authentication, you need to:
1. Set up JWT authentication or another authentication mechanism
2. Include valid authentication tokens in requests
3. Ensure tokens contain the necessary claims (user ID, username, email)

## See Also

- `WeatherForecastController.cs` - Shows user ID usage in an existing controller
- `ClaimsPrincipalExtensions.cs` - Extension methods source code
- `UserController.cs` - Complete examples of user ID usage
