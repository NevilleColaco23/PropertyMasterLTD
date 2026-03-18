# MongoDB Query Debug Extensions - Usage Guide

## Overview
The `MongoQueryDebugExtensions` provides extension methods that work with **ANY** `INamedQuery` implementation, allowing you to easily debug MongoDB queries in Compass.

## Setup

### 1. Add the using statement to your handler:
```csharp
using MyWarehouse.Application.Common.Dependencies.DataAccess.Extensions;
```

## Usage Examples

### Example 1: GetBookingsListQueryHandler (Already Implemented ✅)

```csharp
using MyWarehouse.Application.Common.Dependencies.DataAccess.Extensions;

public class GetBookingsListQueryHandler : IRequestHandler<GetBookingsListQuery, IListResponseModel<GetBookingsListDTO>>
{
    public async Task<IListResponseModel<GetBookingsListDTO>> Handle(GetBookingsListQuery request, CancellationToken cancellationToken)
    {
        // Create your query
        var mongoQuery = new GetBookingsMongoQuery(
            request.BookingId, 
            request.SearchItem, 
            request.PageIndex, 
            request.PageSize, 
            request.OrderBy, 
            request.ActiveSortDirection, 
            request.PropertyIds);

        // 🔍 DEBUG: Set breakpoint here and inspect these variables
        var debugInfo = mongoQuery.GetDebugInfo("Bookings");           // Full debug info
        var debugPipeline = mongoQuery.GetPipelineAsJsonString();     // Just the pipeline JSON
        var debugSummary = mongoQuery.GetDebugSummary();              // Quick summary

        var menuList = _unitOfWork.Bookings?.GetPagedListBy<GetBookingsListDTO>(
            MongoCollections.BookingsCollection, mongoQuery);

        // 🔍 DEBUG: Check results
        var resultCount = menuList.results.Count;

        // ... rest of handler
    }
}
```

### Example 2: GetRecentBookingsMongoQuery (Dashboard)

```csharp
using MyWarehouse.Application.Common.Dependencies.DataAccess.Extensions;

public class GetRecentBookingsQueryHandler : IRequestHandler<GetRecentBookingsQuery, List<RecentBookingDTO>>
{
    public async Task<List<RecentBookingDTO>> Handle(GetRecentBookingsQuery request, CancellationToken cancellationToken)
    {
        // Create query
        var mongoQuery = new GetRecentBookingsMongoQuery(request.UserId, request.Limit);

        // 🔍 DEBUG: Set breakpoint here
        var debugInfo = mongoQuery.GetDebugInfo("Bookings");
        var debugPipeline = mongoQuery.GetPipelineAsJsonString();

        // Execute query
        var results = await _unitOfWork.Bookings?.GetListBy<RecentBookingDTO>(
            MongoCollections.BookingsCollection, mongoQuery);

        return results;
    }
}
```

### Example 3: GetMenuListQueryByUserId (Menu Queries)

```csharp
using MyWarehouse.Application.Common.Dependencies.DataAccess.Extensions;

public class GetMenuListQueryHandler : IRequestHandler<GetMenuListQuery, IListResponseModel<GetMenuDTO>>
{
    public async Task<IListResponseModel<GetMenuDTO>> Handle(GetMenuListQuery request, CancellationToken cancellationToken)
    {
        // Create query
        var mongoQuery = new GetMenuListQueryByUserId(request.UserId);

        // 🔍 DEBUG: Works with ANY INamedQuery!
        var debugInfo = mongoQuery.GetDebugInfo("MenuPermissions");
        var debugPipeline = mongoQuery.GetPipelineAsJsonString();

        var results = _unitOfWork.MenuPermissions?.GetListBy<GetMenuDTO>(
            MongoCollections.MenuPermissionsCollection, mongoQuery);

        return results;
    }
}
```

## Extension Methods Available

### 1. `GetPipelineAsJsonString()`
Returns the MongoDB aggregation pipeline as formatted JSON for MongoDB Compass.

**Usage:**
```csharp
var pipeline = mongoQuery.GetPipelineAsJsonString();
// Copy this to MongoDB Compass Aggregation tab
```

**Output Example:**
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

### 2. `GetDebugInfo(string collectionName)`
Returns full debug information including query type, collection name, and formatted pipeline.

**Usage:**
```csharp
var debugInfo = mongoQuery.GetDebugInfo("YourCollectionName");
// Inspect in debugger or log if needed
```

