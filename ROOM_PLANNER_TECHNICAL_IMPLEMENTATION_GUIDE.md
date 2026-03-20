# Room Planner - Complete Technical Implementation Guide

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Technology Stack](#technology-stack)
4. [Backend Implementation](#backend-implementation)
5. [Frontend Implementation](#frontend-implementation)
6. [Data Flow](#data-flow)
7. [API Endpoints](#api-endpoints)
8. [MongoDB Queries](#mongodb-queries)
9. [Component Hierarchy](#component-hierarchy)
10. [Key Methods Reference](#key-methods-reference)
11. [Troubleshooting History](#troubleshooting-history)
12. [Testing & Debugging](#testing--debugging)

---

## Overview

The Room Planner is a professional Gantt-chart style booking visualization system for hotel room management. It displays rooms across a timeline with horizontal booking bars showing occupancy status, guest information, and booking durations.

### Key Features
- ✅ Gantt-chart style horizontal booking bars
- ✅ Real-time room availability tracking
- ✅ Interactive booking details dialogs
- ✅ Color-coded status indicators
- ✅ Sticky headers and sidebar for easy navigation
- ✅ Monthly navigation with today indicator
- ✅ Occupancy statistics dashboard
- ✅ MongoDB aggregation pipeline for efficient queries
- ✅ Responsive design with horizontal/vertical scrolling

---

## Architecture

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                      Frontend (Angular 17)                   │
├─────────────────────────────────────────────────────────────┤
│  Dashboard1Component                                         │
│  ├── Room Planner Tab (Gantt Chart)                         │
│  │   ├── Gantt Header (Days)                                │
│  │   ├── Room Sidebar (Room Info)                           │
│  │   ├── Timeline Grid (Day Columns)                        │
│  │   └── Booking Bars (Overlay)                             │
│  └── BookingDetailsDialogComponent                          │
│      └── Material Dialog (Room & Booking Info)              │
└─────────────────────────────────────────────────────────────┘
                              ↓ HTTP
┌─────────────────────────────────────────────────────────────┐
│                  API Layer (.NET 6 Web API)                  │
├─────────────────────────────────────────────────────────────┤
│  DashboardController                                         │
│  ├── GET /api/v1/dashboard/rooms                            │
│  └── GET /api/v1/dashboard/calendar-events                  │
└─────────────────────────────────────────────────────────────┘
                              ↓ MediatR
┌─────────────────────────────────────────────────────────────┐
│              Application Layer (CQRS Pattern)                │
├─────────────────────────────────────────────────────────────┤
│  GetRoomsByPropertyQuery                                     │
│  └── GetRoomsByPropertyQueryHandler                          │
│      └── GetRoomsMongoQuery (Aggregation Pipeline)          │
└─────────────────────────────────────────────────────────────┘
                              ↓ MongoDB.Driver
┌─────────────────────────────────────────────────────────────┐
│                    MongoDB Database                          │
├─────────────────────────────────────────────────────────────┤
│  Collections:                                                │
│  ├── Room (PascalCase schema)                               │
│  ├── Bookings (camelCase schema)                            │
│  └── Property (_id based)                                   │
└─────────────────────────────────────────────────────────────┘
```

### Design Pattern: CQRS with MediatR
- **Queries**: Read operations (GetRoomsByPropertyQuery)
- **Handlers**: Business logic execution (GetRoomsByPropertyQueryHandler)
- **DTOs**: Data transfer objects (GetRoomListDTO)
- **Repository**: MongoDB queries encapsulated (GetRoomsMongoQuery)

---

## Technology Stack

### Frontend Technologies

#### Core Framework
```json
{
  "framework": "Angular",
  "version": "17.x",
  "architecture": "Standalone Components"
}
```

#### UI Libraries & Components
| Library | Version | Purpose | Components Used |
|---------|---------|---------|-----------------|
| **Angular Material** | 17.x | UI Component Library | `MatDialog`, `MatSnackBar`, `MatTooltip`, `MatIcon`, `MatButton`, `MatCard`, `MatTabs`, `MatDivider`, `MatChips`, `MatProgressSpinner` |
| **Angular CDK** | 17.x | Component Dev Kit | Overlay, Portal |
| **Angular Gridster2** | 17.x | Dashboard Grid Layout | Gridster, GridsterItem |
| **RxJS** | 7.x | Reactive Programming | `Observable`, `of`, `catchError`, `pipe` |

#### TypeScript Configuration
```typescript
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "ES2022",
    "strict": true,
    "strictNullChecks": true
  }
}
```

### Backend Technologies

#### Core Framework
```csharp
// .NET 6.0 LTS
// C# 10.0 Language Features
```

#### NuGet Packages
| Package | Version | Purpose |
|---------|---------|---------|
| **MediatR** | 12.x | CQRS Pattern Implementation |
| **MongoDB.Driver** | 3.x | MongoDB Database Access |
| **MongoDB.Bson** | 3.x | BSON Document Handling |
| **Microsoft.AspNetCore.Mvc** | 6.0 | Web API Controllers |

#### Database
- **MongoDB**: 7.x or higher
- **Connection**: IMongoDatabase (Dependency Injection)
- **Collections**: Room, Bookings, Property

---

## Backend Implementation

### 1. API Controller

**File**: `WebApi/API/V1/DashboardController.cs`

```csharp
[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet("rooms")]
    public async Task<ActionResult<List<GetRoomListDTO>>> GetRoomsByProperty(
        [FromQuery] int userId,
        [FromQuery] List<int> propertyIds,
        [FromQuery] bool activeOnly = true)
    {
        var query = new GetRoomsByPropertyQuery
        {
            UserId = userId,
            PropertyIds = propertyIds,
            ActiveOnly = activeOnly
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("calendar-events")]
    public async Task<ActionResult<List<CalendarEventDTO>>> GetCalendarEvents(
        [FromQuery] int userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] List<int> propertyIds)
    {
        // Returns booking events for calendar
        // Implementation calls booking query
    }
}
```

**HTTP Request Example**:
```http
GET /api/v1/dashboard/rooms?userId=1&propertyIds=1&activeOnly=true
Host: localhost:5000
Authorization: Bearer {token}
Property-Id: 1
```

**Response Format**:
```json
[
  {
    "id": "12",
    "roomId": 12,
    "roomNumber": "101",
    "roomName": "Deluxe Room 101",
    "roomType": "Deluxe",
    "propertyId": 1,
    "propertyName": "Unknown Property",
    "floor": 1,
    "capacity": 2,
    "status": "Available",
    "amenities": ["WiFi", "TV", "AC"],
    "pricePerNight": 120.00,
    "isActive": true
  }
]
```

### 2. Query Definition

**File**: `classfiles/Application/Dashboard/Queries/GetRoomsByPropertyQuery.cs`

```csharp
public class GetRoomsByPropertyQuery : IRequest<List<GetRoomListDTO>>
{
    public int UserId { get; set; }
    public List<int> PropertyIds { get; set; }
    public bool ActiveOnly { get; set; } = true;
}
```

**MediatR Pattern**:
- `IRequest<T>`: Defines return type
- Sent to handler via `_mediator.Send(query)`
- Decouples controller from business logic

### 3. Query Handler

**File**: `classfiles/Application/Dashboard/Queries/GetRoomsByPropertyQueryHandler.cs`

```csharp
public class GetRoomsByPropertyQueryHandler : 
    IRequestHandler<GetRoomsByPropertyQuery, List<GetRoomListDTO>>
{
    private readonly IMongoDatabase _database;

    public async Task<List<GetRoomListDTO>> Handle(
        GetRoomsByPropertyQuery request, 
        CancellationToken cancellationToken)
    {
        // 1. Get MongoDB collection
        var roomsCollection = _database.GetCollection<BsonDocument>(
            MongoCollections.RoomCollection); // "Room"

        // 2. Create aggregation pipeline
        var mongoQuery = new GetRoomsMongoQuery(
            request.UserId,
            request.PropertyIds,
            request.ActiveOnly
        );

        // 3. Execute aggregation
        var pipelineStages = mongoQuery.BsonPipeline
            .Select(stage => (BsonDocument)stage)
            .ToArray();

        var roomDocs = await roomsCollection
            .Aggregate<BsonDocument>(pipelineStages)
            .ToListAsync(cancellationToken);

        // 4. Map BsonDocument to DTO
        var rooms = new List<GetRoomListDTO>();
        foreach (var doc in roomDocs)
        {
            rooms.Add(new GetRoomListDTO
            {
                Id = doc["_id"].ToString(),
                RoomId = doc.GetValue("roomId", 0).AsInt32,
                RoomNumber = doc.GetValue("roomNumber", "").AsString,
                // ... (full mapping)
                Floor = GetNullableInt(doc, "floor"),
                PricePerNight = GetNullableDecimal(doc, "pricePerNight")
            });
        }

        return rooms;
    }

    // Helper: Safely extract nullable int from BsonDocument
    int? GetNullableInt(BsonDocument document, string fieldName)
    {
        if (!document.Contains(fieldName)) return null;
        var value = document.GetValue(fieldName, BsonNull.Value);
        return value.IsBsonNull ? null : (int?)value.AsInt32;
    }

    // Helper: Safely extract nullable decimal from BsonDocument
    decimal? GetNullableDecimal(BsonDocument document, string fieldName)
    {
        if (!document.Contains(fieldName)) return null;
        var value = document.GetValue(fieldName, BsonNull.Value);
        return value.IsBsonNull ? null : (decimal?)value.ToDecimal();
    }
}
```

**Key Points**:
- ✅ **Null-safe mapping**: Handles BsonNull properly
- ✅ **Debug logging**: Console.WriteLine for diagnostics
- ✅ **Error handling**: Try-catch returns empty list on failure
- ✅ **Async/await**: Non-blocking database operations

### 4. MongoDB Aggregation Query

**File**: `classfiles/Application/Dashboard/Queries/GetRoomsMongoQuery.cs`

```csharp
public class GetRoomsMongoQuery : INamedQuery
{
    private readonly int _userId;
    private readonly List<int> _propertyIds;
    private readonly bool _activeOnly;

    public BsonArray? BsonPipeline => GetRoomsPipeline();

    private BsonArray GetRoomsPipeline()
    {
        var pipeline = new BsonArray();

        // STAGE 1: $match - Filter rooms
        var andConditions = new BsonArray();

        // Filter: Active rooms
        if (_activeOnly)
        {
            andConditions.Add(new BsonDocument("$or", new BsonArray
            {
                new BsonDocument("Active", true),
                new BsonDocument("isActive", true)
            }));
        }

        // Filter: By property IDs
        if (_propertyIds?.Any() == true)
        {
            var propertyIdsArray = new BsonArray(
                _propertyIds.Select(id => new BsonInt32(id)));
            
            andConditions.Add(new BsonDocument("$or", new BsonArray
            {
                new BsonDocument("PropertyId", 
                    new BsonDocument("$in", propertyIdsArray)),
                new BsonDocument("propertyId", 
                    new BsonDocument("$in", propertyIdsArray))
            }));
        }

        // Filter: Not deleted
        andConditions.Add(new BsonDocument("$or", new BsonArray
        {
            new BsonDocument("IsDeleted", new BsonDocument("$ne", true)),
            new BsonDocument("IsDeleted", new BsonDocument("$exists", false))
        }));

        var matchDocument = new BsonDocument("$and", andConditions);
        pipeline.Add(new BsonDocument("$match", matchDocument));

        // STAGE 2: $lookup - Join with Property collection
        pipeline.Add(new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "Property" },
            { "localField", "PropertyId" },
            { "foreignField", "_id" },  // Property uses _id as key
            { "as", "propertyInfo" }
        }));

        // STAGE 3: $project - Map and transform fields
        pipeline.Add(new BsonDocument("$project", new BsonDocument
        {
            { "_id", "$_id" },
            { "roomId", new BsonDocument("$ifNull", 
                new BsonArray { "$RoomId", "$_id" }) },
            { "roomNumber", new BsonDocument("$ifNull", 
                new BsonArray { "$roomNumber", "$RoomCode" }) },
            { "roomName", new BsonDocument("$ifNull", new BsonArray 
                { "$RoomName", "$roomName", "$RoomCode", "$roomNumber" }) },
            { "roomType", new BsonDocument("$ifNull", 
                new BsonArray { "$roomType", "Standard" }) },
            { "propertyId", new BsonDocument("$ifNull", 
                new BsonArray { "$PropertyId", "$propertyId" }) },
            { "propertyName", new BsonDocument("$ifNull", new BsonArray 
            { 
                new BsonDocument("$arrayElemAt", 
                    new BsonArray { "$propertyInfo.Name", 0 }),  // Property.Name
                "Unknown Property" 
            })},
            { "floor", "$floor" },
            { "capacity", "$capacity" },
            { "status", new BsonDocument("$ifNull", 
                new BsonArray { "$status", "Available" }) },
            { "amenities", new BsonDocument("$ifNull", 
                new BsonArray { "$amenities", new BsonArray() }) },
            { "pricePerNight", "$pricePerNight" },
            { "isActive", new BsonDocument("$ifNull", new BsonArray 
                { "$isActive", "$Active", true }) }
        }));

        // STAGE 4: $sort - Order results
        pipeline.Add(new BsonDocument("$sort", new BsonDocument
        {
            { "propertyName", 1 },
            { "floor", 1 },
            { "roomNumber", 1 }
        }));

        return pipeline;
    }
}
```

**Pipeline Breakdown**:

1. **$match**: Filters documents
   - Active = true
   - PropertyId IN [1, 2, ...]
   - IsDeleted != true

2. **$lookup**: Left outer join
   - From: Property collection
   - On: Room.PropertyId = Property._id

3. **$project**: Field transformation
   - Maps PascalCase/camelCase variants
   - Provides default values with $ifNull
   - Extracts property name from joined array

4. **$sort**: Orders results
   - By property name, floor, room number

### 5. Data Transfer Object (DTO)

**File**: `classfiles/Application/Dashboard/DTOs/GetRoomListDTO.cs`

```csharp
public class GetRoomListDTO
{
    public string Id { get; set; }              // MongoDB _id
    public int RoomId { get; set; }             // Room identifier
    public string RoomNumber { get; set; }      // e.g., "101"
    public string RoomName { get; set; }        // e.g., "Deluxe Room 101"
    public string RoomType { get; set; }        // e.g., "Deluxe", "Standard"
    public int PropertyId { get; set; }         // Property reference
    public string PropertyName { get; set; }    // Property display name
    public int? Floor { get; set; }             // Nullable floor number
    public int? Capacity { get; set; }          // Nullable guest capacity
    public string Status { get; set; }          // "Available", "Occupied", etc.
    public List<string> Amenities { get; set; } // ["WiFi", "TV", "AC"]
    public decimal? PricePerNight { get; set; } // Nullable price
    public bool IsActive { get; set; }          // Active status
}
```

**Nullable Fields**:
- `int? Floor`: Not all rooms have floor info
- `int? Capacity`: Optional capacity
- `decimal? PricePerNight`: Price may not be set

---

## Frontend Implementation

### 1. Main Component

**File**: `app/src/app/dashboard/dashboard1/dashboard1.component.ts`

#### Imports
```typescript
import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule, MatTabChangeEvent } from '@angular/material/tabs';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { DashboardService } from '../../services/dashboard.service';
import { BookingDetailsDialogComponent, BookingDetailsData } from '../booking-details-dialog/booking-details-dialog.component';
```

#### Component Decorator
```typescript
@Component({
  selector: 'app-dashboard1',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatTabsModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    // ... other imports
  ],
  templateUrl: './dashboard1.component.html',
  styleUrls: [
    './dashboard1.component.css',
    './room-planner-gantt.css'  // Gantt-specific styles
  ],
  encapsulation: ViewEncapsulation.None
})
```

#### Class Properties
```typescript
export class Dashboard1Component implements OnInit {
  // ===== Room Planner Properties =====
  loadingRoomPlanner = false;
  currentPlannerMonth = new Date();
  plannerDays: Array<{ date: Date; dayOfWeek: string }> = [];
  rooms: Array<any> = [];
  roomBookings: Map<string, any> = new Map();           // Individual days
  roomBookingBars: Map<number, BookingBar[]> = new Map(); // Continuous bars
  totalRooms = 0;
  occupiedRoomsToday = 0;
  availableRoomsToday = 0;
  occupancyRateToday = 0;

  constructor(
    private dashboardService: DashboardService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {}
}
```

#### Interface Definitions
```typescript
export interface BookingBar {
  bookingId: string;
  guestName: string;
  type: string;           // 'occupied' | 'checkin' | 'checkout' | 'maintenance'
  color: string;
  startDate: Date;
  endDate: Date;
  startCol: number;       // Grid column start (1-based)
  span: number;           // Number of days to span
}
```

### 2. Key Methods

#### Tab Change Handler
```typescript
onTabChange(event: MatTabChangeEvent): void {
  console.log(`Tab changed to index: ${event.index}, label: ${event.tab.textLabel}`);
  this.selectedTabIndex = event.index;

  if (event.index === 1) { // Room Planner tab
    this.loadRoomPlannerData();
  }
}
```

#### Load Room Planner Data
```typescript
private loadRoomPlannerData(): void {
  this.loadingRoomPlanner = true;
  this.generatePlannerDays();

  const userId = this.getCurrentUserId();
  let propertyIds = this.getSelectedPropertyIds();

  // Default to property ID 1 if none selected
  if (!propertyIds || propertyIds.length === 0) {
    console.warn('⚠️ No properties selected, defaulting to PropertyId: 1');
    propertyIds = [1];
  }

  console.log(`🏨 Loading Room Planner for properties: ${JSON.stringify(propertyIds)}`);

  this.loadRoomsForPlanner(propertyIds, userId);
}
```

#### Generate Planner Days
```typescript
private generatePlannerDays(): void {
  const year = this.currentPlannerMonth.getFullYear();
  const month = this.currentPlannerMonth.getMonth();
  const daysInMonth = new Date(year, month + 1, 0).getDate();

  this.plannerDays = [];
  for (let day = 1; day <= daysInMonth; day++) {
    const date = new Date(year, month, day);
    this.plannerDays.push({
      date: date,
      dayOfWeek: date.toLocaleDateString('en-US', { weekday: 'short' })
    });
  }
}
```

#### Load Rooms from API
```typescript
private loadRoomsForPlanner(propertyIds: number[], userId: number): void {
  console.log('🏨 Loading rooms from backend API...');

  this.dashboardService.getRoomsByProperty(userId, propertyIds, true).pipe(
    catchError(error => {
      console.error('❌ Error loading rooms:', error);
      this.snackBar.open('Failed to load rooms', 'Close', { duration: 3000 });
      return of([]);
    })
  ).subscribe(rooms => {
    console.log(`✅ Rooms loaded from API: ${rooms.length} rooms`);

    // Map API response to component format
    this.rooms = rooms.map(room => ({
      id: room.roomId,
      roomNumber: room.roomNumber,
      roomName: room.roomName || room.roomNumber,
      roomType: room.roomType,
      propertyId: room.propertyId,
      propertyName: room.propertyName,
      floor: room.floor,
      capacity: room.capacity,
      status: room.status,
      amenities: room.amenities,
      pricePerNight: room.pricePerNight
    }));

    this.totalRooms = this.rooms.length;

    // Load bookings for the month
    const startDate = new Date(
      this.currentPlannerMonth.getFullYear(), 
      this.currentPlannerMonth.getMonth(), 
      1
    );
    const endDate = new Date(
      this.currentPlannerMonth.getFullYear(), 
      this.currentPlannerMonth.getMonth() + 1, 
      0
    );

    this.dashboardService.getCalendarEvents(userId, startDate, endDate, propertyIds).pipe(
      catchError(error => {
        console.error('Error loading room planner data:', error);
        return of([]);
      })
    ).subscribe(events => {
      this.processBookingsForPlanner(events);
      this.calculateOccupancyStats();
      this.loadingRoomPlanner = false;
      this.cdr.detectChanges();
    });
  });
}
```

#### Process Bookings into Bars
```typescript
private processBookingsForPlanner(events: any[]): void {
  this.roomBookings.clear();
  this.roomBookingBars.clear();

  events.forEach(event => {
    // Extract room number from event title
    const roomMatch = event.title.match(/Room (\d+)/);
    if (roomMatch) {
      const roomNumber = roomMatch[1];
      const room = this.rooms.find(r => r.roomNumber === roomNumber);

      if (room) {
        const startDate = new Date(event.start);
        const endDate = event.end ? new Date(event.end) : startDate;

        // Add booking for each day (backward compatibility)
        let currentDate = new Date(startDate);
        while (currentDate <= endDate) {
          const key = `${room.id}-${currentDate.toISOString().split('T')[0]}`;
          this.roomBookings.set(key, {
            type: event.type,
            guestName: event.description?.split('-')[1]?.trim() || 'Guest',
            bookingId: event.id,
            color: event.color
          });
          currentDate.setDate(currentDate.getDate() + 1);
        }

        // Create continuous booking bar
        this.addBookingBar(room.id, {
          bookingId: event.id,
          guestName: event.description?.split('-')[1]?.trim() || 'Guest',
          type: event.type,
          color: event.color,
          startDate: startDate,
          endDate: endDate
        });
      }
    }
  });
}
```

#### Add Booking Bar (Grid Calculation)
```typescript
private addBookingBar(roomId: number, booking: {
  bookingId: string;
  guestName: string;
  type: string;
  color: string;
  startDate: Date;
  endDate: Date;
}): void {
  const monthStart = new Date(
    this.currentPlannerMonth.getFullYear(),
    this.currentPlannerMonth.getMonth(),
    1
  );
  const monthEnd = new Date(
    this.currentPlannerMonth.getFullYear(),
    this.currentPlannerMonth.getMonth() + 1,
    0
  );

  // Clip booking to month boundaries
  const bookingStart = new Date(Math.max(booking.startDate.getTime(), monthStart.getTime()));
  const bookingEnd = new Date(Math.min(booking.endDate.getTime(), monthEnd.getTime()));

  // Calculate start column (1-based, +1 for room label column)
  let startCol = 2; // Start after room sidebar
  const dayDiff = Math.floor((bookingStart.getTime() - monthStart.getTime()) / (1000 * 60 * 60 * 24));
  startCol += dayDiff;

  // Calculate span (number of days)
  const span = Math.floor((bookingEnd.getTime() - bookingStart.getTime()) / (1000 * 60 * 60 * 24)) + 1;

  if (span > 0) {
    const bar: BookingBar = {
      ...booking,
      startCol,
      span
    };

    if (!this.roomBookingBars.has(roomId)) {
      this.roomBookingBars.set(roomId, []);
    }
    this.roomBookingBars.get(roomId)!.push(bar);
  }
}
```

**Grid Calculation Example**:
```
Month: January 2026 (31 days)
Booking: Jan 15 - Jan 20

Calculation:
- monthStart = Jan 1, 2026 00:00:00
- bookingStart = Jan 15, 2026 00:00:00
- dayDiff = (Jan 15 - Jan 1) / (24 * 60 * 60 * 1000) = 14 days
- startCol = 2 (sidebar) + 14 = 16
- span = (Jan 20 - Jan 15) + 1 = 6 days

Result:
- Bar starts at column 16
- Spans 6 columns
- CSS: grid-column-start: 16; grid-column: span 6;
```

#### Template Helper Methods
```typescript
getBookingBars(roomId: number): BookingBar[] {
  return this.roomBookingBars.get(roomId) || [];
}

isToday(date: Date): boolean {
  const today = new Date();
  return date.toDateString() === today.toDateString();
}

isWeekend(date: Date): boolean {
  const day = date.getDay();
  return day === 0 || day === 6; // Sunday or Saturday
}
```

#### Event Handlers
```typescript
onRoomDayClick(room: any, date: Date): void {
  const bookingInfo = this.getBookingInfo(room.id, date);
  
  const dialogData: BookingDetailsData = {
    room: {
      roomNumber: room.roomNumber,
      roomName: room.roomName,
      roomType: room.roomType,
      floor: room.floor,
      capacity: room.capacity,
      status: room.status
    },
    date: new Date(date),
    booking: bookingInfo
  };
  
  const dialogRef = this.dialog.open(BookingDetailsDialogComponent, {
    width: '600px',
    data: dialogData
  });
  
  dialogRef.afterClosed().subscribe(result => {
    if (result?.action === 'create') {
      this.snackBar.open('Booking creation coming soon!', 'Close', { duration: 3000 });
    } else if (result?.action === 'view') {
      this.snackBar.open('View booking details coming soon!', 'Close', { duration: 3000 });
    }
  });
}

onBookingBarClick(room: any, bar: BookingBar, event: Event): void {
  event.stopPropagation(); // Prevent day cell click
  
  const dialogData: BookingDetailsData = {
    room: { /* room info */ },
    date: bar.startDate,
    booking: {
      type: bar.type,
      guestName: bar.guestName,
      bookingId: bar.bookingId,
      color: bar.color
    }
  };
  
  this.dialog.open(BookingDetailsDialogComponent, {
    width: '600px',
    data: dialogData
  });
}
```

### 3. Service Layer

**File**: `app/src/app/services/dashboard.service.ts`

```typescript
@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = '/api/v1/dashboard';

  constructor(private http: HttpClient) {}

  getRoomsByProperty(
    userId: number,
    propertyIds: number[],
    activeOnly: boolean = true
  ): Observable<GetRoomListDTO[]> {
    const params = new HttpParams()
      .set('userId', userId.toString())
      .set('propertyIds', propertyIds.join(','))
      .set('activeOnly', activeOnly.toString());

    return this.http.get<GetRoomListDTO[]>(`${this.apiUrl}/rooms`, { params });
  }

  getCalendarEvents(
    userId: number,
    startDate: Date,
    endDate: Date,
    propertyIds: number[]
  ): Observable<CalendarEvent[]> {
    const params = new HttpParams()
      .set('userId', userId.toString())
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString())
      .set('propertyIds', propertyIds.join(','));

    return this.http.get<CalendarEvent[]>(`${this.apiUrl}/calendar-events`, { params });
  }
}
```

### 4. HTML Template

**File**: `app/src/app/dashboard/dashboard1/dashboard1.component.html`

```html
<!-- Room Planner Tab -->
<mat-tab label="Room Planner">
  <div class="tab-content">
    <div class="room-planner-container">
      <!-- Header with Navigation -->
      <div class="planner-header">
        <h2>
          <mat-icon>hotel</mat-icon>
          Room Occupancy Calendar
        </h2>
        <div class="planner-controls">
          <button mat-icon-button (click)="navigateMonth(-1)">
            <mat-icon>chevron_left</mat-icon>
          </button>
          <span class="current-month">{{ currentPlannerMonth | date:'MMMM yyyy' }}</span>
          <button mat-icon-button (click)="navigateMonth(1)">
            <mat-icon>chevron_right</mat-icon>
          </button>
          <button mat-icon-button (click)="goToToday()">
            <mat-icon>today</mat-icon>
          </button>
          <button mat-raised-button color="primary" (click)="refreshRoomPlanner()">
            <mat-icon>refresh</mat-icon>
            Refresh
          </button>
        </div>
      </div>

      <!-- Gantt Chart -->
      <div class="room-planner-gantt" *ngIf="!loadingRoomPlanner">
        <!-- Legend -->
        <div class="planner-legend">
          <div class="legend-item">
            <div class="legend-color occupied"></div>
            <span>Occupied</span>
          </div>
          <!-- More legend items... -->
        </div>

        <!-- Gantt Container -->
        <div class="gantt-container">
          <!-- Sticky Header -->
          <div class="gantt-header">
            <div class="room-sidebar-header">
              <div class="room-col">Room</div>
              <div class="type-col">Type</div>
            </div>
            <div class="gantt-timeline-header">
              <div class="day-header" 
                   *ngFor="let day of plannerDays" 
                   [class.today]="isToday(day.date)">
                <div class="day-number">{{ day.date | date:'d' }}</div>
                <div class="day-name">{{ day.date | date:'EEE' }}</div>
              </div>
            </div>
          </div>

          <!-- Scrollable Body -->
          <div class="gantt-body">
            <div class="gantt-row" 
                 *ngFor="let room of rooms; let i = index" 
                 [class.even]="i % 2 === 0">
              
              <!-- Room Sidebar -->
              <div class="room-sidebar">
                <div class="room-col">
                  <span class="room-number">{{ room.roomNumber }}</span>
                </div>
                <div class="type-col">
                  <span class="room-type">{{ room.roomType }}</span>
                </div>
              </div>

              <!-- Timeline Grid -->
              <div class="gantt-timeline">
                <!-- Day Columns (clickable background) -->
                <div class="day-column" 
                     *ngFor="let day of plannerDays" 
                     [class.today]="isToday(day.date)"
                     [class.weekend]="isWeekend(day.date)"
                     (click)="onRoomDayClick(room, day.date)">
                </div>

                <!-- Booking Bars Overlay -->
                <div class="booking-bars-layer">
                  <div class="booking-bar" 
                       *ngFor="let bar of getBookingBars(room.id)"
                       [style.grid-column]="'span ' + bar.span"
                       [style.grid-column-start]="bar.startCol - 1"
                       [class.occupied]="bar.type === 'occupied'"
                       [class.checkin]="bar.type === 'checkin'"
                       [class.checkout]="bar.type === 'checkout'"
                       [class.maintenance]="bar.type === 'maintenance'"
                       [matTooltip]="bar.guestName + ' (' + (bar.startDate | date:'MMM d') + ' - ' + (bar.endDate | date:'MMM d') + ')'"
                       (click)="onBookingBarClick(room, bar, $event)">
                    <span class="bar-label">{{ bar.guestName }}</span>
                    <div class="bar-marker start"></div>
                    <div class="bar-marker end"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div class="planner-loading" *ngIf="loadingRoomPlanner">
        <mat-spinner diameter="60"></mat-spinner>
        <p>Loading room availability...</p>
      </div>
    </div>
  </div>
</mat-tab>
```

### 5. CSS Styling

**File**: `app/src/app/dashboard/dashboard1/room-planner-gantt.css`

Key style highlights:

```css
/* Gantt Container */
.gantt-container {
  overflow-x: auto;
  overflow-y: auto;
  max-height: calc(100vh - 400px);
  background: white;
}

/* Sticky Header */
.gantt-header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: white;
  box-shadow: 0 2px 8px rgba(0,0,0,0.08);
}

