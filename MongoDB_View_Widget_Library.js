// ============================================
// MongoDB Query: View Current Widget Library
// Run this in MongoDB Compass MongoSH tab
// ============================================

use PropertyMaster;

print("\n" + "=".repeat(70));
print("📚 CURRENT WIDGET LIBRARY OVERVIEW");
print("=".repeat(70) + "\n");

// Count total widgets
const totalWidgets = db.WidgetLibrary.countDocuments({});
const activeWidgets = db.WidgetLibrary.countDocuments({ IsActive: true });
const inactiveWidgets = totalWidgets - activeWidgets;

print("📊 STATISTICS:");
print(`   Total Widgets: ${totalWidgets}`);
print(`   ✅ Active: ${activeWidgets}`);
print(`   ❌ Inactive: ${inactiveWidgets}\n`);

// Get all categories
print("📁 WIDGETS BY CATEGORY:\n");

const categories = db.WidgetLibrary.distinct("Category", { IsActive: true });

categories.sort().forEach(category => {
  const widgets = db.WidgetLibrary.find({ 
    IsActive: true, 
    Category: category 
  }).sort({ Name: 1 });
  
  const count = widgets.count();
  print(`\n${category} (${count} widgets):`);
  print("─".repeat(70));
  
  widgets.forEach(w => {
    const size = `${w.DefaultSize.width}x${w.DefaultSize.height}`;
    const permissions = w.RequiredPermissions.join(", ");
    print(`\n   Widget: ${w.Name}`);
    print(`   ID: ${w.WidgetId}`);
    print(`   Type: ${w.WidgetType}`);
    print(`   Size: ${size} (${w.MinSize.width}x${w.MinSize.height} - ${w.MaxSize.width}x${w.MaxSize.height})`);
    print(`   Icon: ${w.Icon}`);
    print(`   Permissions: ${permissions}`);
    print(`   Description: ${w.Description}`);
  });
});

// Widget types summary
print("\n" + "=".repeat(70));
print("🎨 WIDGET TYPES DISTRIBUTION:");
print("=".repeat(70) + "\n");

const typeStats = db.WidgetLibrary.aggregate([
  { $match: { IsActive: true } },
  { $group: { 
      _id: "$WidgetType", 
      count: { $sum: 1 },
      widgets: { $push: "$Name" }
  }},
  { $sort: { count: -1 } }
]).toArray();

typeStats.forEach(stat => {
  print(`${stat._id.padEnd(15)} : ${stat.count} widget(s)`);
  stat.widgets.forEach(name => {
    print(`   - ${name}`);
  });
  print("");
});

// Check for user dashboards
print("=".repeat(70));
print("👤 USER DASHBOARDS:");
print("=".repeat(70) + "\n");

const dashboardStats = db.DashboardConfigurations.aggregate([
  { $group: {
      _id: "$UserId",
      dashboardCount: { $sum: 1 },
      dashboards: { 
        $push: {
          name: "$DashboardName",
          isDefault: "$IsDefault",
          widgetCount: { $size: "$Layout.widgets" }
        }
      }
  }},
  { $sort: { _id: 1 } }
]).toArray();

if (dashboardStats.length === 0) {
  print("   ⚠️  No dashboards found. Users haven't created any dashboards yet.\n");
  print("   💡 TIP: Run 'MongoDB_Create_Default_Dashboard_For_All_Users.js' to seed defaults.\n");
} else {
  dashboardStats.forEach(userStat => {
    print(`\nUser ID ${userStat._id}: ${userStat.dashboardCount} dashboard(s)`);
    userStat.dashboards.forEach(dash => {
      const defaultBadge = dash.isDefault ? "⭐ DEFAULT" : "";
      print(`   - ${dash.name} ${defaultBadge}`);
      print(`     Widgets: ${dash.widgetCount}`);
    });
  });
  print("");
}

// Most popular widgets (most used in dashboards)
print("=".repeat(70));
print("⭐ MOST USED WIDGETS:");
print("=".repeat(70) + "\n");

const popularWidgets = db.DashboardConfigurations.aggregate([
  { $unwind: "$Layout.widgets" },
  { $group: {
      _id: "$Layout.widgets.widgetId",
      usageCount: { $sum: 1 },
      widgetType: { $first: "$Layout.widgets.widgetType" }
  }},
  { $sort: { usageCount: -1 } },
  { $limit: 10 }
]).toArray();

if (popularWidgets.length === 0) {
  print("   ⚠️  No usage data available (no dashboards exist yet).\n");
} else {
  popularWidgets.forEach((widget, index) => {
    const widgetInfo = db.WidgetLibrary.findOne({ WidgetId: widget._id });
    const name = widgetInfo ? widgetInfo.Name : widget._id;
    print(`   ${(index + 1).toString().padStart(2)}. ${name.padEnd(30)} - Used ${widget.usageCount} time(s)`);
  });
  print("");
}

// Available vs Used
print("=".repeat(70));
print("📈 WIDGET UTILIZATION:");
print("=".repeat(70) + "\n");

const usedWidgetIds = db.DashboardConfigurations.aggregate([
  { $unwind: "$Layout.widgets" },
  { $group: { _id: "$Layout.widgets.widgetId" } }
]).toArray().map(w => w._id);

const unusedWidgets = db.WidgetLibrary.find({ 
  IsActive: true,
  WidgetId: { $nin: usedWidgetIds }
}).toArray();

print(`   ✅ Widgets in use: ${usedWidgetIds.length}`);
print(`   ⚠️  Unused widgets: ${unusedWidgets.length}\n`);

if (unusedWidgets.length > 0) {
  print("   Unused widgets:");
  unusedWidgets.forEach(w => {
    print(`   - ${w.Name} (${w.WidgetId})`);
  });
  print("");
}

// Sample widget configurations
print("=".repeat(70));
print("🔍 SAMPLE WIDGET CONFIGURATIONS:");
print("=".repeat(70) + "\n");

const sampleDashboard = db.DashboardConfigurations.findOne({});
if (sampleDashboard && sampleDashboard.Layout && sampleDashboard.Layout.widgets) {
  print(`Sample from: ${sampleDashboard.DashboardName} (User ${sampleDashboard.UserId})\n`);
  
  sampleDashboard.Layout.widgets.slice(0, 3).forEach(widget => {
    const widgetInfo = db.WidgetLibrary.findOne({ WidgetId: widget.widgetId });
    print(`Widget: ${widgetInfo ? widgetInfo.Name : widget.widgetId}`);
    print(`   Position: (${widget.position.x}, ${widget.position.y})`);
    print(`   Size: ${widget.position.width}x${widget.position.height}`);
    print(`   Settings: ${JSON.stringify(widget.settings, null, 6)}`);
    print("");
  });
} else {
  print("   No dashboard configurations found.\n");
}

print("=".repeat(70));
print("✨ Report Complete!");
print("=".repeat(70) + "\n");
