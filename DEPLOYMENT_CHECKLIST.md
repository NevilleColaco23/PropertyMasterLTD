# Railway Deployment Checklist

## ✅ What's Ready

- ✅ **MongoDB**: Already hosted on Railway
- ✅ **Dockerfiles**: WebApi and AccessLogWorker have proper Dockerfiles
- ✅ **GitHub**: Code is in repository
- ✅ **Health Check**: `/api/health` endpoint added
- ✅ **Production config**: appsettings.Production.json created
- ✅ **Message Queue**: RabbitMQ integration code complete
- ✅ **Worker Service**: Properly structured with scoped services

## ⚠️ Before Deployment - Required Actions

### 1. **Setup RabbitMQ** (Choose One)

#### Option A: CloudAMQP (Easiest - Free Tier Available)
```bash
1. Go to: https://www.cloudamqp.com
2. Sign up and create an instance (choose "Little Lemur" free tier)
3. Copy the AMQP URL (format: amqp://user:pass@host/vhost)
4. Use this URL for RabbitMQ configuration on Railway
```

#### Option B: Railway RabbitMQ Service
```bash
1. In Railway project: "New" → "Empty Service"  
2. Settings → "Deploy from Docker Image"
3. Image: rabbitmq:3-management
4. Add variables: RABBITMQ_DEFAULT_USER, RABBITMQ_DEFAULT_PASS
5. Generate domain for management UI (port 15672)
```

### 2. **Update CORS for Production**

Update `WebApi\appsettings.Production.json`:
```json
{
  "CorsSettings": {
    "AllowedOrigins": [
      "https://your-frontend-domain.railway.app",
      "https://your-custom-domain.com"
    ]
  }
}
```

### 3. **Generate Secure JWT Key**

Run this PowerShell command to generate a secure key:
```powershell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
```

Add to Railway environment variables (NOT appsettings.json):
```
AuthenticationSettings__JwtSigningKeyBase64=<generated-key-here>
```

### 4. **Push Code to GitHub**

```bash
git add .
git commit -m "Ready for Railway deployment - Added health checks and production config"
git push origin Implementing_RabbitMQ
```

## 🚀 Deployment Steps on Railway

### Step 1: Deploy WebApi

1. Go to Railway dashboard
2. Click "New Project" or use existing project
3. Click "New" → "GitHub Repo"
4. Select: `NevilleColaco23/testAngularAPIDocker`
5. Branch: `Implementing_RabbitMQ`
6. **Root Directory**: `/WebApi` (Important!)
7. Railway will auto-detect Dockerfile

**Environment Variables to Add:**
```bash
# MongoDB (copy from your existing Railway MongoDB)
MONGODB_URI=mongodb://mongo:EcMPWsskZQhygpalucMlhpEwbxaapZFL@gondola.proxy.rlwy.net:31267/ListingDB?authSource=admin

# RabbitMQ (from CloudAMQP or Railway RabbitMQ service)
RabbitMq__Host=<your-rabbitmq-host>
RabbitMq__Port=5672
RabbitMq__Username=admin
RabbitMq__Password=<your-password>
RabbitMq__VirtualHost=/
RabbitMq__Exchange=accesslog.exchange
RabbitMq__Queue=accesslog.queue
RabbitMq__RoutingKey=accesslog

# ASP.NET
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:$PORT

# JWT (IMPORTANT: Use secure generated key)
AuthenticationSettings__JwtSigningKeyBase64=<your-secure-key>
AuthenticationSettings__JwtIssuer=MyWarehouse
AuthenticationSettings__JwtAudience=MyWarehouse

# CORS (after deployment, add your Railway domain)
CorsSettings__AllowedOrigins__0=https://your-frontend.railway.app
```

### Step 2: Deploy AccessLogWorker

1. In same Railway project: "New" → "GitHub Repo"
2. Select same repository
3. Branch: `Implementing_RabbitMQ`
4. **Root Directory**: `/AccessLogWorker` (Important!)
5. Settings → **Remove** public domain (worker doesn't need HTTP)

**Environment Variables:**
```bash
# MongoDB
MONGODB_URI=mongodb://mongo:EcMPWsskZQhygpalucMlhpEwbxaapZFL@gondola.proxy.rlwy.net:31267/ListingDB?authSource=admin

# RabbitMQ (same as WebApi)
RabbitMq__Host=<your-rabbitmq-host>
RabbitMq__Port=5672
RabbitMq__Username=admin
RabbitMq__Password=<your-password>
RabbitMq__VirtualHost=/
RabbitMq__Exchange=accesslog.exchange
RabbitMq__Queue=accesslog.queue
RabbitMq__RoutingKey=accesslog

# .NET
DOTNET_ENVIRONMENT=Production
```

### Step 3: Verify Deployment

1. **Check WebApi health**: `https://your-webapi.railway.app/api/health`
2. **Check logs** in Railway dashboard for both services
3. **Test API endpoints**
4. **Verify Worker** is consuming messages (check logs)
5. **Check MongoDB** - data should be saved

## 📋 Summary

**Ready to deploy?**
- ⚠️ **Need RabbitMQ solution first** (CloudAMQP recommended)
- ✅ Code is ready
- ✅ Dockerfiles work (with Messaging.Shared fix)
- ✅ Health check added
- ✅ Configuration structure correct

**Next Steps:**
1. Setup CloudAMQP account (5 minutes)
2. Push code to GitHub
3. Deploy WebApi to Railway (10 minutes)
4. Deploy Worker to Railway (5 minutes)
5. Test end-to-end

**Would you like me to help you set up CloudAMQP or make any other improvements before deployment?** 🚀