/* Room Sidebar (Sticky) */
.room-sidebar {
  position: sticky;
  left: 0;
  z-index: 10;
  width: 240px;
  background: white;
}

/* Timeline Grid */
.gantt-timeline {
  display: flex;
  position: relative;
}

/* Day Columns */
.day-column {
  flex: 0 0 50px;
  min-width: 50px;
  border-right: 1px solid #e2e8f0;
  cursor: pointer;
}

/* Booking Bars Layer (Overlay) */
.booking-bars-layer {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: grid;
  grid-template-columns: repeat(auto-fill, 50px);
  padding: 8px 0;
  pointer-events: none;  /* Allow clicks to pass through */
}

/* Booking Bar */
.booking-bar {
  position: relative;
  height: 40px;
  border-radius: 6px;
  cursor: pointer;
  pointer-events: all;  /* Re-enable clicks on bars */
  box-shadow: 0 2px 8px rgba(0,0,0,0.15);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.booking-bar:hover {
  transform: translateY(-3px) scale(1.02);
  box-shadow: 0 6px 16px rgba(0,0,0,0.25);
  z-index: 20;
}

/* Color Variants */
.booking-bar.occupied {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
}

.booking-bar.checkin {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
}

.booking-bar.checkout {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.booking-bar.maintenance {
  background: linear-gradient(135deg, #6b7280 0%, #4b5563 100%);
}
```

**CSS Grid Positioning**:
```css
/* Set via Angular template binding */
.booking-bar {
  grid-column-start: var(--start-col);  /* [style.grid-column-start] */
  grid-column: span var(--span);        /* [style.grid-column] */
}
```

### 6. Booking Details Dialog

**File**: `app/src/app/dashboard/booking-details-dialog/booking-details-dialog.component.ts`

```typescript
@Component({
  selector: 'app-booking-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>info</mat-icon>
      {{ getDialogTitle() }}
    </h2>

    <mat-dialog-content>
      <!-- Room Information -->
      <div class="info-section">
        <h3><mat-icon>hotel</mat-icon> Room Information</h3>
        <div class="info-grid">
          <div class="info-item">
            <span class="label">Room Number:</span>
            <span class="value">{{ data.room.roomNumber }}</span>
          </div>
          <!-- More fields... -->
        </div>
      </div>

      <mat-divider></mat-divider>

      <!-- Booking Status -->
      <div class="info-section">
        <h3><mat-icon>{{ getBookingIcon() }}</mat-icon> Status</h3>
        <mat-chip [ngClass]="getStatusClass()">
          {{ getStatusLabel() }}
        </mat-chip>
        
        <div class="info-grid" *ngIf="data.booking">
          <div class="info-item">
            <span class="label">Guest:</span>
            <span class="value">{{ data.booking.guestName }}</span>
          </div>
        </div>
      </div>
    </mat-dialog-content>

    <mat-dialog-actions>
      <button mat-button (click)="close()">Close</button>
      <button mat-raised-button color="primary" 
              *ngIf="!data.booking"
              (click)="createBooking()">Create Booking</button>
      <button mat-raised-button color="primary" 
              *ngIf="data.booking"
              (click)="viewFullBooking()">View Full Booking</button>
    </mat-dialog-actions>
  `,
  styles: [/* Styles... */]
})
export class BookingDetailsDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<BookingDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: BookingDetailsData
  ) {}

  close(): void {
    this.dialogRef.close();
  }

  createBooking(): void {
    this.dialogRef.close({ 
      action: 'create', 
      room: this.data.room, 
      date: this.data.date 
    });
  }

  viewFullBooking(): void {
    this.dialogRef.close({ 
      action: 'view', 
      bookingId: this.data.booking?.bookingId 
    });
  }
}
```

---

## Data Flow

### Complete Request/Response Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. User Action: Clicks "Room Planner" Tab                   │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Angular Component: onTabChange()                         │
│    - Detects tab index === 1                                │
│    - Calls loadRoomPlannerData()                            │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Component: loadRoomPlannerData()                         │
│    - Sets loadingRoomPlanner = true                         │
│    - Generates plannerDays array (1-31)                     │
│    - Gets propertyIds from localStorage                     │
│    - Calls loadRoomsForPlanner()                            │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Service: dashboardService.getRoomsByProperty()           │
│    HTTP GET /api/v1/dashboard/rooms                         │
│    Query Params: userId=1, propertyIds=1, activeOnly=true   │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. API Controller: DashboardController.GetRoomsByProperty() │
│    - Validates request                                       │
│    - Creates GetRoomsByPropertyQuery                         │
│    - Sends to MediatR                                        │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. MediatR: Routes to Handler                               │
│    GetRoomsByPropertyQueryHandler.Handle()                   │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. Query Handler:                                            │
│    - Gets IMongoDatabase from DI                            │
│    - Gets "Room" collection                                 │
│    - Creates GetRoomsMongoQuery                             │
│    - Builds aggregation pipeline                            │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 8. MongoDB Aggregation:                                      │
│    Stage 1: $match (Active=true, PropertyId=1, !IsDeleted)  │
│    Stage 2: $lookup (Join with Property)                    │
│    Stage 3: $project (Map fields, defaults)                 │
│    Stage 4: $sort (propertyName, floor, roomNumber)         │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 9. MongoDB Returns: BsonDocument[]                          │
│    [                                                         │
│      { _id: 12, roomId: 12, roomNumber: "101", ... },       │
│      { _id: 1001, roomId: 1001, roomNumber: "102", ... }    │
│    ]                                                         │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 10. Handler: Maps BsonDocument → GetRoomListDTO             │
│     - Extracts fields with GetValue()                       │
│     - Handles nulls with GetNullableInt/Decimal()           │
│     - Returns List<GetRoomListDTO>                          │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 11. API Response: JSON Array                                │
│     Status: 200 OK                                           │
│     Body: [{ id: "12", roomId: 12, ... }, ...]              │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 12. Angular Service: Returns Observable<GetRoomListDTO[]>   │
│     Component subscribes with .pipe(catchError())            │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 13. Component: Processes Rooms                              │
│     - Maps API DTO to component room format                 │
│     - Sets this.rooms = [...]                               │
│     - Calls getCalendarEvents() for bookings                │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 14. Component: Processes Booking Events                     │
│     - Calls processBookingsForPlanner()                     │
│     - Creates roomBookings Map (day-by-day)                 │
│     - Creates roomBookingBars Map (continuous bars)         │
│     - Calls addBookingBar() for each booking                │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 15. Component: Calculates Grid Positions                    │
│     - For each booking:                                      │
│       • Calculate startCol (day offset + 2)                 │
│       • Calculate span (end - start + 1)                    │
│       • Create BookingBar object                            │
│       • Add to roomBookingBars Map                          │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 16. Template: Renders Gantt Chart                           │
│     - *ngFor rooms → gantt-row                              │
│     - *ngFor plannerDays → day-column                       │
│     - *ngFor getBookingBars(room.id) → booking-bar          │
│     - [style.grid-column-start]="bar.startCol - 1"          │
│     - [style.grid-column]="'span ' + bar.span"              │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ 17. User Sees: Professional Gantt Chart                     │
│     - Room list on left (sticky)                            │
│     - Days across top (sticky header)                       │
│     - Colored booking bars spanning dates                   │
│     - Interactive hover effects                              │
└─────────────────────────────────────────────────────────────┘
```

---

## API Endpoints

### 1. Get Rooms by Property

**Endpoint**: `GET /api/v1/dashboard/rooms`

**Query Parameters**:
```typescript
{
  userId: number;        // Required: User ID
  propertyIds: number[]; // Required: Array of property IDs
  activeOnly: boolean;   // Optional: Filter active rooms (default: true)
}
```

**Request Example**:
```http
GET /api/v1/dashboard/rooms?userId=1&propertyIds=1&activeOnly=true
Host: localhost:5000
Authorization: Bearer eyJhbGc...
Property-Id: 1
```

**Response Example**:
```json
{
  "status": 200,
  "data": [
    {
      "id": "12",
      "roomId": 12,
      "roomNumber": "xxxco10",
      "roomName": "R1",
      "roomType": "Standard",
      "propertyId": 1,
      "propertyName": "Unknown Property",
      "floor": null,
      "capacity": null,
      "status": "Available",
      "amenities": [],
      "pricePerNight": null,
      "isActive": true
    },
    {
      "id": "1001",
      "roomId": 1001,
      "roomNumber": "101",
      "roomName": "Deluxe Room 101",
      "roomType": "Deluxe",
      "propertyId": 1,
      "propertyName": "Unknown Property",
      "floor": 1,
      "capacity": 2,
      "status": "Available",
      "amenities": ["WiFi", "TV", "Mini-bar", "Safe"],
      "pricePerNight": 120.00,
      "isActive": true
    }
  ]
}
```

### 2. Get Calendar Events

**Endpoint**: `GET /api/v1/dashboard/calendar-events`

**Query Parameters**:
```typescript
{
  userId: number;
  startDate: string;     // ISO 8601 format
  endDate: string;       // ISO 8601 format
  propertyIds: number[];
}
```

**Request Example**:
```http
GET /api/v1/dashboard/calendar-events?userId=1&startDate=2026-03-01T00:00:00Z&endDate=2026-03-31T23:59:59Z&propertyIds=1
```

**Response Example**:
```json
[
  {
    "id": "TEST001",
    "title": "Room 101 - Booking",
    "description": "Booking - John Doe",
    "start": "2026-03-15T14:00:00Z",
    "end": "2026-03-20T11:00:00Z",
    "type": "occupied",
    "color": "#ef4444"
  }
]
```

---

## MongoDB Queries

### Aggregation Pipeline Breakdown

#### Stage 1: $match
```javascript
{
  $match: {
    $and: [
      {
        $or: [
          { Active: true },
          { isActive: true }
        ]
      },
      {
        $or: [
          { PropertyId: { $in: [1] } },
          { propertyId: { $in: [1] } }
        ]
      },
      {
        $or: [
          { IsDeleted: { $ne: true } },
          { IsDeleted: { $exists: false } }
        ]
      }
    ]
  }
}
```

**Purpose**: Filter active, non-deleted rooms for specified properties

**Result**: Reduces 11 rooms → 8 rooms (for PropertyId=1)

#### Stage 2: $lookup
```javascript
{
  $lookup: {
    from: "Property",
    localField: "PropertyId",
    foreignField: "_id",
    as: "propertyInfo"
  }
}
```

**Purpose**: Left outer join with Property collection

**Result**: Adds propertyInfo array to each room document

#### Stage 3: $project
```javascript
{
  $project: {
    _id: "$_id",
    roomId: { $ifNull: ["$RoomId", "$_id"] },
    roomNumber: { $ifNull: ["$roomNumber", "$RoomCode"] },
    roomName: { 
      $ifNull: ["$RoomName", "$roomName", "$RoomCode", "$roomNumber"] 
    },
    roomType: { $ifNull: ["$roomType", "Standard"] },
    propertyId: { $ifNull: ["$PropertyId", "$propertyId"] },
    propertyName: { 
      $ifNull: [
        { $arrayElemAt: ["$propertyInfo.Name", 0] },
        "Unknown Property"
      ] 
    },
    floor: "$floor",
    capacity: "$capacity",
    status: { $ifNull: ["$status", "Available"] },
    amenities: { $ifNull: ["$amenities", []] },
    pricePerNight: "$pricePerNight",
    isActive: { $ifNull: ["$isActive", "$Active", true] }
  }
}
```

**Purpose**: 
- Map and normalize field names
- Provide default values
- Extract property name from joined array

**Result**: Consistent output schema

#### Stage 4: $sort
```javascript
{
  $sort: {
    propertyName: 1,
    floor: 1,
    roomNumber: 1
  }
}
```

**Purpose**: Order rooms logically

**Result**: Sorted by property → floor → room number

---

## Component Hierarchy

```
Dashboard1Component
├── Tab: Dashboard (Gridster Widgets)
└── Tab: Room Planner ✓ ACTIVE
    ├── Planner Header
    │   ├── Title: "Room Occupancy Calendar"
    │   └── Controls
    │       ├── Previous Month Button
    │       ├── Current Month Label
    │       ├── Next Month Button
    │       ├── Go to Today Button
    │       └── Refresh Button
    │
    ├── Room Planner Gantt
    │   ├── Legend
    │   │   ├── Occupied (Red)
    │   │   ├── Check-in (Blue)
    │   │   ├── Check-out (Orange)
    │   │   └── Maintenance (Gray)
    │   │
    │   └── Gantt Container
    │       ├── Gantt Header (Sticky)
    │       │   ├── Room Sidebar Header
    │       │   │   ├── Room Column
    │       │   │   └── Type Column
    │       │   └── Timeline Header
    │       │       └── Day Headers (*ngFor plannerDays)
    │       │           ├── Day Number
    │       │           └── Day Name
    │       │
    │       └── Gantt Body (Scrollable)
    │           └── Gantt Rows (*ngFor rooms)
    │               ├── Room Sidebar (Sticky)
    │               │   ├── Room Number
    │               │   └── Room Type
    │               │
    │               └── Gantt Timeline
    │                   ├── Day Columns (*ngFor plannerDays)
    │                   │   └── (click)="onRoomDayClick()"
    │                   │
    │                   └── Booking Bars Layer (Overlay)
    │                       └── Booking Bars (*ngFor getBookingBars())
    │                           ├── [style.grid-column-start]
    │                           ├── [style.grid-column]
    │                           ├── (click)="onBookingBarClick()"
    │                           ├── Bar Label (Guest Name)
    │                           ├── Start Marker
    │                           └── End Marker
    │
    ├── Loading State (*ngIf loadingRoomPlanner)
    │   ├── Spinner
    │   └── "Loading room availability..."
    │
    └── Summary Stats Cards
        ├── Total Rooms
        ├── Occupied Today
        ├── Available Today
        └── Occupancy Rate

BookingDetailsDialogComponent (Opened on click)
├── Dialog Title
│   └── "Room {number} - {date}"
├── Dialog Content
│   ├── Room Information Section
│   │   ├── Room Number
│   │   ├── Room Name
│   │   ├── Room Type
│   │   ├── Floor
│   │   ├── Capacity
│   │   └── Date
│   │
│   └── Status Section
│       ├── Status Chip (Color-coded)
│       ├── Guest Name (if booked)
│       └── Booking ID (if booked)
│
└── Dialog Actions
    ├── Close Button
    ├── Create Booking (if available)
    └── View Full Booking (if occupied)
```

---

## Key Methods Reference

### Backend Methods

| Method | File | Purpose | Parameters | Returns |
|--------|------|---------|------------|---------|
| `GetRoomsByProperty` | DashboardController.cs | API endpoint for rooms | userId, propertyIds, activeOnly | ActionResult<List<GetRoomListDTO>> |
| `Handle` | GetRoomsByPropertyQueryHandler.cs | Executes query and maps results | GetRoomsByPropertyQuery | Task<List<GetRoomListDTO>> |
| `GetRoomsPipeline` | GetRoomsMongoQuery.cs | Builds MongoDB aggregation | - | BsonArray |
| `GetNullableInt` | GetRoomsByPropertyQueryHandler.cs | Safely extracts nullable int | BsonDocument, fieldName | int? |
| `GetNullableDecimal` | GetRoomsByPropertyQueryHandler.cs | Safely extracts nullable decimal | BsonDocument, fieldName | decimal? |

### Frontend Methods

| Method | Purpose | Called By | Calls |
|--------|---------|-----------|-------|
| `onTabChange` | Detects Room Planner tab activation | MatTabGroup | `loadRoomPlannerData()` |
| `loadRoomPlannerData` | Initializes room planner | `onTabChange()` | `generatePlannerDays()`, `loadRoomsForPlanner()` |
| `generatePlannerDays` | Creates days array for current month | `loadRoomPlannerData()` | - |
| `loadRoomsForPlanner` | Fetches rooms from API | `loadRoomPlannerData()` | `dashboardService.getRoomsByProperty()` |
| `processBookingsForPlanner` | Converts events to booking maps | Subscribe callback | `addBookingBar()` |
| `addBookingBar` | Calculates grid position for booking | `processBookingsForPlanner()` | - |
| `getBookingBars` | Returns booking bars for room | Template (*ngFor) | - |
| `onRoomDayClick` | Opens dialog for day click | Template (click) | `dialog.open()` |
| `onBookingBarClick` | Opens dialog for bar click | Template (click) | `dialog.open()` |
| `isToday` | Checks if date is today | Template [class.today] | - |
| `isWeekend` | Checks if date is weekend | Template [class.weekend] | - |
| `navigateMonth` | Changes displayed month | Template (click) | `loadRoomPlannerData()` |
| `goToToday` | Resets to current month | Template (click) | `loadRoomPlannerData()` |
| `refreshRoomPlanner` | Reloads data | Template (click) | `loadRoomPlannerData()` |
| `calculateOccupancyStats` | Computes occupancy metrics | Subscribe callback | `getRoomStatus()` |

---

## Troubleshooting History

### Issues Encountered & Resolved

#### Issue 1: MongoDB Sample Data Script Errors
**Symptom**: NumberLong syntax warnings, duplicate key errors

**Cause**: 
- Deprecated NumberLong(1001) format
- Script re-runs without cleanup

**Solution**:
- Changed to NumberLong("1001") string format
- Added cleanup code with deleteMany

**Files**: SampleData_RoomPlanner.js

---

#### Issue 2: API Returning Empty Array
**Symptom**: 200 OK response but 0 rooms returned

**Root Causes (Multiple)**:
1. Backend MongoDB query $or logic error
2. Wrong collection name ("rooms" vs "Room")
3. Wrong Property foreign key ("PropertyId" vs "_id")
4. C# BsonDocument mapping throwing exceptions on null fields

**Solutions**:
1. Fixed $or/$and logic to use BsonArray with proper nesting
2. Changed MongoCollections.RoomsCollection → MongoCollections.RoomCollection
3. Updated $lookup foreignField from "PropertyId" to "_id"
4. Implemented GetNullableInt() and GetNullableDecimal() helpers

**Files**: 
- GetRoomsMongoQuery.cs
- GetRoomsByPropertyQueryHandler.cs
- MongoCollections.cs

---

#### Issue 3: Property Name Not Showing
**Symptom**: propertyName always "Unknown Property"

**Cause**: $lookup and $project using wrong field names
- Property collection uses `Name` not `PropertyName`
- Property collection uses `_id` not `PropertyId`

**Solution**:
- Changed $lookup foreignField to "_id"
- Changed $project to extract "$propertyInfo.Name"

**Files**: GetRoomsMongoQuery.cs

---

#### Issue 4: Frontend Default Property Selection
**Symptom**: Room Planner empty when no properties selected in localStorage

**Cause**: Frontend sent empty array [], backend returned all rooms or nothing

**Solution**: Added default fallback to PropertyId: [1]

**Files**: dashboard1.component.ts

---

### Debugging Tools Created

1. **Test_AggregationPipeline.js**
   - Tests MongoDB aggregation directly in Compass
   - Stage-by-stage debugging
   - Verifies query works before testing C# code

2. **Debug Logging**
   - Added Console.WriteLine throughout query handler
   - Logs collection counts, pipeline JSON, results

---

## Testing & Debugging

### Manual Testing Checklist

```
✓ Backend
  ✓ API endpoint returns 200 OK
  ✓ Query parameters parsed correctly
  ✓ MongoDB aggregation executes
  ✓ All 8 rooms returned for PropertyId=1
  ✓ Null fields handled properly (floor, capacity, pricePerNight)
  ✓ Property names extracted from $lookup
  ✓ Results sorted correctly

✓ Frontend
  ✓ Tab navigation triggers load
  ✓ Loading spinner displays
  ✓ Rooms render in sidebar
  ✓ Days render in header
  ✓ Booking bars appear with correct positioning
  ✓ Bar colors match booking types
  ✓ Hover effects work
  ✓ Click on bar opens dialog
  ✓ Click on empty cell opens dialog
  ✓ Today indicator highlights correct column
  ✓ Weekend shading applies
  ✓ Month navigation works
  ✓ Statistics calculate correctly

✓ Dialog
  ✓ Opens with room details
  ✓ Shows booking info if occupied
  ✓ Shows "Available" if empty
  ✓ Action buttons display correctly
  ✓ Close button works
```

### MongoDB Compass Testing

**Test Script**: Test_AggregationPipeline.js

**Usage**:
1. Open MongoDB Compass
2. Connect to database
3. Select "ListingDB"
4. Open Mongosh shell
5. Paste and run Test_AggregationPipeline.js
6. Verify 8 rooms returned

**Expected Output**:
```
🧪 Testing Room Planner Aggregation Pipeline

📊 Current Room Collection State:
   Total rooms: 11
   Rooms with PropertyId=1: 8
   Rooms with Active=true: 9
   Non-deleted rooms: 9

✅ Aggregation Result: 8 rooms found
📋 Rooms returned:
   - Room xxxco10 (R1) - PropertyId: 1
   - Room 101 (Deluxe Room 101) - PropertyId: 1
   ...
```

### Browser DevTools Debugging

**Console Logs**:
```javascript
Tab changed to index: 1, label: Room Planner
🏨 Loading Room Planner for properties: [1]
🏨 Loading rooms from backend API...
✅ Rooms loaded from API: 8 rooms
```

**Network Tab**:
```
GET /api/v1/dashboard/rooms?userId=1&propertyIds=1&activeOnly=true
Status: 200 OK
Response: Array(8) [...]
```

### API Testing with Postman/cURL

```bash
curl -X GET "http://localhost:5000/api/v1/dashboard/rooms?userId=1&propertyIds=1&activeOnly=true" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Property-Id: 1"
```

---

## Performance Considerations

### MongoDB Optimization

1. **Indexes**:
```javascript
// Recommended indexes
db.Room.createIndex({ PropertyId: 1, Active: 1, IsDeleted: 1 });
db.Room.createIndex({ RoomCode: 1 });
db.Property.createIndex({ _id: 1 });
```

2. **Aggregation Performance**:
- $match first to reduce documents early
- $lookup only after filtering
- $project only needed fields
- $sort uses indexed fields when possible

### Frontend Optimization

1. **Change Detection**:
   - Uses `ChangeDetectorRef.detectChanges()` after async updates
   - OnPush strategy considered for future

2. **Rendering**:
   - CSS Grid for efficient bar positioning
   - Sticky positioning for headers/sidebar (no JS scrolling)
   - GPU-accelerated transforms for hover effects

3. **Data Structures**:
   - Map for O(1) lookup of bookings by room-date key
   - Separate Maps for day-by-day and continuous bars

---

## Deployment Notes

### Build Configuration

**Angular**:
```bash
# Development
ng serve

# Production Build
ng build --configuration production
```

**.NET API**:
```bash
# Development
dotnet run

# Production
dotnet publish -c Release -o ./publish
```

### Environment Variables

```env
# appsettings.json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "ListingDB"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### CORS Configuration

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

---

## Future Enhancements

### Planned Features

1. **Drag & Drop Rescheduling**
   - Drag booking bars to new dates
   - Resize bars to change duration
   - Visual feedback during drag

2. **Multi-Level Bookings**
   - Stack overlapping bookings
   - Show overbooking conflicts
   - Color-code conflicts

3. **Advanced Filters**
   - Filter by room type, floor
   - Search by guest name
   - Date range selection

4. **Export/Print**
   - PDF export
   - Excel export
   - Print-friendly view

5. **Real-Time Updates**
   - SignalR integration
   - Live booking updates
   - Collaborative editing

6. **Mobile Optimization**
   - Touch-friendly controls
   - Responsive layout
   - Swipe navigation

---

## Conclusion

The Room Planner implementation demonstrates a complete full-stack solution using:
- ✅ Modern Angular 17 with standalone components
- ✅ .NET 6 with CQRS pattern (MediatR)
- ✅ MongoDB with aggregation pipelines
- ✅ Material Design UI
- ✅ Professional Gantt-chart visualization
- ✅ Responsive, interactive user experience

The system successfully handles:
- Multi-property room management
- Real-time booking visualization
- Complex MongoDB queries with joins
- Null-safe data mapping
- Interactive dialogs and navigation

---

## Document Metadata

**Version**: 1.0
**Created**: 2024
**Last Updated**: Current Session
**Author**: GitHub Copilot
**Status**: ✅ Complete Implementation

**Related Documentation**:
- ROOM_PLANNER_IMPLEMENTATION.md
- ROOM_PLANNER_GANTT_REDESIGN.md
- ROOM_PLANNER_BOOKING_DETAILS_FEATURE.md
- ROOM_PLANNER_SAMPLE_DATA_GUIDE.md
- TROUBLESHOOTING_ROOM_PLANNER.md

---

**END OF TECHNICAL IMPLEMENTATION GUIDE**
