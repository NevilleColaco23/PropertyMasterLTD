# Booking Data Seed Script

## Overview
This script generates **5000 test bookings** for the Property Master system:
- **2500 bookings** for `propertyId = -1`
- **2500 bookings** for `propertyId = 1`

## Features
- Realistic date ranges (past 3 months to future 6 months)
- Varied stay durations (1-14 days)
- Random room assignments (Floors 1-10, Rooms 1-50)
- Multiple payment statuses (Pending, Paid, Partially Paid, Refunded)
- Diverse booking sources (Website, Phone, Walk-in, Travel Agent, OTA)
- Special requests and guest counts (1-4 guests)
- Auto-generated unique booking IDs
- 90% confirmed bookings

## How to Run

### Method 1: MongoDB Shell
```bash
# Navigate to the script directory
cd Database/Scripts

# Run the script in MongoDB shell
mongosh your_database_name seed-bookings-data.js
```

### Method 2: MongoDB Compass
1. Open MongoDB Compass
2. Connect to your database
3. Open the "Mongosh" tab at the bottom
4. Copy and paste the entire script
5. Press Enter to execute

### Method 3: Node.js with MongoDB Driver
```javascript
const { MongoClient } = require('mongodb');
const fs = require('fs');

async function runScript() {
    const client = new MongoClient('your_connection_string');
    await client.connect();
    const db = client.db('your_database_name');
    
    // Read and execute the script
    const script = fs.readFileSync('./seed-bookings-data.js', 'utf8');
    await db.eval(script);
    
    await client.close();
}

runScript();
```

## Data Structure

Each booking contains:
```json
{
  "_id": NumberLong("4000000000000001"),
  "bookingId": "WEG851B3IY",           // Unique 10-char ID
  "guestId": NumberLong("..."),         // Guest reference
  "staffId": NumberLong("..."),         // Staff reference
  "roomNumber": "838",                  // Room number (e.g., Floor 8, Room 38)
  "bookingDate": ISODate("..."),        // When booking was made
  "checkInDate": ISODate("..."),        // Check-in date
  "checkOutDate": ISODate("..."),       // Check-out date
  "numberOfGuests": 1,                  // 1-4 guests
  "totalPrice": 1558.21,                // Total booking price
  "paymentStatusId": NumberLong("..."), // Payment status reference
  "bookingSourceId": NumberLong("..."), // Booking source reference
  "specialRequests": [],                // Array of special requests
  "isConfirmed": true,                  // Confirmation status
  "lastModified": ISODate("..."),       // Last modification date
  "propertyId": -1                      // Property reference
}
```

## Date Ranges

The script generates bookings for a specific 4-month period in 2026:

- **Check-in Dates**: January 1, 2026 to April 30, 2026
- **Booking Dates**: December 1, 2025 to April 30, 2026 (bookings made before check-in)
- **Check-out Dates**: Check-in date + 1 to 14 days (may extend into May 2026)
- **Last Modified**: Booking date to check-in date

This creates a realistic distribution:
- ✅ All bookings for Q1 2026 (Jan-Apr)
- ✅ Bookings made in advance (Dec 2025 onwards)
- ✅ Mix of short stays (1-3 days) and longer stays (up to 14 days)
- ✅ Even distribution across all 4 months

## Reference IDs

### Payment Status IDs
- `5000000000000010` - Pending
- `5000000000000011` - Paid
- `5000000000000012` - Partially Paid
- `5000000000000013` - Refunded

### Booking Source IDs
- `5000000000000001` - Website
- `5000000000000002` - Phone
- `5000000000000003` - Walk-in
- `5000000000000004` - Travel Agent
- `5000000000000005` - Online Travel Agency (OTA)

### ID Ranges
- **Booking IDs**: `4000000000000001` to `4000000000005000`
- **Guest IDs**: `1000000000003000` to `1000000000003499` (500 unique guests)
- **Staff IDs**: `2000000000000040` to `2000000000000059` (20 staff members)

## Verification Queries

After running the script, verify the data:

```javascript
// Count total bookings
db.bookings.countDocuments()
// Expected: 5000

// Count by property
db.bookings.countDocuments({ propertyId: -1 })  // Expected: 2500
db.bookings.countDocuments({ propertyId: 1 })   // Expected: 2500

// Upcoming bookings (next 30 days)
const thirtyDaysFromNow = new Date();
thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);
db.bookings.countDocuments({ 
    checkInDate: { $gte: new Date(), $lte: thirtyDaysFromNow } 
})

// Current in-house guests
db.bookings.countDocuments({ 
    checkInDate: { $lte: new Date() },
    checkOutDate: { $gte: new Date() }
})

// Confirmed bookings
db.bookings.countDocuments({ isConfirmed: true })
// Expected: ~4500 (90% confirmation rate)

// Sample booking
db.bookings.findOne({ propertyId: 1 })
```

## Sample Special Requests

The script includes varied special requests:
- Early check-in
- Late check-out
- Extra pillows / towels
- Non-smoking room
- High floor / Ground floor
- Away from elevator
- Quiet room
- King bed / Twin beds
- City view / Ocean view / Pool access
- Disability access
- Balcony

## Performance Notes

- Script execution time: ~2-5 seconds (depending on system)
- Memory usage: ~50-100 MB during generation
- Insert operation: Batch insert for optimal performance
- Index recommendations:
  ```javascript
  db.bookings.createIndex({ propertyId: 1 })
  db.bookings.createIndex({ checkInDate: 1 })
  db.bookings.createIndex({ checkOutDate: 1 })
  db.bookings.createIndex({ bookingDate: 1 })
  db.bookings.createIndex({ bookingId: 1 }, { unique: true })
  db.bookings.createIndex({ guestId: 1 })
  db.bookings.createIndex({ roomNumber: 1 })
  ```

## Cleaning Up

To remove the test data:

```javascript
// Remove all generated bookings
db.bookings.deleteMany({
    _id: { 
        $gte: NumberLong("4000000000000001"), 
        $lte: NumberLong("4000000000005000") 
    }
})

// Or remove by property
db.bookings.deleteMany({ propertyId: -1 })
db.bookings.deleteMany({ propertyId: 1 })
```

## Next Steps

After populating the data:
1. ✅ Verify data in MongoDB Compass
2. Create backend API endpoints for calendar widget
3. Implement booking queries:
   - Get bookings by property and date range
   - Get check-ins for a specific date
   - Get check-outs for a specific date
   - Get current in-house guests
4. Connect calendar widget to real data
5. Test calendar filtering and display

## Troubleshooting

### Error: "NumberLong is not defined"
- **Solution**: You're running in Node.js. Use `Long.fromString()` from `mongodb` package or run in MongoDB shell.

### Error: "db is not defined"
- **Solution**: Make sure you're connected to a database in MongoDB shell: `use your_database_name`

### Performance Issues
- **Solution**: Create indexes on frequently queried fields (see Index recommendations above)

### Duplicate Key Error
- **Solution**: The collection might already have bookings with these IDs. Change `bookingIdCounter` start value or clean existing data first.

---

**Created**: Current session  
**Purpose**: Test data for Calendar Widget implementation  
**Related Files**: 
- `WIDGET_DATA_SOURCE_ANALYSIS.md` - Widget analysis
- Calendar Widget frontend: `app/src/app/widgets/calendar-widget/`
- Dashboard component: `app/src/app/dashboard/dashboard1/`
