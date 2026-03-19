# ROOM PLANNER TROUBLESHOOTING CHECKLIST

## 🔍 Step 1: Verify MongoDB Data

Run `Debug_RoomPlanner.js` in MongoDB Compass or mongosh:
```bash
mongosh "your-connection-string"
use ListingDB
load("Debug_RoomPlanner.js")
```

Expected output:
- ✅ 7 rooms found for Property 1
- ✅ 6 bookings found for Property 1
- ✅ Property ID 1 exists
- ✅ Query returns rooms

---

## 🌐 Step 2: Check Browser Console

1. Open the application in browser
2. Press **F12** to open Developer Tools
3. Go to **Console** tab
4. Click on **Room Planner** tab
5. Look for these messages:

### ✅ Good messages (what you should see):
```
🏨 Loading rooms from backend API...
✅ Rooms loaded from API: 7 rooms
```

### ❌ Error messages (what to look for):
```
❌ Error loading rooms: ...
Failed to load rooms
HTTP 400/500 errors
```

---

## 🎯 Step 3: Verify Property Selection

The Room Planner only loads data for **selected properties**.

1. Click **"Change Properties"** button (top right)
2. Make sure **Property 1** is selected
3. The sample data uses PropertyId: 1

**How to check in browser console:**
```javascript
// Open console (F12) and run:
localStorage.getItem('selectedPropertyIds')
// Should return: "[1]" or include 1 in the array
```

**If property not selected:**
```javascript
// Run this in browser console to select Property 1:
localStorage.setItem('selectedPropertyIds', JSON.stringify([1]))
// Then refresh the page or switch tabs
```

---

## 🔧 Step 4: Check API Endpoint

Open **Network** tab in browser Developer Tools (F12):

1. Clear network log
2. Click **Room Planner** tab
3. Look for API call: `GET /api/v1/dashboard/rooms`

### Check request:
- URL should include: `?userId=1&propertyIds=1&activeOnly=true`
- Method: GET
- Status: Should be **200 OK**

### Check response:
- Should return JSON array with 7 room objects
- Each room should have: `roomId`, `roomNumber`, `roomName`, `propertyId`, etc.

### Common issues:
- **404 Not Found** → API endpoint not registered (check DashboardController)
- **500 Server Error** → Backend error (check API logs)
- **Empty array []** → No rooms match query (check PropertyId filter)

---

## 🎨 Step 5: Force Refresh

1. Switch to **Room Planner** tab
2. Click **"Refresh"** button (circular arrow icon)
3. Check if rooms appear

---

## 🐛 Step 6: Backend Debugging

If API returns empty array, check backend query:

### Add console logging:
In `GetRoomsByPropertyQueryHandler.cs`, add after line that executes query:
```csharp
Console.WriteLine($"MongoDB query returned {results.Count} rooms");
Console.WriteLine($"Property IDs filter: {string.Join(",", query.PropertyIds)}");
Console.WriteLine($"User ID: {query.UserId}");
```

### Check MongoDB aggregation:
Run this directly in MongoDB Compass:
```javascript
db.Room.aggregate([
  { $match: { 
    $or: [
      { PropertyId: 1 },
      { propertyId: 1 }
    ],
    $or: [
      { IsDeleted: { $ne: true } },
      { isDeleted: { $ne: true } }
    ]
  }},
  { $project: {
    roomId: { $ifNull: ["$RoomId", "$roomId"] },
    roomNumber: { $ifNull: ["$RoomCode", "$roomNumber"] },
    roomName: { $ifNull: ["$RoomName", "$roomName"] }
  }}
])
```

Should return 7 rooms.

---

## ✅ Quick Fix Commands

### In MongoDB Compass/mongosh:
```javascript
// 1. Verify data exists
db.Room.countDocuments({ PropertyId: 1, IsDeleted: false })  // Should be 7
db.Bookings.countDocuments({ propertyId: 1, Status: "Active" })  // Should be 6

// 2. Check property exists
db.Property.findOne({ PropertyId: 1 })

// 3. Test the exact query the backend uses
db.Room.aggregate([
  { $match: { 
    $or: [{ PropertyId: 1 }, { propertyId: 1 }],
    $or: [{ IsDeleted: { $ne: true } }, { isDeleted: { $ne: true } }]
  }},
  { $project: { roomNumber: { $ifNull: ["$RoomCode", "$roomNumber"] } } }
])
```

### In Browser Console (F12):
```javascript
// 1. Check selected properties
localStorage.getItem('selectedPropertyIds')

// 2. Set Property 1 if not selected
localStorage.setItem('selectedPropertyIds', JSON.stringify([1]))

// 3. Check user ID
const token = localStorage.getItem('token')
const payload = JSON.parse(atob(token.split('.')[1]))
console.log('User ID:', payload.UserId)

// 4. Manually call API to test
fetch('/api/v1/dashboard/rooms?userId=1&propertyIds=1&activeOnly=true', {
  headers: { 'Authorization': `Bearer ${token}` }
})
.then(r => r.json())
.then(d => console.log('API Response:', d))
```

---

## 🎬 Expected Behavior

When working correctly:
1. Click **Room Planner** tab
2. See **7 rooms** listed on left side
3. See **colored booking bars** on calendar
4. See **statistics** at top: "7 Total Rooms", "Occupancy Rate", etc.
5. Each room row shows bookings as colored cells for specific dates

---

## 📸 What You Should See

```
Room Occupancy Calendar              March 2026              🔄 Refresh

Statistics:
[7 Total Rooms] [2 Occupied] [5 Available] [29% Occupancy Rate]

Room Layout:
---------------------------------------------------------------------------
Room | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | ...
---------------------------------------------------------------------------
101  |   |   |   |   |   | ■ | ■ | ■ | ■ | ■  | ■  | ■  | ...  ← Occupied
102  |   |   |   | ■ | ■ | ■ | ■ |   |   |    |    |    | ...  ← Check-in today
103  |   |   | ■ | ■ | ■ | ■ | ■ | ■ | ■ |    |    |    | ...  ← Check-out today
201  |   |   |   |   |   |   |   |   |   | ■  | ■  | ■  | ...  ← Future booking
```

■ = Colored booking cell (red for occupied, blue for check-in, orange for checkout)

---

## 🆘 Still Not Working?

1. **Share browser console output** - Look for errors in Console tab
2. **Share API response** - Check Network tab for /rooms endpoint
3. **Check backend logs** - Look for errors in API console
4. **Verify property selection** - Run localStorage command above
5. **Re-run sample data script** - Delete and re-insert data

---

## 📝 Most Common Issues:

| Issue | Cause | Solution |
|-------|-------|----------|
| Empty calendar | Property not selected | Select Property 1 in UI |
| API 404 error | Endpoint not registered | Check DashboardController has GetRoomsByProperty endpoint |
| Empty API response | Wrong PropertyId | Verify sample data uses PropertyId: 1 |
| No bookings show | Wrong month | Sample data uses current month, navigate to correct month |
| UI not updating | Need manual refresh | Click Refresh button or switch tabs |
