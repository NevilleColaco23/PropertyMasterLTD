# JWT User ID Extraction - Implementation Complete ✅

This repository now includes a complete JWT authentication system with user ID extraction functionality.

## 📚 Documentation

### Quick Start
- **[QUICKSTART.md](./QUICKSTART.md)** - Get up and running in minutes with step-by-step instructions

### Detailed Guides
- **[JWT_USER_ID_GUIDE.md](./JWT_USER_ID_GUIDE.md)** - Complete guide on using JWT and extracting user IDs
- **[SECURITY.md](./SECURITY.md)** - Security considerations and production deployment checklist

## 🎯 What's Implemented

### Backend (ASP.NET Core)
- ✅ JWT Bearer authentication middleware
- ✅ User model with MongoDB integration
- ✅ `/account/login` endpoint for authentication
- ✅ `/account/SignUp` endpoint for user registration
- ✅ JWT token generation with userId claim
- ✅ Password hashing (development implementation)

### Frontend (Angular)
- ✅ jwt-decode package integration
- ✅ `getUserId()` method to extract user ID from JWT
- ✅ `getDecodedToken()` method to get all JWT claims
- ✅ User ID display in navbar user info modal
- ✅ Comprehensive unit tests
- ✅ TypeScript interfaces for type safety

## 🚀 Quick Example

After logging in, extract the user ID anywhere in your Angular app:

```typescript
import { AuthService } from './core/auth/services/auth.service';

constructor(private authService: AuthService) {}

ngOnInit() {
  const userId = this.authService.getUserId();
  console.log('User ID:', userId);  // e.g., "507f1f77bcf86cd799439011"
}
```

## 🔧 Getting Started

1. **Clone and setup:**
   ```bash
   git clone https://github.com/NevilleColaco23/testAngularAPIDocker.git
   cd testAngularAPIDocker
   ```

2. **Backend:**
   ```bash
   cd testAngularAPI.Server
   dotnet restore
   dotnet run
   ```

3. **Frontend:**
   ```bash
   cd app
   npm install
   npm start
   ```

4. **Navigate to** `http://localhost:4200` and sign up!

## 📖 How It Works

1. User signs up or logs in through the Angular frontend
2. Backend validates credentials and generates a JWT token containing:
   - `userId` - User's unique ID
   - `username` - User's username
   - `email` - User's email
   - Other standard JWT claims
3. Frontend stores the token in localStorage
4. `AuthService.getUserId()` decodes the token and extracts the user ID
5. The user ID is available throughout the application

## 🔐 Security Notes

⚠️ **Important:** This implementation uses simplified security for demonstration purposes.

Before production deployment:
- Replace SHA256 password hashing with BCrypt or Argon2
- Move JWT secret key to environment variables
- Review and implement all recommendations in [SECURITY.md](./SECURITY.md)

## 📋 API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/account/login` | POST | Authenticate user and return JWT token |
| `/account/SignUp` | POST | Register new user and return JWT token |

## 🧪 Testing

Run Angular unit tests:
```bash
cd app
npm test
```

Run backend:
```bash
cd testAngularAPI.Server
dotnet build
dotnet run
```

## 📦 Key Dependencies

**Backend:**
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
- MongoDB.Driver 3.4.0
- System.IdentityModel.Tokens.Jwt 7.0.3

**Frontend:**
- jwt-decode (latest)
- @angular/core 20.0.4

## 🤝 Contributing

See [SECURITY.md](./SECURITY.md) for security best practices when contributing.

## 📄 License

This project is for demonstration and educational purposes.

## 💡 Need Help?

- Check [QUICKSTART.md](./QUICKSTART.md) for setup instructions
- See [JWT_USER_ID_GUIDE.md](./JWT_USER_ID_GUIDE.md) for detailed usage examples
- Review [SECURITY.md](./SECURITY.md) for production deployment

---

**Implementation Status:** ✅ Complete and tested
- Backend: JWT authentication with userId in token
- Frontend: getUserId() method to extract user ID from JWT
- Documentation: Comprehensive guides and security checklist
- Tests: Unit tests for JWT decoding functionality
- Security: CodeQL scan passed (0 alerts)
