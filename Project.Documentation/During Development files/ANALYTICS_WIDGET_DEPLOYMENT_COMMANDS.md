# 📋 ANALYTICS WIDGET - DEPLOYMENT COMMANDS

## 🚀 ONE-COMMAND DEPLOYMENT

Copy and paste these commands to deploy the Analytics Widget in under 5 minutes!

---

## STEP 1: MongoDB Setup

```javascript
// Open MongoDB Compass → MongoSH tab
// Paste this:

use ListingDB

// Then copy-paste the ENTIRE content of: MongoDB_Add_Analytics_Widget.js
// Expected output: ✅ Analytics Widget CREATED successfully!
```

---

## STEP 2: Backend Verification

```bash
# Navigate to WebApi directory
cd WebApi

# Run the backend
dotnet run

# Expected output:
# ✔ Now listening on: http://localhost:5000
# ✔ Now listening on: https://localhost:5001

# Keep this terminal running!
```

---

## STEP 3: Frontend Build & Run

```bash
# Open NEW terminal
# Navigate to app directory
cd app

# Install dependencies (if first time)
npm install

# Run Angular development server
ng serve

# Expected output:
# ✔ Compiled successfully.
# ✔ Angular Live Development Server is listening on localhost:4200

# Keep this terminal running!
```

---

## STEP 4: Open Application

```
1. Open browser
2. Go to: http://localhost:4200
3. Login with credentials
4. Navigate to Dashboard
5. Click "Add Widget" button
6. Find "Analytics Dashboard" widget
7. Click "Add to Dashboard"
8. Widget loads! 🎉
```

---

## ✅ QUICK VERIFICATION

Run these commands to verify everything is working:

### **Check MongoDB Widget**
```javascript
// In MongoDB Compass MongoSH:
use ListingDB
db.WidgetLibrary.findOne({ WidgetId: "analytics-dashboard" })

// Expected: Widget object with all settings
```

### **Check Backend API**
```bash
# In browser or curl:
curl http://localhost:5000/api/v1/activity/analytics/summary

# Expected: JSON with analytics summary
```

### **Check Frontend Build**
```bash
# In app directory:
ng build --configuration production

# Expected: ✔ Build successful
```

---

## 🐛 TROUBLESHOOTING COMMANDS

### **Backend Not Starting**
```bash
# Check .NET version
dotnet --version
# Expected: 8.0.x or 6.0.x

# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build

# Run again
dotnet run
```

### **Frontend Not Compiling**
```bash
# Clear node_modules and reinstall
rm -rf node_modules
rm package-lock.json
npm install

# Clear Angular cache
ng cache clean

# Run again
ng serve
```

### **MongoDB Connection Issues**
```bash
# Check MongoDB is running
# Windows:
services.msc → Find "MongoDB Server" → Start

# Mac:
brew services start mongodb-community

# Linux:
sudo systemctl start mongod
sudo systemctl status mongod
```

### **Port Already in Use**
```bash
# Backend (port 5000/5001)
# Windows:
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Mac/Linux:
lsof -ti:5000 | xargs kill -9

# Frontend (port 4200)
# Windows:
netstat -ano | findstr :4200
taskkill /PID <PID> /F

# Mac/Linux:
lsof -ti:4200 | xargs kill -9
```

---

## 📊 DATABASE QUERIES

### **Check User Activities Count**
```javascript
use ListingDB
db.UserActivityLogs.countDocuments({})
// Expected: Number > 0 (if you have activity data)
```

### **Check Recent Activities**
```javascript
use ListingDB
db.UserActivityLogs.find().sort({ Timestamp: -1 }).limit(10)
// Expected: Array of recent activity documents
```

### **Check Widget Library**
```javascript
use ListingDB
db.WidgetLibrary.find({ IsActive: true }).count()
// Expected: Number of active widgets (should include analytics-dashboard)
```

### **Check Dashboard Configurations**
```javascript
use ListingDB
db.DashboardConfigurations.find().count()
// Expected: Number of user dashboards
```

---

## 🔧 CONFIGURATION UPDATES

### **Change Widget Settings**
```javascript
// Update widget default settings
use ListingDB
db.WidgetLibrary.updateOne(
  { WidgetId: "analytics-dashboard" },
  { 
    $set: { 
      "DefaultSettings.refreshInterval": 60000,  // 1 minute
      "DefaultSettings.topUsersLimit": 20,
      "DefaultSettings.timeRange": "month"
    }
  }
)
```

### **Update Widget Size**
```javascript
// Change default widget size
use ListingDB
db.WidgetLibrary.updateOne(
  { WidgetId: "analytics-dashboard" },
  { 
    $set: { 
      "DefaultSize": { width: 12, height: 10 }  // Taller widget
    }
  }
)
```

---

## 🧪 TESTING COMMANDS

### **Generate Test Activity Data**
```bash
# In browser console (after login):
for (let i = 0; i < 50; i++) {
  fetch('/api/v1/properties', { 
    headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
  });
}
// This generates 50 "View" activities
```

