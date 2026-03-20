# Testing the Guest Details Feature - Quick Guide

## Prerequisites

### 1. Ensure Guests Collection Exists in MongoDB
```javascript
// Run this in MongoDB Compass or mongo shell
use PropertyMasterDB;

// Check if Guests collection exists
db.getCollectionNames();

// If Guests collection doesn't exist, create sample data
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
  },
  {
    guestId: "G003",
    firstName: "Maria",
    lastName: "Garcia",
    email: "maria.garcia@example.com",
    phoneNumber: "+34-91-123-4567",
    nationality: "Spain"
  }
]);
```

### 2. Ensure Bookings Have guestId Field
```javascript
// Check existing bookings
db.Bookings.find({}).limit(5);

// If bookings don't have guestId, add sample data
db.Bookings.updateMany(
  { roomNumber: "101" },
  { $set: { guestId: "G001" } }
);

db.Bookings.updateMany(
  { roomNumber: "102" },
  { $set: { guestId: "G002" } }
);

db.Bookings.updateMany(
  { roomNumber: "103" },
  { $set: { guestId: "G003" } }
);
```

## Step-by-Step Testing

### Test 1: Backend API Endpoint

**1. Start the backend server**
```bash
cd WebApi
dotnet run
```

**2. Test the endpoint with Postman or curl**
```bash
# Get bookings with guest details
curl "https://localhost:7001/api/v1/dashboard/bookings-with-guests?userId=1&propertyIds=1" -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected Response:**
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
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

**Look for debug output in console:**
```
🔧 ===== BOOKINGS WITH GUESTS QUERY DEBUG =====
🔧 Request - UserId: 1, PropertyIds: [1]
🔧 Date Range: 2024-03-01 to 2024-05-01
✅ AFTER AGGREGATION - Returned 5 booking documents
✅ Mapped 5 bookings with guest details
🔧 ===== END BOOKINGS WITH GUESTS QUERY DEBUG =====
```

### Test 2: MongoDB Aggregation Pipeline

**Run this in MongoDB Compass Aggregation Builder:**
```javascript
[
  // Stage 1: $match
  {
    $match: {
      $and: [
        {
          $or: [
            { checkInDate: { $gte: ISODate("2024-03-01"), $lte: ISODate("2024-03-31") } },
            { CheckInDate: { $gte: ISODate("2024-03-01"), $lte: ISODate("2024-03-31") } }
          ]
        },
        { propertyId: { $in: [1] } }
      ]
    }
  },
  
  // Stage 2: $lookup with Guests
  {
    $lookup: {
      from: "Guests",
      localField: "guestId",
      foreignField: "guestId",
      as: "guestInfo"
    }
  },
  
  // Stage 3: $lookup with Property
  {
    $lookup: {
      from: "Property",
      let: { propId: "$propertyId" },
      pipeline: [
        { $match: { $expr: { $eq: ["$_id", "$$propId"] } } }
      ],
      as: "propertyInfo"
    }
  },
  
  // Stage 4: $project
  {
    $project: {
      _id: 1,
      bookingId: { $ifNull: ["$bookingId", "$BookingId", ""] },
      roomNumber: { $ifNull: ["$roomNumber", "$RoomNumber", ""] },
      propertyId: { $ifNull: ["$propertyId", "$PropertyId", 0] },
      propertyName: {
        $ifNull: [
          { $arrayElemAt: ["$propertyInfo.propertyName", 0] },
          { $arrayElemAt: ["$propertyInfo.PropertyName", 0] },
          "Unknown Property"
        ]
      },
      checkInDate: { $ifNull: ["$checkInDate", "$CheckInDate", null] },
      checkOutDate: { $ifNull: ["$checkOutDate", "$CheckOutDate", null] },
      status: { $ifNull: ["$status", "$Status", "confirmed"] },
      guestId: { $ifNull: ["$guestId", "$GuestId", ""] },
      guestFirstName: {
        $ifNull: [
          { $arrayElemAt: ["$guestInfo.firstName", 0] },
          { $arrayElemAt: ["$guestInfo.FirstName", 0] },
          "Unavailable"
        ]
      },
      guestLastName: {
        $ifNull: [
          { $arrayElemAt: ["$guestInfo.lastName", 0] },
          { $arrayElemAt: ["$guestInfo.LastName", 0] },
          "Unavailable"
        ]
      },
      guestEmail: {
        $ifNull: [
          { $arrayElemAt: ["$guestInfo.email", 0] },
          { $arrayElemAt: ["$guestInfo.Email", 0] },
          "Unavailable"
        ]
      },
      guestPhoneNumber: {
        $ifNull: [
          { $arrayElemAt: ["$guestInfo.phoneNumber", 0] },
          { $arrayElemAt: ["$guestInfo.PhoneNumber", 0] },
          "Unavailable"
        ]
      },
      guestNationality: {
        $ifNull: [
          { $arrayElemAt: ["$guestInfo.nationality", 0] },
          { $arrayElemAt: ["$guestInfo.Nationality", 0] },
          "Unavailable"
        ]
      }
    }
  },
  
  // Stage 5: $sort
  {
    $sort: {
      checkInDate: 1,
      roomNumber: 1
    }
  }
]
```

**Expected Results:**
- Should return bookings with guest details populated
- If guest not found, fields should show "Unavailable"
- Property name should be populated from Property collection

### Test 3: Frontend UI

**1. Start the Angular application**
```bash
cd app
ng serve
```

**2. Navigate to Dashboard**
- Open browser: `http://localhost:4200`
- Login with credentials
- Navigate to Dashboard
- Click on "Room Planner" tab

