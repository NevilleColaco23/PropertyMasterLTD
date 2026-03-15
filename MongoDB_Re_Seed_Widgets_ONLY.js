// ============================================
// RE-SEED WIDGETS ONLY
// Use this if widgets didn't insert properly
// ============================================


print("\n🧹 STEP 1: Clearing existing widgets...\n");
const deleteResult = db.WidgetLibrary.deleteMany({});
print(`   Deleted ${deleteResult.deletedCount} existing widgets\n`);

print("📦 STEP 2: Inserting fresh widget library...\n");

const widgets = [
  {
    WidgetId: "total-properties",
    WidgetType: "kpi-card",
    Name: "Total Properties",
    Description: "Total number of active properties",
    Icon: "hotel",
    Category: "KPI",
    DefaultSettings: { title: "Total Properties", icon: "hotel", color: "#1976d2" },
    DefaultSize: { width: 3, height: 2 },
    MinSize: { width: 2, height: 2 },
    MaxSize: { width: 6, height: 4 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "total-rooms",
    WidgetType: "kpi-card",
    Name: "Total Rooms",
    Description: "Total number of rooms across all properties",
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
    Description: "Number of bookings made today",
    Icon: "event_available",
    Category: "KPI",
    DefaultSettings: { title: "Bookings Today", icon: "event_available", color: "#1976d2" },
    DefaultSize: { width: 3, height: 2 },
    MinSize: { width: 2, height: 2 },
    MaxSize: { width: 6, height: 4 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "occupancy-rate",
    WidgetType: "kpi-card",
    Name: "Occupancy Rate",
    Description: "Current occupancy rate percentage",
    Icon: "people",
    Category: "KPI",
    DefaultSettings: { title: "Occupancy Rate", icon: "people", color: "#1976d2" },
    DefaultSize: { width: 3, height: 2 },
    MinSize: { width: 2, height: 2 },
    MaxSize: { width: 6, height: 4 },
    RequiredPermissions: ["dashboard.view", "analytics.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "revenue-gauge",
    WidgetType: "gauge",
    Name: "Revenue Meter",
    Description: "Visual gauge showing current revenue against monthly target",
    Icon: "speed",
    Category: "KPI",
    DefaultSettings: { title: "Revenue vs Target", min: 0, max: 100000, target: 80000, unit: "currency" },
    DefaultSize: { width: 3, height: 3 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 4, height: 4 },
    RequiredPermissions: ["dashboard.view", "analytics.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "revenue-chart",
    WidgetType: "chart",
    Name: "Revenue Chart",
    Description: "Monthly revenue visualization",
    Icon: "trending_up",
    Category: "Analytics",
    DefaultSettings: { title: "Revenue Trend", chartType: "line" },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view", "analytics.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "bookings-chart",
    WidgetType: "chart",
    Name: "Bookings Chart",
    Description: "Booking trends over time",
    Icon: "bar_chart",
    Category: "Analytics",
    DefaultSettings: { title: "Bookings Trend", chartType: "bar" },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "room-distribution",
    WidgetType: "chart",
    Name: "Room Distribution",
    Description: "Pie chart showing distribution of room types",
    Icon: "pie_chart",
    Category: "Analytics",
    DefaultSettings: { title: "Room Types", chartType: "pie", showLegend: true },
    DefaultSize: { width: 4, height: 4 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 6, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "recent-activity",
    WidgetType: "list",
    Name: "Recent Activity",
    Description: "Latest system activity and events",
    Icon: "list",
    Category: "Activity",
    DefaultSettings: { title: "Recent Activity", itemsToShow: 10 },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "recent-bookings",
    WidgetType: "list",
    Name: "Recent Bookings",
    Description: "Latest booking records",
    Icon: "receipt_long",
    Category: "Bookings",
    DefaultSettings: { title: "Recent Bookings", itemsToShow: 5 },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "notifications-list",
    WidgetType: "list",
    Name: "Notifications",
    Description: "System notifications and alerts",
    Icon: "notifications",
    Category: "Activity",
    DefaultSettings: { title: "Notifications", itemsToShow: 8 },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "booking-calendar",
    WidgetType: "calendar",
    Name: "Booking Calendar",
    Description: "Visual calendar of bookings and events",
    Icon: "calendar_month",
    Category: "Bookings",
    DefaultSettings: { title: "Bookings Calendar", view: "month" },
    DefaultSize: { width: 12, height: 6 },
    MinSize: { width: 8, height: 5 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "booking-timeline",
    WidgetType: "timeline",
    Name: "Booking Timeline",
    Description: "Visual timeline of bookings",
    Icon: "timeline",
    Category: "Bookings",
    DefaultSettings: { title: "Booking Timeline", timeRange: "week", groupBy: "property" },
    DefaultSize: { width: 12, height: 5 },
    MinSize: { width: 8, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "properties-table",
    WidgetType: "table",
    Name: "Properties Table",
    Description: "Sortable and filterable table of all properties",
    Icon: "table_chart",
    Category: "Properties",
    DefaultSettings: { title: "Properties", pageSize: 10, sortBy: "PropertyName" },
    DefaultSize: { width: 12, height: 6 },
    MinSize: { width: 6, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "property-map",
    WidgetType: "map",
    Name: "Property Map",
    Description: "Interactive map showing property locations",
    Icon: "map",
    Category: "Location",
    DefaultSettings: { title: "Property Locations", zoom: 12, mapType: "roadmap" },
    DefaultSize: { width: 6, height: 6 },
    MinSize: { width: 4, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "financial-summary",
    WidgetType: "stats",
    Name: "Financial Overview",
    Description: "Multi-metric financial summary",
    Icon: "account_balance",
    Category: "Finance",
    DefaultSettings: { title: "Financial Overview", metrics: ["revenue", "expenses", "profit", "growth"], timeframe: "month" },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view", "finance.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "maintenance-tasks",
    WidgetType: "tasks",
    Name: "Maintenance Tasks",
    Description: "Pending maintenance tasks",
    Icon: "task_alt",
    Category: "Operations",
    DefaultSettings: { title: "Pending Tasks", showCompleted: false, sortBy: "priority" },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view", "maintenance.view"],
    IsActive: true,
    CreatedAt: new Date()
  }
];

// Bulk insert
const result = db.WidgetLibrary.insertMany(widgets);
print(`   ✅ Inserted ${result.insertedIds.length} widgets\n`);

// Verification
print("🔍 VERIFICATION:\n");
const count = db.WidgetLibrary.countDocuments({});
print(`   Total widgets in database: ${count}\n`);

if (count === 17) {
  print("✅ SUCCESS! All 17 widgets inserted correctly.\n");
} else {
  print(`⚠️  WARNING: Expected 17 widgets but found ${count}\n`);
}

// List all
print("📋 All widgets:");
db.WidgetLibrary.find({}).sort({ Category: 1, Name: 1 }).forEach((w, i) => {
  print(`   ${(i+1).toString().padStart(2)}. ${w.Name.padEnd(25)} | ${w.Category.padEnd(12)} | ${w.WidgetId}`);
});

print("\n✨ Re-seed complete!\n");
