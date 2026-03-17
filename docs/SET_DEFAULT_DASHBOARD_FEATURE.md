# Set Default Dashboard Feature

## Overview
Added functionality to allow users to set any dashboard as their default dashboard, which will be loaded automatically when they first open the dashboard page.

## Implementation Date
March 17, 2026

## Features Added

### 1. Removed Welcome Subtitle
- Removed the "Welcome to your property management dashboard" subtitle
- Provides cleaner, more professional header appearance

### 2. Set as Default Button
- Star icon button appears next to the dashboard selector
- Only visible when:
  - A dashboard is selected
  - The current dashboard is NOT already the default
  - User is NOT in edit mode
- Clicking prompts for confirmation before setting as default

### 3. Visual Indicators
- Default dashboard shown with filled star (⭐) icon in dropdown
- "Default" badge displayed next to default dashboard name in selector
- Empty star (☆) icon on "Set as Default" button

## User Experience Flow

### Scenario 1: User opens dashboard for first time
1. System loads all user dashboards
2. If default dashboard exists → load it automatically
3. If no default → load first dashboard in list
4. If no dashboards exist → show empty state

### Scenario 2: User sets a dashboard as default
1. **User selects** a non-default dashboard from dropdown
2. **Empty star button** appears next to dashboard selector
3. **User clicks** the star button
4. **Confirmation dialog** appears: "Set '[Dashboard Name]' as your default dashboard?"
5. **User confirms** → API call made
6. **System updates**:
   - Previous default dashboard → isDefault = false
   - Selected dashboard → isDefault = true
7. **Dashboard list refreshes** → star icon and badge updated
8. **Success message** shown: "Default dashboard updated successfully"

