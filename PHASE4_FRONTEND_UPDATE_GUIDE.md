# 🔄 Phase 4 Frontend Updates - Implementation Guide

## What We're Updating

We need to replace mock data methods with real API calls in `dashboard1.component.ts`.

## Changes Summary

### Methods to Update:
1. ✅ `getKpiCardData()` → Call real KPI API
2. ✅ `getListItems()` → Call real activity API  
3. ✅ `getCalendarEvents()` → Call real calendar API
4. ✅ `getWidgetData()` → Route to real APIs based on widget type

### New Imports Needed:
```typescript
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { KpiValueResponse, ActivityItemResponse, CalendarEventResponse } from '../../models/dashboard.models';
```

---

## Updated Methods

### 1. Update getWidgetData() Method

**Replace the existing `getWidgetData()` method with:**

```typescript
/**
 * Get widget data by type - PHASE 4: Uses real APIs
 */
getWidgetData(widgetId: string, widgetType: string): any {
  const userId = this.getCurrentUserId();
  
  switch (widgetType) {
    case 'kpi-card':
      // Return a placeholder, real data loaded via loadKpiData()
      return {
        title: this.getWidgetTitle(widgetId),
        value: 0,
        icon: this.getWidgetIcon(widgetId),
        color: '#1976d2',
        showTrend: false
      };
      
    case 'list':
      // Return placeholder, real data loaded via loadActivityData()
      return {
        title: 'Recent Activity',
        items: [],
        emptyMessage: 'Loading...'
      };
      
    case 'chart':
      return this.getDefaultChartWidget();
      
    case 'calendar':
      // Return placeholder, real data loaded via loadCalendarData()
      return {
        title: 'Booking Calendar',
        events: [],
        currentDate: new Date()
      };
      
    default:
      return {};
  }
}
```

### 2. Add Real Data Loading Methods

**Add these new methods after `loadDefaultWidgets()`:**

```typescript
// ==========================================
// PHASE 4: REAL DATA LOADING METHODS
// ==========================================

/**
 * Load real KPI data for all KPI widgets
 */
loadKpiData(): void {
  const userId = this.getCurrentUserId();
  const kpiWidgets = this.dashboardItems.filter(item => item.widgetType === 'kpi-card');
  
  if (kpiWidgets.length === 0) return;

  // Create parallel requests for all KPI widgets
  const kpiRequests = kpiWidgets.map(widget =>
    this.dashboardService.getKpiValue(widget.widgetId, userId).pipe(
      catchError(error => {
        console.error(`Error loading KPI ${widget.widgetId}:`, error);
        return of(null); // Return null on error, don't break other requests
      })
    )
  );

  forkJoin(kpiRequests).subscribe(results => {
    results.forEach((kpiData, index) => {
      if (kpiData) {
        const widget = kpiWidgets[index];
        widget.data = {
          title: this.getWidgetTitle(widget.widgetId),
          value: kpiData.value,
          icon: this.getWidgetIcon(widget.widgetId),
          color: '#1976d2',
          showTrend: kpiData.showTrend,
          trendValue: kpiData.trendValue,
          trendDirection: kpiData.trendDirection
        };
      }
    });
    
    console.log('KPI data loaded:', kpiWidgets.length);
  });
}

/**
 * Load real activity data for list widgets
 */
loadActivityData(): void {
  const userId = this.getCurrentUserId();
  const listWidgets = this.dashboardItems.filter(item => item.widgetType === 'list');
  
  if (listWidgets.length === 0) return;

  this.dashboardService.getRecentActivity(userId, 10).subscribe({
    next: (activities) => {
      listWidgets.forEach(widget => {
        widget.data = {
          title: 'Recent Activity',
          items: activities.map(activity => ({
            id: activity.id,
            icon: activity.icon,
            iconColor: activity.iconColor,
            title: activity.title,
            subtitle: activity.subtitle,
            timestamp: new Date(activity.timestamp),
            metadata: activity.metadata
          })),
          emptyMessage: 'No recent activity'
        };
      });
      
      console.log('Activity data loaded:', activities.length);
    },
    error: (error) => {
      console.error('Error loading activity data:', error);
      // Keep placeholder data on error
    }
  });
}

/**
 * Load real calendar events for calendar widgets
 */
loadCalendarData(): void {
  const userId = this.getCurrentUserId();
  const calendarWidgets = this.dashboardItems.filter(item => item.widgetType === 'calendar');
  
  if (calendarWidgets.length === 0) return;

  const today = new Date();
  const startDate = new Date(today.getFullYear(), today.getMonth(), 1); // First day of month
  const endDate = new Date(today.getFullYear(), today.getMonth() + 1, 0); // Last day of month

  this.dashboardService.getCalendarEvents(userId, startDate, endDate).subscribe({
    next: (events) => {
      calendarWidgets.forEach(widget => {
        widget.data = {
          title: 'Booking Calendar',
          events: events.map(event => ({
            id: event.id,
            title: event.title,
            start: new Date(event.start),
            end: event.end ? new Date(event.end) : undefined,
            color: event.color,
            type: event.type
          })),
          currentDate: new Date()
        };
      });
      
      console.log('Calendar data loaded:', events.length);
    },
    error: (error) => {
      console.error('Error loading calendar data:', error);
      // Keep placeholder data on error
    }
  });
}

/**
 * Get widget title by widget ID
 */
private getWidgetTitle(widgetId: string): string {
  const titles: { [key: string]: string } = {
    'total-properties': 'Total Properties',
    'total-rooms': 'Total Rooms',
    'bookings-today': 'Bookings Today',
    'occupancy-rate': 'Occupancy Rate'
  };
  return titles[widgetId] || widgetId.replace(/-/g, ' ').toUpperCase();
}

/**
 * Get widget icon by widget ID
 */
private getWidgetIcon(widgetId: string): string {
  const icons: { [key: string]: string } = {
    'total-properties': 'hotel',
    'total-rooms': 'meeting_room',
    'bookings-today': 'event_available',
    'occupancy-rate': 'people'
  };
  return icons[widgetId] || 'analytics';
}
```

