# 🖱️ **CLIENT-SIDE ACTIVITY TRACKING GUIDE**

Track all user interactions on the frontend, including property selections, clicks, navigation, and more.

---

## 📋 **TABLE OF CONTENTS**

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Implementation Steps](#implementation-steps)
4. [Example: Track Property Selection](#example-track-property-selection)
5. [Automatic Tracking](#automatic-tracking)
6. [Advanced Tracking](#advanced-tracking)
7. [Best Practices](#best-practices)

---

## 🎯 **OVERVIEW**

**What You Can Track:**
- ✅ Property selections (which property user clicked)
- ✅ Booking selections
- ✅ Filter changes
- ✅ Search queries
- ✅ Button clicks
- ✅ Form submissions
- ✅ Page navigation
- ✅ Time spent on pages
- ✅ Download actions
- ✅ Menu selections
- ✅ Widget interactions
- ✅ And more...

**Benefits:**
- 📊 Understand user behavior
- 🎯 Improve UX based on data
- 🔍 Identify pain points
- 📈 Track feature usage
- 🚀 Optimize conversion funnels

---

## 🏗️ **ARCHITECTURE**

```
┌─────────────────────────────────────────────────────────┐
│  ANGULAR APP (Frontend)                                 │
│                                                          │
│  ┌────────────────┐                                     │
│  │ User Interaction│ (Click, Select, Navigate, etc.)    │
│  └────────┬────────┘                                     │
│           │                                              │
│           ▼                                              │
│  ┌────────────────────────────┐                         │
│  │ ClientActivityService      │                         │
│  │ - trackPropertySelection() │                         │
│  │ - trackButtonClick()       │                         │
│  │ - trackPageView()          │                         │
│  │ - trackSearch()            │                         │
│  └────────┬───────────────────┘                         │
│           │                                              │
│           ▼                                              │
│  ┌────────────────────────────┐                         │
│  │ ActivityService            │ (Existing)              │
│  │ POST /api/v1/activity/log  │                         │
│  └────────┬───────────────────┘                         │
└───────────┼──────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────┐
│  .NET BACKEND API                                        │
│                                                          │
│  ┌────────────────────────────┐                         │
│  │ ActivityController         │                         │
│  │ POST LogClientActivity()   │ (New endpoint)          │
│  └────────┬───────────────────┘                         │
│           │                                              │
│           ▼                                              │
│  ┌────────────────────────────┐                         │
│  │ UserActivityService        │                         │
│  └────────┬───────────────────┘                         │
│           │                                              │
│           ▼                                              │
│  ┌────────────────────────────┐                         │
│  │ MongoDB - UserActivityLogs │                         │
│  │  ActivityType: "Selection" │                         │
│  │  EntityType: "Property"    │                         │
│  │  EntityId: 123             │                         │
│  │  Metadata: { ... }         │                         │
│  └────────────────────────────┘                         │
└─────────────────────────────────────────────────────────┘
```

---

## 🚀 **IMPLEMENTATION STEPS**

### **STEP 1: Add Backend Endpoint for Client Activity**

**File**: `WebApi/API/V1/ActivityController.cs`

Add this new endpoint:

```csharp
/// <summary>
/// Log client-side activity (user interactions, selections, etc.)
/// </summary>
[HttpPost("log-client")]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<ActionResult> LogClientActivity([FromBody] LogClientActivityCommand command)
{
    try
    {
        // Extract user info from authenticated request
        var userId = GetUserIdFromClaims();
        var username = User?.Identity?.Name ?? "Unknown";
        
        await _userActivityService.LogActivityAsync(
            userId: userId,
            username: username,
            activityType: command.ActivityType,
            entityType: command.EntityType,
            entityId: command.EntityId,
            action: command.Action,
            description: command.Description,
            module: command.Module,
            ipAddress: GetClientIpAddress(),
            userAgent: Request.Headers["User-Agent"].ToString(),
            sessionId: HttpContext.TraceIdentifier,
            traceId: HttpContext.TraceIdentifier,
            isSuccess: true,
            errorMessage: null,
            durationMs: command.DurationMs,
            metadata: command.Metadata
        );
        
        return Ok(new { success = true });
    }
    catch (Exception ex)
    {
        // Log error but don't fail the request
        _logger.LogError(ex, "Failed to log client activity");
        return Ok(new { success = false, error = ex.Message });
    }
}

private int GetUserIdFromClaims()
{
    var userIdClaim = User?.FindFirst("UserId") ?? User?.FindFirst("sub");
    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
    {
        return userId;
    }
    return 0;
}

private string GetClientIpAddress()
{
    return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
}
```

**Add DTO**:

**File**: `classfiles/Application/UserActivity/DTOs/UserActivityDTOs.cs`

```csharp
public class LogClientActivityCommand
{
    public ActivityType ActivityType { get; set; }
    public string EntityType { get; set; }
    public int? EntityId { get; set; }
    public string Action { get; set; }
    public string Description { get; set; }
    public string Module { get; set; }
    public int DurationMs { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
```

---

### **STEP 2: Create Client-Side Activity Service**

**File**: `app/src/app/services/client-activity.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface ClientActivityLog {
  activityType: string;
  entityType: string;
  entityId?: number;
  action: string;
  description: string;
  module: string;
  durationMs?: number;
  metadata?: any;
}

@Injectable({
  providedIn: 'root'
})
export class ClientActivityService {
  private apiUrl = `${environment.apiUrl}/activity/log-client`;
  private activityQueue: ClientActivityLog[] = [];
  private batchSize = 10;
  private batchInterval = 5000; // 5 seconds

  constructor(private http: HttpClient) {
    // Start batch processor
    this.startBatchProcessor();
  }

  /**
   * Track property selection
   */
  trackPropertySelection(propertyId: number, propertyName: string): void {
    this.logActivity({
      activityType: 'Selection',
      entityType: 'Property',
      entityId: propertyId,
      action: 'Selected Property',
      description: `User selected property: ${propertyName}`,
      module: 'Property',
      metadata: {
        propertyId,
        propertyName,
        timestamp: new Date().toISOString()
      }
    });
  }

  /**
   * Track booking selection
   */
  trackBookingSelection(bookingId: number, bookingRef: string): void {
    this.logActivity({
      activityType: 'Selection',
      entityType: 'Booking',
      entityId: bookingId,
      action: 'Selected Booking',
      description: `User selected booking: ${bookingRef}`,
      module: 'Booking',
      metadata: {
        bookingId,
        bookingRef
      }
    });
  }

  /**
   * Track button click
   */
  trackButtonClick(buttonName: string, context: string): void {
    this.logActivity({
      activityType: 'Click',
      entityType: 'Button',
      action: `Clicked ${buttonName}`,
      description: `User clicked ${buttonName} button in ${context}`,
      module: context,
      metadata: {
        buttonName,
        context
      }
    });
  }

  /**
   * Track page view
   */
  trackPageView(pageName: string, url: string, duration?: number): void {
    this.logActivity({
      activityType: 'PageView',
      entityType: 'Page',
      action: 'Viewed Page',
      description: `User viewed ${pageName} page`,
      module: pageName,
      durationMs: duration,
      metadata: {
        pageName,
        url,
        duration
      }
    });
  }

  /**
   * Track search
   */
  trackSearch(searchTerm: string, module: string, resultsCount?: number): void {
    this.logActivity({
      activityType: 'Search',
      entityType: module,
      action: 'Search',
      description: `User searched for: "${searchTerm}"`,
      module,
      metadata: {
        searchTerm,
        resultsCount
      }
    });
  }

  /**
   * Track filter change
   */
  trackFilterChange(filterName: string, filterValue: any, module: string): void {
    this.logActivity({
      activityType: 'Filter',
      entityType: module,
      action: 'Applied Filter',
      description: `User applied filter: ${filterName} = ${filterValue}`,
      module,
      metadata: {
        filterName,
        filterValue
      }
    });
  }

  /**
   * Track form submission
   */
  trackFormSubmission(formName: string, success: boolean): void {
    this.logActivity({
      activityType: success ? 'Create' : 'Error',
      entityType: 'Form',
      action: success ? 'Form Submitted' : 'Form Error',
      description: `User ${success ? 'submitted' : 'failed to submit'} ${formName}`,
      module: formName,
      metadata: {
        formName,
        success
      }
    });
  }

  /**
   * Track download
   */
  trackDownload(fileName: string, fileType: string): void {
    this.logActivity({
      activityType: 'Export',
      entityType: 'File',
      action: 'Downloaded File',
      description: `User downloaded ${fileName}`,
      module: 'Export',
      metadata: {
        fileName,
        fileType
      }
    });
  }

  /**
   * Track widget interaction
   */
  trackWidgetInteraction(widgetId: string, action: string): void {
    this.logActivity({
      activityType: 'WidgetInteraction',
      entityType: 'Widget',
      action,
      description: `User interacted with widget: ${widgetId}`,
      module: 'Dashboard',
      metadata: {
        widgetId,
        action
      }
    });
  }

  /**
   * Generic activity logging
   */
  private logActivity(activity: ClientActivityLog): void {
    // Add to queue
    this.activityQueue.push(activity);

    // If queue is full, flush immediately
    if (this.activityQueue.length >= this.batchSize) {
      this.flushQueue();
    }
  }

  /**
   * Start batch processor
   */
  private startBatchProcessor(): void {
    setInterval(() => {
      if (this.activityQueue.length > 0) {
        this.flushQueue();
      }
    }, this.batchInterval);
  }

  /**
   * Flush queue to backend
   */
  private flushQueue(): void {
    if (this.activityQueue.length === 0) return;

    const batch = [...this.activityQueue];
    this.activityQueue = [];

    // Send batch to backend
    batch.forEach(activity => {
      this.http.post(this.apiUrl, activity)
        .pipe(
          catchError(error => {
            console.error('Failed to log client activity:', error);
            return of(null);
          })
        )
        .subscribe();
    });
  }
}
```

---

### **STEP 3: Use in Components**

**Example: Track Property Selection**

**File**: `app/src/app/property/property-landing/property-landing.component.ts`

```typescript
import { ClientActivityService } from '../../services/client-activity.service';

export class PropertyLandingComponent {
  constructor(
    private clientActivity: ClientActivityService
    // ... other services
  ) {}

  onPropertyClick(property: any): void {
    // Track the selection
    this.clientActivity.trackPropertySelection(
      property.id,
      property.propertyName
    );

    // Navigate or open property details
    this.router.navigate(['/property/details', property.id]);
  }

  onSearch(searchTerm: string): void {
    // Track the search
    this.clientActivity.trackSearch(
      searchTerm,
      'Property',
      this.properties.length
    );

    // Perform search
    this.searchProperties(searchTerm);
  }

  onFilterChange(filterName: string, value: any): void {
    // Track filter change
    this.clientActivity.trackFilterChange(
      filterName,
      value,
      'Property'
    );

    // Apply filter
    this.applyFilter(filterName, value);
  }

  onExportClick(): void {
    // Track button click
    this.clientActivity.trackButtonClick('Export', 'Property');

    // Perform export
    this.exportProperties();
  }
}
```

**In Template**:

```html
<!-- Track property selection -->
<mat-card *ngFor="let property of properties" 
          (click)="onPropertyClick(property)"
          class="property-card">
  <mat-card-header>
    <mat-card-title>{{ property.propertyName }}</mat-card-title>
  </mat-card-header>
  <mat-card-content>
    {{ property.address }}
  </mat-card-content>
</mat-card>

<!-- Track search -->
<mat-form-field>
  <input matInput 
         placeholder="Search properties..."
         (keyup.enter)="onSearch($event.target.value)">
</mat-form-field>

<!-- Track filter -->
<mat-select (selectionChange)="onFilterChange('city', $event.value)">
  <mat-option value="all">All Cities</mat-option>
  <mat-option value="mumbai">Mumbai</mat-option>
  <mat-option value="delhi">Delhi</mat-option>
</mat-select>

<!-- Track button click -->
<button mat-raised-button (click)="onExportClick()">
  <mat-icon>download</mat-icon>
  Export
</button>
```

---

## 🔄 **AUTOMATIC TRACKING**

### **Auto-Track Page Views**

**File**: `app/src/app/app.component.ts`

```typescript
import { Router, NavigationEnd } from '@angular/router';
import { ClientActivityService } from './services/client-activity.service';

export class AppComponent implements OnInit {
  private pageStartTime: number;

  constructor(
    private router: Router,
    private clientActivity: ClientActivityService
  ) {}

  ngOnInit(): void {
    // Track page views automatically
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        // Log previous page duration
        if (this.pageStartTime) {
          const duration = Date.now() - this.pageStartTime;
          this.clientActivity.trackPageView(
            this.getCurrentPageName(event.url),
            event.url,
            duration
          );
        }

        // Start timing new page
        this.pageStartTime = Date.now();
      }
    });
  }

  private getCurrentPageName(url: string): string {
    const segments = url.split('/').filter(s => s);
    return segments[0] || 'Home';
  }
}
```

---

### **Auto-Track Form Submissions**

Create a directive:

**File**: `app/src/app/directives/track-form.directive.ts`

```typescript
import { Directive, HostListener, Input } from '@angular/core';
import { ClientActivityService } from '../services/client-activity.service';

@Directive({
  selector: '[trackForm]'
})
export class TrackFormDirective {
  @Input() trackForm: string = 'Form';

  constructor(private clientActivity: ClientActivityService) {}

  @HostListener('ngSubmit', ['$event'])
  onSubmit(event: any): void {
    this.clientActivity.trackFormSubmission(this.trackForm, true);
  }
}
```

**Usage**:

```html
<form [formGroup]="propertyForm" 
      (ngSubmit)="onSubmit()" 
      trackForm="Property Form">
  <!-- form fields -->
</form>
```

---

## 📊 **VIEW CLIENT ACTIVITY IN MONGODB**

```javascript
use ListingDB

// View property selections
db.UserActivityLogs.find({ 
  ActivityType: "Selection", 
  EntityType: "Property" 
}).sort({ Timestamp: -1 }).limit(10).pretty()

// Most selected properties
db.UserActivityLogs.aggregate([
  { $match: { ActivityType: "Selection", EntityType: "Property" } },
  { $group: { 
      _id: "$EntityId", 
      count: { $sum: 1 },
      propertyName: { $first: "$Metadata.propertyName" }
  }},
  { $sort: { count: -1 } },
  { $limit: 10 }
])

// Search terms
db.UserActivityLogs.find({ 
  ActivityType: "Search" 
}, {
  "Metadata.searchTerm": 1,
  "Metadata.resultsCount": 1,
  Timestamp: 1
}).sort({ Timestamp: -1 }).limit(20).pretty()

// User journey (chronological)
db.UserActivityLogs.find({ 
  UserId: 19 
}).sort({ Timestamp: 1 }).pretty()
```

---

## 🎯 **ADVANCED TRACKING**

### **1. Heatmap Tracking**

```typescript
trackMouseClick(x: number, y: number, element: string): void {
  this.clientActivity.logActivity({
    activityType: 'Click',
    entityType: 'UI Element',
    action: 'Mouse Click',
    description: `Click at (${x}, ${y}) on ${element}`,
    module: 'UI',
    metadata: {
      x, y, element,
      screenWidth: window.innerWidth,
      screenHeight: window.innerHeight
    }
  });
}
```

### **2. Scroll Depth**

```typescript
@HostListener('window:scroll')
onScroll(): void {
  const scrollPercent = 
    (window.scrollY / (document.body.scrollHeight - window.innerHeight)) * 100;
  
  if (scrollPercent > this.maxScroll) {
    this.maxScroll = Math.round(scrollPercent / 10) * 10; // Round to 10%
    this.clientActivity.logActivity({
      activityType: 'Scroll',
      entityType: 'Page',
      action: `Scrolled ${this.maxScroll}%`,
      description: `User scrolled ${this.maxScroll}% of page`,
      module: 'UI',
      metadata: { scrollPercent: this.maxScroll }
    });
  }
}
```

### **3. Feature Usage**

```typescript
trackFeatureUsage(featureName: string): void {
  this.clientActivity.logActivity({
    activityType: 'FeatureUsage',
    entityType: 'Feature',
    action: `Used ${featureName}`,
    description: `User used feature: ${featureName}`,
    module: 'Features',
    metadata: { featureName }
  });
}
```

---

## ✅ **BEST PRACTICES**

### **1. Privacy & Compliance**

```typescript
// Don't track sensitive data
❌ trackSearch('john.doe@email.com', 'Users');
✅ trackSearch('[email]', 'Users');

// Respect user privacy
if (userHasConsentedToTracking()) {
  this.clientActivity.trackPropertySelection(id, name);
}
```

### **2. Performance**

```typescript
// ✅ Use batching (already implemented)
// ✅ Queue activities, flush periodically
// ✅ Don't block UI thread
// ✅ Use async/catchError to prevent errors

// ❌ Don't track every tiny interaction
// ❌ Don't send immediately (use batching)
```

### **3. Meaningful Data**

```typescript
// ✅ Good: Specific, actionable
trackPropertySelection(123, "Sunset Villa");

// ❌ Bad: Vague, not useful
trackClick("button");
```

---

## 📋 **QUICK REFERENCE**

### **Common Tracking Scenarios**

| User Action | Tracking Call |
|-------------|---------------|
| **Property Selected** | `trackPropertySelection(id, name)` |
| **Booking Selected** | `trackBookingSelection(id, ref)` |
| **Search Query** | `trackSearch(term, module, count)` |
| **Filter Applied** | `trackFilterChange(name, value, module)` |
| **Button Clicked** | `trackButtonClick(name, context)` |
| **Page Viewed** | `trackPageView(name, url, duration)` |
| **Form Submitted** | `trackFormSubmission(name, success)` |
| **File Downloaded** | `trackDownload(fileName, type)` |
| **Widget Interacted** | `trackWidgetInteraction(id, action)` |

---

## 🎉 **SUMMARY**

**You Can Now Track:**
- ✅ **Property Selections**: Which properties users click on
- ✅ **User Journeys**: Complete path through your app
- ✅ **Search Behavior**: What users search for
- ✅ **Feature Usage**: Which features are most used
- ✅ **User Engagement**: Time spent, scroll depth, clicks

**Data Stored:**
- All tracked in same MongoDB `UserActivityLogs` collection
- Queryable via Activity REST endpoints
- Visible in Activity Widget
- Analyzable in Analytics Widget

**Next Steps:**
1. Add backend endpoint (`LogClientActivity`)
2. Create `ClientActivityService`
3. Track property selections in property component
4. View data in MongoDB
5. Analyze with Activity Widget

**This gives you complete visibility into how users interact with your application!** 🚀
