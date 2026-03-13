# 🎯 Phase 3: Quick Implementation Guide

## ✅ What's Already Done

1. **gridster2 installed** - npm package added
2. **GridsterConfigService created** - Helper service ready
3. **WidgetPickerDialogComponent created** - Modal ready

## 🔧 What You Need to Do

Follow these steps to complete Phase 3:

---

## Step 1: Update dashboard1.component.ts Imports

**Add these imports at the top:**

```typescript
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';
import { GridsterModule, GridsterConfig, GridsterItem } from 'angular-gridster2';
import { WidgetPickerDialogComponent } from '../../widgets/widget-picker-dialog/widget-picker-dialog.component';
import { GridsterConfigService } from '../../services/gridster-config.service';
import { SaveDashboardRequest } from '../../models/dashboard.models';
```

**Add to imports array in @Component:**

```typescript
imports: [
  // ... existing imports
  MatDialogModule,
  MatTooltipModule,
  MatBadgeModule,
  GridsterModule
]
```

---

## Step 2: Add Phase 3 Properties

**Add these properties to the class:**

```typescript
// Edit mode
editMode = false;
hasUnsavedChanges = false;

// Gridster
options: GridsterConfig;
dashboardItems: Array<GridsterItem & {
  widgetId: string;
  widgetType: string;
  settings?: any;
  data?: any;
}> = [];
```

---

## Step 3: Update Constructor

**Add MatDialog to constructor:**

```typescript
constructor(
  private router: Router,
  private dashboardService: DashboardService,
  private snackBar: MatSnackBar,
  private dialog: MatDialog  // ADD THIS
) {
  // Initialize gridster
  this.options = GridsterConfigService.getDefaultConfig(false);
}
```

---

## Step 4: Add Phase 3 Methods

**Copy these methods into your component:**

```typescript
/**
 * Toggle edit mode
 */
toggleEditMode(): void {
  if (this.editMode && this.hasUnsavedChanges) {
    const confirmExit = confirm('You have unsaved changes. Discard them?');
    if (!confirmExit) return;
  }

  this.editMode = !this.editMode;
  this.options = GridsterConfigService.getDefaultConfig(this.editMode);
  
  if (this.options.api?.optionsChanged) {
    this.options.api.optionsChanged();
  }

  if (!this.editMode) {
    this.hasUnsavedChanges = false;
  }
}

/**
 * Open widget picker
 */
openWidgetPicker(): void {
  const addedIds = this.dashboardItems.map(i => i.widgetId);
  
  const dialogRef = this.dialog.open(WidgetPickerDialogComponent, {
    width: '700px',
    data: {
      availableWidgets: this.widgetLibrary,
      addedWidgetIds: addedIds
    }
  });

  dialogRef.afterClosed().subscribe((widget: any) => {
    if (widget) this.addWidget(widget);
  });
}

/**
 * Add widget
 */
addWidget(widget: any): void {
  const pos = this.findNextPosition();
  
  this.dashboardItems.push({
    x: pos.x,
    y: pos.y,
    cols: widget.defaultSize.width,
    rows: widget.defaultSize.height,
    widgetId: widget.widgetId,
    widgetType: widget.widgetType,
    settings: { ...widget.defaultSettings },
    data: this.getWidgetData(widget.widgetId, widget.widgetType)
  });

  this.hasUnsavedChanges = true;
  this.snackBar.open(`${widget.name} added`, 'Close', { duration: 2000 });
}

/**
 * Remove widget
 */
removeWidget(item: any): void {
  const name = this.widgetLibrary.find(w => w.widgetId === item.widgetId)?.name || 'Widget';
  
  if (confirm(`Remove ${name}?`)) {
    const index = this.dashboardItems.indexOf(item);
    if (index > -1) {
      this.dashboardItems.splice(index, 1);
      this.hasUnsavedChanges = true;
    }
  }
}

/**
 * Save dashboard
 */
saveDashboard(): void {
  this.loading = true;

  const widgets = this.dashboardItems.map(item => 
    GridsterConfigService.toWidgetConfig(item)
  );
  
  const request: SaveDashboardRequest = {
    id: this.dashboardConfig?.id,
    userId: this.getCurrentUserId(),
    dashboardName: 'My Dashboard',
    isDefault: true,
    layout: {
      columns: 12,
      rowHeight: 80,
      widgets
    }
  };

  this.dashboardService.saveDashboard(request).subscribe({
    next: (id) => {
      this.snackBar.open('Saved!', 'Close', { duration: 3000 });
      this.hasUnsavedChanges = false;
      this.editMode = false;
      this.loading = false;
      this.loadDashboard();
    },
    error: (err) => {
      this.snackBar.open('Save failed', 'Close', { duration: 3000 });
      this.loading = false;
    }
  });
}

/**
 * Cancel edits
 */
cancelEdit(): void {
  if (this.hasUnsavedChanges) {
    if (!confirm('Discard changes?')) return;
  }
  
  this.editMode = false;
  this.hasUnsavedChanges = false;
  this.loadDashboard();
}

/**
 * Item changed
 */
itemChange(item: GridsterItem): void {
  if (this.editMode) {
    this.hasUnsavedChanges = true;
  }
}

/**
 * Find next position
 */
private findNextPosition(): { x: number; y: number } {
  if (this.dashboardItems.length === 0) return { x: 0, y: 0 };
  
  let maxY = 0;
  this.dashboardItems.forEach(item => {
    const bottom = (item.y || 0) + (item.rows || 2);
    if (bottom > maxY) maxY = bottom;
  });
  
  return { x: 0, y: maxY };
}

/**
 * Get widget data by type
 */
getWidgetData(widgetId: string, widgetType: string): any {
  switch (widgetType) {
    case 'kpi-card':
      return {
        title: widgetId.replace(/-/g, ' ').toUpperCase(),
        value: this.getKpiValue(widgetId),
        icon: 'analytics',
        color: '#1976d2'
      };
    case 'list':
      return {
        title: 'Recent Activity',
        items: this.getListItems(widgetId)
      };
    case 'chart':
      return {
        title: 'Chart',
        labels: ['Jan', 'Feb', 'Mar'],
        datasets: [{ label: 'Data', data: [10, 20, 30] }]
      };
    case 'calendar':
      return {
        title: 'Calendar',
        events: this.getCalendarEvents()
      };
    default:
      return {};
  }
}
```

