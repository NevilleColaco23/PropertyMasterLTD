# Chart Widget Time Period Filter Implementation

## Overview
Added time period filtering options to the chart widget, allowing users to switch between different date ranges and grouping options (daily, weekly, monthly).

## Implementation Date
March 17, 2026

## Features Added

### 1. Menu Enhancement
Added a comprehensive menu with two sections:
- **Chart Type**: Switch between Line and Bar charts
- **Time Period**: Select from 4 different time ranges:
  - Last 7 Days (daily)
  - Last 30 Days (daily)
  - Last 90 Days (weekly grouping)
  - Last 6 Months (monthly grouping)

### 2. Visual Indicators
- Active menu items highlighted with blue background
- Check icons showing current selection
- Period label displayed in chart title subtitle
- Section labels for better organization
- Divider between menu sections

### 3. Data Persistence
- Selected filters stored in widget settings
- Dashboard marked as changed when filters modified
- Settings persisted when dashboard is saved

## Technical Details

### Files Modified

#### 1. `app/src/app/widgets/chart-widget/chart-widget.component.ts`
**Imports Added:**
```typescript
import { Output, EventEmitter } from '@angular/core';
import { MatDividerModule } from '@angular/material/divider';
```

**New Interface:**
```typescript
export interface ChartFilterChange {
  daysBack: number;
  groupBy: 'day' | 'week' | 'month';
}
```

**New Properties:**
```typescript
@Output() filterChange = new EventEmitter<ChartFilterChange>();
daysBack: number = 30;
groupBy: 'day' | 'week' | 'month' = 'day';
currentPeriodLabel: string = 'Last 30 Days';
```

**New Methods:**
```typescript
changePeriod(days: number, group: 'day' | 'week' | 'month'): void
updatePeriodLabel(): void
```

**CSS Additions:**
- `.subtitle` - Period label styling
- `.menu-section` - Menu section container
- `.menu-section-label` - Section headers
- `.mat-mdc-menu-item.active` - Active item highlighting
- `.check-icon` - Check mark styling

#### 2. `app/src/app/dashboard/dashboard1/dashboard1.component.html`
**Chart Widget Updated:**
```html
<app-chart-widget 
  [data]="item.data" 
  [settings]="item.settings"
  (filterChange)="onChartFilterChange($event, item)"
  style="display: block; height: 100%; width: 100%;"></app-chart-widget>
```

#### 3. `app/src/app/dashboard/dashboard1/dashboard1.component.ts`
**New Method:**
```typescript
onChartFilterChange(filter: { daysBack: number; groupBy: 'day' | 'week' | 'month' }, item: DashboardGridsterItem): void
```

### Menu Structure
```
┌─────────────────────────────┐
│ Chart Type                  │
├─────────────────────────────┤
│ 📈 Line Chart         ✓     │
│ 📊 Bar Chart                │
├─────────────────────────────┤
│ Time Period                 │
├─────────────────────────────┤
│ 📅 Last 7 Days              │
│ 📅 Last 30 Days        ✓    │
│ 📅 Last 90 Days (Weekly)    │
│ 📅 Last 6 Months            │
└─────────────────────────────┘
```

## User Experience Flow

1. **User clicks menu button** (⋮) in chart widget header
2. **Menu appears** with Chart Type and Time Period sections
3. **User selects time period** (e.g., "Last 7 Days")
4. **Component updates**:
   - Emits `filterChange` event
   - Updates period label in title
   - Marks menu item as active
5. **Dashboard component**:
   - Updates widget settings
   - Marks dashboard as changed
   - Reloads chart data with new filters
6. **Backend API called** with new parameters:
   - `daysBack`: 7, 30, 90, or 180
   - `groupBy`: 'day', 'week', or 'month'
7. **Chart updates** with new data and date range

## Backend Integration

The backend API already supports these parameters:
```csharp
// API Endpoint: /api/v1/dashboard/activity/booking-trends
public async Task<IActionResult> GetBookingTrends(
    [FromQuery] int userId, 
    [FromQuery] int daysBack = 30,
    [FromQuery] string groupBy = "day")
```

The handler (`GetBookingTrendsQueryHandler`) automatically aggregates data based on:
- **Daily**: Individual dates within the range
- **Weekly**: Groups by week starting Monday
- **Monthly**: Groups by calendar month

## CSS Styling Details

### Menu Sections
- Padding: 8px vertical
- Section labels: 11px, bold, uppercase, gray

### Active States
- Background: `rgba(25, 118, 210, 0.08)` (light blue)
- Text color: `#1976d2` (Material blue)
- Icons: Blue when active

### Check Icons
- Size: 18x18px
- Position: Right-aligned
- Color: Material blue

### Period Label (Subtitle)
- Font size: 11px
- Weight: 400
- Opacity: 0.9
- Position: Below title in header

## Testing

### Test Cases
1. ✅ Menu opens on click
2. ✅ Active item shows check mark
3. ✅ Period changes update title
4. ✅ Data reloads with correct parameters
5. ✅ Settings persist in dashboard
6. ✅ Multiple charts can have different periods
7. ✅ Refresh button still works independently

### Verified Scenarios
- Switch from 30 days to 7 days → Chart shows last week
- Change to 90 days weekly → Chart shows 12-13 weeks grouped
- Change to 6 months → Chart shows 6 monthly data points
- Switch between line and bar with different periods
- Save dashboard with custom period settings
- Reload dashboard → Period settings restored

## Future Enhancements

### Potential Additions
1. **Custom Date Range**: Date picker for specific start/end dates
2. **Year-over-Year**: Compare current period to same period last year
3. **Export Data**: Download chart data as CSV/Excel
4. **Zoom Controls**: Zoom in/out on specific date ranges
5. **Quick Actions**: "Today", "This Week", "This Month" buttons
6. **Period Comparison**: Side-by-side comparison of different periods

### UI Improvements
- Keyboard shortcuts (1-4 for quick period selection)
- Period selector directly in header (chips/tabs)
- Animated transitions between period changes
- Loading indicator during data fetch

## Dependencies
- Angular Material Menu (MatMenuModule)
- Angular Material Divider (MatDividerModule)
- Chart.js 4.4.0 (for visualization)
- ng2-charts (Angular wrapper)

## Related Documentation
- [Chart Widget Implementation](./CHART_WIDGET_IMPLEMENTATION.md)
- [Chart.js Implementation](./CHARTJS_IMPLEMENTATION.md)
- [Widget Refresh Feature](./WIDGET_REFRESH_FEATURE.md)
- [Dashboard Widgets Status](./DASHBOARD_WIDGETS_STATUS.md)

## Summary
This feature gives users flexible control over the time range and granularity of their booking trends chart, making it easier to analyze patterns at different scales (daily for short-term, weekly for medium-term, monthly for long-term trends). The implementation is clean, performant, and follows Angular best practices with event-driven communication between components.
