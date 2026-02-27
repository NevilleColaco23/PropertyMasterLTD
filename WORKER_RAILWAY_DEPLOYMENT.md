# AccessLogWorker Railway Deployment - Quick Start

## 🚀 Step-by-Step Deployment

### Step 1: Push Latest Code to GitHub
```bash
git add .
git commit -m "AccessLogWorker ready for Railway deployment"
git push origin Implementing_RabbitMQ
```

### Step 2: Setup RabbitMQ First (Required!)

#### Option A: CloudAMQP (Recommended - 5 minutes)
1. Go to https://www.cloudamqp.com
2. Click "Sign Up" (free tier available)
3. Create instance: "Little Lemur" (free)
4. Copy the **AMQP URL** (looks like: `amqp://user:pass@host.cloudamqp.com/vhost`)
5. Convert format for .NET config:
   ```
   Host: host.cloudamqp.com
   Port: 5672
   Username: user (from URL)
   Password: pass (from URL)
   VirtualHost: /vhost (from URL)
   ```

### Step 3: Deploy Worker to Railway

1. **Go to Railway Dashboard** → Your project (where MongoDB is)

2. **Click "New"** → **"GitHub Repo"**

3. **Select Repository:**
   - Repository: `NevilleColaco23/testAngularAPIDocker`
   - Branch: `Implementing_RabbitMQ`

4. **Railway will detect the repo, then configure:**
   - Click on the new service
   - Go to **Settings**

5. **Configure Build:**
   - **Root Directory**: `/` (leave empty or enter `/`)
   - **Dockerfile Path**: `AccessLogWorker/Dockerfile`
   - **Build Command**: (leave empty)

6. **Add Environment Variables:**
   Click "Variables" tab and add:

   ```bash
   # MongoDB (your existing Railway MongoDB)
   MONGODB_URI=mongodb://mongo:EcMPWsskZQhygpalucMlhpEwbxaapZFL@gondola.proxy.rlwy.net:31267/ListingDB?authSource=admin

   # RabbitMQ (from CloudAMQP or your RabbitMQ service)
   RabbitMq__Host=<your-rabbitmq-host>
   RabbitMq__Port=5672
   RabbitMq__Username=<your-username>
   RabbitMq__Password=<your-password>
   RabbitMq__VirtualHost=/
   RabbitMq__Exchange=accesslog.exchange
   RabbitMq__Queue=accesslog.queue
   RabbitMq__RoutingKey=accesslog

   # .NET Environment
   DOTNET_ENVIRONMENT=Production
   ```

7. **Remove Public Domain:**
   - Settings → **Networking**
   - **Remove/Disable** the public domain
   - Worker doesn't need HTTP access

8. **Deploy:**
   - Railway will automatically build and deploy
   - Watch the logs for any errors

### Step 4: Verify Deployment

1. **Check Logs in Railway:**
   - Should see: "✅ MongoDB connection successful!"
   - Should see: "Started consuming messages from queue: accesslog.queue"

2. **Check RabbitMQ Admin UI:**
   - For CloudAMQP: Go to your CloudAMQP dashboard
   - Should see `accesslog.queue` created
   - Should show 1 consumer connected

3. **Send Test Message:**
   - Use your WebApi (running locally or deployed)
   - Login or navigate → triggers AccessLog
   - Check Worker logs for "=== MESSAGE RECEIVED ==="
   - Verify data saved in MongoDB

## ⚠️ Common Issues

### Issue 1: Build fails with "Cannot find project"
**Fix:** Make sure Root Directory is `/` (empty), not `/AccessLogWorker`

### Issue 2: Worker starts but crashes
**Fix:** Check environment variables are set correctly (Railway format: `RabbitMq__Host` with double underscore)

### Issue 3: Can't connect to MongoDB
**Fix:** Check `authSource=admin` is in the connection string

### Issue 4: Can't connect to RabbitMQ
**Fix:** Verify RabbitMQ is running and accessible from Railway

## 📝 Quick Reference

**Railway Service Config:**
```
Name: AccessLogWorker
Source: GitHub (NevilleColaco23/testAngularAPIDocker)
Branch: Implementing_RabbitMQ
Root: / (empty)
Dockerfile: AccessLogWorker/Dockerfile
Public Domain: ❌ Disabled
```

**Required Environment Variables:** 8 RabbitMQ vars + 1 MongoDB + 1 DOTNET_ENVIRONMENT = 10 total

## Next Steps After Worker Deployment

1. ✅ Deploy AccessLogWorker
2. ⏭️ Deploy WebApi (similar process)
3. ⏭️ Update WebApi CORS to allow Railway domain
4. ⏭️ Test end-to-end flow

**Ready to start? Push your code to GitHub first!** 🚀
