# Test Bookings for Today - KPI Widget Testing

## Purpose
This script creates **5 test bookings for TODAY** to verify the **"Bookings Today" KPI widget** is working correctly.

## Problem It Solves
- The seed data (`seed-bookings-data.js`) creates bookings for Jan-Apr 2026
- Current date is in 2025, so "Bookings Today" KPI shows **0**
- This test script creates bookings with **today's date** to verify live backend integration

## What It Does
Creates 5 bookings with:
- ✅ **bookingDate** = TODAY (when booking was made)
- ✅ **CreatedAt** = TODAY (backend uses this field for KPI calculation)
- ✅ **checkInDate** = 3 days from today (future reservation)
- ✅ **checkOutDate** = 5-7 days from today
- ✅ **propertyId** = -1
- ✅ **Status** = "Active"
- ✅ **Unique booking IDs**: `4100000000000001` - `4100000000000005`

## How to Run

### Quick Method (MongoDB Compass):
1. Open MongoDB Compass
2. Connect to your `ListingDB` database
3. Open the **Mongosh** tab at the bottom
4. Copy-paste the entire script from `test-bookings-today.js`
5. Press **Enter**

### Alternative (MongoDB Shell):
```bash
mongosh ListingDB test-bookings-today.js
```

## Expected Results

After running the script:
1. ✅ **5 bookings** inserted successfully
2. ✅ All bookings have **today's date**
3. ✅ "Bookings Today" KPI should show **5**
4. ✅ Console shows sample booking details

## Verification

After running, verify in MongoDB Compass:

```javascript
// Count today's bookings
db.bookings.countDocuments({
    CreatedAt: { 
        $gte: new Date(new Date().setHours(0,0,0,0)),
        $lt: new Date(new Date().setHours(23,59,59,999))
    }
})
// Should return: 5

// View the test bookings
db.bookings.find({
    _id: { 
        $gte: NumberLong("4100000000000001"), 
        $lte: NumberLong("4100000000000005") 
    }
}).pretty()
```

## Test in Dashboard

1. Open your Angular application
2. Navigate to the dashboard
3. Look for the **"Bookings Today"** KPI card
4. It should now show: **5** (instead of 0)
5. May show a trend indicator compared to yesterday

## Backend API Endpoint

The KPI widget calls:
```
GET /api/v1/dashboard/kpi/bookings-today?userId={userId}
```

Backend logic (from `DashboardKpiQueryHandlers.cs`):
```csharp
// Count bookings created today
var bookingsToday = await collection.CountDocumentsAsync(
    Builders<BsonDocument>.Filter.And(
        Builders<BsonDocument>.Filter.Gte("CreatedAt", today),
        Builders<BsonDocument>.Filter.Lt("CreatedAt", tomorrow)
    ),
    cancellationToken: cancellationToken
);
```

## Important Notes

### Why Two Date Fields?
- **bookingDate**: Your application's booking date field
- **CreatedAt**: Backend KPI handler uses this for "created today" queries
- The script sets **both** to ensure compatibility

### Data Structure Differences
Your MongoDB schema uses:
- `bookingDate` - Application level
- `checkInDate` - Guest arrival date
- `checkOutDate` - Guest departure date

Backend KPI handler expects:
- `CreatedAt` - For "Bookings Today" calculation
- This script bridges the gap by adding both fields

## Cleanup

To remove the test bookings after testing:

```javascript
// Remove all 5 test bookings
db.bookings.deleteMany({
    _id: { 
        $gte: NumberLong("4100000000000001"), 
        $lte: NumberLong("4100000000000005") 
    }
})

// Or remove by special request marker
db.bookings.deleteMany({
    specialRequests: 'Testing KPI widget'
})
```

## Troubleshooting

### KPI Still Shows 0
1. **Check browser network tab**:
   - Look for `/api/v1/dashboard/kpi/bookings-today` call
   - Check the response value

2. **Verify data in MongoDB**:
   ```javascript
   db.bookings.find({ CreatedAt: { $exists: true } }).count()
   ```

3. **Check backend logs** for any errors

4. **Verify time zones**:
   - Script uses `new Date()` (local time)
   - Backend uses `DateTime.UtcNow` (UTC)
   - May need to adjust if you're in a different timezone

### Fix for Timezone Issues
If you're not in UTC timezone, modify the script:

```javascript
// Use UTC for today
const now = new Date();
const today = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate()));
```

## Next Steps

After confirming this works:

1. ✅ **Update seed-bookings-data.js** to include both `bookingDate` and `CreatedAt`
2. ✅ **Create similar test scripts** for:
   - Occupancy Rate (needs bookings with today between check-in/check-out)
   - Properties (needs active properties)
   - Rooms (needs properties with rooms)
3. ✅ **Add Calendar Widget backend** to query actual bookings
4. ✅ **Connect Chart Widget** to booking trends

## Related Files

- `seed-bookings-data.js` - Main booking seed data (Jan-Apr 2026)
- `BOOKING_SEED_DATA_README.md` - Documentation for seed data
- `WIDGET_DATA_SOURCE_ANALYSIS.md` - Widget data source analysis
- Backend: `classfiles/Application/Dashboard/Queries/DashboardKpiQueryHandlers.cs`
- Frontend: `app/src/app/dashboard/dashboard1/dashboard1.component.ts`

---

**Created**: Current session  
**Purpose**: Test "Bookings Today" KPI widget with live backend integration  
**Status**: Ready to run
