# JWT User ID Extraction - Usage Guide

This guide explains how to extract the user ID from JWT tokens in your application.

## Backend Implementation

The backend JWT authentication has been configured with the following features:

### AccountController Endpoints

1. **POST /account/login**
   - Authenticates a user with username and password
   - Returns a JWT token containing the user ID in the claims

2. **POST /account/SignUp**
   - Registers a new user
   - Returns a JWT token containing the user ID in the claims

### JWT Token Structure

The JWT token includes the following claims:
- `sub`: User ID (standard JWT claim)
- `userId`: User ID (custom claim for easier access)
- `username`: Username
- `email`: User email
- `jti`: Unique token identifier
- `iat`: Token issued at timestamp

## Frontend Implementation

### Getting the User ID

The `AuthService` provides two methods to extract user information from JWT tokens:

#### 1. Get User ID Only

```typescript
import { AuthService } from './core/auth/services/auth.service';

constructor(private authService: AuthService) {}

getUserInfo() {
  // Get just the user ID
  const userId = this.authService.getUserId();
  console.log('User ID:', userId);
}
```

#### 2. Get Full Decoded Token

```typescript
import { AuthService } from './core/auth/services/auth.service';

constructor(private authService: AuthService) {}

getFullTokenInfo() {
  // Get the entire decoded token with all claims
  const decodedToken = this.authService.getDecodedToken();
  console.log('User ID:', decodedToken.userId);
  console.log('Username:', decodedToken.username);
  console.log('Email:', decodedToken.email);
  console.log('Token expiration:', decodedToken.exp);
}
```

### Example: Using User ID in a Component

```typescript
import { Component, OnInit } from '@angular/core';
import { AuthService } from './core/auth/services/auth.service';

@Component({
  selector: 'app-my-component',
  template: `
    <div *ngIf="userId">
      <p>Current User ID: {{ userId }}</p>
    </div>
  `
})
export class MyComponent implements OnInit {
  userId: string | null = null;

  constructor(private authService: AuthService) {}

  ngOnInit() {
    // Get user ID when component initializes
    this.userId = this.authService.getUserId();
    
    // Subscribe to auth state changes
    this.authService.signInState.subscribe(userData => {
      if (userData) {
        this.userId = this.authService.getUserId();
        console.log('User logged in with ID:', this.userId);
      } else {
        this.userId = null;
        console.log('User logged out');
      }
    });
  }

  makeAuthenticatedRequest() {
    const userId = this.authService.getUserId();
    if (userId) {
      // Use the user ID in your API calls
      this.http.get(`/api/users/${userId}/profile`).subscribe(...);
    }
  }
}
```

### Example: Displaying User ID in UI

The `navbar-login-info` component has been updated to display the user ID:

```typescript
// In navbar-login-info.component.ts
ngOnInit() {
  this.sub = this.as.signInState.subscribe(userData => {
    this.isLoggedIn = userData != null;

    if (this.isLoggedIn) {
      this.username = userData.username;
      this.email = userData.email;
      this.userId = this.as.getUserId(); // Extract user ID from JWT
      this.externalLogin = userData.externalAuthenticationProvider;
      this.validityDays = Math.round(this.as.getValidityDays());
    }
  });
}
```

## Security Considerations

⚠️ **IMPORTANT**: This implementation is designed for development and demonstration purposes.

For a complete guide on security best practices and production deployment, please see [SECURITY.md](./SECURITY.md).

### Quick Security Notes:

1. **Token Storage**: JWT tokens are stored in localStorage. For higher security requirements, consider using httpOnly cookies.

2. **Token Validation**: The backend validates JWT tokens on protected endpoints using the configured JWT authentication middleware.

3. **Password Hashing**: The current implementation uses SHA256 for password hashing. **This is NOT secure for production.** See [SECURITY.md](./SECURITY.md) for proper password hashing with BCrypt or Argon2.

4. **Secret Key**: The JWT secret key is stored in `appsettings.Development.json` for development. For production, use environment variables or a secure key vault. See [SECURITY.md](./SECURITY.md) for details.

## Testing the Implementation

1. **Start the backend**:
   ```bash
   cd testAngularAPI.Server
   dotnet run
   ```

2. **Start the frontend**:
   ```bash
   cd app
   npm start
   ```

3. **Test the signup flow**:
   - Navigate to the signup page
   - Create a new account
   - After signup, the JWT token will be automatically stored
   - Click on your username in the navbar to see your user ID

4. **Test the login flow**:
   - Sign out if you're logged in
   - Log in with your credentials
   - Your user ID will be extracted from the JWT and displayed

## API Reference

### AuthService Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `getUserId()` | `string \| null` | Extracts and returns the user ID from the JWT token |
| `getDecodedToken()` | `any \| null` | Returns the fully decoded JWT token with all claims |
| `getUserToken()` | `string \| null` | Returns the raw JWT token string with Bearer prefix |
| `isSignedIn()` | `boolean` | Checks if a user is currently signed in |
| `signOut()` | `void` | Signs out the current user and clears stored tokens |

### JWT Claims Available

- `sub` - Subject (user ID)
- `userId` - User ID (custom claim)
- `username` - Username
- `email` - User email
- `jti` - JWT ID (unique identifier)
- `iat` - Issued at timestamp
- `exp` - Expiration timestamp
- `iss` - Issuer
- `aud` - Audience
