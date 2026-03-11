# Property Selection to Bookings Flow

## Overview
This document explains how property IDs are passed from property selection to booking queries.

## Flow Diagram

```
User Selects Property → localStorage → Booking Component → BookingsService → API Controller → Query Handler → MongoDB Query
```

## Implementation Details

### 1. Frontend - Property Selection (TypeScript)

**File:** `app/src/app/property/property-selection/property-selection.component.ts`

When user clicks "Apply Selection":
```typescript
applySelection(): void {
  const selected = this.toppings.value ?? [];
  const propertyIds = selected.map(p => (p as any)?.id).filter(id => id != null);
  localStorage.setItem('selectedPropertyIds', JSON.stringify(propertyIds));
  this.router.navigate(['/propertyLanding']);
}
```

Helper method to retrieve IDs:
```typescript
static getSelectedPropertyIds(): number[] {
  const stored = localStorage.getItem('selectedPropertyIds');
  if (!stored) return [];
  return JSON.parse(stored) as number[];
}
```

### 2. Frontend - Bookings Service

**File:** `app/src/app/bookings/services/bookings.service.ts`

Service method that reads from localStorage and calls API:
```typescript
getBookingsForSelectedProperty(searchItem?: string, pageIndex: number = 1, pageSize: number = 10) {
  const propertyIds = this.getSelectedPropertyIds();
  const propertyId = propertyIds.length > 0 ? propertyIds[0] : undefined;
  
  return this.getBookings(propertyId, undefined, searchItem, pageIndex, pageSize);
}
```

### 3. Frontend - Bookings Component

**File:** `app/src/app/Menu/reports/reports.component.ts`

Component that displays bookings with property filtering:
```typescript
ngOnInit(): void {
  this.loadSelectedProperty();
  this.getBookings();
}

loadSelectedProperty(): void {
  const propertyIds = PropertySelectionComponent.getSelectedPropertyIds();
  this.selectedPropertyId = propertyIds.length > 0 ? propertyIds[0] : null;
  console.log('Selected Property ID:', this.selectedPropertyId);
}

getBookings(): void {
  this.isLoading = true;

  let params = new HttpParams();
  params = params.set('PageIndex', (this.pageIndex + 1).toString());
  params = params.set('PageSize', this.pageSize.toString());

  // Add PropertyId parameter if a property is selected
  if (this.selectedPropertyId != null) {
    params = params.set('PropertyId', this.selectedPropertyId.toString());
  }

  params = params.set('OrderBy', this.orderBy);
  params = params.set('SearchItem', this.filterString);
  params = params.set('ActiveSortDirection', this.sortOrder == 'asc' ? 1 : -1);

  this.http.get<any>(this.pathAPI + '/Bookings/GetBookings', { params: params })
    .subscribe({
      next: (data: Booking[]) => {
        this.dataSource.data = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Subscription error:', err);
        this.isLoading = false;
      }
    });
}
```

### 4. Backend - API Controller

**File:** `WebApi/API/V1/BookingsController.cs`

Controller receives propertyId as query parameter:
```csharp
[HttpGet("GetBookings")]
public async Task<ActionResult<IListResponseModel<GetBookingsListDTO>>> GetBookings(
    [FromQuery] GetBookingsListQuery query)
    => Ok(await _mediator.Send(query));
```

### 5. Backend - Query Model

**File:** `classfiles/Application/Common/Bookings/GetBookingsListQuery.cs`

Query model with PropertyId property:
```csharp
public class GetBookingsListQuery : ListQueryModel<GetBookingsListDTO>
{
    public string BookingId { get; init; }
    public int? PropertyId { get; init; }  // ✅ Added
}
```

Query handler passes propertyId to MongoDB query:
```csharp
var menuList = _unitOfWork.Bookings?.GetPagedListBy<GetBookingsListDTO>(
    MongoCollections.BookingsCollection,
    new GetBookingsMongoQuery(
        request.BookingId,
        request.SearchItem,
        request.PageIndex,
        request.PageSize,
        request.OrderBy,
        request.ActiveSortDirection,
        request.PropertyId  // ✅ Property ID passed here
    ));
```

### 6. Backend - MongoDB Query

**File:** `classfiles/Application/Common/Bookings/BookingsMongoQuery/GetBookingsMongoQuery.cs`

MongoDB aggregation pipeline builder:
```csharp
public GetBookingsMongoQuery(
    string bookingId,
    string filterString,
    int pageIndex,
    int pageSize,
    string orderBy,
    int sortDirection,
    int? propertyId = null,  // ✅ Added property ID parameter
    bool getAllProperties = false)
{
    _propertyId = propertyId;
    // ... other assignments
}

private BsonArray GetBookingsPipeline()
{
    var matchDoc = new BsonDocument();
    
    // Filter by property ID if provided
    if (!_getAllProperties && _propertyId.HasValue)
    {
        matchDoc.Add("propertyId", _propertyId.Value);  // ✅ Filters bookings
    }
    
    // ... rest of pipeline
}
```

## API Request Example

```
GET /api/v1/Bookings/GetBookings?PropertyId=5&PageIndex=1&PageSize=10&OrderBy=checkInDate&ActiveSortDirection=-1
```

## MongoDB Query Generated

```javascript
db.bookings.aggregate([
  {
    $match: {
      propertyId: 5  // Filters by selected property
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

## Usage in Components

To use in any Angular component (like reports.component.ts):

```typescript
import { PropertySelectionComponent } from '../../property/property-selection/property-selection.component';

// In ngOnInit or any method
loadSelectedProperty(): void {
  const propertyIds = PropertySelectionComponent.getSelectedPropertyIds();
  this.selectedPropertyId = propertyIds.length > 0 ? propertyIds[0] : null;
}

// Then add PropertyId to your API parameters
if (this.selectedPropertyId != null) {
  params = params.set('PropertyId', this.selectedPropertyId.toString());
}
```

## Testing

1. **Select a property** in the property selection component
2. **Check localStorage**: `localStorage.getItem('selectedPropertyIds')`
3. **Navigate to bookings page** - it should automatically load bookings for selected property
4. **Check network tab** - API call should include `PropertyId` parameter
5. **Verify results** - only bookings for that property should appear

## Notes

- Property IDs are stored as JSON array in localStorage
- Currently uses first property ID if multiple selected
- To support multiple properties, modify the backend to accept an array
- Clear localStorage on logout for security