**Output Example:**
```
=== MongoDB Query Debug Info ===
Query Type: GetBookingsMongoQuery
Collection: Bookings
Has Pipeline: True
Pipeline Stages: 2
Query String: N/A

=== MongoDB Compass Pipeline ===
[
  {
    "$match": {
      "propertyId": {
        "$in": [1]
      }
    }
  },
  ...
]
==========================================
```

### 3. `GetDebugSummary()`
Returns a quick one-line summary of the query.

**Usage:**
```csharp
var summary = mongoQuery.GetDebugSummary();
// Example output: "[GetBookingsMongoQuery] Pipeline stages: 2"
```

## Debugging Workflow

### Step 1: Add debug variables in your handler
```csharp
var mongoQuery = new YourMongoQuery(...);

// Add these lines
var debugInfo = mongoQuery.GetDebugInfo("YourCollection");
var debugPipeline = mongoQuery.GetPipelineAsJsonString();
```

### Step 2: Set a breakpoint
Set a breakpoint on the line after `debugInfo` is assigned.

### Step 3: Run in debug mode
When the breakpoint hits, hover over the variables or use the Immediate Window:
```csharp
// In Immediate Window:
debugInfo
debugPipeline
```

### Step 4: Copy to MongoDB Compass
1. Copy the value of `debugPipeline`
2. Open MongoDB Compass
3. Navigate to your collection
4. Click "Aggregations" tab
5. Paste the pipeline
6. Click "Run"

### Step 5: Verify results
Compare the results in Compass with what your API returns.

## Common Use Cases

### Case 1: No results returned
```csharp
var mongoQuery = new GetBookingsMongoQuery(...);
var debugInfo = mongoQuery.GetDebugInfo("Bookings");  // ← Set breakpoint
var results = _repository.GetListBy(..., mongoQuery);
var count = results.Count;  // ← Count is 0?

// Inspect debugInfo to see what filters are applied
// Copy debugPipeline and test in Compass
```

### Case 2: Wrong results returned
```csharp
var mongoQuery = new GetMenuListQueryByUserId(userId);
var debugPipeline = mongoQuery.GetPipelineAsJsonString();  // ← Set breakpoint

// Copy pipeline to Compass and verify:
// - Are the match conditions correct?
// - Is the sort order right?
// - Are lookups joining the correct collections?
```

### Case 3: Performance issues
```csharp
var mongoQuery = new GetRecentBookingsMongoQuery(userId, limit);
var debugSummary = mongoQuery.GetDebugSummary();  // Quick check
var debugInfo = mongoQuery.GetDebugInfo("Bookings");  // Full details

// Check in Compass:
// - How many documents are being scanned?
// - Are indexes being used?
// - Can stages be reordered for better performance?
```

## Benefits

✅ **Universal**: Works with ANY `INamedQuery` implementation
✅ **Non-invasive**: No console output, only local variables
✅ **Easy debugging**: Set breakpoint and inspect
✅ **MongoDB Compass ready**: Direct copy-paste to Compass
✅ **Minimal code**: Just 2 lines to add debugging capability

## Notes

- The extension methods only work when the query has a `BsonPipeline` property
- If `BsonPipeline` is null, methods return safe default values
- Collection name in `GetDebugInfo()` is for documentation purposes only
- You can use these extensions in any query handler across your application

## Supported Query Classes

These extension methods work with all these query classes (and more!):

- ✅ `GetBookingsMongoQuery`
- ✅ `GetRecentBookingsMongoQuery`
- ✅ `GetMenuListQueryByUserId`
- ✅ Any custom query implementing `INamedQuery`

## Quick Reference Card

```csharp
// Basic pattern (copy-paste this):

using MyWarehouse.Application.Common.Dependencies.DataAccess.Extensions;

public async Task<TResponse> Handle(TRequest request, CancellationToken ct)
{
    var mongoQuery = new YourMongoQuery(...);
    
    // 🔍 Add these 2 lines for debugging:
    var debugInfo = mongoQuery.GetDebugInfo("CollectionName");
    var debugPipeline = mongoQuery.GetPipelineAsJsonString();
    
    // Execute query
    var results = await _repository.GetListBy(..., mongoQuery);
    
    // 🔍 Optional: Check result count
    var resultCount = results.Count;
    
    return results;
}
```

**Set breakpoint on the `debugInfo` line and inspect!** 🎯
