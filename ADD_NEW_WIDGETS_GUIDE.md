# 📦 How to Add New Widgets - Complete Guide

## 📊 Where Widgets Are Stored

### Two MongoDB Collections:

#### 1. **WidgetLibrary** Collection
**Purpose**: Defines all AVAILABLE widgets (the widget catalog)
**Location**: `PropertyMaster.WidgetLibrary`

**Structure**:
```javascript
{
  "_id": ObjectId,
  "WidgetId": "property-map",           // Unique identifier
  "WidgetType": "map",                  // Type: kpi-card, chart, list, calendar, map, table, etc.
  "Name": "Property Map",               // Display name
  "Description": "Interactive property location map",
  "Icon": "map",                        // Material icon name
  "Category": "Location",               // Category for grouping
  "DefaultSettings": {                  // Widget-specific settings
    "zoom": 12,
    "mapType": "roadmap"
  },
  "DefaultSize": {                      // Default grid size
    "width": 6,
    "height": 4
  },
  "MinSize": {                          // Minimum allowed size
    "width": 4,
    "height": 3
  },
  "MaxSize": {                          // Maximum allowed size
    "width": 12,
    "height": 8
  },
  "RequiredPermissions": [              // Security permissions
    "dashboard.view",
    "properties.view"
  ],
  "IsActive": true,                     // Enable/disable widget
  "CreatedAt": ISODate("2024-01-15T10:30:00Z")
}
```

#### 2. **DashboardConfigurations** Collection
**Purpose**: Stores USER'S dashboard layouts (which widgets they've added)
**Location**: `PropertyMaster.DashboardConfigurations`

**Structure**:
```javascript
{
  "_id": ObjectId,
  "UserId": 1,
  "DashboardName": "My Dashboard",
  "IsDefault": true,
  "Layout": {
    "columns": 12,
    "rowHeight": 80,
    "widgets": [                        // USER'S SELECTED WIDGETS
      {
        "widgetId": "property-map",     // References WidgetLibrary.WidgetId
        "widgetType": "map",
        "position": {
          "x": 0,                       // Grid position
          "y": 2,
          "width": 6,
          "height": 4
        },
        "settings": {                   // User's customized settings
          "zoom": 15,
          "mapType": "satellite"
        }
      }
    ]
  },
  "CreatedAt": ISODate("2024-01-15T10:30:00Z"),
  "UpdatedAt": ISODate("2024-01-15T10:30:00Z")
}
```

---

## 🎯 Currently Available Widgets

### KPI Cards (4 widgets)
- ✅ `total-properties` - Total Properties count
- ✅ `total-rooms` - Total Rooms count
- ✅ `bookings-today` - Today's bookings
- ✅ `occupancy-rate` - Current occupancy percentage

### Charts (2 widgets)
- ✅ `revenue-chart` - Revenue trend (line chart)
- ✅ `bookings-chart` - Booking trends (bar chart)

### Lists (3 widgets)
- ✅ `recent-activity` - Recent system activity
- ✅ `recent-bookings` - Latest bookings
- ✅ `notifications-list` - System notifications

### Calendar (1 widget)
- ✅ `booking-calendar` - Booking calendar view

**Total**: 10 widgets

---

## ➕ How to Add New Widgets

### Step 1: Add Widget to MongoDB WidgetLibrary

Create a MongoDB script to add new widgets:

