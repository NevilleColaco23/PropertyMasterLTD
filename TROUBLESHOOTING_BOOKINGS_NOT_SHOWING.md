# Troubleshooting: Bookings Not Showing in Room Planner Grid

## Problem
Room Planner grid displays rooms but booking bars are not visible.

## Diagnostic Steps

### Step 1: Check Browser Console

**Open Developer Tools (F12) → Console tab**

Look for these log messages:

**Expected Success Logs:**
```
🔍 Loading bookings with guest details...
✅ Bookings with guests loaded: X bookings
📊 ===== PROCESSING BOOKINGS FOR PLANNER =====
📊 Received X bookings from API
✅ Successfully processed: X bookings
✅ Created X room booking bars
```

**Problem Indicators:**
```
⚠️ No bookings received from API!
⚠️ Skipped (room not found): X bookings
❌ Error loading bookings with guests
```

### Step 2: Check Network Tab

**Open Developer Tools (F12) → Network tab**

1. Filter by "bookings-with-guests"
2. Click on the request
3. Check the response:

**Expected Response:**
```json
[
  {
    "id": "...",
    "bookingId": "B001",
    "roomNumber": "101",
    "propertyId": 1,
    "propertyName": "Grand Hotel",
    "checkInDate": "2024-03-15T00:00:00Z",
    "checkOutDate": "2024-03-18T00:00:00Z",
    "status": "confirmed",
    "guestId": "G001",
    "guestFirstName": "John",
    "guestLastName": "Doe",
    "guestEmail": "john.doe@example.com",
    "guestPhoneNumber": "+1-555-0101",
    "guestNationality": "USA"
  }
]
```

**Problem Response:**
```json
[]  // Empty array - no bookings found
```

### Step 3: Check Backend Console

**Look for debug output in .NET console:**

**Expected Success:**
```
🔧 ===== BOOKINGS WITH GUESTS QUERY DEBUG =====
🔧 Request - UserId: 1, PropertyIds: [1]
🔧 Date Range: 2024-03-01 to 2024-03-31
✅ AFTER AGGREGATION - Returned 5 booking documents
✅ Mapped 5 bookings with guest details
```

**Problem Indicators:**
```
✅ AFTER AGGREGATION - Returned 0 booking documents
⚠️ No bookings exist in the date range
❌ Error fetching bookings: ...
```

## Common Issues and Solutions

### Issue 1: No Bookings in Current Month

**Symptom:** API returns empty array `[]`

**Cause:** Bookings collection has no data for the displayed month

**Solution:** Run `Debug_BookingsNotShowing.js` in MongoDB Compass:

```bash
# Open MongoDB Compass
# Connect to database
# Click "Aggregations" tab
# Open the script file: Debug_BookingsNotShowing.js
# Click "Run"
```

This script will:
- Check if bookings exist
- Show date ranges
- Generate sample bookings for current month

**Or manually create bookings:**

```javascript
use PropertyMasterDB;

const now = new Date();
const currentMonth = now.getMonth();
const currentYear = now.getFullYear();

db.Bookings.insertMany([
    {
        bookingId: "CURRENT_001",
        roomNumber: "101",
        propertyId: 1,
        checkInDate: new Date(currentYear, currentMonth, 5),
        checkOutDate: new Date(currentYear, currentMonth, 8),
        status: "confirmed",
        guestId: "G001"
    },
    {
        bookingId: "CURRENT_002",
        roomNumber: "102",
        propertyId: 1,
        checkInDate: new Date(currentYear, currentMonth, 10),
        checkOutDate: new Date(currentYear, currentMonth, 15),
        status: "confirmed",
        guestId: "G002"
    },
    {
        bookingId: "CURRENT_003",
        roomNumber: "103",
        propertyId: 1,
        checkInDate: new Date(currentYear, currentMonth, 12),
        checkOutDate: new Date(currentYear, currentMonth, 18),
        status: "confirmed",
        guestId: "G003"
    }
]);
```