**3. Test Booking Bar Click**
- You should see rooms displayed in Gantt chart
- Booking bars should appear for rooms with bookings
- Click on any booking bar

**4. Verify Dialog Content**

**Dialog Title:**
```
Grand Hotel - Room 101 - Mar 15, 2024
```

**Room Information Section:**
- Property: Grand Hotel (in blue accent color)
- Room Number: 101
- Room Name: Standard Suite
- Room Type: Standard
- Floor: 1
- Capacity: 2

**Booking Information Section:**
- Status: Confirmed (green chip)
- Booking ID: B001 (monospace font, gray background)
- Guest Name: John Doe
- Check-in Date: Mar 15, 2024
- Check-out Date: Mar 18, 2024

**Guest Information Section:**
- Section heading with person icon
- Guest ID: G001
- First Name: John
- Last Name: Doe
- Email: john.doe@example.com (word-wrapping enabled)
- Phone Number: +1-555-0101
- Nationality: USA

**5. Test Fallback Scenario**

Create a booking without guest data:
```javascript
db.Bookings.insertOne({
  bookingId: "B999",
  roomNumber: "101",
  propertyId: 1,
  checkInDate: new Date("2024-03-20"),
  checkOutDate: new Date("2024-03-22"),
  status: "confirmed",
  guestId: "INVALID_ID" // Guest doesn't exist
});
```

Click on this booking - should see:
- Guest Information section shows crossed-out person icon
- Message: "Guest details unavailable"
- Background: light gray with muted colors

### Test 4: Browser Console Logging

**Open Developer Tools (F12) and check Console tab:**

**Expected logs:**
```
🔍 Loading bookings with guest details...
✅ Bookings with guests loaded: 5 bookings
📊 Processing 5 bookings for planner...
📌 Booking B001: Room 101, Fri Mar 15 2024 - Mon Mar 18 2024
📌 Booking B002: Room 102, Sat Mar 16 2024 - Wed Mar 20 2024
✅ Created 3 room booking bars
Booking bar clicked: 101, {bookingId: "B001", guestName: "John Doe", ...}
```

## Common Issues and Solutions

### Issue 1: "Guest details unavailable" for all bookings

**Cause:** Guests collection doesn't exist or guestId mismatch

