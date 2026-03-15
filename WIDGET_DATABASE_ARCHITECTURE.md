# 🗄️ Widget Database Architecture - Visual Guide

## 📊 Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        MongoDB Database                              │
│                      PropertyMaster                                  │
└─────────────────────────────────────────────────────────────────────┘
                                │
                ┌───────────────┴───────────────┐
                │                               │
                ▼                               ▼
┌───────────────────────────┐   ┌──────────────────────────────┐
│   WidgetLibrary           │   │  DashboardConfigurations     │
│   (Widget Catalog)        │   │  (User's Dashboards)         │
├───────────────────────────┤   ├──────────────────────────────┤
│ • Widget definitions      │   │ • UserId                     │
│ • Default settings        │   │ • DashboardName              │
│ • Size constraints        │   │ • IsDefault                  │
│ • Permissions             │   │ • Layout (columns, widgets)  │
│ • Categories              │   │   └─> Widget positions       │
│                           │   │      └─> Custom settings     │
│ Currently: ~18 widgets    │   │                              │
└───────────────────────────┘   └──────────────────────────────┘
                │                               │
                │                               │
                ▼                               ▼
        ┌──────────────┐              ┌──────────────┐
        │  GET /api/   │              │  GET /api/   │
        │  widgets     │              │  user/{id}   │
        └──────────────┘              └──────────────┘
                │                               │
                └───────────────┬───────────────┘
                                │
                                ▼
                    ┌───────────────────────┐
                    │   Angular Frontend    │
                    │   Dashboard Component │
                    └───────────────────────┘
                                │
                ┌───────────────┼───────────────┐
                ▼               ▼               ▼
        ┌──────────┐    ┌──────────┐    ┌──────────┐
        │ Widget   │    │ Gridster │    │ Widget   │
        │ Picker   │    │ Grid     │    │ Data     │
        │ Dialog   │    │ Layout   │    │ Loader   │
        └──────────┘    └──────────┘    └──────────┘
```

---

## 🏗️ Collection Schemas

### 1. WidgetLibrary Collection

```javascript
{
  // Metadata
  "_id": ObjectId("..."),                    // MongoDB auto-generated
  "WidgetId": "property-map",                // Unique identifier (key)
  "WidgetType": "map",                       // Component type
  "Name": "Property Map",                    // Display name
  "Description": "Interactive map...",       // Description
  "Icon": "map",                             // Material icon
  "Category": "Location",                    // Grouping
  
  // Configuration
  "DefaultSettings": {                       // Initial settings
    "zoom": 12,
    "mapType": "roadmap"
  },
  
  // Grid Constraints
  "DefaultSize": { "width": 6, "height": 6 },  // Initial size
  "MinSize": { "width": 4, "height": 4 },      // Minimum allowed
  "MaxSize": { "width": 12, "height": 8 },     // Maximum allowed
  
  // Security
  "RequiredPermissions": [                   // Access control
    "dashboard.view",
    "properties.view"
  ],
  
  // Status
  "IsActive": true,                          // Show in picker?
  "CreatedAt": ISODate("2024-01-15T10:30:00Z")
}
```

**Purpose**: Define what widgets are AVAILABLE in the system

**Count**: Currently ~18 widgets (10 original + 8 new)

**Key Fields**:
- `WidgetId` - Referenced by dashboard configurations
- `WidgetType` - Maps to Angular component selector
- `DefaultSettings` - Initial configuration for new instances
- `IsActive` - Control visibility in widget picker

---

### 2. DashboardConfigurations Collection

```javascript
{
  // Identity
  "_id": ObjectId("..."),                    // MongoDB auto-generated
  "UserId": 1,                               // Owner
  "DashboardName": "My Dashboard",           // User-defined name
  "IsDefault": true,                         // Load on startup?
  
  // Layout Definition
  "Layout": {
    "columns": 12,                           // Grid columns (Bootstrap-style)
    "rowHeight": 80,                         // Pixels per row
    
    "widgets": [                             // User's widget instances
      {
        // Widget Reference
        "widgetId": "property-map",          // ← References WidgetLibrary.WidgetId
        "widgetType": "map",                 // ← References WidgetLibrary.WidgetType
        
        // Grid Position
        "position": {
          "x": 0,                            // Column start (0-11)
          "y": 2,                            // Row start
          "width": 6,                        // Columns span
          "height": 6                        // Rows span
        },
        
        // User Customizations
        "settings": {                        // Overrides DefaultSettings
          "zoom": 15,                        // User changed from 12 to 15
          "mapType": "satellite"             // User changed from roadmap
        }
      },
      {
        "widgetId": "total-properties",      // Another widget instance
        "widgetType": "kpi-card",
        "position": { "x": 6, "y": 0, "width": 3, "height": 2 },
        "settings": {}                       // Using defaults
      }
      // ... more widgets
    ]
  },
  
  // Timestamps
  "CreatedAt": ISODate("2024-01-15T10:30:00Z"),
  "UpdatedAt": ISODate("2024-01-20T14:45:00Z")
}
```

**Purpose**: Store EACH USER'S dashboard layouts

**Count**: One or more per user (multi-dashboard support)

**Key Fields**:
- `UserId` - Who owns this dashboard
- `DashboardName` - User-defined name ("Analytics", "Overview", etc.)
- `IsDefault` - Which dashboard loads first
- `widgets[]` - Array of widget instances with positions and settings

---

## 🔄 Relationship Between Collections

```
WidgetLibrary (Template)          DashboardConfigurations (Instance)
┌────────────────────────┐        ┌──────────────────────────────┐
│ WidgetId: "property-map"│◄──────│ widgetId: "property-map"     │
│ WidgetType: "map"       │        │ widgetType: "map"            │
│ DefaultSettings: {      │        │ position: { x: 0, y: 2 }     │
│   zoom: 12,             │        │ settings: {                  │
│   mapType: "roadmap"    │        │   zoom: 15,  ← User override │
│ }                       │        │   mapType: "satellite" ←──┐  │
│ DefaultSize: {          │        │ }                         │  │
│   width: 6,             │        └───────────────────────────┼──┘
│   height: 6             │                                    │
│ }                       │        User customized the widget  │
└────────────────────────┘         based on template          ▼
                                                    Template provides defaults
```

**Key Concept**:
- **WidgetLibrary** = Class/Template (what CAN be added)
- **DashboardConfigurations** = Instances (what IS added)

---

## 📝 Example: How It Works

### Scenario: User adds "Property Map" widget

**Step 1**: User opens Widget Picker
```
Frontend calls: GET /api/v1/dashboard/widgets
Backend queries: db.WidgetLibrary.find({ IsActive: true })
Returns: All available widgets including "property-map"
```

**Step 2**: User selects "Property Map" from picker
```
Frontend gets:
{
  WidgetId: "property-map",
  WidgetType: "map",
  DefaultSettings: { zoom: 12, mapType: "roadmap" },
  DefaultSize: { width: 6, height: 6 }
}
```

**Step 3**: Frontend adds widget to grid
```
dashboardItems.push({
  x: 0,
  y: 2,
  cols: 6,  ← From DefaultSize.width
  rows: 6,  ← From DefaultSize.height
  widgetId: "property-map",
  widgetType: "map",
  settings: { zoom: 12, mapType: "roadmap" },  ← From DefaultSettings
  data: null
});
```

**Step 4**: User drags widget and changes settings
```
New position: { x: 6, y: 4 }
New settings: { zoom: 15, mapType: "satellite" }
```

**Step 5**: User saves dashboard
```
Frontend calls: POST /api/v1/dashboard
Body: {
  userId: 1,
  dashboardName: "My Dashboard",
  layout: {
    widgets: [
      {
        widgetId: "property-map",
        widgetType: "map",
        position: { x: 6, y: 4, width: 6, height: 6 },
        settings: { zoom: 15, mapType: "satellite" }  ← User's customization
      }
    ]
  }
}

Backend stores in: db.DashboardConfigurations.insertOne(...)
```

**Step 6**: User reloads page
```
Frontend calls: GET /api/v1/dashboard/user/1
Backend queries: db.DashboardConfigurations.findOne({ 
  UserId: 1, 
  IsDefault: true 
})
Returns: User's saved dashboard with custom positions and settings
```

---

## 🎯 Widget Categories (Current)

```
PropertyMaster.WidgetLibrary
│
├─ 📊 KPI (5 widgets)
│  ├─ total-properties
│  ├─ total-rooms
│  ├─ bookings-today
│  ├─ occupancy-rate
│  └─ revenue-gauge ✨ NEW
│
├─ 📈 Analytics (4 widgets)
│  ├─ revenue-chart
│  ├─ bookings-chart
│  ├─ room-distribution ✨ NEW
│  └─ monthly-targets ✨ NEW
│
├─ 📅 Bookings (2 widgets)
│  ├─ booking-calendar
│  └─ booking-timeline ✨ NEW
│
├─ 📋 Activity (3 widgets)
│  ├─ recent-activity
│  ├─ recent-bookings
│  └─ notifications-list
│
├─ 🏢 Properties (1 widget)
│  └─ properties-table ✨ NEW
│
├─ 📍 Location (1 widget)
│  └─ property-map ✨ NEW
│
├─ 💰 Finance (1 widget)
│  └─ financial-summary ✨ NEW
│
└─ ⚙️ Operations (1 widget)
   └─ maintenance-tasks ✨ NEW

TOTAL: ~18 widgets (10 original + 8 new)
```

---

## 🔍 Query Examples

### Get all active widgets
```javascript
db.WidgetLibrary.find({ IsActive: true })
```

### Get widgets by category
```javascript
db.WidgetLibrary.find({ 
  Category: "Analytics",
  IsActive: true 
})
```

### Get user's default dashboard
```javascript
db.DashboardConfigurations.findOne({ 
  UserId: 1,
  IsDefault: true 
})
```

### Get all dashboards for user
```javascript
db.DashboardConfigurations.find({ UserId: 1 })
```

### Count widgets by type
```javascript
db.WidgetLibrary.aggregate([
  { $match: { IsActive: true } },
  { $group: { 
      _id: "$WidgetType", 
      count: { $sum: 1 } 
  }}
])
```

---

## 📊 Storage Size Estimate

### WidgetLibrary
- Documents: ~18-50 (depending on widgets added)
- Size per document: ~1-2 KB
- **Total**: ~50-100 KB (very small)

### DashboardConfigurations
- Documents: 1-10 per user
- Size per document: ~5-20 KB (depends on widget count)
- For 100 users with 2 dashboards each: ~1-4 MB
- **Total**: Scales with users

**Conclusion**: Very lightweight storage! 💾

---

## ✅ Summary

| Aspect | Details |
|--------|---------|
| **Collections** | 2 (WidgetLibrary, DashboardConfigurations) |
| **WidgetLibrary** | Template/catalog of available widgets |
| **DashboardConfigurations** | User's saved dashboard layouts |
| **Relationship** | Dashboard widgets reference WidgetLibrary by `widgetId` |
| **Current Widgets** | ~18 (expandable) |
| **Storage** | Minimal (~50 KB for widgets, scales with users) |
| **Flexibility** | High - add widgets without code changes |
| **Multi-tenant** | Yes - per-user dashboards |

---

Would you like me to create a script to view your current widget library, or help you add specific widget types?