### Issue 2: Room Number Mismatch

**Symptom:** Browser console shows:
```
⚠️ Room NOT FOUND for booking: roomNumber
⚠️ Skipped (room not found): X bookings
```

**Cause:** Room numbers in Bookings don't match Room numbers in Room collection

**Solution:** Check room numbers in both collections:

```javascript
// Check rooms
db.Room.find({ PropertyId: 1 }, { RoomNumber: 1, roomNumber: 1 }).limit(10);

// Check bookings
db.Bookings.find({}, { roomNumber: 1, RoomNumber: 1 }).limit(10);

// Update bookings to match room numbers
db.Bookings.updateMany(
    { roomNumber: "Room101" },  // Wrong format
    { $set: { roomNumber: "101" } }  // Correct format
);
```

### Issue 3: Property ID Filter Mismatch

**Symptom:** Backend returns 0 results, but bookings exist

**Cause:** Selected property ID doesn't match booking propertyId

**Solution:**

```javascript
// Check which property IDs have bookings
db.Bookings.distinct("propertyId");
db.Bookings.distinct("PropertyId");

// Check selected property in frontend
// Open browser console and type:
localStorage.getItem('selectedPropertyIds');

// Ensure they match!

// If needed, update bookings:
db.Bookings.updateMany(
    { propertyId: { $exists: false } },
    { $set: { propertyId: 1 } }
);
```

### Issue 4: Date Field Casing Mismatch

**Symptom:** Some bookings show, others don't

**Cause:** Mixed camelCase/PascalCase field names

**Solution:** Standardize field names:

```javascript
// Check field name casing
db.Bookings.findOne({}, { checkInDate: 1, CheckInDate: 1, checkOutDate: 1, CheckOutDate: 1 });

// Option 1: Convert all to camelCase
db.Bookings.find({ CheckInDate: { $exists: true } }).forEach(doc => {
    db.Bookings.updateOne(
        { _id: doc._id },
        {
            $set: {
                checkInDate: doc.CheckInDate,
                checkOutDate: doc.CheckOutDate
            },
            $unset: { CheckInDate: "", CheckOutDate: "" }
        }
    );
});

// Option 2: Convert all to PascalCase
db.Bookings.find({ checkInDate: { $exists: true } }).forEach(doc => {
    db.Bookings.updateOne(
        { _id: doc._id },
        {
            $set: {
                CheckInDate: doc.checkInDate,
                CheckOutDate: doc.checkOutDate
            },
            $unset: { checkInDate: "", checkOutDate: "" }
        }
    );
});
```

### Issue 5: Dates Outside Visible Range

**Symptom:** API returns bookings but grid is empty

**Cause:** Booking dates don't overlap with displayed month

**Solution:**

```javascript
// Check booking dates vs displayed month
const displayedMonth = new Date(2024, 2, 1); // March 2024
const monthStart = new Date(displayedMonth.getFullYear(), displayedMonth.getMonth(), 1);
const monthEnd = new Date(displayedMonth.getFullYear(), displayedMonth.getMonth() + 1, 0);

print(`Displayed month: ${monthStart.toISOString()} to ${monthEnd.toISOString()}`);

// Find bookings that overlap with this month
db.Bookings.find({
    $or: [
        {
            checkInDate: { $lte: monthEnd },
            checkOutDate: { $gte: monthStart }
        },
        {
            CheckInDate: { $lte: monthEnd },
            CheckOutDate: { $gte: monthStart }
        }
    ]
});
```

**Frontend Fix:** Navigate to correct month using prev/next buttons

### Issue 6: Guests Collection Missing

**Symptom:** Backend logs show "Guest details unavailable"

**Cause:** Guests collection doesn't exist or guestId mismatch

**Solution:**

