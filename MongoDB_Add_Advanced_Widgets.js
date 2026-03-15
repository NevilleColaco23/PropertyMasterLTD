// ============================================
// MongoDB Script: Add 8 New Widget Types
// Run this in MongoDB Compass MongoSH tab
// ============================================

use PropertyMaster;

print("\n🎨 Adding 8 New Advanced Widgets to WidgetLibrary...\n");

const newWidgets = [
  // 1. Table Widget - Properties Table
  {
    WidgetId: "properties-table",
    WidgetType: "table",
    Name: "Properties Table",
    Description: "Sortable and filterable table of all properties with pagination",
    Icon: "table_chart",
    Category: "Properties",
    DefaultSettings: {
      title: "Properties",
      pageSize: 10,
      sortBy: "PropertyName",
      columns: ["PropertyName", "Location", "TotalRooms", "IsActive"]
    },
    DefaultSize: { width: 12, height: 6 },
    MinSize: { width: 6, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // 2. Map Widget - Property Locations
  {
    WidgetId: "property-map",
    WidgetType: "map",
    Name: "Property Map",
    Description: "Interactive map showing all property locations with markers",
    Icon: "map",
    Category: "Location",
    DefaultSettings: {
      title: "Property Locations",
      zoom: 12,
      mapType: "roadmap",
      showClusters: true,
      centerLat: 0,
      centerLng: 0
    },
    DefaultSize: { width: 6, height: 6 },
    MinSize: { width: 4, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // 3. Gauge Widget - Revenue vs Target
  {
    WidgetId: "revenue-gauge",
    WidgetType: "gauge",
    Name: "Revenue Meter",
    Description: "Visual gauge showing current revenue against monthly target",
    Icon: "speed",
    Category: "KPI",
    DefaultSettings: {
      title: "Revenue vs Target",
      min: 0,
      max: 100000,
      target: 80000,
      unit: "currency",
      threshold: 70000
    },
    DefaultSize: { width: 3, height: 3 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 4, height: 4 },
    RequiredPermissions: ["dashboard.view", "analytics.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // 4. Pie Chart - Room Type Distribution
  {
    WidgetId: "room-distribution",
    WidgetType: "chart",
    Name: "Room Distribution",
    Description: "Pie chart showing distribution of room types across properties",
    Icon: "pie_chart",
    Category: "Analytics",
    DefaultSettings: {
      title: "Room Types",
      chartType: "pie",
      dataSource: "room-types-summary",
      showLegend: true
    },
    DefaultSize: { width: 4, height: 4 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 6, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // 5. Tasks Widget - Maintenance Tasks
  {
    WidgetId: "maintenance-tasks",
    WidgetType: "tasks",
    Name: "Maintenance Tasks",
    Description: "List of pending and completed maintenance tasks with priorities",
    Icon: "task_alt",
    Category: "Operations",
    DefaultSettings: {
      title: "Pending Tasks",
      showCompleted: false,
      sortBy: "priority",
      filterBy: "all"
    },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view", "maintenance.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // 6. Multi-Stats Widget - Financial Summary
  {
    WidgetId: "financial-summary",
    WidgetType: "stats",
    Name: "Financial Overview",
    Description: "Multi-metric card showing revenue, expenses, profit and growth",
    Icon: "account_balance",
    Category: "Finance",
    DefaultSettings: {
      title: "Financial Overview",
      metrics: ["revenue", "expenses", "profit", "growth"],
      timeframe: "month",
      showTrends: true
    },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view", "finance.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // 7. Progress Widget - Monthly Targets
  {
    WidgetId: "monthly-targets",
    WidgetType: "progress",
    Name: "Monthly Targets",
    Description: "Progress bars showing completion of monthly KPIs and targets",
    Icon: "trending_up",
    Category: "KPI",
    DefaultSettings: {
      title: "Monthly Progress",
      showPercentage: true,
      targets: ["bookings", "revenue", "occupancy"],
      colorScheme: "success"
    },
    DefaultSize: { width: 4, height: 4 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 6, height: 5 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // 8. Timeline Widget - Booking Timeline
  {
    WidgetId: "booking-timeline",
    WidgetType: "timeline",
    Name: "Booking Timeline",
    Description: "Visual timeline of bookings showing check-ins and check-outs",
    Icon: "timeline",
    Category: "Bookings",
    DefaultSettings: {
      title: "Booking Timeline",
      timeRange: "week",
      groupBy: "property",
      showLegend: true
    },
    DefaultSize: { width: 12, height: 5 },
    MinSize: { width: 8, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  }
];

// Insert new widgets (skip if already exists)
let insertedCount = 0;
let skippedCount = 0;
let errors = [];

newWidgets.forEach(widget => {
  try {
    const existing = db.WidgetLibrary.findOne({ WidgetId: widget.WidgetId });
    if (!existing) {
      db.WidgetLibrary.insertOne(widget);
      insertedCount++;
      print(`  ✅ Added: ${widget.Name} (${widget.WidgetId})`);
    } else {
      skippedCount++;
      print(`  ⏭️  Skipped (exists): ${widget.Name}`);
    }
  } catch (error) {
    errors.push({ widget: widget.Name, error: error.message });
    print(`  ❌ Error adding ${widget.Name}: ${error.message}`);
  }
});

print("\n" + "=".repeat(60));
print("📊 SUMMARY");
print("=".repeat(60));
print(`   ✅ Successfully Inserted: ${insertedCount} widgets`);
print(`   ⏭️  Skipped (already exist): ${skippedCount} widgets`);
print(`   ❌ Errors: ${errors.length}`);
print(`   📦 Total Attempted: ${newWidgets.length} widgets`);
print("=".repeat(60) + "\n");

if (errors.length > 0) {
  print("❌ ERRORS:");
  errors.forEach(err => {
    print(`   - ${err.widget}: ${err.error}`);
  });
  print("");
}

// Show all active widgets by category
print("📚 ALL ACTIVE WIDGETS BY CATEGORY:\n");

const categories = db.WidgetLibrary.distinct("Category", { IsActive: true });

categories.forEach(category => {
  print(`\n📁 ${category}:`);
  const widgets = db.WidgetLibrary.find({ 
    IsActive: true, 
    Category: category 
  }).sort({ Name: 1 });
  
  widgets.forEach(w => {
    const size = `${w.DefaultSize.width}x${w.DefaultSize.height}`;
    print(`   - ${w.Name.padEnd(30)} | ${w.WidgetType.padEnd(10)} | ${size.padEnd(8)} | ${w.WidgetId}`);
  });
});

// Total count
const totalActive = db.WidgetLibrary.countDocuments({ IsActive: true });
print(`\n📈 Total Active Widgets: ${totalActive}\n`);

// Verify the new widgets
print("🔍 VERIFICATION - Newly Added Widgets:");
newWidgets.forEach(widget => {
  const found = db.WidgetLibrary.findOne({ WidgetId: widget.WidgetId });
  if (found) {
    print(`   ✅ ${widget.WidgetId} - FOUND in database`);
  } else {
    print(`   ❌ ${widget.WidgetId} - NOT FOUND`);
  }
});

print("\n✨ Widget library update complete!\n");
print("💡 TIP: Restart your backend API to reload the widget library cache.\n");
