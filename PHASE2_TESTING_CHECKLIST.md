# 🧪 Phase 2 Testing Checklist

## Pre-Test Setup

### ✅ Verify Current State
- [x] Backend API running?
- [x] MongoDB running?
- [x] Phase 1 widgets seeded (4 KPI widgets)?
- [ ] Phase 2 widgets seeded (6 new widgets)?

---

## Test 1: Seed Phase 2 Widgets (5 minutes)

### Instructions:
1. Open **MongoDB Compass**
2. Connect to your database
3. Select database (e.g., "PropertyMaster")
4. Click **"_MONGOSH"** tab at bottom
5. Open file: `MongoDB_Phase2_Widget_Seed.js`
6. Copy entire contents
7. Paste into mongosh terminal
8. Press **Enter**

### Expected Output:
```
🎨 Phase 2: Adding Additional Widget Types...
📦 Inserting new widgets...
  ✅ Added: Revenue Chart
  ✅ Added: Bookings Chart
  ✅ Added: Booking Calendar
  ✅ Added: Recent Activity
  ✅ Added: Recent Bookings
  ✅ Added: Notifications

📊 Inserted 6 new widgets
📊 Total Widgets: 10
  📊 KPI Widgets: 4
  📊 Analytics Widgets: 2
  📊 Bookings Widgets: 1
  📊 Activity Widgets: 3
```

### Verification:
```javascript
// Run in mongosh
db.WidgetLibrary.countDocuments()
// Should return: 10

db.WidgetLibrary.find({}, {Name: 1, WidgetType: 1, _id: 0}).pretty()
// Should show all 10 widgets
```

**Status**: [ ] PASSED / [ ] FAILED

---

## Test 2: Build Angular App (2 minutes)

### Instructions:
```powershell
cd app
npm run build
```

### Expected Output:
```
✔ Building...
Application bundle generation complete.
```

### Check for Errors:
- [ ] No compilation errors
- [ ] Chart widget component compiles
- [ ] List widget component compiles
- [ ] Calendar widget component compiles
- [ ] Dashboard component compiles

**Status**: [ ] PASSED / [ ] FAILED

---

## Test 3: Start Angular Development Server (1 minute)

### Instructions:
```powershell
cd app
npm start
```

### Expected Output:
```
** Angular Live Development Server is listening on localhost:4200
✔ Compiled successfully
```

### Browser:
- Open: `http://localhost:4200`
- Should auto-navigate or manually go to dashboard

**Status**: [ ] PASSED / [ ] FAILED

---

## Test 4: Backend API Health Check (1 minute)

### Verify Backend is Running:
```powershell
# In a new terminal
cd WebApi
dotnet run
```

### Expected Output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

### Test API Endpoint:
```bash
# Using curl or browser
https://localhost:5001/api/v1/dashboard/widgets?activeOnly=true
```

**Expected**: JSON array with 10 widgets

**Status**: [ ] PASSED / [ ] FAILED

---

## Test 5: Visual Inspection (5 minutes)

### Navigate to Dashboard:
`http://localhost:4200/propertyLanding/dashboard1`

### Check Sections:

#### 1. Key Metrics Section
- [ ] 4 KPI cards visible
- [ ] Total Properties: 24 (with ↗ +12%)
- [ ] Total Rooms: 156
- [ ] Bookings Today: 12
- [ ] Occupancy Rate: 78% (with ↗ +5%)
- [ ] Cards have hover effect
- [ ] Icons display correctly

#### 2. Analytics Section
- [ ] Section title: "Analytics"
- [ ] 1 chart widget visible
- [ ] Chart widget has title: "Booking Trends"
- [ ] Chart shows data summary (Total: 143, Avg: 24)
- [ ] Chart menu button (⋮) present

#### 3. Two-Column Layout

**Left Column - Activity Feed:**
- [ ] Section title: "Activity Feed"
- [ ] List widget visible
- [ ] 5 activity items shown
- [ ] Each item has:
  - [ ] Icon with color
  - [ ] Title and subtitle
  - [ ] Timestamp (e.g., "1h ago")
  - [ ] Metadata badge
- [ ] Items have hover effect

**Right Column - Calendar:**
- [ ] Section title: "Calendar"
- [ ] Calendar widget visible
- [ ] Current month displayed
- [ ] Navigation arrows (◄ ►)
- [ ] "Today" button present
- [ ] Calendar grid with days
- [ ] Today is highlighted
- [ ] Event indicators visible (colored dots)
- [ ] Statistics: "This Month" section
- [ ] Event count displayed

**Status**: [ ] PASSED / [ ] FAILED

---

## Test 6: Browser Console Check (2 minutes)

### Open Developer Tools (F12)

#### Console Tab - Check for Logs:
```javascript
✅ Dashboard1 component initialized
✅ Dashboard config loaded: null
✅ Widget library loaded: [Array with 10 widgets]
```

#### Check for Errors:
- [ ] No red errors
- [ ] No 404 errors
- [ ] No compilation errors

**Status**: [ ] PASSED / [ ] FAILED

---

## Test 7: Network Tab Verification (2 minutes)

### Developer Tools → Network Tab

#### Expected API Calls:

1. **GET dashboard config**
```
GET /api/v1/dashboard/user/1?defaultOnly=true
Status: 200 OK (or 404 if no config)
```

2. **GET widget library**
```
GET /api/v1/dashboard/widgets?activeOnly=true
Status: 200 OK
Response: Array with 10 objects
```

