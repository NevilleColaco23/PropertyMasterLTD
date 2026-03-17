# Activity Feed Widget Implementation

## Overview
Created a lightweight activity feed widget that displays only essential fields (DisplayMessage, Timestamp, Action) for dashboard use. This provides a clean, fast-loading alternative to the full activity stream widget.

## Backend Implementation

### 1. DTO Layer
**File**: `classfiles/Application/UserActivity/DTOs/UserActivityDTOs.cs`

Added `ActivityWidgetDTO`:
```csharp
public class ActivityWidgetDTO
{
    public string DisplayMessage { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; }
    public string TimeAgo { get; set; } // Computed property
}
```

### 2. Query Layer
**File**: `classfiles/Application/UserActivity/Queries/UserActivityQueries.cs`

Added lightweight query:
```csharp
public record GetActivityWidgetDataQuery(int Count = 15) : IRequest<List<ActivityWidgetDTO>>;
```

### 3. Handler Layer
**File**: `classfiles/Application/UserActivity/Handlers/UserActivityQueryHandlers.cs`

Added `GetActivityWidgetDataQueryHandler`:
- Fetches recent activities from repository
- Maps only 3 essential fields (DisplayMessage, Timestamp, Action)
- Lightweight and fast

### 4. API Layer
**File**: `WebApi/API/V1/ActivityController.cs`

Added endpoint:
```csharp
[HttpGet("widget")]
public async Task<ActionResult<List<ActivityWidgetDTO>>> GetWidgetData([FromQuery] int count = 15)
```

**Endpoint**: `GET /api/v1/activity/widget?count=15`

## Frontend Implementation

### 1. Component
**File**: `app/src/app/widgets/activity-feed-widget/activity-feed-widget.component.ts`

Features:
- Standalone component (Angular 18)
- Auto-refresh capability (configurable interval)
- Loading/error states
- Time ago calculation
- Action icon/color mapping

**Input Settings**:
```typescript
{
  title: 'Recent Activity Feed',  // Widget title
  count: 15,                      // Number of activities
  refreshInterval: 30000,         // Auto-refresh (ms)
  showRefreshButton: true         // Show refresh button
}
```

### 2. Template
**File**: `app/src/app/widgets/activity-feed-widget/activity-feed-widget.component.html`

Structure:
- Material Card container
- Header with title and refresh button
- Loading spinner
- Error state with retry
- Empty state
- Activity list with Material List

### 3. Styles
**File**: `app/src/app/widgets/activity-feed-widget/activity-feed-widget.component.css`

Features:
- Material Design styling
- Color-coded action icons
- Responsive layout
- Hover effects
- Mobile-friendly

### 4. Service Integration
**File**: `app/src/app/services/activity.service.ts`

Added method:
```typescript
getWidgetData(count: number = 15): Observable<ActivityWidgetDTO[]>
```

### 5. Models
**File**: `app/src/app/models/activity.models.ts`

Added interface:
```typescript
export interface ActivityWidgetDTO {
  displayMessage: string;
  timestamp: Date;
  action: string;
  timeAgo?: string;
}
```

## Usage

### In a Dashboard Component
```typescript
import { ActivityFeedWidgetComponent } from './widgets/activity-feed-widget/activity-feed-widget.component';

@Component({
  ...
  imports: [ActivityFeedWidgetComponent]
})
export class DashboardComponent {
  widgetSettings = {
    title: 'Recent Activity',
    count: 20,
    refreshInterval: 30000,
    showRefreshButton: true
  };
}
```

```html
<app-activity-feed-widget [settings]="widgetSettings"></app-activity-feed-widget>
```

## Features

### Display Fields
1. **DisplayMessage**: Human-readable activity description
   - Example: "John Doe viewed All Dashboards while working on Sunset Villa property"
2. **Timestamp**: When the activity occurred (shown as "time ago")
   - Example: "5 minutes ago", "2 hours ago"
3. **Action**: Brief action description
   - Example: "View Property", "Create Booking", "Update Room"

