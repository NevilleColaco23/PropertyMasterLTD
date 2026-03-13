# Dashboard Implementation Guide

## Overview
A new dashboard component (`dashboard1`) has been created and integrated into the application routing. This dashboard will serve as the landing page when users click the "Home" navigation item.

## What Was Created

### 1. Dashboard Component Files
- **Location**: `app/src/app/dashboard/dashboard1/`
- **Files**:
  - `dashboard1.component.ts` - Main component logic
  - `dashboard1.component.html` - Dashboard template with Material cards
  - `dashboard1.component.css` - Responsive styling

### 2. Features Included
The dashboard includes:
- **Summary Cards**: Display key metrics (Properties, Rooms, Bookings, Occupancy Rate)
- **Quick Actions**: Buttons for common tasks (New Booking, View Calendar, View Reports)
- **Recent Activity**: Section for displaying recent system activity
- **Responsive Design**: Works on desktop and mobile devices
- **Material Design**: Uses Angular Material components for consistent UI

### 3. Routing Configuration
- **Route Path**: `/propertyLanding/dashboard1`
- **Default Route**: When navigating to `/propertyLanding`, it automatically redirects to the dashboard
- **Route File**: `app/src/app/property/property-landing/property-landing.routes.ts`

## How to Access the Dashboard

### Option 1: Direct Navigation (Current Setup)
When you navigate to `/propertyLanding`, you'll automatically see the dashboard as the default landing page.

### Option 2: Configure "Home" Menu Item (Recommended)
To make the "Home" navigation link go to the dashboard, you need to update the menu configuration in your MongoDB database:

#### Update MongoDB Menu Collection
Find the "Home" menu item in your `Menus` collection and update the `path` field:

```javascript
db.Menus.updateOne(
  { label: "Home" },  // Find the Home menu item
  { $set: { path: "/propertyLanding/dashboard1" } }  // Set the path to dashboard
)
```

#### Full Menu Document Example
```javascript
{
  "_id": ObjectId("..."),
  "label": "Home",
  "path": "/propertyLanding/dashboard1",  // ← This should point to dashboard1
  "order": 1,
  "hasDropdown": false,
  "isVisible": true,
  "createdAt": ISODate("..."),
  "updatedAt": ISODate("...")
}
```

## Testing the Dashboard

### 1. Run the Application
```bash
cd app
npm start
```

### 2. Navigate to the Dashboard
After logging in and selecting a property, you'll be automatically redirected to the dashboard.

Alternatively, you can navigate directly to:
```
http://localhost:4200/propertyLanding/dashboard1
```

### 3. Verify the Navigation
- Click on different menu items to ensure routing works
- Click "Home" to verify it navigates to the dashboard (after database update)

## Next Steps - Enhancing the Dashboard

### 1. Connect to Real Data
Currently, the dashboard shows placeholder values (0). You can enhance it by:

**a) Create a Dashboard Service**
```typescript
// app/src/app/dashboard/services/dashboard.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private http: HttpClient) {}

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>('/api/v1/dashboard/stats');
  }
}
```

**b) Update the Component**
Inject the service and load real data in `ngOnInit()`.

### 2. Add Backend API Endpoint
Create a controller in your WebApi project:

```csharp
// WebApi/API/V1/DashboardController.cs
[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{v:apiVersion}/dashboard")]
public class DashboardController : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDTO>> GetStats()
    {
        // Implement logic to fetch dashboard statistics
        return Ok(new DashboardStatsDTO
        {
            TotalProperties = ...,
            TotalRooms = ...,
            BookingsToday = ...,
            OccupancyRate = ...
        });
    }
}
```

### 3. Add Charts and Visualizations
Consider integrating a charting library like:
- **ng2-charts** (Chart.js wrapper for Angular)
- **ngx-echarts** (Apache ECharts wrapper)

```bash
npm install ng2-charts chart.js
```

### 4. Add Real-time Updates
Implement SignalR or polling to update dashboard metrics in real-time.

## File Structure
```
app/src/app/
├── dashboard/
│   └── dashboard1/
│       ├── dashboard1.component.ts
│       ├── dashboard1.component.html
│       └── dashboard1.component.css
├── property/
│   └── property-landing/
│       └── property-landing.routes.ts  (updated)
```

## Important Notes

1. **Default Route**: The dashboard is set as the default child route of `/propertyLanding`, so users will see it immediately after property selection.

2. **Menu Configuration**: The navigation menu items are dynamically loaded from the database via the `/menu/GetinitialData` API endpoint. To change where "Home" navigates, update the MongoDB `Menus` collection.

3. **Permissions**: Ensure that users have the appropriate permissions in the `MenuPermissions` collection to see the Home menu item.

4. **Styling**: The dashboard uses Angular Material components. Make sure Material theme is properly configured in your app.

## Troubleshooting

### Dashboard Not Showing
- Check browser console for errors
- Verify route is registered correctly
- Ensure user is authenticated and has selected a property

### Menu Item Not Working
- Verify the `path` field in MongoDB `Menus` collection
- Check that user has permission in `MenuPermissions` collection
- Clear browser cache and reload

### Styling Issues
- Ensure Angular Material styles are imported in `angular.json`
- Check that Material theme is configured
- Verify Material icon font is loaded

## Database Updates Required

### 1. Menus Collection
```javascript
// Find and update the Home menu
db.Menus.updateOne(
  { label: "Home" },
  { $set: { 
    path: "/propertyLanding/dashboard1",
    hasDropdown: false,
    isVisible: true
  }}
)
```

### 2. MenuPermissions Collection
Ensure users have access to the Home menu:
```javascript
// Verify user has permission
db.MenuPermissions.find({
  UserId: 1,  // Replace with actual user ID
  AccessLevel: { $ne: "hidden" },
  isActive: true
})
```

## Summary

✅ Dashboard component created with Material Design
✅ Routing configured with automatic redirect
✅ Responsive layout for mobile and desktop
✅ Ready for data integration
✅ Follows existing app architecture patterns

The dashboard is now fully integrated and ready to use. Update the MongoDB menu configuration to complete the "Home" navigation setup.