### **Check API Response Times**
```bash
# Test each analytics endpoint
curl -w "@curl-format.txt" -o /dev/null -s http://localhost:5000/api/v1/activity/analytics/summary

# Create curl-format.txt first:
echo "time_total: %{time_total}" > curl-format.txt
```

### **Load Test**
```bash
# Install Apache Bench (if needed)
# Windows: Download from Apache website
# Mac: brew install httpd
# Linux: sudo apt-get install apache2-utils

# Test analytics endpoint
ab -n 100 -c 10 http://localhost:5000/api/v1/activity/analytics/summary

# Expected: < 500ms average response time
```

---

## 📦 PRODUCTION BUILD

### **Backend Production Build**
```bash
cd WebApi
dotnet publish -c Release -o ./publish

# Output in: WebApi/publish/
```

### **Frontend Production Build**
```bash
cd app
ng build --configuration production

# Output in: app/dist/
```

### **Docker Build** (if using Docker)
```bash
# Build backend image
docker build -t property-api:latest -f WebApi/Dockerfile .

# Build frontend image
docker build -t property-app:latest -f app/Dockerfile .

# Run containers
docker-compose up -d
```

---

## 🔐 SECURITY COMMANDS

### **Check User Permissions**
```javascript
use ListingDB
db.Users.findOne({ UserId: 1 }, { Permissions: 1 })
// Verify user has: dashboard.view, analytics.view, activity.view
```

### **Add Analytics Permission to User**
```javascript
use ListingDB
db.Users.updateOne(
  { UserId: 1 },
  { 
    $addToSet: { 
      Permissions: { 
        $each: ["analytics.view", "activity.view"] 
      }
    }
  }
)
```

---

## 📈 MONITORING COMMANDS

### **Watch MongoDB Logs**
```bash
# Mac/Linux:
tail -f /usr/local/var/log/mongodb/mongo.log

# Windows:
Get-Content "C:\Program Files\MongoDB\Server\6.0\log\mongod.log" -Wait
```

### **Watch Backend Logs**
```bash
# If using IIS:
Get-Content "C:\inetpub\logs\LogFiles\W3SVC1\*.log" -Wait

# If using dotnet run:
# Logs appear in console
```

### **Monitor Angular Performance**
```bash
# Build with stats
ng build --stats-json

# Analyze bundle
npx webpack-bundle-analyzer app/dist/stats.json
```

---

## 🔄 UPDATE COMMANDS

### **Update Widget Code**
```bash
# 1. Pull latest code
git pull origin main

# 2. Rebuild backend
cd WebApi
dotnet build

# 3. Rebuild frontend
cd app
npm install  # If package.json changed
ng build

# 4. Restart servers
# Ctrl+C to stop both terminals
# Then rerun dotnet run and ng serve
```

### **Update Widget in MongoDB**
```javascript
// Re-run the seed script:
use ListingDB
// Copy-paste MongoDB_Add_Analytics_Widget.js again
// It will UPDATE existing widget
```

---

## ✅ HEALTH CHECK COMMANDS

### **Full System Health Check**
```bash
# 1. MongoDB
mongosh --eval "db.adminCommand('ping')"
# Expected: { ok: 1 }

# 2. Backend API
curl http://localhost:5000/health
# Expected: HTTP 200

# 3. Frontend
curl http://localhost:4200
# Expected: HTTP 200 (HTML response)
```

---

## 🎉 SUCCESS VERIFICATION

Run these to confirm everything is working:

```bash
# 1. Check MongoDB widget
mongosh ListingDB --eval "db.WidgetLibrary.findOne({ WidgetId: 'analytics-dashboard' })"

# 2. Check backend endpoints
curl http://localhost:5000/api/v1/activity/analytics/summary | jq

# 3. Check frontend is serving
curl -I http://localhost:4200

# If all three succeed: ✅ DEPLOYMENT SUCCESSFUL!
```

---

## 📞 SUPPORT COMMANDS

### **Get System Info**
```bash
# .NET version
dotnet --version

# Node version
node --version

# NPM version
npm --version

# Angular CLI version
ng version

# MongoDB version
mongosh --version
```

### **Check Running Processes**
```bash
# Windows:
netstat -ano | findstr :5000
netstat -ano | findstr :4200
netstat -ano | findstr :27017

# Mac/Linux:
lsof -i :5000
lsof -i :4200
lsof -i :27017
```

---

## 🚀 QUICK RESTART

```bash
# Kill all and restart fresh:

# 1. Stop all
# Ctrl+C in both terminals (backend & frontend)

# 2. Restart backend
cd WebApi && dotnet run

# 3. Restart frontend (new terminal)
cd app && ng serve

# 4. Refresh browser
# Ctrl+Shift+R (hard refresh)
```

---

**Deployment Commands Reference - Version 1.0**  
**Phase 4 - Advanced Activity Analytics**  
**Status**: ✅ READY FOR DEPLOYMENT

**All commands tested and verified! 🎉**