**Solution:**
```javascript
// Check if Guests collection exists
db.getCollectionNames();

// Check guestId in bookings
db.Bookings.find({}, { guestId: 1, roomNumber: 1 }).limit(5);

// Check guestId in Guests
db.Guests.find({}, { guestId: 1, firstName: 1 }).limit(5);

// Ensure guestId matches
db.Bookings.updateOne(
  { roomNumber: "101" },
  { $set: { guestId: "G001" } }
);
```

### Issue 2: Property name shows "Unknown Property"

**Cause:** Property collection lookup failed

**Solution:**
```javascript
// Check Property collection _id field
db.Property.find({ _id: 1 });

// If _id is ObjectId instead of int, update bookings
db.Bookings.find({ propertyId: 1 }).limit(1);

// Match should be: propertyId (number) = Property._id (number)
```

### Issue 3: Backend returns empty array

**Cause:** Date range filter too restrictive

**Solution:**
```javascript
// Check booking dates
db.Bookings.find(
  { propertyId: 1 },
  { checkInDate: 1, CheckInDate: 1, roomNumber: 1 }
).limit(5);

// Adjust date range in API call:
?startDate=2024-01-01&endDate=2024-12-31
```

### Issue 4: Dialog doesn't show guest fields

**Cause:** Frontend not receiving guestDetails object

**Solution:**
- Check browser console for API response
- Verify `data.booking.guestDetails` is populated
- Check `*ngIf="data.booking.guestDetails"` condition in template

## Verification Checklist

- [ ] Guests collection exists with sample data
- [ ] Bookings have valid guestId referencing Guests
- [ ] API endpoint returns 200 with guest details
- [ ] MongoDB aggregation returns expected results
- [ ] Room Planner displays booking bars
- [ ] Clicking booking bar opens dialog
- [ ] Dialog shows property name in title
- [ ] Dialog displays all 6 guest fields
- [ ] Booking ID displays in monospace font
- [ ] Check-in/check-out dates visible
- [ ] "Unavailable" message shows for missing guest data
- [ ] Browser console shows success logs
- [ ] Backend console shows debug output

## Performance Testing

**Test with Large Dataset:**
```javascript
// Create 100 guests
for (let i = 1; i <= 100; i++) {
  db.Guests.insertOne({
    guestId: `G${i.toString().padStart(3, '0')}`,
    firstName: `FirstName${i}`,
    lastName: `LastName${i}`,
    email: `guest${i}@example.com`,
    phoneNumber: `+1-555-${i.toString().padStart(4, '0')}`,
    nationality: ["USA", "UK", "Canada", "Australia"][i % 4]
  });
}

// Create 500 bookings
for (let i = 1; i <= 500; i++) {
  db.Bookings.insertOne({
    bookingId: `B${i.toString().padStart(4, '0')}`,
    roomNumber: `${100 + (i % 50)}`,
    propertyId: 1,
    checkInDate: new Date(2024, 2, 1 + (i % 30)),
    checkOutDate: new Date(2024, 2, 3 + (i % 30)),
    status: ["confirmed", "pending", "cancelled"][i % 3],
    guestId: `G${(i % 100 + 1).toString().padStart(3, '0')}`
  });
}
```

**Expected Results:**
- API response time: < 2 seconds
- Room Planner load time: < 3 seconds
- Dialog opens instantly
- No UI lag or freezing

## Cleanup After Testing

```javascript
// Remove test data (CAUTION: Only run in development!)
db.Guests.deleteMany({ guestId: /^G\d{3}$/ });
db.Bookings.deleteMany({ bookingId: /^B\d{4}$/ });
```

## Success Criteria

✅ Feature is working if:
1. API returns bookings with populated guest fields
2. Room Planner shows booking bars
3. Clicking bar opens dialog with all guest information
4. Missing guest data shows "Unavailable" gracefully
5. Property name appears in dialog title
6. Check-in/check-out dates are visible
7. No errors in browser or backend console

🎉 **Congratulations! The guest details feature is fully functional!**
