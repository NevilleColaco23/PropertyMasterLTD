# ⚡ Quick Testing Commands - Copy & Paste

## 1️⃣ Seed MongoDB (Phase 2 Widgets)

```javascript
// ============================================
// COPY THIS ENTIRE BLOCK INTO MONGODB COMPASS
// ============================================
// 1. Open MongoDB Compass
// 2. Select your database
// 3. Click "_MONGOSH" tab
// 4. Paste below and press Enter
// ============================================

print("\n🎨 Phase 2: Adding Additional Widget Types...\n");

print("📦 Inserting new widgets...");

const newWidgets = [
  {
    WidgetId: "revenue-chart",
    WidgetType: "chart",
    Name: "Revenue Chart",
    Description: "Monthly revenue visualization",
    Icon: "trending_up",
    Category: "Analytics",
    DefaultSettings: { title: "Revenue Trend", chartType: "line" },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view", "analytics.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "bookings-chart",
    WidgetType: "chart",
    Name: "Bookings Chart",
    Description: "Booking trends over time",
    Icon: "bar_chart",
    Category: "Analytics",
    DefaultSettings: { title: "Bookings Trend", chartType: "bar" },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "booking-calendar",
    WidgetType: "calendar",
    Name: "Booking Calendar",
    Description: "Visual calendar of bookings and events",
    Icon: "calendar_month",
    Category: "Bookings",
    DefaultSettings: { title: "Bookings Calendar", view: "month" },
    DefaultSize: { width: 12, height: 6 },
    MinSize: { width: 8, height: 5 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "recent-activity",
    WidgetType: "list",
    Name: "Recent Activity",
    Description: "Latest system activity and events",
    Icon: "list",
    Category: "Activity",
    DefaultSettings: { title: "Recent Activity", itemsToShow: 10 },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "recent-bookings",
    WidgetType: "list",
    Name: "Recent Bookings",
    Description: "Latest booking records",
    Icon: "receipt_long",
    Category: "Bookings",
    DefaultSettings: { title: "Recent Bookings", itemsToShow: 5 },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "notifications-list",
    WidgetType: "list",
    Name: "Notifications",
    Description: "System notifications and alerts",
    Icon: "notifications",
    Category: "Activity",
    DefaultSettings: { title: "Notifications", itemsToShow: 8 },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  }
];

let insertedCount = 0;
newWidgets.forEach(widget => {
  const existing = db.WidgetLibrary.findOne({ WidgetId: widget.WidgetId });
  if (!existing) {
    db.WidgetLibrary.insertOne(widget);
    insertedCount++;
    print(`  ✅ Added: ${widget.Name}`);
  } else {
    print(`  ⏭️  Skipped (exists): ${widget.Name}`);
  }
});

print(`\n📊 Inserted ${insertedCount} new widgets`);
print(`📊 Total Widgets: ${db.WidgetLibrary.countDocuments()}`);
print("\n✅ Phase 2 widget library update complete!\n");
```

---

## 2️⃣ Verify MongoDB Data

```javascript
// Check total widget count
db.WidgetLibrary.countDocuments()
// Expected: 10

// List all widgets
db.WidgetLibrary.find({}, {Name: 1, WidgetType: 1, Category: 1, _id: 0}).pretty()

// Count by type
db.WidgetLibrary.countDocuments({ WidgetType: "kpi-card" })  // 4
db.WidgetLibrary.countDocuments({ WidgetType: "chart" })     // 2
db.WidgetLibrary.countDocuments({ WidgetType: "calendar" })  // 1
db.WidgetLibrary.countDocuments({ WidgetType: "list" })      // 3
```

---

## 3️⃣ Build Angular App

```powershell
# PowerShell
cd app
npm run build
```

**Expected**: ✔ Building... Application bundle generation complete.

---

## 4️⃣ Start Angular Development Server

```powershell
# PowerShell - Terminal 1
cd app
npm start
```

**Expected**: ✔ Compiled successfully
**Open**: http://localhost:4200

---

## 5️⃣ Start .NET Backend API

```powershell
# PowerShell - Terminal 2
cd WebApi
dotnet run
```

**Expected**: Now listening on: https://localhost:5001

---

## 6️⃣ Test API Endpoints

### Using PowerShell:

```powershell
# Get Widget Library
Invoke-RestMethod -Uri "https://localhost:5001/api/v1/dashboard/widgets?activeOnly=true" -Headers @{"Authorization"="Bearer YOUR_TOKEN"} -SkipCertificateCheck

# Get User Dashboard
Invoke-RestMethod -Uri "https://localhost:5001/api/v1/dashboard/user/1?defaultOnly=true" -Headers @{"Authorization"="Bearer YOUR_TOKEN"} -SkipCertificateCheck
```

### Using Browser:
```
https://localhost:5001/api/v1/dashboard/widgets?activeOnly=true
```