```javascript
// MongoDB_Add_New_Widgets.js
use PropertyMaster;

print("🎨 Adding new widgets to library...\n");

const newWidgets = [
  // Example 1: Table Widget
  {
    WidgetId: "properties-table",
    WidgetType: "table",
    Name: "Properties Table",
    Description: "Sortable table of all properties",
    Icon: "table_chart",
    Category: "Properties",
    DefaultSettings: {
      title: "Properties",
      pageSize: 10,
      sortBy: "name",
      columns: ["name", "location", "rooms", "status"]
    },
    DefaultSize: { width: 12, height: 6 },
    MinSize: { width: 6, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // Example 2: Map Widget
  {
    WidgetId: "property-map",
    WidgetType: "map",
    Name: "Property Map",
    Description: "Interactive map showing property locations",
    Icon: "map",
    Category: "Location",
    DefaultSettings: {
      title: "Property Locations",
      zoom: 12,
      mapType: "roadmap",
      showClusters: true
    },
    DefaultSize: { width: 6, height: 6 },
    MinSize: { width: 4, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // Example 3: Gauge/Meter Widget
  {
    WidgetId: "revenue-gauge",
    WidgetType: "gauge",
    Name: "Revenue Meter",
    Description: "Gauge showing revenue vs target",
    Icon: "speed",
    Category: "KPI",
    DefaultSettings: {
      title: "Revenue vs Target",
      min: 0,
      max: 100000,
      target: 80000,
      unit: "currency"
    },
    DefaultSize: { width: 3, height: 3 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 4, height: 4 },
    RequiredPermissions: ["dashboard.view", "analytics.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // Example 4: Image/Gallery Widget
  {
    WidgetId: "property-gallery",
    WidgetType: "gallery",
    Name: "Property Gallery",
    Description: "Image carousel of featured properties",
    Icon: "photo_library",
    Category: "Media",
    DefaultSettings: {
      title: "Featured Properties",
      autoplay: true,
      interval: 5000,
      showThumbnails: true
    },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // Example 5: Stats/Metrics Widget
  {
    WidgetId: "financial-summary",
    WidgetType: "stats",
    Name: "Financial Summary",
    Description: "Multi-metric financial overview",
    Icon: "account_balance",
    Category: "Finance",
    DefaultSettings: {
      title: "Financial Overview",
      metrics: ["revenue", "expenses", "profit", "growth"],
      timeframe: "month"
    },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view", "finance.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // Example 6: Weather Widget
  {
    WidgetId: "location-weather",
    WidgetType: "weather",
    Name: "Weather",
    Description: "Current weather at property locations",
    Icon: "wb_sunny",
    Category: "Information",
    DefaultSettings: {
      title: "Weather",
      units: "metric",
      showForecast: true
    },
    DefaultSize: { width: 3, height: 3 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 4, height: 4 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // Example 7: Tasks/To-Do Widget
  {
    WidgetId: "maintenance-tasks",
    WidgetType: "tasks",
    Name: "Maintenance Tasks",
    Description: "Pending maintenance and tasks",
    Icon: "task_alt",
    Category: "Operations",
    DefaultSettings: {
      title: "Pending Tasks",
      showCompleted: false,
      sortBy: "priority"
    },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view", "maintenance.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // Example 8: Pie Chart Widget
  {
    WidgetId: "room-distribution",
    WidgetType: "chart",
    Name: "Room Distribution",
    Description: "Pie chart of room types",
    Icon: "pie_chart",
    Category: "Analytics",
    DefaultSettings: {
      title: "Room Types",
      chartType: "pie",
      dataSource: "room-types-summary"
    },
    DefaultSize: { width: 4, height: 4 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 6, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  }
];

// Insert widgets (skip if already exists)
let insertedCount = 0;
let skippedCount = 0;

newWidgets.forEach(widget => {
  const existing = db.WidgetLibrary.findOne({ WidgetId: widget.WidgetId });
  if (!existing) {
    db.WidgetLibrary.insertOne(widget);
    insertedCount++;
    print(`  ✅ Added: ${widget.Name} (${widget.WidgetId})`);
  } else {
    skippedCount++;
    print(`  ⏭️  Skipped (exists): ${widget.Name}`);
  }
});

print(`\n📊 Summary:`);
print(`   ✅ Inserted: ${insertedCount}`);
print(`   ⏭️  Skipped: ${skippedCount}`);
print(`   📦 Total: ${newWidgets.length}\n`);

// Show all widgets
print("📚 All Available Widgets:");
db.WidgetLibrary.find({ IsActive: true }).forEach(w => {
  print(`   - ${w.Name} (${w.WidgetType})`);
});
```

**Run this in MongoDB Compass**:
1. Open MongoDB Compass
2. Connect to your database
3. Click **_MONGOSH** tab at bottom
4. Paste the script
5. Press Enter

---

### Step 2: Create Angular Widget Component

Create the new widget component (example for Table widget):

```bash
cd app/src/app/widgets
ng generate component table-widget --standalone
```

