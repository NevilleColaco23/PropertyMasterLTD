# Multiple Property Selection Implementation

## Overview
Updated the booking system to support filtering by **multiple selected properties** instead of a single property.

## Changes Made

### 1. Backend - Query Model

**File:** `classfiles/Application/Common/Bookings/GetBookingsListQuery.cs`

Changed from single property to multiple properties:
```csharp
// Before
public int? PropertyId { get; init; }

// After
public List<int> PropertyIds { get; init; } = new List<int>();
```

### 2. Backend - MongoDB Query

**File:** `classfiles/Application/Common/Bookings/BookingsMongoQuery/GetBookingsMongoQuery.cs`

**Constructor updated:**
```csharp
// Before
public GetBookingsMongoQuery(..., int? propertyId = null, bool getAllProperties = false)

// After
public GetBookingsMongoQuery(..., List<int> propertyIds = null, bool getAllProperties = false)
```

**Field updated:**
```csharp
// Before
private readonly int? _propertyId;

// After
private readonly List<int> _propertyIds;
```

**Filter logic updated to use MongoDB `$in` operator:**
```csharp
// Before - Single property
if (!_getAllProperties && _propertyId.HasValue)
{
    matchDoc.Add("propertyId", _propertyId.Value);
}

// After - Multiple properties
if (!_getAllProperties && _propertyIds != null && _propertyIds.Any())
{
    var propertyIdsArray = new BsonArray(_propertyIds.Select(id => new BsonInt32(id)));
    matchDoc.Add("propertyId", new BsonDocument("$in", propertyIdsArray));
}
```

### 3. Frontend - Reports Component

**File:** `app/src/app/Menu/reports/reports.component.ts`

**Property changed:**
```typescript
// Before
selectedPropertyId: number | null = null;

// After
selectedPropertyIds: number[] = [];
```

**Load method updated:**
```typescript
// Before
loadSelectedProperty(): void {
  const propertyIds = PropertySelectionComponent.getSelectedPropertyIds();
  this.selectedPropertyId = propertyIds.length > 0 ? propertyIds[0] : null;
}

// After
loadSelectedProperty(): void {
  this.selectedPropertyIds = PropertySelectionComponent.getSelectedPropertyIds();
  console.log('Selected Property IDs:', this.selectedPropertyIds);
}
```

**API call updated:**
```typescript
// Before
if (this.selectedPropertyId != null) {
  params = params.set('PropertyId', this.selectedPropertyId.toString());
}

// After
if (this.selectedPropertyIds && this.selectedPropertyIds.length > 0) {
  this.selectedPropertyIds.forEach(id => {
    params = params.append('PropertyIds', id.toString());
  });
}
```

### 4. Frontend - Bookings Service

**File:** `app/src/app/bookings/services/bookings.service.ts`

**Method signature updated:**
```typescript
// Before
getBookings(
  propertyId?: number,
  ...
): Observable<BookingsListResponse>

// After
getBookings(
  propertyIds?: number[],
  ...
): Observable<BookingsListResponse>
```

**Parameter handling updated:**
```typescript
// Before
if (propertyId != null) {
  params = params.set('PropertyId', propertyId.toString());
}

// After
if (propertyIds && propertyIds.length > 0) {
  propertyIds.forEach(id => {
    params = params.append('PropertyIds', id.toString());
  });
}
```

## How It Works

### User Flow:
1. User selects **multiple properties** in property selection component
2. Property IDs are saved to localStorage as JSON array: `[1, 5, 7]`
3. Reports component loads and retrieves all selected property IDs
4. API request includes multiple PropertyIds parameters: `?PropertyIds=1&PropertyIds=5&PropertyIds=7`
5. Backend receives `List<int> PropertyIds` in query model
6. MongoDB query uses `$in` operator to match any of the property IDs
7. Results show bookings from all selected properties

### MongoDB Query Generated:
```javascript
db.bookings.aggregate([
  {
    $match: {
      propertyId: { $in: [1, 5, 7] }  // Matches bookings from any of these properties
    }
  },
  {
    $facet: {
      results: [
        { $sort: { checkInDate: -1 } },
        { $skip: 0 },
        { $limit: 10 }
      ],
      totalCount: [
        { $count: "count" }
      ]
    }
  }
])
```

### API Request Example:
```
GET /api/v1/Bookings/GetBookings?PropertyIds=1&PropertyIds=5&PropertyIds=7&PageIndex=1&PageSize=10&OrderBy=checkInDate&ActiveSortDirection=-1
```

### ASP.NET Core Model Binding:
ASP.NET Core automatically binds multiple query parameters with the same name to a `List<int>`:
```
?PropertyIds=1&PropertyIds=5&PropertyIds=7
```
Binds to:
```csharp
public List<int> PropertyIds { get; init; } = new List<int>();
```

## Benefits

✅ **Flexible Filtering** - Users can view bookings from one or multiple properties simultaneously  
✅ **Efficient Query** - MongoDB `$in` operator is indexed and performant  
✅ **Cross-Property Analysis** - Easy to compare bookings across properties  
✅ **User Experience** - No need to switch between properties to view bookings  

## Testing

1. Select multiple properties in property selection
2. Check localStorage: `localStorage.getItem('selectedPropertyIds')` should show: `[1,5,7]`
3. Navigate to reports/bookings page
4. Check console: Should log "Selected Property IDs: [1, 5, 7]"
5. Check network tab: API URL should contain `PropertyIds=1&PropertyIds=5&PropertyIds=7`
6. Verify results: Bookings from all selected properties should appear

## Notes

- Empty array or null means "get all bookings" (no property filter)
- Single property selection works the same: `[5]` → `?PropertyIds=5`
- MongoDB `$in` operator is efficient with proper indexing on `propertyId` field
- Model binding automatically handles array parameters in query string