### Scenario 3: Current dashboard is already default
- No star button shown (since it's already default)
- Filled star icon shown in dropdown
- "Default" badge visible next to dashboard name

## Technical Implementation

### Frontend Changes

#### 1. `dashboard1.component.html`
**Removed:**
```html
<p class="dashboard-subtitle">Welcome to your property management dashboard</p>
```

**Added:**
```html
<!-- Set as Default Button -->
@if (selectedDashboard && !isCurrentDashboardDefault() && !editMode) {
  <button mat-icon-button 
          (click)="setAsDefaultDashboard()"
          [disabled]="loading"
          matTooltip="Set as Default Dashboard"
          color="accent">
    <mat-icon>star_border</mat-icon>
  </button>
}
```

#### 2. `dashboard1.component.ts`
**New Methods:**

```typescript
/**
 * Set current dashboard as default
 */
setAsDefaultDashboard(): void {
  if (!this.selectedDashboard) {
    this.snackBar.open('No dashboard selected', 'Close', { duration: 3000 });
    return;
  }

  const dashboardName = this.dashboardConfig?.dashboardName || 'this dashboard';
  const confirmSetDefault = confirm(`Set "${dashboardName}" as your default dashboard?`);
  if (!confirmSetDefault) return;

  this.loading = true;
  const userId = this.getCurrentUserId();

  this.dashboardService.setDefaultDashboard(this.selectedDashboard, userId).subscribe({
    next: (success) => {
      if (success) {
        this.snackBar.open('Default dashboard updated successfully', 'Close', { duration: 3000 });
        this.loadUserDashboards(); // Reload to update isDefault flags
      } else {
        this.snackBar.open('Failed to set default dashboard', 'Close', { duration: 3000 });
        this.loading = false;
      }
    },
    error: (error) => {
      console.error('Error setting default dashboard:', error);
      this.snackBar.open('Failed to set default dashboard', 'Close', { duration: 3000 });
      this.loading = false;
    }
  });
}

/**
 * Check if current dashboard is the default
 */
isCurrentDashboardDefault(): boolean {
  if (!this.selectedDashboard) return false;
  const currentDashboard = this.userDashboards.find(d => d.id === this.selectedDashboard);
  return currentDashboard?.isDefault || false;
}
```

#### 3. `dashboard.service.ts`
**Existing Method (already implemented):**
```typescript
/**
 * Set a dashboard as default
 */
setDefaultDashboard(dashboardId: string, userId: number): Observable<boolean> {
  return this.http.post(`${this.apiUrl}/${dashboardId}/set-default`, null, {
    params: new HttpParams().set('userId', userId.toString()),
    observe: 'response'
  }).pipe(
    catchError((error: HttpErrorResponse) => {
      console.error('Set default dashboard error:', error);
      return of({ status: error.status } as any);
    }),
    map((response: any) => response.status === 200)
  );
}
```

### Backend API

#### Endpoint: `POST /api/v1/dashboard/{id}/set-default`

**Controller:** `DashboardController.cs` (Lines 120-136)

```csharp
/// <summary>
/// Set dashboard as default
/// </summary>
/// <param name="id">Dashboard ID</param>
/// <param name="userId">User ID</param>
/// <returns>Success status</returns>
[HttpPost("{id}/set-default")]
[ProducesResponseType(200)]
[ProducesResponseType(404)]
[LogUpdate("Dashboard", Description = "Set dashboard as default")]
public async Task<ActionResult> SetDefaultDashboard(string id, [FromQuery] int userId)
{
    var result = await _mediator.Send(new SetDefaultDashboardCommand 
    { 
        DashboardId = id,
        UserId = userId 
    });

    if (!result)
        return NotFound(new { message = "Dashboard not found or access denied" });

    return Ok(new { message = "Dashboard set as default" });
}
```

**Command Handler:** `SetDefaultDashboardCommand`
- Finds dashboard by ID
- Verifies user has access (dashboard.UserId matches userId)
- Sets all user's dashboards to isDefault = false
- Sets selected dashboard to isDefault = true
- Saves changes to MongoDB
- Returns success/failure boolean

## Button Visibility Logic

The "Set as Default" button is shown when ALL conditions are true:
1. `selectedDashboard` exists (a dashboard is loaded)
2. `!isCurrentDashboardDefault()` (current dashboard is NOT default)
3. `!editMode` (user is NOT in edit mode)

This ensures:
- Button doesn't show when no dashboard selected
- Button doesn't show on already-default dashboards (avoid confusion)
- Button doesn't clutter UI during edit mode

## UI/UX Considerations

### Icon Choices
- **star_border** (☆): Empty star for "Set as Default" button (indicates action to take)
- **star** (⭐): Filled star in dropdown for default dashboards (indicates current state)

### Placement
- Button positioned between dashboard selector and other action buttons
- Uses `mat-icon-button` for compact, icon-only design
- Accent color (`color="accent"`) to match Material Design theme

### Confirmation
- Confirmation dialog prevents accidental changes
- Shows dashboard name in confirmation message for clarity
- User can cancel if clicked by mistake

### Feedback
- Success message: "Default dashboard updated successfully"
- Error message: "Failed to set default dashboard"
- Loading state prevents multiple clicks during API call

## Testing Scenarios

### ✅ Test Case 1: Set default on fresh account
**Given:** User has 3 dashboards, none marked default
**When:** User selects "Dashboard 2" and clicks set default
**Then:** 
- Dashboard 2 becomes default
- Next login loads Dashboard 2 automatically
- Star icon appears next to Dashboard 2 in dropdown

### ✅ Test Case 2: Change default dashboard
**Given:** User has Dashboard 1 as default
**When:** User switches to Dashboard 3 and sets as default
**Then:**
- Dashboard 1 loses default status
- Dashboard 3 becomes new default
- UI updates to show Dashboard 3 with star icon

### ✅ Test Case 3: Button visibility
**Given:** User viewing default dashboard
**When:** Dashboard loads
**Then:**
- Set default button NOT visible (already default)
- Star icon visible in dropdown

### ✅ Test Case 4: Edit mode
**Given:** User in edit mode
**When:** User looks at header
**Then:**
- Set default button hidden (edit mode active)
- Other edit buttons visible

### ✅ Test Case 5: Cancel confirmation
**Given:** User clicks set default button
**When:** Confirmation dialog appears
**Then:**
- User can click Cancel → no changes made
- User can click OK → default updated

## Database Schema

### DashboardConfiguration Collection
```json
{
  "_id": "dashboard-123",
  "userId": 1,
  "dashboardName": "My Dashboard",
  "isDefault": true,  // ← Flag to track default dashboard
  "layout": { ... },
  "createdDate": "2026-03-17T00:00:00Z",
  "lastModifiedDate": "2026-03-17T00:00:00Z"
}
```

**Important:** Only ONE dashboard per user should have `isDefault: true`

## Benefits

### User Benefits
1. **Convenience**: Default dashboard loads automatically on login
2. **Personalization**: Users can choose which dashboard they see most
3. **Efficiency**: No need to select dashboard every time
4. **Flexibility**: Easy to change default as needs evolve

### System Benefits
1. **Better UX**: Faster load times (no selection step)
2. **User engagement**: Encourages dashboard customization
3. **Analytics**: Can track which dashboards users prefer as defaults
4. **Onboarding**: New users can be assigned default dashboard

## Future Enhancements

### Potential Additions
1. **Role-based defaults**: Different default dashboards per user role
2. **Property-specific defaults**: Different defaults per property
3. **Time-based defaults**: Different dashboards for different times (morning vs evening)
4. **Quick switch**: Keyboard shortcut to toggle between favorite dashboards
5. **Multiple favorites**: Star multiple dashboards for quick access
6. **Dashboard categories**: Organize dashboards (Work, Personal, Reports)

## Related Features
- Dashboard selector dropdown (existing)
- Save/Delete dashboard (existing)
- Edit mode (existing)
- Dashboard persistence (MongoDB)

## API Endpoints Used
- `GET /api/v1/dashboard/user/{userId}/all` - Load all dashboards
- `POST /api/v1/dashboard/{id}/set-default` - Set dashboard as default
- `GET /api/v1/dashboard/user/{userId}?defaultOnly=true` - Load default dashboard

## Activity Logging
The `SetDefaultDashboard` endpoint includes `[LogUpdate]` attribute:
```csharp
[LogUpdate("Dashboard", Description = "Set dashboard as default")]
```

This logs:
- User ID who made the change
- Dashboard ID that was set as default
- Timestamp of the action
- Description: "Set dashboard as default"

## Security Considerations
- User can only set their own dashboards as default (userId validation)
- Endpoint verifies dashboard belongs to user before updating
- Returns 404 if dashboard not found or access denied
- No sensitive data exposed in error messages

## Summary
This feature enhances user experience by allowing personalized dashboard defaults. The implementation is clean, well-integrated with existing code, and provides clear visual feedback. Users can easily identify and change their default dashboard with a single click, making the dashboard system more user-friendly and efficient.
