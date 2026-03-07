# Property Master V4.0 (MyWarehouse)

[![.NET 6.0](https://img.shields.io/badge/.NET-6.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular 21](https://img.shields.io/badge/Angular-21-DD0031?logo=angular)](https://angular.io/)
[![MongoDB](https://img.shields.io/badge/MongoDB-6.0-47A248?logo=mongodb)](https://www.mongodb.com/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-7.2-FF6600?logo=rabbitmq)](https://www.rabbitmq.com/)
[![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?logo=docker)](https://www.docker.com/)

A comprehensive property management system built with .NET 6 backend, Angular 21 frontend, and microservices architecture featuring real-time messaging, email notifications, and access logging capabilities.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [Deployment](#deployment)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Contributing](#contributing)

## 🎯 Overview

Property Master V4.0 is a full-stack property management application designed with a modern microservices architecture. The system handles property listings, partner management, bookings, transactions, and includes advanced features like real-time access logging, email notifications, and comprehensive API versioning.

### Key Highlights

- **Clean Architecture**: Domain-Driven Design (DDD) with CQRS pattern using MediatR
- **Microservices**: Separate worker services for access logging and email processing
- **Real-time Communication**: SignalR integration for live updates
- **Message Queue**: RabbitMQ for asynchronous event processing
- **NoSQL Database**: MongoDB for flexible data storage
- **Authentication**: JWT-based authentication with Google OAuth2 support
- **API Versioning**: RESTful API with Swagger/OpenAPI documentation
- **Containerization**: Full Docker support with docker-compose orchestration
- **Cloud Deployment**: Configured for Railway.app and Vercel deployment

## 🏗️ Architecture

The application follows a **Clean Architecture** pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│  ┌──────────────────┐              ┌──────────────────┐    │
│  │  Angular 21 SPA  │              │   WebAPI (.NET)  │    │
│  │   (Frontend)     │◄────────────►│   Controllers    │    │
│  └──────────────────┘              └──────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  MediatR (CQRS)  │  AutoMapper  │  FluentValidation │  │
│  │  Commands/Queries │  Behaviors   │  Validators       │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Entities  │  Value Objects  │  Domain Events        │  │
│  │  Product   │  Partner        │  Property             │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  MongoDB   │  RabbitMQ    │  JWT Auth  │  Serilog   │  │
│  │  Identity  │  AutoMapper  │  Azure KV  │  Redis     │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Worker Services                           │
│  ┌───────────────────────┐  ┌──────────────────────────┐   │
│  │ AccessLogWorker       │  │ EmailWorker              │   │
│  │ (.NET 8 Background)   │  │ (.NET 8 Background)      │   │
│  │ Consumes RabbitMQ     │  │ Processes Email Outbox   │   │
│  └───────────────────────┘  └──────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### Message Flow Architecture

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│   WebAPI     │────────►│   RabbitMQ   │────────►│ AccessLog    │
│              │ Publish │   Exchange   │ Consume │   Worker     │
│              │         │   + Queue    │         │              │
└──────────────┘         └──────────────┘         └──────────────┘
                                                          │
                                                          ▼
                                                   ┌──────────────┐
                                                   │   MongoDB    │
                                                   │   Storage    │
                                                   └──────────────┘
```

## 🛠️ Technology Stack

### Backend Technologies

#### Core Framework
- **.NET 6.0** - Main WebAPI framework
- **.NET 8.0** - Worker services (AccessLogWorker, EmailWorker)
- **C# 10.0** - Programming language
- **ASP.NET Core 6.0** - Web framework

#### Architecture & Patterns
- **MediatR 10.0.1** - CQRS and Mediator pattern implementation
- **AutoMapper 11.0.0** - Object-to-object mapping
- **FluentValidation 11.1.0** - Input validation
- **Clean Architecture** - Separation of concerns
- **Domain-Driven Design (DDD)** - Domain modeling

#### Database & Storage
- **MongoDB 2.19.1 / 3.6.0** - Primary NoSQL database
  - Used for: Properties, Partners, Products, Bookings, Transactions, Users, Access Logs
  - Custom repository pattern with Unit of Work
  - Connection string: Railway.app hosted MongoDB
- **Entity Framework Core 6.0.6** - ORM (for SQL Server support)
- **AspNetCore.Identity.MongoDbCore 3.1.2** - Identity management with MongoDB
- **Redis** - Caching layer (configured, connection: `machost:6379`)

#### Message Queue & Events
- **RabbitMQ.Client 7.2.0/7.2.1** - Message broker
  - **CloudAMQP** (raccoon.lmq.cloudamqp.com) - Hosted RabbitMQ service
  - Exchange: `accesslog.exchange` (Direct)
  - Queue: `accesslog.queue`
  - Routing Key: `accesslog`
  - SSL/TLS enabled (Port 5671)
  - Used for: Access log event streaming, asynchronous processing

#### Authentication & Security
- **Microsoft.AspNetCore.Authentication.JwtBearer 6.0.6** - JWT authentication
- **Microsoft.IdentityModel.Tokens** - Token validation
- **Google.Apis.Auth 1.57.0** - Google OAuth2 authentication
- **Azure.Identity 1.6.0** - Azure authentication
- **Azure Key Vault** - Secret management (optional)
  - Azure.Extensions.AspNetCore.Configuration.Secrets 1.2.2

#### Logging & Monitoring
- **Serilog 5.0.0** - Structured logging
  - Serilog.AspNetCore
  - Serilog.Enrichers.Environment 2.2.0
  - Serilog.Sinks.Loggly 5.4.0
- **Loggly** - Cloud-based log management (configured)
- File logging to `logs/Error_log-.txt`
- Enrichers: MachineName, ProcessId, ThreadId, LogContext

#### API Documentation & Versioning
- **Swashbuckle.AspNetCore 6.3.1** - Swagger/OpenAPI
- **Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer 5.0.0** - API versioning
- Custom Swagger filters for authorization and grouping

#### Email Services
- **Resend 0.2.1** - Modern email API service (EmailWorker)
- **MailKit 4.14.1** - Email client library
- **FluentEmail.Core 3.0.2** - Email composition
- Outbox pattern for reliable email delivery

#### Real-time Communication
- **Microsoft.AspNetCore.SignalR 1.1.0** - Real-time web functionality
- ChatHub implementation for live messaging

#### Development & Code Quality
- **Microsoft.CodeAnalysis.NetAnalyzers 6.0.0** - Static code analysis
- **Microsoft.EntityFrameworkCore.Design 6.0.6** - Design-time tools
- **Microsoft.VisualStudio.Azure.Containers.Tools.Targets** - Docker integration
- **StringToExpression 1.1.1** - Dynamic LINQ expressions

### Frontend Technologies

#### Core Framework
- **Angular 21.1.1** - Single Page Application framework
- **TypeScript 5.9.3** - Programming language
- **RxJS 7.8.0** - Reactive programming
- **Zone.js 0.15.0** - Execution context management

#### UI Components & Design
- **Angular Material 21.1.1** - Material Design components
  - @angular/cdk 21.1.1 (Component Dev Kit)
- **FontAwesome 7.1.0** - Icon library
  - @fortawesome/angular-fontawesome 4.0.0
  - @fortawesome/fontawesome-svg-core 7.1.0
  - @fortawesome/free-solid-svg-icons 7.1.0

#### Utilities
- **jwt-decode 4.0.0** - JWT token decoding
- **Express 5.1.0** - Server-side rendering (SSR)
- **@angular/ssr** - Angular Server-Side Rendering

#### Build & Development Tools
- **@angular/cli 21.1.1** - Command-line interface
- **@angular/build 21.1.1** - Build system
- **@angular/compiler-cli 21.1.1** - Template compiler

#### Testing
- **Jasmine 5.7.0** - Testing framework
- **Karma 6.4.0** - Test runner
  - karma-chrome-launcher
  - karma-coverage
  - karma-jasmine 5.1.0
  - karma-jasmine-html-reporter 2.1.0

### DevOps & Infrastructure

#### Containerization
- **Docker** - Container platform
  - Multi-stage Dockerfiles for optimized images
  - .dockerignore for build optimization
- **Docker Compose** - Multi-container orchestration
  - Services: api, accesslog-worker, mongo, rabbitmq
  - Volumes: mongo-data, rabbitmq-data

#### Cloud Services & Hosting

##### Production Deployment
- **Railway.app** - Backend hosting
  - WebAPI: `https://theretreatapp.up.railway.app`
  - MongoDB hosting (Gondola proxy)
  - Environment variable management
  - Custom railway.json configurations
  
- **Vercel** - Frontend hosting
  - Angular SPA deployment: `https://property-master-silk.vercel.app`
  - CDN distribution
  - Automatic deployments

##### External Services
- **CloudAMQP** - Managed RabbitMQ service
  - Host: raccoon.lmq.cloudamqp.com
  - Plan: Free tier with SSL
  - Virtual host: fqccxvvi
  
- **Resend.com** - Email delivery service
  - Transactional email API
  - High deliverability rates

#### Version Control
- **Git** - Source control
- **GitHub** - Repository hosting
  - Repository: `https://github.com/NevilleColaco23/testAngularAPIDocker`
  - Branch: Mar2026

### Additional Libraries & Packages

#### Shared Libraries
- **Messaging.Shared** - Custom shared library
  - Multi-targeting: .NET 6.0 and .NET 8.0
  - RabbitMQ models and interfaces
  - AccessLogEvent models

#### Other Dependencies
- **Newtonsoft.Json 13.0.3** - JSON serialization
- **System.Text.Json** - Built-in JSON handling

## 📁 Project Structure

```
PropertyMasterV4.0/
│
├── 📁 app/                                    # Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/                         # Core modules (auth, system)
│   │   │   ├── Menu/                         # Menu components
│   │   │   ├── property/                     # Property feature module
│   │   │   └── environments/                 # Environment configs
│   │   ├── public/                           # Static assets
│   │   └── styles/                           # Global styles
│   ├── angular.json                          # Angular configuration
│   ├── package.json                          # NPM dependencies
│   └── tsconfig.json                         # TypeScript config
│
├── 📁 WebApi/                                # Main Web API Project
│   ├── API/
│   │   ├── V1/                               # Version 1 API endpoints
│   │   │   ├── AccountController.cs          # Authentication endpoints
│   │   │   ├── PropertyController.cs         # Property CRUD
│   │   │   ├── PartnerController.cs          # Partner management
│   │   │   ├── ProductController.cs          # Product management
│   │   │   ├── BookingsController.cs         # Booking operations
│   │   │   ├── TransactionController.cs      # Transaction handling
│   │   │   ├── AccessLoggingController.cs    # Access log endpoints
│   │   │   └── MessageController.cs          # Messaging endpoints
│   │   ├── DomainControllers/
│   │   │   └── MenuController.cs             # Menu/Navigation
│   │   └── ApiStartup.cs                     # API configuration
│   │
│   ├── Authentication/                        # Auth services
│   │   ├── Services/
│   │   │   └── CurrentUserService.cs         # Current user context
│   │   ├── Dtos/                             # Auth DTOs
│   │   └── AuthenticationStartup.cs
│   │
│   ├── Messaging Queue/
│   │   └── RabbitMqPublisher.cs              # RabbitMQ publisher
│   │
│   ├── SignalR/
│   │   └── ChatHub.cs                        # Real-time chat hub
│   │
│   ├── CORS/                                 # CORS configuration
│   ├── ErrorHandling/                        # Global error handling
│   ├── Logging/                              # Logging configuration
│   ├── Swagger/                              # API documentation
│   ├── Versioning/                           # API versioning
│   │
│   ├── Program.cs                            # Application entry point
│   ├── Startup.cs                            # Services configuration
│   ├── Dockerfile                            # Docker configuration
│   ├── docker-compose.yml                    # Multi-container setup
│   ├── appsettings.json                      # Application settings
│   └── WebApi.csproj                         # Project file
│
├── 📁 classfiles/                            # Core Business Logic
│   │
│   ├── 📁 Domain/                            # Domain Layer (Entities)
│   │   ├── Property/
│   │   │   └── Property.cs                   # Property entity
│   │   ├── Partners/
│   │   │   ├── Partner.cs                    # Partner entity
│   │   │   └── Address.cs                    # Value object
│   │   ├── Products/
│   │   │   └── Product.cs                    # Product entity
│   │   ├── Bookings/
│   │   │   └── Bookings.cs                   # Booking entity
│   │   ├── Transactions/
│   │   │   ├── Transaction.cs                # Transaction entity
│   │   │   └── TransactionLine.cs            # Transaction details
│   │   ├── AccessLog/
│   │   │   └── AccessLog.cs                  # Access log entity
│   │   ├── Users/
│   │   │   └── Users.cs                      # User entity
│   │   ├── Common/
│   │   │   ├── Menus/                        # Menu entities
│   │   │   ├── ValueObjects/                 # Money, Mass value objects
│   │   │   ├── IEntity.cs                    # Entity interface
│   │   │   ├── IAudited.cs                   # Audit interface
│   │   │   └── ISoftDeletable.cs             # Soft delete interface
│   │   └── Domain.csproj
│   │
│   ├── 📁 Application/                       # Application Layer (CQRS)
│   │   ├── Property/
│   │   │   ├── CreateProperty/
│   │   │   │   └── CreatePropertyCommand.cs
│   │   │   ├── GetProperty/
│   │   │   │   ├── GetPropertyListQuery.cs
│   │   │   │   └── GetPropertyDTO.cs
│   │   │   └── UpdateProperty/
│   │   │       └── UpdatePropertyCommand.cs
│   │   ├── Partners/
│   │   │   ├── UpdatePartner/
│   │   │   └── DeletePartner/
│   │   ├── Products/
│   │   │   ├── UpdateProduct/
│   │   │   └── DeleteProduct/
│   │   ├── Logging/
│   │   │   └── CreateLog/
│   │   │       └── CreateLogCommand.cs
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── RequestValidationBehavior.cs
│   │   │   │   └── ExceptionLoggingBehavior.cs
│   │   │   └── Dependencies/
│   │   │       └── DataAccess/
│   │   │           └── Repositories/
│   │   ├── ApplicationStartup.cs             # Application DI setup
│   │   └── Application.csproj
│   │
│   ├── 📁 Infrastructure/                    # Infrastructure Layer
│   │   ├── Authentication/                   # JWT, OAuth2
│   │   │   ├── Core/
│   │   │   │   └── Services/
│   │   │   │       ├── UserService.cs
│   │   │   │       └── JwtTokenService.cs
│   │   │   ├── External/
│   │   │   │   └── Services/
│   │   │   │       ├── ExternalAuthenticationVerifier.cs
│   │   │   │       └── ExternalSignInService.cs
│   │   │   └── Startup.cs
│   │   ├── Identity/                         # ASP.NET Identity
│   │   ├── Persistence/                      # Data access
│   │   ├── AzureKeyVault/                    # Secret management
│   │   ├── ApplicationDependencies/          # DI configuration
│   │   ├── InfrastructureStartup.cs
│   │   └── Infrastructure.csproj
│   │
│   ├── 📁 MongoDBBackend/                    # MongoDB Abstraction
│   │   ├── IMongoRepository.cs               # Repository interface
│   │   ├── MongoRepository.cs                # Repository implementation
│   │   ├── MongoContext.cs                   # DB context
│   │   ├── MongoUnitOfWork.cs                # Unit of work pattern
│   │   ├── CounterService.cs                 # Auto-increment IDs
│   │   └── MongoDBBackend.csproj
│   │
│   └── 📁 SampleData/                        # Seed data
│       └── SampleData.csproj
│
├── 📁 AccessLogWorker/                       # Access Log Consumer Service
│   ├── Services/
│   │   └── AccessLogMessageProcessor.cs      # Message processing logic
│   ├── Worker.cs                             # Background service
│   ├── Program.cs                            # Entry point
│   ├── Dockerfile                            # Docker config
│   ├── appsettings.json                      # Worker settings
│   └── AccessLogConsumerWorker.csproj
│
├── 📁 EmailWorker/                           # Email Processing Service
│   ├── ResendEmailSender.cs                  # Resend integration
│   ├── Worker.cs                             # Background service
│   ├── Program.cs                            # Entry point
│   ├── Dockerfile                            # Docker config
│   ├── appsettings.json                      # Worker settings
│   └── EmailWorker.csproj
│
├── 📁 Messaging.Shared/                      # Shared Messaging Library
│   ├── Models/
│   │   └── AccessLogEvent.cs                 # Event model
│   ├── RabbitMqOptions.cs                    # Configuration model
│   ├── IRabbitMqPublisher.cs                 # Publisher interface
│   └── Messaging.Shared.csproj
│
├── 📁 APIUnitProject.Tests/                  # Unit Tests
│   └── APIUnitProject.Tests.csproj
│
├── 📁 Project.Documentation/                 # Documentation
│   ├── Messaging Queue/
│   │   └── README.md.txt
│   └── Project.Documentation.csproj
│
├── .dockerignore                             # Docker ignore file
└── PropertyMasterV4.0.sln                    # Solution file
```

## ✨ Features

### Core Functionality

#### 1. Property Management
- Create, Read, Update, Delete (CRUD) properties
- Property listing with filtering and pagination
- Image upload and management
- Property details with rich metadata

#### 2. Partner Management
- Partner profiles and contact information
- Address management (Value Object pattern)
- Partner-specific business rules (invariants)
- Soft delete capability

#### 3. Product Management
- Product catalog with categories
- Stock management
- Product invariants and validation
- Product variants support

#### 4. Bookings System
- Property booking management
- Booking status tracking
- Date range validation
- Conflict detection

#### 5. Transaction Management
- Transaction recording
- Transaction lines (line items)
- Transaction types (Sale, Purchase, etc.)
- Financial reporting data

### Advanced Features

#### 6. User Management & Authentication
- User registration and login
- JWT token-based authentication
- Google OAuth2 integration
- Role-based authorization
- User profile management

#### 7. Access Logging & Monitoring
- Automatic request/response logging
- IP address tracking
- User agent detection
- Action tracking
- Asynchronous log processing via RabbitMQ
- Centralized log storage in MongoDB

#### 8. Email System (Resend + MongoDB Queue)
- **Queue-based email delivery** using MongoDB outbox pattern
- **Resend.com** integration for transactional emails
- **Custom domain support** (masterproperty.site via Namecheap)
- **Automated retry mechanism** with exponential backoff (up to 5 attempts)
- **EmailWorker** background service (.NET 8) for processing
- **Email templates** for user activation, notifications
- **Delivery tracking** with status persistence (Pending, Sent, Failed)
- **High deliverability** with SPF, DKIM, and DMARC configuration
- **Production-ready** deployment on Railway.app
- Email templates (Razor templates)
- Bid invite notifications
- Transactional emails via Resend
- Background processing worker

#### 9. Real-time Features
- SignalR chat functionality
- Live notifications
- Real-time updates

#### 10. API Features
- RESTful API design
- API versioning (v1)
- Swagger/OpenAPI documentation
- Health check endpoints
- CORS configuration
- Rate limiting ready

### Technical Features

#### 11. Architecture Patterns
- **CQRS** (Command Query Responsibility Segregation)
- **Mediator Pattern** via MediatR
- **Repository Pattern** with Unit of Work
- **Domain-Driven Design**
- **Value Objects** (Money, Mass, Address)
- **Specification Pattern**

#### 12. Data Validation
- FluentValidation for input validation
- Pipeline behaviors for cross-cutting concerns
- Exception handling middleware
- Model state validation

#### 13. Logging & Diagnostics
- Structured logging with Serilog
- Log enrichment (machine name, process ID, thread ID)
- Error file logging
- Loggly integration for cloud logging
- Request/response logging

#### 14. Security Features
- JWT token authentication
- Secure password hashing (Identity)
- HTTPS enforcement (development)
- CORS policy configuration
- Azure Key Vault integration (optional)
- Secret management

#### 15. Performance & Scalability
- MongoDB for horizontal scalability
- Redis caching layer
- Message queue for async processing
- Background workers for heavy tasks
- Connection pooling

## 📋 Prerequisites

### Required Software

1. **.NET SDK**
   - .NET 6.0 SDK (for WebAPI)
   - .NET 8.0 SDK (for Worker Services)
   - Download: https://dotnet.microsoft.com/download

2. **Node.js & NPM**
   - Node.js 20.x or higher
   - NPM 10.x or higher
   - Download: https://nodejs.org/

3. **Angular CLI**
   ```bash
   npm install -g @angular/cli@21
   ```

4. **MongoDB**
   - MongoDB 6.0 or higher
   - Local installation or cloud service (MongoDB Atlas, Railway)
   - Download: https://www.mongodb.com/try/download/community

5. **RabbitMQ**
   - RabbitMQ 3.x or higher
   - Local installation or cloud service (CloudAMQP)
   - Download: https://www.rabbitmq.com/download.html

### Optional Software

6. **Docker Desktop**
   - For containerized deployment
   - Download: https://www.docker.com/products/docker-desktop

7. **Redis** (optional, for caching)
   - Redis 6.x or higher
   - Download: https://redis.io/download

8. **Visual Studio 2022** or **VS Code**
   - Visual Studio 2022 (recommended for .NET development)
   - VS Code with C# extension

### Cloud Services (Production)

9. **Railway.app Account** (for backend hosting)
10. **Vercel Account** (for frontend hosting)
11. **CloudAMQP Account** (for managed RabbitMQ)
12. **Resend Account** (for email delivery)

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/NevilleColaco23/testAngularAPIDocker.git
cd testAngularAPIDocker
```

### 2. Backend Setup

#### Install .NET Dependencies

```bash
# Restore all NuGet packages
dotnet restore PropertyMasterV4.0.sln
```

#### Configure Backend Settings

Edit `WebApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017/ListingDB",
    "Redis": "localhost:6379"
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchange": "accesslog.exchange",
    "Queue": "accesslog.queue",
    "RoutingKey": "accesslog",
    "UseSsl": false
  },
  "AuthenticationSettings": {
    "JwtIssuer": "MyWarehouse",
    "JwtAudience": "MyWarehouse",
    "TokenExpirationSeconds": 86400,
    "JwtSigningKeyBase64": "your-secret-key-here-min-32-chars"
  },
  "ExternalAuthenticationSettings": {
    "GoogleClientId": "your-google-client-id"
  },
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://localhost:4200"
    ]
  }
}
```

#### Configure Worker Services

Edit `AccessLogWorker/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017/ListingDB"
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchange": "accesslog.exchange",
    "Queue": "accesslog.queue",
    "RoutingKey": "accesslog",
    "UseSsl": false
  }
}
```

Edit `EmailWorker/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017/ListingDB"
  },
  "Email": {
    "ApiKey": "your-resend-api-key",
    "From": "noreply@yourdomain.com"
  }
}
```

### 3. Frontend Setup

```bash
# Navigate to Angular app
cd app

