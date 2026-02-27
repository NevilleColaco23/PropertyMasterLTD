# Railway Deployment Guide

## Prerequisites
- ✅ GitHub repository (you have: https://github.com/NevilleColaco23/testAngularAPIDocker)
- ✅ Railway account (sign up at railway.app)
- ✅ MongoDB already hosted on Railway

## What Needs to be Done

### 1. **Setup RabbitMQ on Railway**

Railway doesn't have a native RabbitMQ template, so you have two options:

#### Option A: Use CloudAMQP (Recommended for Railway)
1. Sign up at https://www.cloudamqp.com (free tier available)
2. Create an instance
3. Get the AMQP URL: `amqp://user:password@host:5672/vhost`
4. Add to Railway environment variables

#### Option B: Deploy RabbitMQ as a separate Railway service
1. Create new service from Docker image: `rabbitmq:3-management`
2. Add environment variables:
   ```
   RABBITMQ_DEFAULT_USER=admin
   RABBITMQ_DEFAULT_PASS=admin
   ```
3. Use internal Railway networking

### 2. **Railway Services to Deploy**

You need **3 services** on Railway:

#### Service 1: WebApi
- **Source**: GitHub repo, branch `Implementing_RabbitMQ`
- **Root Directory**: `/WebApi`
- **Dockerfile**: `/WebApi/Dockerfile`
- **Port**: 80 (Railway auto-detects from EXPOSE)

#### Service 2: AccessLogWorker
- **Source**: Same GitHub repo
- **Root Directory**: `/AccessLogWorker`
- **Dockerfile**: `/AccessLogWorker/Dockerfile`
- **No exposed port** (background worker)

#### Service 3: EmailWorker (if needed)
- **Source**: Same GitHub repo
- **Root Directory**: `/EmailWorker`
- **Dockerfile**: `/EmailWorker/Dockerfile`

### 3. **Environment Variables Needed**

#### For WebApi:
```bash
# MongoDB (already have)
MONGODB_URI=mongodb://mongo:password@host:31267/ListingDB?authSource=admin

# RabbitMQ
RabbitMq__Host=<rabbitmq-host>
RabbitMq__Port=5672
RabbitMq__Username=admin
RabbitMq__Password=admin
RabbitMq__VirtualHost=/
RabbitMq__Exchange=accesslog.exchange
RabbitMq__Queue=accesslog.queue
RabbitMq__RoutingKey=accesslog

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80

# JWT (update with secure values)
AuthenticationSettings__JwtSigningKeyBase64=<generate-secure-key>
AuthenticationSettings__JwtIssuer=MyWarehouse
AuthenticationSettings__JwtAudience=MyWarehouse
```

#### For AccessLogWorker:
```bash
# MongoDB
MONGODB_URI=mongodb://mongo:password@host:31267/ListingDB?authSource=admin

# RabbitMQ (same as WebApi)
RabbitMq__Host=<rabbitmq-host>
RabbitMq__Port=5672
RabbitMq__Username=admin
RabbitMq__Password=admin
RabbitMq__VirtualHost=/
RabbitMq__Exchange=accesslog.exchange
RabbitMq__Queue=accesslog.queue
RabbitMq__RoutingKey=accesslog

# ASP.NET Core
DOTNET_ENVIRONMENT=Production
```

### 4. **Code Changes Needed for Production**

#### A. Update Dockerfile build context references
Both Dockerfiles reference `Messaging.Shared` which needs to be added:

#### B. Add health checks (Railway requirement)
WebApi should expose a health endpoint at `/health` or `/healthz`

#### C. Remove hardcoded secrets
Your `appsettings.json` has hardcoded JWT keys - these should come from environment variables in production.

### 5. **Deployment Steps on Railway**

1. **Connect GitHub**: Link your repository to Railway
2. **Create Project**: Create a new Railway project
3. **Add MongoDB**: Already done ✅
4. **Add RabbitMQ**: CloudAMQP or custom service
5. **Deploy WebApi**:
   - New service → Deploy from GitHub
   - Set root directory: `/WebApi`
   - Configure environment variables
6. **Deploy AccessLogWorker**:
   - New service → Deploy from GitHub  
   - Set root directory: `/AccessLogWorker`
   - Configure environment variables
7. **Configure networking**: Services can talk via internal URLs

### 6. **Quick Checklist Before Deployment**

- [ ] Fix Dockerfiles to properly copy Messaging.Shared
- [ ] Add health check endpoint to WebApi
- [ ] Setup RabbitMQ (CloudAMQP recommended)
- [ ] Configure all environment variables on Railway
- [ ] Remove hardcoded secrets from appsettings.json
- [ ] Test locally with docker-compose
- [ ] Update CORS to allow Railway domain

## Current Status: ⚠️ Almost Ready

**Needs these fixes first:**
1. Fix Dockerfiles for Messaging.Shared reference
2. Add health check endpoint
3. Setup RabbitMQ hosting solution

**Would you like me to implement these fixes now?**
