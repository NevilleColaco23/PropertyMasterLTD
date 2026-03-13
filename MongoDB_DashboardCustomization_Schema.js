// MongoDB Collections Design for Dashboard Customization

// ============================================
// Collection: DashboardConfigurations
// ============================================
{
  "_id": ObjectId,
  "UserId": 1,
  "DashboardName": "My Custom Dashboard",
  "IsDefault": true,
  "Layout": {
    "columns": 12,  // Grid system (12-column like Bootstrap)
    "rowHeight": 80, // Height of each grid row in pixels
    "widgets": [
      {
        "widgetId": "total-properties",
        "widgetType": "kpi-card",
        "position": {
          "x": 0,      // Column start (0-11)
          "y": 0,      // Row start (0-N)
          "width": 3,  // Columns span (1-12)
          "height": 2  // Rows span (1-N)
        },
        "settings": {
          "title": "Total Properties",
          "icon": "hotel",
          "color": "#1976d2",
          "showTrend": true
        }
      },
      {
        "widgetId": "total-rooms",
        "widgetType": "kpi-card",
        "position": { "x": 3, "y": 0, "width": 3, "height": 2 },
        "settings": {
          "title": "Total Rooms",
          "icon": "meeting_room",
          "color": "#1976d2"
        }
      },
      {
        "widgetId": "bookings-chart",
        "widgetType": "chart",
        "position": { "x": 0, "y": 2, "width": 6, "height": 4 },
        "settings": {
          "chartType": "line",
          "title": "Bookings Trend",
          "dataSource": "bookings-last-30-days"
        }
      },
      {
        "widgetId": "recent-activity",
        "widgetType": "list",
        "position": { "x": 6, "y": 2, "width": 6, "height": 4 },
        "settings": {
          "title": "Recent Activity",
          "itemsToShow": 10
        }
      }
    ]
  },
  "CreatedAt": ISODate("2024-01-15T10:30:00Z"),
  "UpdatedAt": ISODate("2024-01-15T10:30:00Z")
}

// ============================================
// Collection: WidgetLibrary
// ============================================
{
  "_id": ObjectId,
  "WidgetId": "total-properties",
  "WidgetType": "kpi-card",
  "Name": "Total Properties",
  "Description": "Displays the total number of active properties",
  "Icon": "hotel",
  "Category": "KPI",
  "DefaultSettings": {
    "title": "Total Properties",
    "icon": "hotel",
    "color": "#1976d2"
  },
  "DefaultSize": {
    "width": 3,
    "height": 2
  },
  "MinSize": {
    "width": 2,
    "height": 2
  },
  "MaxSize": {
    "width": 6,
    "height": 4
  },
  "RequiredPermissions": ["dashboard.view", "properties.view"],
  "IsActive": true,
  "CreatedAt": ISODate("2024-01-15T10:30:00Z")
}

// Additional widget examples:
{
  "_id": ObjectId,
  "WidgetId": "occupancy-rate",
  "WidgetType": "kpi-card",
  "Name": "Occupancy Rate",
  "Description": "Current occupancy percentage",
  "Icon": "people",
  "Category": "KPI",
  "DefaultSize": { "width": 3, "height": 2 }
}

{
  "_id": ObjectId,
  "WidgetId": "revenue-chart",
  "WidgetType": "chart",
  "Name": "Revenue Chart",
  "Description": "Monthly revenue visualization",
  "Icon": "trending_up",
  "Category": "Analytics",
  "DefaultSize": { "width": 6, "height": 4 }
}

{
  "_id": ObjectId,
  "WidgetId": "booking-calendar",
  "WidgetType": "calendar",
  "Name": "Booking Calendar",
  "Description": "Visual calendar of bookings",
  "Icon": "calendar_month",
  "Category": "Bookings",
  "DefaultSize": { "width": 12, "height": 6 }
}

// ============================================
// Collection: DashboardTemplates
// ============================================
{
  "_id": ObjectId,
  "TemplateName": "Hotel Manager Dashboard",
  "Description": "Default dashboard for hotel managers",
  "RoleId": 2, // Manager role
  "Layout": {
    // Same structure as DashboardConfigurations.Layout
  },
  "IsPublic": true,
  "PreviewImage": "https://cdn.example.com/templates/manager-dashboard.png",
  "CreatedAt": ISODate("2024-01-15T10:30:00Z")
}

// ============================================
// Indexes for Performance
// ============================================

// DashboardConfigurations indexes
db.DashboardConfigurations.createIndex({ "UserId": 1, "IsDefault": 1 });
db.DashboardConfigurations.createIndex({ "UserId": 1, "DashboardName": 1 });

// WidgetLibrary indexes
db.WidgetLibrary.createIndex({ "WidgetId": 1 }, { unique: true });
db.WidgetLibrary.createIndex({ "Category": 1, "IsActive": 1 });

// DashboardTemplates indexes
db.DashboardTemplates.createIndex({ "RoleId": 1 });
db.DashboardTemplates.createIndex({ "IsPublic": 1 });

// ============================================
// Sample Data - Seed Script
// ============================================

// Insert default widget library
db.WidgetLibrary.insertMany([
  {
    WidgetId: "total-properties",
    WidgetType: "kpi-card",
    Name: "Total Properties",
    Description: "Displays total active properties",
    Icon: "hotel",
    Category: "KPI",
    DefaultSettings: { title: "Total Properties", icon: "hotel", color: "#1976d2" },
    DefaultSize: { width: 3, height: 2 },
    MinSize: { width: 2, height: 2 },
    MaxSize: { width: 6, height: 4 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "total-rooms",
    WidgetType: "kpi-card",
    Name: "Total Rooms",
    Description: "Displays total available rooms",
    Icon: "meeting_room",
    Category: "KPI",
    DefaultSettings: { title: "Total Rooms", icon: "meeting_room", color: "#1976d2" },
    DefaultSize: { width: 3, height: 2 },
    MinSize: { width: 2, height: 2 },
    MaxSize: { width: 6, height: 4 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "bookings-today",
    WidgetType: "kpi-card",
    Name: "Bookings Today",
    Description: "Check-ins and check-outs for today",
    Icon: "event_available",
    Category: "KPI",
    DefaultSettings: { title: "Bookings Today", icon: "event_available", color: "#1976d2" },
    DefaultSize: { width: 3, height: 2 },
    MinSize: { width: 2, height: 2 },
    MaxSize: { width: 6, height: 4 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "occupancy-rate",
    WidgetType: "kpi-card",
    Name: "Occupancy Rate",
    Description: "Current occupancy percentage",
    Icon: "people",
    Category: "KPI",
    DefaultSettings: { title: "Occupancy Rate", icon: "people", color: "#1976d2" },
    DefaultSize: { width: 3, height: 2 },
    MinSize: { width: 2, height: 2 },
    MaxSize: { width: 6, height: 4 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  }
]);

print("✅ Widget library seeded successfully!");