---

## Step 5: Update render Widgets Method

**Replace the existing renderWidgets method:**

```typescript
renderWidgets(config: DashboardConfiguration): void {
  this.dashboardItems = [];
  
  config.layout.widgets.forEach(widget => {
    const item: any = {
      x: widget.position.x,
      y: widget.position.y,
      cols: widget.position.width,
      rows: widget.position.height,
      widgetId: widget.widgetId,
      widgetType: widget.widgetType,
      settings: widget.settings,
      data: this.getWidgetData(widget.widgetId, widget.widgetType)
    };
    
    this.dashboardItems.push(item);
  });
}
```

---

## Step 6: Update loadDefaultKpiCards Method

**Replace loadDefaultKpiCards:**

```typescript
loadDefaultKpiCards(): void {
  this.dashboardItems = [
    {
      x: 0, y: 0, cols: 3, rows: 2,
      widgetId: 'total-properties',
      widgetType: 'kpi-card',
      data: {
        title: 'Total Properties',
        value: 24,
        icon: 'hotel',
        color: '#1976d2',
        showTrend: true,
        trendValue: 12,
        trendDirection: 'up'
      }
    },
    {
      x: 3, y: 0, cols: 3, rows: 2,
      widgetId: 'total-rooms',
      widgetType: 'kpi-card',
      data: {
        title: 'Total Rooms',
        value: 156,
        icon: 'meeting_room',
        color: '#1976d2'
      }
    },
    {
      x: 6, y: 0, cols: 3, rows: 2,
      widgetId: 'bookings-today',
      widgetType: 'kpi-card',
      data: {
        title: 'Bookings Today',
        value: 12,
        icon: 'event_available',
        color: '#1976d2'
      }
    },
    {
      x: 9, y: 0, cols: 3, rows: 2,
      widgetId: 'occupancy-rate',
      widgetType: 'kpi-card',
      data: {
        title: 'Occupancy Rate',
        value: '78%',
        icon: 'people',
        color: '#1976d2',
        showTrend: true,
        trendValue: 5,
        trendDirection: 'up'
      }
    },
    {
      x: 0, y: 2, cols: 6, rows: 4,
      widgetId: 'revenue-chart',
      widgetType: 'chart',
      data: this.getDefaultChartWidget()
    },
    {
      x: 6, y: 2, cols: 3, rows: 4,
      widgetId: 'recent-activity',
      widgetType: 'list',
      data: this.getDefaultListWidget()
    },
    {
      x: 9, y: 2, cols: 3, rows: 4,
      widgetId: 'booking-calendar',
      widgetType: 'calendar',
      data: this.getDefaultCalendarWidget()
    }
  ];
}
```

---

## ✅ Verification Checklist

After making these changes:

- [ ] File compiles without errors
- [ ] Dashboard loads with widgets in grid
- [ ] Edit mode button appears
- [ ] Can toggle edit mode on/off
- [ ] Can drag widgets (edit mode)
- [ ] Can resize widgets (edit mode)
- [ ] Add widget button appears (edit mode)
- [ ] Can remove widgets (edit mode)
- [ ] Save button works
- [ ] Changes persist after save

---

## 🐛 Common Issues

### Issue: "Cannot find GridsterModule"
**Fix**: Restart `npm start`

### Issue: "dialog is undefined"
**Fix**: Add `MatDialog` to constructor

### Issue: Widgets don't show
**Fix**: Check `dashboardItems` array in console

### Issue: Can't drag
**Fix**: Ensure `editMode = true` and gridster config updated

---

## 🎯 Next: Update HTML Template

Once the TypeScript is done, we'll update the HTML template to use gridster.

**Ready to continue?** Let me know when Step 6 is complete! 🚀