# Install NPM packages
npm install

# Return to root
cd ..
```

#### Configure Frontend Environment

Edit `app/src/app/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:44346/api/v1'
};
```

### 4. Database Setup

#### MongoDB

1. Start MongoDB service:
   ```bash
   # Windows
   net start MongoDB
   
   # macOS (Homebrew)
   brew services start mongodb-community
   
   # Linux
   sudo systemctl start mongod
   ```

2. Create database (automatic on first run)
   - Database: `ListingDB`
   - Collections created automatically by application

#### RabbitMQ

1. Start RabbitMQ service:
   ```bash
   # Windows
   rabbitmq-server
   
   # macOS
   brew services start rabbitmq
   
   # Linux
   sudo systemctl start rabbitmq-server
   ```

2. Access Management UI:
   - URL: http://localhost:15672
   - Default credentials: guest/guest

3. Create exchange and queue (or let application auto-create):
   - Exchange: `accesslog.exchange` (type: direct, durable)
   - Queue: `accesslog.queue` (durable)
   - Binding: Route queue to exchange with key `accesslog`

## ⚙️ Configuration

### Environment Variables

#### WebAPI

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `PORT` | HTTP port | 5000 | No |
| `ASPNETCORE_ENVIRONMENT` | Environment | Development | No |
| `MONGODB_URI` | MongoDB connection | See appsettings | Yes |
| `RabbitMq__Host` | RabbitMQ host | localhost | Yes |
| `RabbitMq__Port` | RabbitMQ port | 5672 | Yes |
| `RabbitMq__Username` | RabbitMQ user | guest | Yes |
| `RabbitMq__Password` | RabbitMQ password | guest | Yes |
| `JWT_SIGNING_KEY` | JWT secret | See appsettings | Yes |

#### AccessLogWorker

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `MONGODB_URI` | MongoDB connection | See appsettings | Yes |
| `RabbitMq__Host` | RabbitMQ host | localhost | Yes |
| `RabbitMq__Port` | RabbitMQ port | 5672 | Yes |

#### EmailWorker

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `MONGODB_URI` | MongoDB connection | See appsettings | Yes |
| `RESEND_API_KEY` | Resend API key | None | Yes |
| `EMAIL_FROM` | Sender email address | noreply@yourdomain.com | Yes |
| `AppSettings__MongoDbDatabaseName` | Database name | ListingDB | Yes |

---

## 📧 Email System Setup

The email system uses **Resend.com** for transactional email delivery with a MongoDB-based outbox pattern for reliability.

### Architecture Overview

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│   WebAPI     │────────►│   MongoDB    │────────►│ EmailWorker  │
│              │  Queue  │  EmailOutbox │  Poll   │ (Background) │
│              │         │  Collection  │         │              │
└──────────────┘         └──────────────┘         └──────────────┘
                                                          │
                                                          ▼
                                                   ┌──────────────┐
                                                   │  Resend API  │
                                                   │  (Delivery)  │
                                                   └──────────────┘
```

