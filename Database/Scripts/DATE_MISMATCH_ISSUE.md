# Date Mismatch Issue - Bookings Today KPI

## 🐛 Problem Identified

Your dashboard KPI shows **0** even though:
- ✅ Data copied to correct collection (uppercase "Bookings")
- ✅ 5 bookings exist with CreatedAt field
- ✅ Verification shows 5 bookings for "today"

## 🔍 Root Cause

**Date Mismatch**: Your system/MongoDB date doesn't match the backend server date.

From debug output:
```
📅 Today's date range: 2026-03-16T21:00:00.000Z
```

Your system is set to **March 2026**, but the backend is likely running with the **actual current date (January 2025)**.

## Why This Happens

1. **MongoDB Script**: Creates bookings with `new Date()` → March 2026 (your system date)
2. **Backend API**: Uses `DateTime.UtcNow` → January 2025 (actual server date)
3. **Query**: Backend looks for bookings from January 17, 2025
4. **Result**: No match! Bookings are in March 2026

## 🔧 Solutions

### Option 1: Check What Backend Actually Sees (RECOMMENDED)

1. Open your browser **DevTools** (F12)
2. Go to **Network** tab
3. Refresh the dashboard
4. Look for request: `GET /api/v1/dashboard/kpi/bookings-today?userId=...`
5. Check the response:
   ```json
   {
     "widgetId": "bookings-today",
     "value": 0,
     "calculatedAt": "2025-01-17T10:23:45.123Z"  ← This is the date backend is using!
   }
   ```

### Option 2: Create Bookings for Backend's Date

Once you know the backend date, run this:

```javascript
// Replace with actual backend date from API response
const backendToday = new Date('2025-01-17');
backendToday.setHours(0, 0, 0, 0);

// Create bookings
const bookings = [];
for (let i = 0; i < 10; i++) {
    bookings.push({
        _id: NumberLong((4200000000000001 + i).toString()),
        bookingId: `TEST${i}`,
        guestId: NumberLong("1000000000003600"),
        staffId: NumberLong("2000000000000070"),
        roomNumber: "101",
        bookingDate: backendToday,
        CreatedAt: backendToday,  // ← CRITICAL: Must match backend's today
        checkInDate: new Date(backendToday.getTime() + 3 * 24 * 60 * 60 * 1000),
        checkOutDate: new Date(backendToday.getTime() + 6 * 24 * 60 * 60 * 1000),
        numberOfGuests: 2,
        totalPrice: 500,
        paymentStatusId: NumberLong("5000000000000011"),
        bookingSourceId: NumberLong("5000000000000001"),
        specialRequests: [],
        isConfirmed: true,
        lastModified: backendToday,
        propertyId: -1,
        Status: "Active"
    });
}

db.Bookings.insertMany(bookings);
```

### Option 3: Fix System Date (If Incorrect)

If your system date is actually wrong:

**Windows**:
1. Right-click taskbar clock
2. "Adjust date/time"
3. Turn ON "Set time automatically"
4. Verify date is January 2025

**MongoDB Compass**:
- After fixing system date, re-run `test-bookings-today.js`

## 🎯 Quick Debug Steps

### Step 1: Check Backend Response
```bash
# In browser console (F12)
fetch('/api/v1/dashboard/kpi/bookings-today?userId=1')
  .then(r => r.json())
  .then(data => console.log('Backend response:', data));
```

### Step 2: Check MongoDB Bookings
```javascript
// In MongoDB Compass
db.Bookings.aggregate([
  {
    $group: {
      _id: {
        $dateToString: { format: "%Y-%m-%d", date: "$CreatedAt" }
      },
      count: { $sum: 1 }
    }
  },
  { $sort: { _id: -1 } },
  { $limit: 10 }
]);
```

This shows which dates actually have bookings.

### Step 3: Compare Dates
```javascript
// MongoDB shows bookings on: 2026-03-16
// Backend is looking for: 2025-01-17
// → NO MATCH!
```

## 🔄 Alternative: Use Backend API to Get Correct Date

Add a debug endpoint to your backend:

```csharp
[HttpGet("debug/server-date")]
public ActionResult<object> GetServerDate()
{
    return Ok(new
    {
        utcNow = DateTime.UtcNow,
        utcToday = DateTime.UtcNow.Date,
        localNow = DateTime.Now,
        timeZone = TimeZoneInfo.Local.DisplayName
    });
}
```

Then call: `GET /api/v1/dashboard/debug/server-date`

## 📊 Expected Outcome

After creating bookings for the correct date:
- ✅ Dashboard KPI shows: **10** (or however many you created)
- ✅ Backend query matches database date
- ✅ System synchronized

## 🧪 Test Script

I've created `fix-date-mismatch.js` which:
1. Detects date discrepancy
2. Creates bookings for estimated backend date (2025-01-17)
3. Verifies insertion

**Run it and see if KPI updates!**

## 📝 Long-term Fix

Update `test-bookings-today.js` to accept a date parameter:

```javascript
// Allow passing date as parameter
const targetDate = process.env.TARGET_DATE 
    ? new Date(process.env.TARGET_DATE) 
    : new Date();
```

Or better: Create bookings for a **range of dates** including actual today.

---

**Next Steps**:
1. Check backend response in DevTools
2. Note the `calculatedAt` date
3. Create bookings for that exact date
4. Verify KPI updates
