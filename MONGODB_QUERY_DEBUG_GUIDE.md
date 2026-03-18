# MongoDB Bookings Query Debugging Guide

## Overview
This guide helps you debug the `GetBookingsMongoQuery` by generating MongoDB Compass-compatible query strings.

## Method 1: Run the API and Check Console Output

### Steps:
1. **Run your WebAPI project**
2. **Make a request to the bookings endpoint**:
   ```
   GET /api/v1/Bookings/GetBookings?pageIndex=1&pageSize=10&propertyIds=1
   ```
3. **Check the console output** - you'll see:
   ```
   =================================================
   BOOKINGS QUERY DEBUG
   =================================================
   === MongoDB Bookings Query Debug Info ===
   BookingId: null
   FilterString: null
   PageIndex: 1
   PageSize: 10
   OrderBy: bookingDate
   SortDirection: -1
   GetAllProperties: False
   PropertyIds: 1
   PropertyIds Count: 1

   === MongoDB Compass Query ===
   Collection: Bookings (or your collection name)
   Pipeline:
   [
     {
       "$match": {
         "propertyId": {
           "$in": [1]
         }
       }
     },
     {
       "$facet": {
         "results": [
           { "$sort": { "bookingDate": -1 } },
           { "$skip": 0 },
           { "$limit": 10 }
         ],
         "totalCount": [
           { "$count": "count" }
         ]
       }
     }
   ]
   ```

4. **Copy the Pipeline JSON** and paste it into MongoDB Compass

## Method 2: Use the Debug Helper Class

### In your code (or in a test):
```csharp
using MyWarehouse.Application.Common.Bookings.BookingsMongoQuery;

// Generate a specific query
QueryDebugHelper.GenerateBookingsQuery();

// Or generate multiple test scenarios
QueryDebugHelper.GenerateTestScenarios();
```

## Method 3: Use Debugger Breakpoint

1. Set a breakpoint in `GetBookingsListQueryHandler` at line ~35 (after creating mongoQuery)
2. Run in debug mode
3. In the **Immediate Window**, type:
   ```csharp
   mongoQuery.GetDebugInfo()
   ```
4. Copy the output

## Using the Query in MongoDB Compass

### Steps:
1. **Open MongoDB Compass**
2. **Connect to your database**
3. **Select the `Bookings` collection** (or whatever your collection is named)
4. **Click the "Aggregations" tab**
5. **Paste the pipeline JSON** (the part inside the square brackets `[...]`)
6. **Click "Run"** to execute

### Example Query to Paste in Compass:
```json
[
  {
    "$match": {
      "propertyId": {
        "$in": [1]
      }
    }
  },
  {
    "$facet": {
      "results": [
        { "$sort": { "bookingDate": -1 } },
        { "$skip": 0 },
        { "$limit": 10 }
      ],
      "totalCount": [
        { "$count": "count" }
      ]
    }
  }
]
```

## Common Issues & Debugging

### Issue 1: No Results Returned
**Check:**
- ✅ Do documents exist in the `Bookings` collection?
  ```javascript
  db.Bookings.countDocuments()
  ```
- ✅ Do documents have the `propertyId` field?
  ```javascript
  db.Bookings.findOne()
  ```
- ✅ Is the propertyId value correct? (Check if it's stored as int or string)
  ```javascript
  db.Bookings.find({ propertyId: 1 }).limit(1)
  db.Bookings.find({ propertyId: "1" }).limit(1)
  ```

### Issue 2: PropertyId Field Name Mismatch
**Check:**
- Your MongoDB documents might use a different field name
- Try removing the `$match` stage temporarily to see all documents:
  ```json
  [
    {
      "$facet": {
        "results": [
          { "$limit": 10 }
        ],
        "totalCount": [
          { "$count": "count" }
        ]
      }
    }
  ]
  ```

### Issue 3: Case Sensitivity
- Field names in MongoDB are case-sensitive
- Your documents might have `PropertyId` instead of `propertyId`
- Check actual field names:
  ```javascript
  db.Bookings.findOne()
  ```

## Quick Test Queries for Compass

### Get ALL bookings (no filter):
```json
[
  {
    "$facet": {
      "results": [
        { "$sort": { "bookingDate": -1 } },
        { "$limit": 10 }
      ],
      "totalCount": [
        { "$count": "count" }
      ]
    }
  }
]
```

### Get bookings by propertyId:
```json
[
  {
    "$match": {
      "propertyId": 1
    }
  },
  {
    "$facet": {
      "results": [
        { "$sort": { "bookingDate": -1 } },
        { "$limit": 10 }
      ],
      "totalCount": [
        { "$count": "count" }
      ]
    }
  }
]
```

### Search by room number:
```json
[
  {
    "$match": {
      "$or": [
        { "bookingId": { "$regex": "101", "$options": "i" } },
        { "roomNumber": { "$regex": "101", "$options": "i" } }
      ]
    }
  },
  {
    "$facet": {
      "results": [
        { "$sort": { "bookingDate": -1 } },
        { "$limit": 10 }
      ],
      "totalCount": [
        { "$count": "count" }
      ]
    }
  }
]
```

## Expected Output Format

The `$facet` stage returns results in this format:
```json
[
  {
    "results": [
      { /* booking document 1 */ },
      { /* booking document 2 */ },
      // ... up to pageSize documents
    ],
    "totalCount": [
      { "count": 150 }
    ]
  }
]
```

## Next Steps

1. ✅ Run the API and check console output
2. ✅ Copy the generated query
3. ✅ Test in MongoDB Compass
4. ✅ Verify documents exist and field names match
5. ✅ Adjust the query based on your actual data structure

## Need More Help?

Check these key files:
- Query generation: `GetBookingsMongoQuery.cs`
- Query execution: `GetBookingsListQuery.cs`
- API endpoint: `BookingsController.cs`
- Repository: `RepositoryBaseMongo.cs`