**File: `table-widget.component.ts`**
```typescript
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';

export interface TableWidgetData {
  title: string;
  columns: string[];
  data: any[];
}

@Component({
  selector: 'app-table-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatCardModule
  ],
  template: `
    <mat-card class="widget-card table-widget">
      <mat-card-header>
        <mat-card-title>{{ data?.title || 'Table' }}</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <table mat-table [dataSource]="data?.data || []">
          <!-- Dynamic columns -->
          <ng-container *ngFor="let column of data?.columns || []" [matColumnDef]="column">
            <th mat-header-cell *matHeaderCellDef>{{ column }}</th>
            <td mat-cell *matCellDef="let row">{{ row[column] }}</td>
          </ng-container>
          
          <tr mat-header-row *matHeaderRowDef="data?.columns || []"></tr>
          <tr mat-row *matRowDef="let row; columns: data?.columns || [];"></tr>
        </table>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .table-widget {
      height: 100%;
      display: flex;
      flex-direction: column;
    }
    mat-card-content {
      flex: 1;
      overflow: auto;
    }
  `]
})
export class TableWidgetComponent {
  @Input() data: TableWidgetData | null = null;
}
```

---

### Step 3: Add Backend Data API

**File: `classfiles/Application/Dashboard/Queries/DashboardTableQueries.cs`**
```csharp
using MediatR;

namespace MyWarehouse.Application.Dashboard.Queries
{
    public class GetTableDataQuery : IRequest<TableDataResponse>
    {
        public string WidgetId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class TableDataResponse
    {
        public string WidgetId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<string> Columns { get; set; } = new();
        public List<Dictionary<string, object>> Data { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
```

**File: `classfiles/Application/Dashboard/Queries/DashboardTableQueryHandlers.cs`**
```csharp
using MediatR;
using MongoDB.Driver;

namespace MyWarehouse.Application.Dashboard.Queries
{
    public class GetTableDataQueryHandler : IRequestHandler<GetTableDataQuery, TableDataResponse>
    {
        private readonly IMongoDatabase _database;

        public GetTableDataQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<TableDataResponse> Handle(GetTableDataQuery request, CancellationToken cancellationToken)
        {
            // Example: Get properties data
            if (request.WidgetId == "properties-table")
            {
                var collection = _database.GetCollection<Property>(MongoCollections.PropertiesCollection);
                
                var filter = Builders<Property>.Filter.Eq(p => p.UserId, request.UserId);
                var totalCount = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
                
                var properties = await collection.Find(filter)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Limit(request.PageSize)
                    .ToListAsync(cancellationToken);

                return new TableDataResponse
                {
                    WidgetId = request.WidgetId,
                    Title = "Properties",
                    Columns = new List<string> { "name", "location", "rooms", "status" },
                    Data = properties.Select(p => new Dictionary<string, object>
                    {
                        ["name"] = p.PropertyName,
                        ["location"] = p.Location,
                        ["rooms"] = p.TotalRooms,
                        ["status"] = p.IsActive ? "Active" : "Inactive"
                    }).ToList(),
                    TotalCount = (int)totalCount
                };
            }

            return new TableDataResponse();
        }
    }
}
```

---

### Step 4: Add Controller Endpoint

**File: `WebApi/API/V1/DashboardController.cs`**
```csharp
/// <summary>
/// Get table widget data
/// </summary>
[HttpGet("table/{widgetId}")]
[ProducesResponseType(typeof(TableDataResponse), 200)]
public async Task<ActionResult<TableDataResponse>> GetTableData(
    string widgetId, 
    [FromQuery] int userId,
    [FromQuery] int pageSize = 10,
    [FromQuery] int pageNumber = 1)
{
    var result = await _mediator.Send(new GetTableDataQuery 
    { 
        WidgetId = widgetId,
        UserId = userId,
        PageSize = pageSize,
        PageNumber = pageNumber
    });

    return Ok(result);
}
```

---

### Step 5: Update Frontend Dashboard Component

**File: `app/src/app/dashboard/dashboard1/dashboard1.component.ts`**

Add import:
```typescript
import { TableWidgetComponent, TableWidgetData } from '../../widgets/table-widget/table-widget.component';
```

Add to imports array:
```typescript
imports: [
  // ... existing imports
  TableWidgetComponent
],
```

Add to template (in the gridster):
```html
<!-- Table Widget -->
<app-table-widget 
  *ngIf="item.widgetType === 'table'" 
  [data]="item.data">
</app-table-widget>
```

Add to `loadWidgetRealData()`:
```typescript
switch (item.widgetType) {
  // ... existing cases
  case 'table':
    this.loadTableWidgetData(item, userId);
    break;
}
```