### 3. Update loadDefaultWidgets() Method

**Replace the existing method to load real data after creating widgets:**

```typescript
loadDefaultWidgets(): void {
  // Create dashboard items with placeholder data
  this.dashboardItems = [
    // Row 1: KPI Cards
    {
      x: 0, y: 0, cols: 3, rows: 2,
      widgetId: 'total-properties',
      widgetType: 'kpi-card',
      data: this.getWidgetData('total-properties', 'kpi-card')
    },
    {
      x: 3, y: 0, cols: 3, rows: 2,
      widgetId: 'total-rooms',
      widgetType: 'kpi-card',
      data: this.getWidgetData('total-rooms', 'kpi-card')
    },
    {
      x: 6, y: 0, cols: 3, rows: 2,
      widgetId: 'bookings-today',
      widgetType: 'kpi-card',
      data: this.getWidgetData('bookings-today', 'kpi-card')
    },
    {
      x: 9, y: 0, cols: 3, rows: 2,
      widgetId: 'occupancy-rate',
      widgetType: 'kpi-card',
      data: this.getWidgetData('occupancy-rate', 'kpi-card')
    },
    // Row 2: Chart Widget
    {
      x: 0, y: 2, cols: 12, rows: 4,
      widgetId: 'revenue-chart',
      widgetType: 'chart',
      data: this.getDefaultChartWidget()
    },
    // Row 3: List and Calendar
    {
      x: 0, y: 6, cols: 6, rows: 4,
      widgetId: 'recent-activity',
      widgetType: 'list',
      data: this.getWidgetData('recent-activity', 'list')
    },
    {
      x: 6, y: 6, cols: 6, rows: 4,
      widgetId: 'booking-calendar',
      widgetType: 'calendar',
      data: this.getWidgetData('booking-calendar', 'calendar')
    }
  ];

  console.log('Loaded default widgets:', this.dashboardItems.length);
  
  // PHASE 4: Load real data after widgets are created
  this.loadKpiData();
  this.loadActivityData();
  this.loadCalendarData();
}
```

### 4. Update renderWidgets() Method

**Replace to load real data after rendering:**

```typescript
renderWidgets(config: DashboardConfiguration): void {
  this.dashboardItems = [];

  config.layout.widgets.forEach(widget => {
    const item: DashboardGridsterItem = {
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

  console.log('Rendered dashboard items:', this.dashboardItems.length);
  
  // PHASE 4: Load real data after widgets are rendered
  this.loadKpiData();
  this.loadActivityData();
  this.loadCalendarData();
}
```

---

## Implementation Steps

### Step 1: Add Imports
At the top of `dashboard1.component.ts`, add:
```typescript
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { 
  KpiValueResponse, 
  ActivityItemResponse, 
  CalendarEventResponse 
} from '../../models/dashboard.models';
```

### Step 2: Replace getWidgetData() Method
Find and replace the entire method with the version above.

### Step 3: Add Three New Methods
Add `loadKpiData()`, `loadActivityData()`, and `loadCalendarData()` after `loadDefaultWidgets()`.

### Step 4: Add Helper Methods
Add `getWidgetTitle()` and `getWidgetIcon()` private methods.

### Step 5: Update loadDefaultWidgets()
Replace the method to call real data loading methods at the end.

### Step 6: Update renderWidgets()
Add real data loading calls at the end of the method.

---

## Testing

After implementation:

1. **Build**: `npm run build`
2. **Start**: `npm start`
3. **Open**: `http://localhost:4200/propertyLanding/dashboard1`

**Expected**:
- Dashboard loads with placeholder values
- Real data populates within 1-2 seconds
- KPI values update with trends
- Activity feed shows real data
- Calendar shows real events

**If no real data exists**:
- KPIs show 0 values
- Activity/Calendar show empty states
- No errors in console

---

## Troubleshooting

### Issue: Data doesn't load
**Check**: Browser console for API errors
**Fix**: Verify backend is running and endpoints are accessible

### Issue: "Failed to load KPI data"
**Check**: Network tab for 401/403 errors
**Fix**: Verify JWT token is valid

### Issue: Properties show 0
**Check**: Do you have Properties in MongoDB?
**Fix**: Seed some test data or endpoints will return 0

---

**Ready to implement?** Let me know if you want me to create the complete updated file! 🚀
