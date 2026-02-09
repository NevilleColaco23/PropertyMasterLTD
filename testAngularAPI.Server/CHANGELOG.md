This file explains how Visual Studio created the project.

The following steps were used to generate this project:
- Create new ASP\.NET Core Web API project.
- Update `launchSettings.json` to register the SPA proxy as a startup assembly.
- Update project file to add a reference to the frontend project and set SPA properties.
- Add project to the startup projects list.
- Write this file.

## Changes

### 2026-02-09 - User ID API Implementation with JWT Authentication
- Added User model with Id, Name, and Email fields
- Added Users collection to MongoDbContext
- Created UserController with endpoints:
  - GET /user - Get all users
  - GET /user/{id} - Get user by ID
  - GET /user/me - Get current authenticated user's ID and information (requires authentication)
  - POST /user - Create a new user
- Added JWT authentication:
  - Added Microsoft.AspNetCore.Authentication.JwtBearer package
  - Configured JWT settings in appsettings.json
  - Configured authentication middleware in Program.cs
- Created AuthController with login endpoint:
  - POST /auth/login - Login with email and get JWT token
- The /user/me endpoint demonstrates how to get the active user ID who is accessing the API