#### Verify Response:
```json
[
  {
    "widgetId": "total-properties",
    "widgetType": "kpi-card",
    "name": "Total Properties",
    ...
  },
  {
    "widgetId": "revenue-chart",
    "widgetType": "chart",
    ...
  },
  ...
]
```

**Status**: [ ] PASSED / [ ] FAILED

---

## Test 8: Responsive Design (3 minutes)

### Desktop View (>1200px):
- [ ] KPI cards: 4 columns
- [ ] Chart widget: Full width
- [ ] List & Calendar: Side by side

### Tablet View (768-1200px):
- [ ] KPI cards: 2 columns
- [ ] Chart widget: Full width
- [ ] List & Calendar: Stacked

### Mobile View (<768px):
- [ ] All widgets: Single column
- [ ] All sections: Stacked vertically
- [ ] Dashboard selector: Full width

**Test Method**:
- Press F12 → Toggle device toolbar
- Test different screen sizes

**Status**: [ ] PASSED / [ ] FAILED

---

## Test 9: Widget Interactions (3 minutes)

### KPI Cards:
- [ ] Hover effect: Card lifts up
- [ ] Trend arrows visible (up/down)
- [ ] Values display correctly

### Chart Widget:
- [ ] Click menu button (⋮)
- [ ] Menu opens with 3 options:
  - [ ] Line Chart
  - [ ] Bar Chart
  - [ ] Pie Chart
- [ ] Selecting changes chart type

### List Widget:
- [ ] Hover over list items
- [ ] Background changes color
- [ ] Timestamps show relative time
- [ ] Metadata badges visible

### Calendar Widget:
- [ ] Click ◄ button: Goes to previous month
- [ ] Click ► button: Goes to next month
- [ ] Click "Today" button: Returns to current month
- [ ] Hover over days with events: Shows tooltip
- [ ] Today is highlighted

**Status**: [ ] PASSED / [ ] FAILED

---

## Test 10: Dashboard Selector (2 minutes)

### Test Dropdown:
- [ ] Dropdown visible in top-right
- [ ] Click dropdown
- [ ] 3 options visible:
  - [ ] Overview Dashboard (current)
  - [ ] Analytics Dashboard
  - [ ] Reports Dashboard
- [ ] Icons display in dropdown
- [ ] Selecting other dashboards shows "not yet implemented" snackbar

**Status**: [ ] PASSED / [ ] FAILED

---

## 🐛 Troubleshooting

### Issue: Widgets not showing

**Check**:
```javascript
// Browser console
const component = ng.getComponent(document.querySelector('app-dashboard1'));
console.log('KPI Cards:', component.kpiCards);
console.log('List Widgets:', component.listWidgets);
console.log('Chart Widgets:', component.chartWidgets);
console.log('Calendar Widgets:', component.calendarWidgets);
```

**Fix**: Verify component initialization

---

### Issue: Build fails

**Error**: "Cannot find module"

**Fix**:
```bash
cd app
npm install
npm run build
```

---

### Issue: API returns 401 Unauthorized

**Check**: JWT token exists
```javascript
localStorage.getItem('token')
```

**Fix**: Login to get new token

---

### Issue: No widgets in MongoDB

**Check**:
```javascript
db.WidgetLibrary.countDocuments()
```

**Fix**: Re-run both seed scripts:
1. `MongoDB_Compass_Dashboard_Seed.js`
2. `MongoDB_Phase2_Widget_Seed.js`

---

### Issue: Calendar not rendering

**Check**: Browser console for errors

**Fix**: Verify CalendarWidgetComponent import in dashboard1.component.ts

---

## 📊 Test Results Summary

| Test | Status | Notes |
|------|--------|-------|
| 1. Seed Widgets | ☐ | |
| 2. Build App | ☐ | |
| 3. Start Server | ☐ | |
| 4. Backend API | ☐ | |
| 5. Visual Check | ☐ | |
| 6. Console Check | ☐ | |
| 7. Network Check | ☐ | |
| 8. Responsive | ☐ | |
| 9. Interactions | ☐ | |
| 10. Dropdown | ☐ | |

**Overall Status**: ☐ PASSED / ☐ FAILED

---

## ✅ Success Criteria

Phase 2 is successful if:
- ✅ All 10 widgets in MongoDB
- ✅ Dashboard displays 7 widgets (4 KPI + 1 chart + 1 list + 1 calendar)
- ✅ No console errors
- ✅ API calls return 200 OK
- ✅ Responsive design works
- ✅ All interactions work
- ✅ Visual design looks good

---

## 🚀 Next Actions After Testing

### If All Tests Pass:
- [ ] Take screenshots
- [ ] Test with real data (Phase 4)
- [ ] Start Phase 3 (Drag-and-drop)

### If Tests Fail:
- [ ] Document error messages
- [ ] Check troubleshooting section
- [ ] Review error logs
- [ ] Ask for help if needed

---

## 📸 Screenshots to Take

1. **Full Dashboard View** - All widgets visible
2. **KPI Cards** - Close-up of metrics
3. **Chart Widget** - Analytics section
4. **List Widget** - Activity feed
5. **Calendar Widget** - Monthly view
6. **Mobile View** - Responsive layout
7. **Browser Console** - No errors
8. **Network Tab** - Successful API calls

---

## 🎓 Learning Points

After testing, you should understand:
- How widget factory pattern works
- How mock data flows to components
- How Angular routing and components interact
- How API integration works
- How responsive design adapts

---

**Testing Time**: ~25 minutes
**Difficulty**: Easy
**Required**: MongoDB Compass, Browser, Code Editor

**Ready to start?** Begin with Test 1! 🚀