Add loader method:
```typescript
loadTableWidgetData(item: DashboardGridsterItem, userId: number): void {
  this.dashboardService.getTableData(item.widgetId, userId, 10, 1).pipe(
    catchError(error => {
      console.error('Error loading table data:', error);
      return of({ widgetId: item.widgetId, title: 'Table', columns: [], data: [], totalCount: 0 });
    })
  ).subscribe(response => {
    item.data = {
      title: response.title,
      columns: response.columns,
      data: response.data
    } as TableWidgetData;
    this.cdr.detectChanges();
  });
}
```

---

### Step 6: Add Service Method

**File: `app/src/app/services/dashboard.service.ts`**

Add interface:
```typescript
export interface TableDataResponse {
  widgetId: string;
  title: string;
  columns: string[];
  data: any[];
  totalCount: number;
}
```

Add method:
```typescript
/**
 * Get table widget data
 */
getTableData(
  widgetId: string, 
  userId: number, 
  pageSize: number = 10,
  pageNumber: number = 1
): Observable<TableDataResponse> {
  const params = new HttpParams()
    .set('userId', userId.toString())
    .set('pageSize', pageSize.toString())
    .set('pageNumber', pageNumber.toString());
  
  return this.http.get<TableDataResponse>(
    `${this.apiUrl}/table/${widgetId}`, 
    { params }
  );
}
```

---

## 📋 Quick Reference: Widget Types

| Widget Type | Purpose | Example Use Case |
|------------|---------|------------------|
| `kpi-card` | Single metric display | Total Properties, Revenue |
| `chart` | Data visualization | Revenue trends, booking patterns |
| `list` | Vertical list of items | Recent activity, notifications |
| `calendar` | Event calendar | Booking calendar, maintenance schedule |
| `table` | Tabular data | Properties list, transactions |
| `map` | Geographic visualization | Property locations |
| `gauge` | Meter/progress indicator | Revenue vs target, capacity |
| `gallery` | Image carousel | Featured properties |
| `stats` | Multi-metric display | Financial summary |
| `weather` | Weather information | Current conditions |
| `tasks` | Task/to-do list | Maintenance tasks |

---

## 🎨 Widget Categories

Organize widgets by category for better UX:

- **KPI**: Key performance indicators
- **Analytics**: Charts and data visualization
- **Bookings**: Booking-related widgets
- **Properties**: Property management
- **Finance**: Financial metrics
- **Activity**: Recent events and notifications
- **Operations**: Tasks and maintenance
- **Location**: Maps and geographic data
- **Media**: Images and galleries
- **Information**: Weather, news, etc.

---

## ✅ Testing New Widgets

1. **Add to MongoDB**: Run MongoDB script to add widget to WidgetLibrary
2. **Verify in Database**: Check MongoDB Compass
3. **Restart Backend**: `dotnet run` (to reload widget library)
4. **Restart Frontend**: `npm start`
5. **Test Widget Picker**: Click "Add Widget" in dashboard
6. **Check Console**: Look for any errors
7. **Test Data Loading**: Verify widget shows real data
8. **Test Resize**: Drag to resize within min/max limits
9. **Test Save**: Save dashboard and reload to verify persistence

---

## 🔍 Troubleshooting

### Widget Not Appearing in Picker
- Check `IsActive: true` in MongoDB
- Verify permissions match user's roles
- Restart backend to reload widget library

### Widget Shows But No Data
- Check browser console for API errors
- Verify backend endpoint exists
- Check MongoDB data exists for user

### Widget Component Not Rendering
- Ensure component is imported in dashboard component
- Check `*ngIf` condition matches `widgetType`
- Verify template syntax

---

## 📦 Current Storage Summary

```
MongoDB Database: PropertyMaster
├── WidgetLibrary (10 widgets) ← Widget catalog/templates
└── DashboardConfigurations ← User's saved dashboards
    ├── User 1's dashboards
    │   ├── Default Dashboard (widgets: [kpi1, chart1, list1])
    │   └── Analytics Dashboard (widgets: [chart1, chart2, gauge1])
    └── User 2's dashboards
        └── Default Dashboard (widgets: [kpi2, calendar1])
```

**Flow**:
1. Admin adds widgets to **WidgetLibrary** ← Available widgets
2. User picks widgets from library ← Widget Picker dialog
3. User arranges widgets on dashboard ← Gridster drag-drop
4. User saves dashboard ← Stored in **DashboardConfigurations**
5. Backend loads user's dashboard ← GET /api/v1/dashboard/user/{userId}
6. Frontend renders saved widgets ← Loads real data from APIs

---

Would you like me to create the MongoDB script with specific new widgets you want to add?