---

## 7️⃣ Browser Console Commands

### Open Developer Tools (F12), then paste:

```javascript
// Check component state
const component = ng.getComponent(document.querySelector('app-dashboard1'));

console.log('=== Dashboard State ===');
console.log('Loading:', component.loading);
console.log('KPI Cards:', component.kpiCards.length);
console.log('List Widgets:', component.listWidgets.length);
console.log('Chart Widgets:', component.chartWidgets.length);
console.log('Calendar Widgets:', component.calendarWidgets.length);
console.log('Widget Library:', component.widgetLibrary.length);

// Check JWT token
const token = localStorage.getItem('token');
if (token) {
  const payload = JSON.parse(atob(token.split('.')[1]));
  console.log('User ID:', payload.UserId);
  console.log('Token expires:', new Date(payload.exp * 1000));
}

// Check for errors
console.log('Errors:', window.performance.getEntriesByType('navigation'));
```

---

## 8️⃣ Debug Commands

### If widgets not showing:

```javascript
// Browser Console
console.log('=== Debug Info ===');
console.log('Route:', window.location.pathname);
console.log('Component exists:', !!document.querySelector('app-dashboard1'));
console.log('KPI cards in DOM:', document.querySelectorAll('app-kpi-card-widget').length);
console.log('List widgets in DOM:', document.querySelectorAll('app-list-widget').length);
console.log('Chart widgets in DOM:', document.querySelectorAll('app-chart-widget').length);
console.log('Calendar widgets in DOM:', document.querySelectorAll('app-calendar-widget').length);
```

### If API calls failing:

```javascript
// Check network requests
fetch('https://localhost:5001/api/v1/dashboard/widgets?activeOnly=true', {
  headers: {
    'Authorization': 'Bearer ' + localStorage.getItem('token')
  }
})
.then(r => r.json())
.then(data => console.log('Widgets:', data))
.catch(err => console.error('Error:', err));
```

---

## 9️⃣ Quick Fixes

### Clear browser cache:
```javascript
// Browser Console
localStorage.clear();
sessionStorage.clear();
location.reload();
```

### Reinstall npm packages:
```powershell
cd app
Remove-Item -Recurse -Force node_modules
Remove-Item -Force package-lock.json
npm install
npm start
```

### Rebuild backend:
```powershell
cd WebApi
dotnet clean
dotnet build
dotnet run
```

---

## 🔟 Screenshot Locations

### Windows:
- **Full Screenshot**: `Win + Shift + S` → Select area → Paste in Paint
- **Save to**: `C:\Users\YOUR_NAME\Pictures\Screenshots\`

### Browser DevTools:
- **F12** → **Console** tab → Right-click console → **Save as...**
- **F12** → **Network** tab → Right-click request → **Copy as cURL**

---

## 📊 Expected Visual Checklist

After running all commands, you should see:

```
✅ Dashboard Header with selector
✅ Key Metrics: 4 KPI cards (24, 156, 12, 78%)
✅ Analytics: 1 Chart widget
✅ Activity Feed: 1 List widget (5 items)
✅ Calendar: 1 Calendar widget (current month)
✅ No console errors (red text)
✅ Network calls: 200 OK
✅ Responsive on mobile/tablet
```

---

## 🚨 Emergency Commands

### Kill all node processes:
```powershell
Get-Process node | Stop-Process -Force
```

### Kill port 4200 (if blocked):
```powershell
netstat -ano | findstr :4200
taskkill /PID <PID_NUMBER> /F
```

### Reset Git branch:
```powershell
git status
git stash
git checkout Dashboard_Implementation
git pull
```

---

## ✅ Success Verification

Run this final check:

```javascript
// MongoDB Compass - mongosh
print("=== FINAL VERIFICATION ===");
print("Total Widgets:", db.WidgetLibrary.countDocuments());
print("Expected: 10");
print("\nWidget Types:");
print("KPI Cards:", db.WidgetLibrary.countDocuments({WidgetType:"kpi-card"}), "/ 4");
print("Charts:", db.WidgetLibrary.countDocuments({WidgetType:"chart"}), "/ 2");
print("Calendars:", db.WidgetLibrary.countDocuments({WidgetType:"calendar"}), "/ 1");
print("Lists:", db.WidgetLibrary.countDocuments({WidgetType:"list"}), "/ 3");

if (db.WidgetLibrary.countDocuments() === 10) {
  print("\n✅ ALL CHECKS PASSED!");
} else {
  print("\n❌ WIDGET COUNT MISMATCH - Rerun seed script");
}
```

---

**Testing Order**:
1. Seed MongoDB → 2. Build App → 3. Start Servers → 4. Open Browser → 5. Verify

**Total Time**: ~10 minutes

**Need Help?** Check `PHASE2_TESTING_CHECKLIST.md` for detailed troubleshooting! 🚀