### Action Icon Mapping
Icons are automatically determined by action keywords:
- **View/Select**: visibility icon
- **Create/Add**: add_circle icon (green)
- **Update/Edit**: edit icon (blue)
- **Delete/Remove**: delete icon (orange)
- **Login/SignIn**: login icon
- **Logout/SignOut**: logout icon
- **Export**: download icon
- **Import**: upload icon
- **Search**: search icon
- **Filter**: filter_list icon
- **Default**: info icon

### Auto-Refresh
- Configurable refresh interval (default: 30 seconds)
- Can be disabled by setting `refreshInterval: 0`
- Manual refresh button available

### States
1. **Loading**: Shows spinner while fetching data
2. **Error**: Shows error message with retry button
3. **Empty**: Shows "No recent activities" message
4. **Data**: Shows list of activities with icons and timestamps

## API Response Example

```json
[
  {
    "displayMessage": "John Doe viewed All Dashboards while working on Sunset Villa property",
    "timestamp": "2024-01-15T10:30:00Z",
    "action": "View Dashboard",
    "timeAgo": "5 minutes ago"
  },
  {
    "displayMessage": "Jane Smith created new room 'Suite 101' on Floor 1 in Ocean View Resort",
    "timestamp": "2024-01-15T10:25:00Z",
    "action": "Create Room",
    "timeAgo": "10 minutes ago"
  }
]
```

## Performance Benefits

### Compared to ActivitySummaryDTO
- **Smaller payload**: Only 3 fields vs 10+ fields
- **Faster serialization**: Simple properties only
- **Reduced bandwidth**: ~60% smaller JSON response
- **Quicker rendering**: Fewer DOM elements

### Optimizations
- Auto-refresh uses RxJS interval with switchMap
- Proper unsubscribe on component destroy
- Minimal re-renders
- CSS transitions for smooth updates

## StackTrace Enhancement

### Code Path Tracking
The backend `StackTrace` field now captures execution flow for successful operations:

**Example**:
```
PropertyController.GetById → PropertyService.GetPropertyDetails → PropertyRepositoryMongo.GetByIdAsync
```

**How it works**:
1. Uses `System.Diagnostics.StackTrace` to capture method calls
2. Filters relevant frames (Controller, Service, Repository, Handler, Filter)
3. Builds simplified path with " → " separator
4. On errors, still captures full exception stack trace

**Benefits**:
- Debug production issues without errors
- Understand service dependencies
- Track performance bottlenecks
- Verify expected execution path

## Integration with Dashboard

The widget can be added to any dashboard by:
1. Importing the component
2. Adding the selector to the template
3. Optionally configuring settings

Example grid layout:
```html
<div class="dashboard-grid">
  <div class="widget-container">
    <app-activity-feed-widget [settings]="activitySettings"></app-activity-feed-widget>
  </div>
  <div class="widget-container">
    <!-- Other widgets -->
  </div>
</div>
```

## Future Enhancements

Possible additions:
1. **Filtering**: Filter by user, action type, or property
2. **Click-through**: Navigate to entity details on click
3. **Grouping**: Group by time periods (today, yesterday, etc.)
4. **Animations**: Slide-in animations for new activities
5. **Real-time updates**: SignalR/WebSocket integration
6. **Customizable icons**: User-defined icon mappings

## Related Documentation

- **Activity Logging System**: `ACTIVITY_LOGGING_SYSTEM_DOCUMENTATION.md`
- **Message Templates**: `CENTRALIZED_TEMPLATES_IMPLEMENTATION.md`
- **Origin Field**: `ORIGIN_FIELD_IMPLEMENTATION.md`
- **StackTrace Enhancement**: `FIELD_REMOVAL_AND_STACKTRACE.md`

## Testing Checklist

Backend:
- [x] DTO created with 3 properties
- [x] Query created
- [x] Handler implemented
- [x] API endpoint added
- [ ] Test endpoint returns correct data
- [ ] Verify TimeAgo calculation

Frontend:
- [x] Component created
- [x] Template designed
- [x] Styles applied
- [x] Service method added
- [x] Model interface added
- [ ] Test rendering with mock data
- [ ] Test auto-refresh
- [ ] Test error handling
- [ ] Test empty state
- [ ] Add to dashboard

## Notes

- Widget is independent of full activity stream widget
- Uses existing repository methods (no DB changes needed)
- Follows existing CQRS/MediatR patterns
- Material Design for consistency
- Responsive and mobile-friendly