### Email Flow

1. **User Action** (e.g., signup) → WebAPI
2. **EmailQueueService** inserts email into MongoDB `EmailOutbox` collection
3. **EmailWorker** polls MongoDB every 2 seconds for pending emails
4. **EmailWorker** sends email via Resend API
5. **Status Updated** in MongoDB (Sent/Failed with retry logic)

### Setup Instructions

#### 1. Create Resend Account

1. Go to [Resend.com](https://resend.com) and sign up
2. Navigate to [API Keys](https://resend.com/api-keys)
3. Create a new API key
4. Copy the API key (starts with `re_...`)

#### 2. Domain Configuration (Namecheap)

To send emails from your custom domain (e.g., `masterproperty.site`):

##### 2.1 Purchase Domain (if needed)
1. Go to [Namecheap](https://www.namecheap.com)
2. Search and purchase your domain
3. Go to **Domain List** → **Manage**

##### 2.2 Add DNS Records in Resend
1. In Resend dashboard, go to [Domains](https://resend.com/domains)
2. Click **Add Domain**
3. Enter your domain (e.g., `masterproperty.site`)
4. Copy the DNS records provided by Resend

##### 2.3 Configure DNS in Namecheap
1. In Namecheap, go to your domain → **Advanced DNS** tab
2. Add the following records from Resend:

| Record Type | Host | Value | TTL |
|------------|------|-------|-----|
| **TXT** (DKIM) | `resend._domainkey` | `p=...` (from Resend) | Automatic |
| **TXT** (SPF) | `send` | `v=spf1 include:send.resend.com ~all` | Automatic |
| **MX** | `send` | `feedback-smtp.{region}.amazonses.com` | Automatic |
| **TXT** (DMARC) | `_dmarc` | `v=DMARC1; p=none;` (optional) | Automatic |

3. Click **Save All Changes**

##### 2.4 Verify DNS Propagation
1. Wait 5-30 minutes for DNS to propagate
2. Check propagation: [DNSChecker.org](https://dnschecker.org)
3. Enter your domain and select TXT/MX records
4. Wait for green checkmarks globally

##### 2.5 Verify Domain in Resend
1. Return to Resend → [Domains](https://resend.com/domains)
2. Click **Verify Domain**
3. Status should change to **Verified** ✅
4. You can now send from `noreply@masterproperty.site`

#### 3. Configure EmailWorker

##### Local Development

Edit `EmailWorker/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017/ListingDB"
  },
  "AppSettings": {
    "MongoDbDatabaseName": "ListingDB"
  }
}
```

**Note:** Do NOT put sensitive credentials in `appsettings.json`. Use environment variables or user secrets:

```bash
# Using .NET User Secrets (Development)
cd EmailWorker
dotnet user-secrets set "Resend:ApiKey" "re_your_api_key_here"
dotnet user-secrets set "Email:From" "noreply@masterproperty.site"
```

##### Production (Railway.app)

Set environment variables in Railway dashboard:

```bash
RESEND_API_KEY=re_your_api_key_here
EMAIL_FROM=noreply@masterproperty.site
ConnectionStrings__MongoDb=mongodb://your-railway-mongo-url
AppSettings__MongoDbDatabaseName=ListingDB
ASPNETCORE_ENVIRONMENT=Production
```

#### 4. Email Templates

Email templates are defined in code. Example from `UserService.cs`:

```csharp
var htmlBody = $@"
    <html>
    <body>
        <h2>Welcome to Property Master!</h2>
        <p>Hi {username},</p>
        <p>Thank you for signing up. Please activate your account by clicking the link below:</p>
        <p><a href='{activationLink}'>Activate Account</a></p>
        <p>If you did not sign up for this account, please ignore this email.</p>
        <p>Best regards,<br/>Property Master Team</p>
    </body>
    </html>";

await _emailQueueService.QueueEmailAsync(email, "Activate Your Account", htmlBody, "activation");
```

#### 5. MongoDB EmailOutbox Schema

The system uses a MongoDB collection called `EmailOutbox`:

```javascript
{
  _id: 1,                           // Auto-increment ID
  type: "activation",               // Email type
  to: "user@example.com",          // Recipient
  subject: "Activate Your Account", // Subject line
  bodyHtml: "<html>...</html>",    // HTML body
  status: 0,                        // 0=Pending, 1=Processing, 2=Sent, 3=Failed
  attempts: 0,                      // Retry count
  nextRunAtUtc: ISODate("..."),    // When to process
  createdAtUtc: ISODate("..."),    // Creation timestamp
  sentAtUtc: null,                  // When sent (null if not sent)
  lockedUntilUtc: null,             // Processing lock
  lastError: null                   // Last error message
}
```

#### 6. Testing the Email System

##### Test Locally

1. Start MongoDB and EmailWorker:
```bash
# Terminal 1: Start EmailWorker
cd EmailWorker
dotnet run
```

2. Start WebAPI:
```bash
# Terminal 2: Start WebAPI
cd WebApi
dotnet run
```

3. Sign up a new user via API:
```bash
curl -X POST http://localhost:5000/api/v1/account/SignUp \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "your-email@example.com",
    "password": "Test@1234",
    "phone": "1234567890"
  }'
```

4. Check EmailWorker logs:
```
info: EmailWorker.Worker[0]
      Sent outbox email 1 -> your-email@example.com
```

5. Check your email inbox!

##### Test on Railway

1. Deploy EmailWorker to Railway (see Deployment section)
2. Verify environment variables are set
3. Test via deployed WebAPI URL
4. Monitor logs in Railway dashboard

#### 7. Monitoring & Troubleshooting

##### Check Email Status in MongoDB

```javascript
// Connect to MongoDB
use ListingDB

// View pending emails
db.EmailOutbox.find({ status: 0 })

// View sent emails
db.EmailOutbox.find({ status: 2 })

// View failed emails
db.EmailOutbox.find({ status: 3 })

// Count emails by status
db.EmailOutbox.aggregate([
  { $group: { _id: "$status", count: { $sum: 1 } } }
])
```

##### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| **Domain not verified** | DNS records not propagated | Wait 30 minutes, check DNSChecker.org |
| **"You can only send to your own email"** | Using test mode | Verify domain in Resend |
| **Emails not sending** | EmailWorker not running | Check Railway logs, restart service |
| **Missing API key error** | Environment variable not set | Set `RESEND_API_KEY` in Railway |
| **MongoDB connection failed** | Wrong connection string | Verify `ConnectionStrings__MongoDb` |

##### Email Retry Logic

The EmailWorker has built-in retry logic:

- **Max Attempts**: 5
- **Retry Delays**: Exponential backoff (2, 4, 8, 16, 32 minutes)
- **Status Tracking**: Pending → Processing → Sent/Failed
- **Automatic Recovery**: Crashed sends are retried after 2 minutes

#### 8. Email Service Comparison

| Feature | Resend | SMTP (Gmail) | SendGrid |
|---------|--------|--------------|----------|
| **Railway Compatible** | ✅ Yes | ❌ Blocked | ✅ Yes |
| **Setup Complexity** | ⭐⭐ Easy | ⭐⭐⭐⭐ Hard | ⭐⭐⭐ Medium |
| **Custom Domain** | ✅ Yes (Namecheap) | ❌ Complex | ✅ Yes |
| **Deliverability** | ⭐⭐⭐⭐⭐ Excellent | ⭐⭐⭐ Good | ⭐⭐⭐⭐ Very Good |
| **Free Tier** | 100/day | 500/day | 100/day |
| **Pricing** | $20/month (50k) | Free (limited) | $20/month (40k) |
| **Our Choice** | ✅ **Selected** | ❌ Not compatible | ⚠️ Alternative |

**Why we chose Resend:**
- ✅ Works perfectly on Railway (no SMTP port blocking)
- ✅ Simple API integration
- ✅ Excellent deliverability with verified domains
- ✅ Easy domain setup with Namecheap
- ✅ Modern developer-friendly platform
- ✅ Built-in bounce handling

---

## 🏃 Running the Application

### Development Mode

#### Option 1: Run Individually

**Terminal 1 - MongoDB:**
```bash
mongod
```

**Terminal 2 - RabbitMQ:**
```bash
rabbitmq-server
```

**Terminal 3 - WebAPI:**
```bash
cd WebApi
dotnet run
# API available at: https://localhost:44346
```

**Terminal 4 - AccessLogWorker:**
```bash
cd AccessLogWorker
dotnet run
```

**Terminal 5 - EmailWorker:**
```bash
cd EmailWorker
dotnet run
```

**Terminal 6 - Angular Frontend:**
```bash
cd app
npm start
# or: ng serve
# App available at: http://localhost:4200
```

#### EmailWorker

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `MONGODB_URI` | MongoDB connection | See appsettings | Yes |
| `RESEND_API_KEY` | Resend API key | None | Yes |
| `EMAIL_FROM` | Sender email address | noreply@yourdomain.com | Yes |
| `EMAIL_FROM` | Sender email | - | Yes |
| `RESEND_API_KEY` | Resend API key | - | Yes |

### CORS Configuration

Configure allowed origins in `WebApi/appsettings.json`:

```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://your-production-domain.com"
    ]
  }
}
```

### Swagger Configuration

```json
{
  "SwaggerSettings": {
    "ApiName": "MyWarehouse",
    "UseSwagger": true,
    "LoginPath": "/account/oauth2/access_token"
  }
}
```

## 🏃 Running the Application

### Option 1: Run Locally (Development)

#### Start Backend Services

**Terminal 1 - WebAPI:**
```bash
cd WebApi
dotnet run
# API runs on https://localhost:44346
```

**Terminal 2 - AccessLog Worker:**
```bash
cd AccessLogWorker
dotnet run
```

**Terminal 3 - Email Worker:**
```bash
cd EmailWorker
dotnet run
```

#### Start Frontend

**Terminal 4 - Angular:**
```bash
cd app
ng serve
# App runs on http://localhost:4200
```

### Option 2: Run with Docker Compose

```bash
# Build and start all services
docker-compose -f WebApi/docker-compose.yml up --build

# Services:
# - WebAPI: http://localhost:8080
# - MongoDB: localhost:27017
# - RabbitMQ: localhost:5672 (Management UI: http://localhost:15672)
# - AccessLog Worker: Running in background
```

### Option 3: Run Individual Docker Containers

```bash
# Build WebAPI image
docker build -t mywarehouse-api -f WebApi/Dockerfile .

# Build AccessLog Worker image
docker build -t accesslog-worker -f AccessLogWorker/Dockerfile .

# Build Email Worker image
docker build -t email-worker -f EmailWorker/Dockerfile .

# Run containers (after starting MongoDB and RabbitMQ)
docker run -p 8080:80 mywarehouse-api
docker run accesslog-worker
docker run email-worker
```

### Verification

1. **WebAPI Health Check:**
   ```
   GET http://localhost:8080/health
   Expected: 200 OK
   ```

2. **Swagger UI:**
   ```
   http://localhost:8080/swagger
   ```

3. **Angular Application:**
   ```
   http://localhost:4200
   ```

4. **RabbitMQ Management:**
   ```
   http://localhost:15672
   Login: guest/guest
   ```

## 🌐 Deployment

### Railway.app (Backend)

#### Prerequisites
- Railway.app account
- Railway CLI installed

#### Deployment Steps

1. **Install Railway CLI:**
   ```bash
   npm install -g @railway/cli
   ```

2. **Login to Railway:**
   ```bash
   railway login
   ```

3. **Create New Project:**
   ```bash
   railway init
   ```

4. **Deploy WebAPI:**
   ```bash
   cd WebApi
   railway up
   ```

5. **Configure Environment Variables in Railway Dashboard:**
   - `PORT`: 5000
   - `MONGODB_URI`: Your Railway MongoDB connection string
   - `RabbitMq__Host`: CloudAMQP host
   - `RabbitMq__Port`: 5671
   - `RabbitMq__Username`: CloudAMQP username
   - `RabbitMq__Password`: CloudAMQP password
   - `RabbitMq__UseSsl`: true
   - `JWT_SIGNING_KEY`: Your secret key

6. **Deploy Workers (Separate Services):**
   ```bash
   # AccessLog Worker
   cd ../AccessLogWorker
   railway up
   
   # Email Worker
   cd ../EmailWorker
   railway up
   ```

7. **Get Deployment URL:**
   ```bash
   railway domain
   # Example: https://theretreatapp.up.railway.app
   ```

### Vercel (Frontend)

#### Prerequisites
- Vercel account
- Vercel CLI installed

#### Deployment Steps

1. **Install Vercel CLI:**
   ```bash
   npm install -g vercel
   ```

2. **Login to Vercel:**
   ```bash
   vercel login
   ```

3. **Navigate to Angular App:**
   ```bash
   cd app
   ```

4. **Update Production Environment:**
   
   Edit `src/app/environments/environment.prod.ts`:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://your-railway-api-url.railway.app/api/v1'
   };
   ```

5. **Build for Production:**
   ```bash
   ng build --configuration production
   ```

6. **Deploy to Vercel:**
   ```bash
   vercel --prod
   ```

7. **Configure Custom Domain (Optional):**
   ```bash
   vercel domains add yourdomain.com
   ```

### CloudAMQP Setup (RabbitMQ)

1. **Create Account:**
   - Visit https://www.cloudamqp.com/
   - Sign up for free tier

2. **Create Instance:**
   - Select plan (Free tier available)
   - Choose region closest to Railway deployment
   - Note connection details

3. **Get Connection Information:**
   - Host: e.g., `raccoon.lmq.cloudamqp.com`
   - Port: 5671 (SSL)
   - Virtual Host: Your instance name
   - Username: Provided
   - Password: Provided

4. **Update Configuration:**
   - Update `appsettings.json` with CloudAMQP details
   - Ensure `UseSsl: true`

### Database Backup & Migration

#### Backup MongoDB

```bash
# Local backup
mongodump --uri="mongodb://localhost:27017/ListingDB" --out=./backup

# Railway MongoDB backup
mongodump --uri="your-railway-mongodb-uri" --out=./backup
```

#### Restore MongoDB

```bash
# Restore to local
mongorestore --uri="mongodb://localhost:27017/ListingDB" ./backup/ListingDB

# Restore to production
mongorestore --uri="your-production-mongodb-uri" ./backup/ListingDB
```

## 📚 API Documentation

### API Base URL

- **Local Development:** `https://localhost:44346/api/v1`
- **Production:** `https://theretreatapp.up.railway.app/api/v1`

### Swagger Documentation

Access interactive API documentation:
- **Local:** `https://localhost:44346/swagger`
- **Production:** `https://theretreatapp.up.railway.app/swagger`

### Authentication

All protected endpoints require JWT token in Authorization header:

```http
Authorization: Bearer <your-jwt-token>
```

### API Endpoints

#### Authentication

```http
POST /api/v1/account/signup
POST /api/v1/account/login
POST /api/v1/account/oauth2/google
POST /api/v1/account/refresh-token
GET  /api/v1/account/user-info
```

#### Properties

```http
GET    /api/v1/property                    # List properties
GET    /api/v1/property/{id}               # Get property by ID
POST   /api/v1/property                    # Create property
PUT    /api/v1/property/{id}               # Update property
DELETE /api/v1/property/{id}               # Delete property
```

#### Partners

```http
GET    /api/v1/partner                     # List partners
GET    /api/v1/partner/{id}                # Get partner by ID
POST   /api/v1/partner                     # Create partner
PUT    /api/v1/partner/{id}                # Update partner
DELETE /api/v1/partner/{id}                # Delete partner
```

#### Products

```http
GET    /api/v1/product                     # List products
GET    /api/v1/product/{id}                # Get product by ID
POST   /api/v1/product                     # Create product
PUT    /api/v1/product/{id}                # Update product
DELETE /api/v1/product/{id}                # Delete product
```

#### Bookings

```http
GET    /api/v1/bookings                    # List bookings
GET    /api/v1/bookings/{id}               # Get booking by ID
POST   /api/v1/bookings                    # Create booking
PUT    /api/v1/bookings/{id}               # Update booking
DELETE /api/v1/bookings/{id}               # Delete booking
```

#### Transactions

```http
GET    /api/v1/transaction                 # List transactions
GET    /api/v1/transaction/{id}            # Get transaction by ID
POST   /api/v1/transaction                 # Create transaction
PUT    /api/v1/transaction/{id}            # Update transaction
DELETE /api/v1/transaction/{id}            # Delete transaction
```

#### Access Logs

```http
GET    /api/v1/accesslogging               # List access logs
GET    /api/v1/accesslogging/{id}          # Get log by ID
POST   /api/v1/accesslogging               # Create log entry
```

#### Menus & Navigation

```http
GET    /api/menu                           # Get menu structure
GET    /api/menu/permissions/{roleId}      # Get menu permissions
```

#### Health & Status

```http
GET    /health                             # Health check
GET    /                                   # API status
```

### Sample Request/Response

#### Create Property

**Request:**
```http
POST /api/v1/property
Content-Type: application/json
Authorization: Bearer <token>

{
  "name": "Luxury Apartment",
  "address": "123 Main St, City, State",
  "price": 500000,
  "bedrooms": 3,
  "bathrooms": 2,
  "squareFeet": 1500,
  "description": "Beautiful modern apartment"
}
```

**Response:**
```json
{
  "id": 1,
  "name": "Luxury Apartment",
  "address": "123 Main St, City, State",
  "price": 500000,
  "bedrooms": 3,
  "bathrooms": 2,
  "squareFeet": 1500,
  "description": "Beautiful modern apartment",
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-15T10:30:00Z"
}
```

### Error Responses

All errors follow consistent format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."]
  },
  "traceId": "00-abc123-def456-00"
}
```

## 🧪 Testing

### Unit Tests

```bash
# Run all tests
dotnet test PropertyMasterV4.0.sln

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test APIUnitProject.Tests/APIUnitProject.Tests.csproj
```

### Frontend Tests

```bash
cd app

# Run unit tests
ng test

# Run tests with coverage
ng test --code-coverage

# Run tests headless (CI/CD)
ng test --browsers=ChromeHeadless --watch=false
```

### Integration Testing

Use Swagger UI or tools like Postman:

1. **Import Swagger Spec:**
   - Export OpenAPI spec from `/swagger/v1/swagger.json`
   - Import into Postman

2. **Test Scenarios:**
   - User registration and login flow
   - Property CRUD operations
   - Access log creation and retrieval
   - Email outbox processing

### Load Testing

Use tools like Apache JMeter or k6:

```bash
# Install k6
brew install k6  # macOS
choco install k6  # Windows

# Run load test
k6 run load-test-script.js
```

## 🤝 Contributing

### Development Workflow

1. **Fork the Repository**
   ```bash
   # Click Fork button on GitHub
   git clone https://github.com/YOUR_USERNAME/testAngularAPIDocker.git
   ```

2. **Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make Changes**
   - Follow coding standards
   - Add unit tests
   - Update documentation

4. **Commit Changes**
   ```bash
   git add .
   git commit -m "feat: add your feature description"
   ```

5. **Push to Fork**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create Pull Request**
   - Go to original repository
   - Click "New Pull Request"
   - Describe changes

### Coding Standards

#### C# / .NET
- Follow Microsoft C# Coding Conventions
- Use async/await for asynchronous operations
- Implement IDisposable for resource management
- Use dependency injection
- Write XML documentation for public APIs

#### TypeScript / Angular
- Follow Angular Style Guide
- Use TypeScript strict mode
- Implement RxJS best practices
- Use async pipe for subscriptions
- Write JSDoc comments

### Commit Message Convention

Follow Conventional Commits:

```
feat: add new feature
fix: bug fix
docs: documentation changes
style: formatting changes
refactor: code refactoring
test: add tests
chore: maintenance tasks
```

## 📝 License

This project is proprietary and confidential. All rights reserved.

## 👤 Author

**Neville Colaco**

- GitHub: [@NevilleColaco23](https://github.com/NevilleColaco23)
- Repository: [testAngularAPIDocker](https://github.com/NevilleColaco23/testAngularAPIDocker)

## 🙏 Acknowledgments

### Technologies & Frameworks
- Microsoft .NET Team for ASP.NET Core
- Angular Team for Angular framework
- MediatR for CQRS pattern implementation
- AutoMapper for object mapping
- FluentValidation for validation
- Serilog for structured logging
- MongoDB team for the database
- RabbitMQ team for messaging

### Services
- Railway.app for backend hosting
- Vercel for frontend hosting
- CloudAMQP for managed RabbitMQ
- Resend for email delivery
- GitHub for source control

## 📞 Support

For issues, questions, or contributions:

1. **GitHub Issues:** https://github.com/NevilleColaco23/testAngularAPIDocker/issues
2. **Pull Requests:** https://github.com/NevilleColaco23/testAngularAPIDocker/pulls
3. **Email:** [Your contact email]

## 🔄 Version History

### Version 4.0 (Current - Mar2026 Branch)
- Complete rewrite with Clean Architecture
- Microservices with worker services
- RabbitMQ message queue integration
- MongoDB as primary database
- Angular 21 frontend upgrade
- Docker containerization
- Railway.app deployment

### Version 3.0
- Angular Material integration
- Improved UI/UX
- Enhanced authentication

### Version 2.0
- Initial Angular frontend
- Basic CRUD operations
- JWT authentication

### Version 1.0
- Initial release
- .NET WebAPI
- Basic property management

## 🗺️ Roadmap

### Planned Features
- [ ] Azure Blob Storage for file uploads
- [ ] Advanced search with Elasticsearch
- [ ] Payment gateway integration (Stripe)
- [ ] Multi-tenant support
- [ ] Mobile app (React Native)
- [ ] GraphQL API endpoint
- [ ] Advanced reporting with charts
- [ ] Notification system (push notifications)
- [ ] Chat system with attachments
- [ ] Calendar integration
- [ ] Document management
- [ ] Audit trail visualization
- [ ] Performance monitoring dashboard
- [ ] Automated testing suite expansion
- [ ] CI/CD pipeline with GitHub Actions

---

**Last Updated:** January 2025  
**Status:** Active Development  
**Branch:** Mar2026  
**Maintainer:** Neville Colaco
