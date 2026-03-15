# ⚡ PHASE 5 - ONE-COMMAND DEPLOYMENT

## 🚀 Quick Deploy (Copy-Paste Ready)

### **Option 1: Full Deployment**

```powershell
# Navigate to project root
cd "C:\Users\nevil\OneDrive\Desktop\Projects to learn\workspace\PropertyMasterV4.0"

# Restart Angular Frontend
cd app
Start-Process powershell -ArgumentList "ng serve"
cd ..

Write-Host "✅ Frontend restarted!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 NEXT STEPS:" -ForegroundColor Cyan
Write-Host "1. Open MongoDB Compass" -ForegroundColor Yellow
Write-Host "2. Connect to your MongoDB instance" -ForegroundColor Yellow
Write-Host "3. Click on 'MongoSH' tab" -ForegroundColor Yellow
Write-Host "4. Type: use ListingDB" -ForegroundColor Yellow
Write-Host "5. Paste contents of: MongoDB_Add_ActivityStream_Widget.js" -ForegroundColor Yellow
Write-Host "6. Press Enter" -ForegroundColor Yellow
Write-Host ""
Write-Host "🎉 Then open browser: http://localhost:4200" -ForegroundColor Green
```

---

### **Option 2: Just Restart Frontend**

```powershell
cd app
ng serve
```

---

## 📋 MongoDB Script (Copy This)

```javascript
// ============================================
// PASTE THIS IN MONGODB COMPASS MONGOSH TAB
// ============================================

// Step 1: Switch database
use ListingDB

// Step 2: Add widget
db.WidgetLibrary.insertOne({
  WidgetId: "user-activity-stream",
  WidgetType: "activity-stream",
  Name: "User Activity Stream",
  Description: "Real-time user activity tracking with statistics and timeline view",
  Icon: "timeline",
  Category: "Activity",
  DefaultSettings: {
    title: "User Activity",
    showStats: true,
    maxActivities: 10,
    refreshInterval: 60000
  },
  DefaultSize: { width: 6, height: 6 },
  MinSize: { width: 4, height: 4 },
  MaxSize: { width: 12, height: 8 },
  RequiredPermissions: ["dashboard.view", "activity.view"],
  IsActive: true,
  CreatedAt: new Date()
})

// Step 3: Verify
db.WidgetLibrary.findOne({ WidgetId: "user-activity-stream" })
```

---

## 🧪 Testing Commands

### **Test 1: Check Widget in Database**
```javascript
use ListingDB
db.WidgetLibrary.find({ WidgetId: "user-activity-stream" }).pretty()
```

**Expected:** Widget details displayed

---

### **Test 2: Check Activity Data**
```javascript
use ListingDB
db.UserActivityLogs.find().limit(5).sort({ Timestamp: -1 }).pretty()
```

**Expected:** Recent activities displayed

---

### **Test 3: Test API Endpoint**
```powershell
# In PowerShell:
Invoke-WebRequest -Uri "https://localhost:44346/api/v1/activity/summary?recentCount=10" -UseBasicParsing
```

**Expected:** JSON response with activity summary

---

### **Test 4: Test API (Alternative)**
```bash
# In bash/Git Bash:
curl https://localhost:44346/api/v1/activity/summary?recentCount=10
```

---

## 📦 Complete Deployment Steps

### **Step-by-Step**

```powershell
# 1️⃣ Check you're in project root
pwd
# Should show: ...\PropertyMasterV4.0

# 2️⃣ Start Frontend
cd app
npm install  # Only if new packages
ng serve

# Wait for "Compiled successfully"
```

```javascript
// 3️⃣ In MongoDB Compass MongoSH:
use ListingDB

// Run the full MongoDB_Add_ActivityStream_Widget.js script
// (See file in project root)
```

```powershell
# 4️⃣ Open Browser
start http://localhost:4200

# 5️⃣ Login and go to Dashboard

# 6️⃣ Click "Customize" → "Add Widget"

# 7️⃣ Find "User Activity Stream" → Add

# 8️⃣ Click "Save Dashboard"

# ✅ DONE!
```

---

## 🔍 Verification Checklist

After deployment, verify:

```powershell
# ✅ Frontend running
# Open: http://localhost:4200
# Check: No errors in browser console (F12)

# ✅ Widget in library
# Login → Dashboard → Customize → Add Widget
# Look for: "User Activity Stream" in Activity category

# ✅ Widget works
# Add widget to dashboard
# Check: Shows statistics and timeline
# Check: Auto-refreshes every 60 seconds

# ✅ Activities tracked
# Create a property or room
# Wait 60 seconds
# Check: New activity appears in widget
```

---

## 🐛 Quick Troubleshooting

### **Widget not showing in Add Widget dialog**
```javascript
// Re-run MongoDB script
use ListingDB
db.WidgetLibrary.deleteOne({ WidgetId: "user-activity-stream" })
// Then paste the full script again
```

### **Widget shows loading forever**
```powershell
# Check backend is running
# Open new terminal:
cd classfiles
dotnet run

# Or check if already running:
Get-Process -Name "PropertyMasterV4*"
```

### **No activities in widget**
```javascript
// Seed sample data
use ListingDB
// Run MongoDB_UserActivity_Setup.js script
```

### **API returns 401 Unauthorized**
```
Solution: Make sure you're logged in to the application
The activity endpoints require authentication
```

---

## 📊 Success Indicators

You know it's working when you see:

1. ✅ **Widget in picker:** "User Activity Stream" appears in Add Widget dialog
2. ✅ **Widget on dashboard:** Purple-blue gradient widget displays
3. ✅ **Statistics:** Three cards showing today/week/month counts
4. ✅ **Timeline:** Activities listed with icons and colors
5. ✅ **Refresh:** Data updates when clicking refresh button
6. ✅ **Auto-update:** New activities appear after 60 seconds

---

## 🎯 Quick Start (TL;DR)

```bash
# 1. Start frontend
cd app && ng serve

# 2. Open MongoDB Compass → MongoSH tab
use ListingDB
# Paste MongoDB_Add_ActivityStream_Widget.js

# 3. Open browser
http://localhost:4200

# 4. Dashboard → Customize → Add Widget → "User Activity Stream"

# 5. Save Dashboard

# ✅ Done!
```

---

## 📚 Documentation Quick Links

- **Complete Guide:** `PHASE5_ACTIVITY_WIDGET_COMPLETE.md`
- **Quick Start:** `PHASE5_QUICK_START.md`
- **Visual Testing:** `PHASE5_VISUAL_TESTING_GUIDE.md`
- **Final Summary:** `PHASE5_FINAL_SUMMARY.md`
- **Master Plan:** `USER_ACTIVITY_WIDGET_MASTER_PLAN.md`

---

## 🎉 Deployment Complete!

Your Activity Stream Widget is now live! 🚀

**Next Steps:**
1. Customize widget settings
2. Track user activities
3. Monitor dashboard usage
4. Enjoy real-time insights!

---

**Happy Tracking! 📊✨**
