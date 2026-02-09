# User Authentication API - Getting Active User ID

This API demonstrates how to get the active user ID who is accessing the API using JWT authentication.

## Features

- JWT-based authentication
- User management (Create, Read)
- Get current authenticated user information

## API Endpoints

### Authentication

#### Login
```
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "optional"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "507f1f77bcf86cd799439011",
  "name": "John Doe",
  "email": "user@example.com"
}
```

### User Management

#### Get Current User (Active User ID)
**This endpoint demonstrates how to get the user ID of the currently authenticated user accessing the API**

```
GET /user/me
Authorization: Bearer <your-jwt-token>
```

Response:
```json
{
  "userId": "507f1f77bcf86cd799439011",
  "name": "John Doe",
  "email": "user@example.com",
  "message": "This is the active user accessing the API"
}
```

#### Create User
```
POST /user
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com"
}
```

#### Get All Users
```
GET /user
```

#### Get User by ID
```
GET /user/{id}
```

## How It Works

1. **Create a user** using `POST /user` endpoint
2. **Login** using `POST /auth/login` with the user's email to get a JWT token
3. **Use the token** in the Authorization header as `Bearer <token>` for authenticated requests
4. **Call** `GET /user/me` to get the current authenticated user's ID and information

The user ID is extracted from the JWT token claims (`ClaimTypes.NameIdentifier`) in the controller.

## JWT Configuration

JWT settings are configured in `appsettings.json`:
- **Key**: Secret key for signing tokens (should be kept secure in production)
- **Issuer**: Token issuer
- **Audience**: Token audience
- **ExpiryInMinutes**: Token expiration time

## Example Usage

```bash
# 1. Create a user
curl -X POST http://localhost:5128/user \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com"}'

# 2. Login to get JWT token
curl -X POST http://localhost:5128/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"john@example.com"}'

# 3. Get current user info (replace TOKEN with actual token from step 2)
curl -X GET http://localhost:5128/user/me \
  -H "Authorization: Bearer TOKEN"
```

## Security Note

This is a simplified implementation for demonstration purposes. In production:
- Implement proper password hashing (e.g., using BCrypt)
- Use HTTPS
- Store JWT secret in secure configuration (Azure Key Vault, AWS Secrets Manager, etc.)
- Implement refresh tokens
- Add rate limiting
- Validate password strength
