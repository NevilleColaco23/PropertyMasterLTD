# Quick Start Guide: JWT User ID Extraction

This guide shows you how to quickly get started with JWT authentication and user ID extraction.

## Prerequisites

- .NET 8.0 SDK
- Node.js and npm
- MongoDB instance (configured in appsettings)

## Backend Setup

1. **Restore NuGet packages:**
   ```bash
   cd testAngularAPI.Server
   dotnet restore
   ```

2. **Run the backend:**
   ```bash
   dotnet run
   ```

   The API will be available at `https://localhost:5001` or `http://localhost:5000`

## Frontend Setup

1. **Install npm packages:**
   ```bash
   cd app
   npm install
   ```

2. **Run the Angular app:**
   ```bash
   npm start
   ```

   The app will be available at `http://localhost:4200`

## Using the JWT User ID Extraction

### 1. Sign Up a New User

Navigate to the signup page and create a new account. The backend will:
- Hash the password (using SHA256 in development)
- Create a user record in MongoDB
- Generate a JWT token with the user ID in the claims
- Return the token to the frontend

### 2. Access User ID in Your Code

After authentication, you can get the user ID anywhere in your Angular application:

```typescript
import { Component, OnInit } from '@angular/core';
import { AuthService } from './core/auth/services/auth.service';

@Component({
  selector: 'app-example',
  template: `
    <div *ngIf="userId">
      <h3>Welcome!</h3>
      <p>Your User ID: {{ userId }}</p>
    </div>
  `
})
export class ExampleComponent implements OnInit {
  userId: string | null = null;

  constructor(private authService: AuthService) {}

  ngOnInit() {
    // Get the user ID from the JWT token
    this.userId = this.authService.getUserId();
    console.log('User ID:', this.userId);
  }
}
```

### 3. View User Information

Click on your username in the navbar to see a modal displaying:
- Username
- Email address
- **User ID** (extracted from JWT)
- Session validity

## Available API Endpoints

### POST /account/SignUp
Create a new user account.

**Request:**
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "securePassword123"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "username": "johndoe",
  "email": "john@example.com",
  "userId": "507f1f77bcf86cd799439011",
  "isExternalLogin": "false",
  "externalAuthenticationProvider": ""
}
```

### POST /account/login
Authenticate an existing user.

**Request:**
```json
{
  "username": "johndoe",
  "password": "securePassword123"
}
```

**Response:** Same as SignUp

## AuthService Methods

The `AuthService` provides several methods for working with JWT tokens:

| Method | Return Type | Description |
|--------|-------------|-------------|
| `getUserId()` | `string \| null` | Extracts user ID from JWT token |
| `getDecodedToken()` | `JwtPayload \| null` | Returns full decoded token |
| `getUserToken()` | `string \| null` | Returns raw token with Bearer prefix |
| `isSignedIn()` | `boolean` | Checks if user is authenticated |
| `signOut()` | `void` | Logs out and clears token |

## JWT Token Structure

The JWT token contains the following claims:

```typescript
{
  "sub": "507f1f77bcf86cd799439011",      // User ID (standard claim)
  "userId": "507f1f77bcf86cd799439011",   // User ID (custom claim)
  "username": "johndoe",                   // Username
  "email": "john@example.com",             // Email
  "jti": "unique-token-id",                // JWT ID
  "iat": 1234567890,                       // Issued at timestamp
  "exp": 1234571490,                       // Expiration timestamp
  "iss": "testAngularAPI",                 // Issuer
  "aud": "testAngularAPIUsers"             // Audience
}
```

## Testing the Implementation

### Test with Swagger (Backend only)

1. Navigate to `https://localhost:5001/swagger`
2. Use the `/account/SignUp` endpoint to create a test user
3. Copy the returned `accessToken`
4. Click "Authorize" and enter: `Bearer {accessToken}`
5. Test protected endpoints

### Test with Frontend

1. Start both backend and frontend
2. Navigate to `http://localhost:4200`
3. Sign up with test credentials
4. Click on your username in the navbar
5. Verify the user ID is displayed

### Test Programmatically

```typescript
// After login/signup
const userId = this.authService.getUserId();
console.log('User ID:', userId);  // Output: "507f1f77bcf86cd799439011"

// Get full token details
const token = this.authService.getDecodedToken();
console.log('Token claims:', token);
// Output: { sub: "...", userId: "...", username: "...", ... }
```

## Common Use Cases

### 1. Making Authenticated API Requests

The `AuthInterceptor` automatically adds the JWT token to all API requests:

```typescript
// The token is automatically added to the Authorization header
this.http.get('/api/users/profile').subscribe(data => {
  console.log('User profile:', data);
});
```

### 2. Getting User-Specific Data

```typescript
getUserProfile() {
  const userId = this.authService.getUserId();
  if (userId) {
    return this.http.get(`/api/users/${userId}/profile`);
  }
  return of(null);
}
```

### 3. Authorization Guards

```typescript
@Injectable()
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): boolean {
    if (this.authService.isSignedIn()) {
      return true;
    }
    this.router.navigate(['/login']);
    return false;
  }
}
```

### 4. Displaying User-Specific Content

```typescript
<div *ngIf="authService.isSignedIn()">
  <h3>Welcome, {{ username }}!</h3>
  <p>Your ID: {{ userId }}</p>
  <button (click)="loadMyData()">Load My Data</button>
</div>
```

## Troubleshooting

### "Invalid token" error
- Ensure the token hasn't expired (default: 1 hour)
- Check that the JWT secret key matches in backend and token
- Verify the token is properly formatted with Bearer prefix

### User ID returns null
- Check if user is logged in: `authService.isSignedIn()`
- Verify token exists: `authService.getUserToken()`
- Check browser console for JWT decode errors

### Token not being sent with requests
- Ensure `AuthInterceptor` is registered in app module
- Verify request URL matches `environment.baseHost`
- Check browser network tab for Authorization header

## Next Steps

- Read [JWT_USER_ID_GUIDE.md](./JWT_USER_ID_GUIDE.md) for detailed implementation guide
- Review [SECURITY.md](./SECURITY.md) for production deployment considerations
- Implement additional features like password reset, email verification, etc.

## Important Security Note

⚠️ **This implementation uses simplified password hashing for demonstration purposes.**

Before deploying to production, please:
1. Replace SHA256 with BCrypt or Argon2
2. Move JWT secret to environment variables
3. Review all security recommendations in [SECURITY.md](./SECURITY.md)

See the [SECURITY.md](./SECURITY.md) file for complete security guidelines.