```javascript
// Check if Guests collection exists
db.getCollectionNames();

// If missing, create it:
db.Guests.insertMany([
    {
        guestId: "G001",
        firstName: "John",
        lastName: "Doe",
        email: "john.doe@example.com",
        phoneNumber: "+1-555-0101",
        nationality: "USA"
    },
    {
        guestId: "G002",
        firstName: "Jane",
        lastName: "Smith",
        email: "jane.smith@example.com",
        phoneNumber: "+44-20-7123-4567",
        nationality: "UK"
    }
]);

// Check guestId matches in bookings
db.Bookings.find({}, { guestId: 1, bookingId: 1 }).limit(5);

// Update bookings with missing guestId
db.Bookings.updateMany(
    { guestId: { $exists: false } },
    { $set: { guestId: "G001" } }
);
```

### Issue 7: Backend Query Not Executing

**Symptom:** No backend console logs

**Cause:** Endpoint not being called or MediatR not configured

**Solution:**

1. **Check if API endpoint exists:**
```bash
# Test with curl or Postman
curl "https://localhost:7001/api/v1/dashboard/bookings-with-guests?userId=1&propertyIds=1"
```

2. **Check MediatR registration in Startup.cs/Program.cs:**
```csharp
// Should have:
builder.Services.AddMediatR(typeof(GetBookingsWithGuestsQueryHandler).Assembly);
```

3. **Restart backend server:**
```bash
cd WebApi
dotnet build
dotnet run
```

## Quick Test Script

Run this in browser console after opening Room Planner:

```javascript
// Check if bookings are loaded
console.log('Rooms loaded:', document.querySelectorAll('.room-row').length);
console.log('Booking bars:', document.querySelectorAll('.booking-bar').length);

// Force reload
location.reload();
```

## Verification Checklist

- [ ] Bookings collection has data
- [ ] Bookings exist for current displayed month
- [ ] Room numbers in Bookings match Room collection
- [ ] Property IDs match between bookings and selected property
- [ ] Date fields use consistent casing (all camelCase OR all PascalCase)
- [ ] Guests collection exists with matching guestIds
- [ ] Backend API endpoint returns data (check Network tab)
- [ ] Frontend processes bookings (check Console tab)
- [ ] Backend console shows successful query execution

## Still Not Working?

### Full Debug Sequence

1. **MongoDB:**
```bash
# Run in MongoDB Compass
# File: Debug_BookingsNotShowing.js
```

2. **Backend:**
```bash
# Check console output
# Should see: "BOOKINGS WITH GUESTS QUERY DEBUG"
```

3. **Frontend:**
```bash
# Open browser console (F12)
# Look for: "Loading bookings with guest details"
# Check Network tab for API response
```

4. **Data Verification:**
```javascript
// Run in MongoDB
use PropertyMasterDB;

// 1. Count bookings
db.Bookings.countDocuments({});

// 2. Check current month
const now = new Date();
const start = new Date(now.getFullYear(), now.getMonth(), 1);
const end = new Date(now.getFullYear(), now.getMonth() + 1, 0);

db.Bookings.find({
    checkInDate: { $lte: end },
    checkOutDate: { $gte: start },
    propertyId: 1
});

// 3. Test aggregation
db.Bookings.aggregate([
    {
        $match: {
            propertyId: 1,
            checkInDate: { $lte: end },
            checkOutDate: { $gte: start }
        }
    },
    { $limit: 5 }
]);
```

## Contact Points

If issue persists:
1. Export booking sample: `db.Bookings.find().limit(5)`
2. Export room sample: `db.Room.find().limit(5)`  
3. Screenshot browser console logs
4. Screenshot backend console logs
5. Screenshot Network tab response

## Most Likely Cause

**90% of the time it's:**
- No bookings for the current displayed month
- Room number format mismatch ("Room101" vs "101")
- Wrong property ID filter

**Quick Fix:**
Run the sample data insert script from `Debug_BookingsNotShowing.js` to create bookings for the current month.
