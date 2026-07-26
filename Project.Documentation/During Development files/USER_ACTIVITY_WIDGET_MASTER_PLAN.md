# 🎯 USER ACTIVITY TRACKING WIDGET - MASTER PLAN

## 📋 PROJECT OVERVIEW

**Widget Name**: **User Activity Monitor** (or "Activity Stream Widget")

**Purpose**: Real-time monitoring and historical tracking of all user actions across the property management system.

**Current State Analysis**:
- ✅ **AccessLog** exists - Tracks HTTP requests, page access
- ✅ **AuditLog** exists - Tracks data changes (Create, Update, Delete)
- ✅ **AccessLogWorker** - Background service processing access logs
- ✅ **RabbitMQ messaging** - Event-driven architecture in place

**Gap**: 
- Need to enhance tracking to capture specific user actions (button clicks, form submissions, etc.)
- Need unified view combining AccessLog + AuditLog + UserActions
- Need real-time widget to display this activity

---

## 🗺️ IMPLEMENTATION PHASES

### **PHASE 1: Enhanced Activity Tracking (Backend)** 🔧
**Duration**: 2-3 hours  
**Goal**: Extend existing logging to capture granular user actions

#### Tasks:
1. **Create UserActivityLog Domain Model**
   - New MongoDB collection: `UserActivityLogs`
   - Fields: UserId, Username, ActivityType, EntityType, EntityId, Action, Description, Metadata, Timestamp, IPAddress, UserAgent
   - Activity Types: PageView, ButtonClick, FormSubmit, Create, Update, Delete, Export, Import, Download, Upload

2. **Create ActivityType Enum**
   ```csharp
   public enum ActivityType {
       PageView, Create, Update, Delete, 
       Export, Download, Upload, Login, Logout,
       Search, Filter, Sort, Print, Email, 
       StatusChange, BulkOperation
   }
   ```

3. **Create UserActivityEvent Message Model**
   - For RabbitMQ event publishing
   - Similar structure to AccessLogEvent

4. **Create UserActivityRepository**
   - MongoDB repository implementation
   - Methods: LogActivity, GetUserActivities, GetRecentActivities, GetActivitiesByType

5. **Create UserActivityService**
   - Service layer for logging activities
   - Method: `LogActivity(userId, activityType, entityType, entityId, action, details)`

6. **Update MongoCollections.cs**
   - Add UserActivityLogs collection name

---

### **PHASE 2: Activity Logging Middleware & Extensions** 🔌
**Duration**: 2-3 hours  
**Goal**: Automatic activity capture with minimal code changes

#### Tasks:
1. **Create ActivityLoggingAttribute**
   - Custom attribute to mark controller actions for activity logging
   - Example: `[ActivityLog(ActivityType.Create, "Property")]`

2. **Create ActivityLoggingActionFilter**
   - Intercepts controller actions with ActivityLoggingAttribute
   - Automatically logs activity after successful execution

3. **Create ActivityLogger Helper**
   - Static helper class for manual activity logging
   - Extension methods for IHttpContextAccessor

4. **Update Existing Controllers**
   - Add ActivityLoggingAttribute to key actions:
     - Property CRUD operations
     - Room CRUD operations
     - Booking operations
     - User management
     - Menu permissions

5. **Create Activity Logging Configuration**
   - Settings for enabling/disabling activity logging
   - Retention policy settings

---

### **PHASE 3: Activity Query API (Backend)** 📡
**Duration**: 1-2 hours  
**Goal**: API endpoints for retrieving activity data

#### Tasks:
1. **Create Activity Queries (CQRS)**
   - `GetRecentActivitiesQuery` - Last N activities (for widget)
   - `GetUserActivitiesQuery` - Activities for specific user
   - `GetEntityActivitiesQuery` - Activities for specific entity (e.g., Property ID)
   - `GetActivityStatisticsQuery` - Aggregated stats (most active users, most common actions)

2. **Create Query Handlers**
   - Implement MediatR handlers for each query
   - Include filtering, pagination, sorting

3. **Create Activity DTOs**
   - UserActivityDTO
   - ActivitySummaryDTO
   - ActivityStatisticsDTO

4. **Create ActivityController**
   - GET `/api/v1/activity/recent?count=50` - For widget
   - GET `/api/v1/activity/user/{userId}?from=date&to=date`
   - GET `/api/v1/activity/entity/{type}/{id}`
   - GET `/api/v1/activity/statistics?period=today|week|month`

5. **Add Dashboard Activity Endpoints to DashboardController**
   - GET `/api/v1/dashboard/activity/recent`
   - GET `/api/v1/dashboard/activity/summary`

---

### **PHASE 4: Widget Backend Integration** 🎨
**Duration**: 1 hour  
**Goal**: Dashboard-specific activity data endpoints

#### Tasks:
1. **Update DashboardActivityQueryHandlers**
   - Add GetRecentUserActivityQuery handler
   - Returns formatted data for activity widget

2. **Seed Activity Widget in MongoDB**
   - Add "user-activity-stream" widget to WidgetLibrary
   - Widget configuration:
     ```javascript
     {
       WidgetId: "user-activity-stream",
       WidgetType: "activity-stream",
       Name: "User Activity Stream",
       Category: "Monitoring",
       DefaultSize: { width: 6, height: 6 }
     }
     ```

3. **Create Sample Activity Data (for testing)**
   - MongoDB script to generate fake activity logs
   - Various activity types for testing

---

### **PHASE 5: Frontend Widget Component** 🖼️
**Duration**: 2-3 hours  
**Goal**: Create the Activity Stream Widget UI

#### Tasks:
1. **Create ActivityStreamWidgetComponent**
   - Location: `app/src/app/widgets/activity-stream-widget/`
   - Shows scrollable list of recent activities
   - Real-time updates (optional: SignalR)

2. **Create Activity Models (TypeScript)**
   ```typescript
   export interface UserActivity {
     id: number;
     userId: number;
     username: string;
     activityType: string;
     action: string;
     description: string;
     timestamp: Date;
     entityType?: string;
     entityId?: number;
     metadata?: any;
   }
   ```

3. **Update DashboardService**
   - Add method: `getRecentActivities(count: number)`
   - Add method: `getUserActivities(userId: number, from?: Date, to?: Date)`

4. **Design Activity Item UI**
   - Timeline-style layout with icons
   - Color coding by activity type
   - User avatar display
   - Time ago formatting (e.g., "5 minutes ago")
   - Entity links (click to navigate)

5. **Add Filtering Options**
   - Filter by activity type
   - Filter by user
   - Filter by date range
   - Search functionality

6. **Add Real-time Updates (Optional)**
   - SignalR integration for live activity feed
   - WebSocket connection to backend

---

### **PHASE 6: Widget Features & Polish** ✨
**Duration**: 2 hours  
**Goal**: Advanced features and refinements

#### Tasks:
1. **Activity Grouping**
   - Group similar activities (e.g., "John created 5 properties")
   - Expandable groups

2. **Activity Details Modal**
   - Click activity to see full details
   - Show before/after values for updates
   - Show metadata/context

3. **Export Functionality**
   - Export activity log to CSV/Excel
   - PDF report generation

4. **Activity Statistics Panel**
   - Summary cards: Total activities today, Most active user, etc.
   - Mini charts

5. **Widget Settings**
   - Configure which activity types to show
   - Auto-refresh interval
   - Number of items to display

6. **Responsive Design**
   - Mobile-friendly layout
   - Compact view for smaller widgets

---

### **PHASE 7: Testing & Optimization** 🧪
**Duration**: 1-2 hours  
**Goal**: Ensure reliability and performance

#### Tasks:
1. **Backend Testing**
   - Unit tests for activity logging
   - Integration tests for activity queries
   - Performance testing with large datasets

2. **Frontend Testing**
   - Component unit tests
   - E2E testing with Cypress/Playwright

3. **Performance Optimization**
   - Database indexing on UserActivityLogs
   - Caching strategy for recent activities
   - Pagination for large datasets

4. **Error Handling**
   - Graceful degradation if activity service is down
   - Retry logic for failed logs

5. **Documentation**
   - API documentation
   - Widget usage guide
   - Configuration guide

---

## 📊 DATABASE SCHEMA

### UserActivityLogs Collection (MongoDB)

```javascript
{
  _id: ObjectId,
  ActivityId: Int32,  // Auto-increment ID
  UserId: Int32,
  Username: String,
  ActivityType: String,  // Enum: PageView, Create, Update, Delete, etc.
  EntityType: String,    // e.g., "Property", "Room", "Booking"
  EntityId: Int32,       // ID of the affected entity
  Action: String,        // e.g., "Created new property", "Updated room status"
  Description: String,   // Detailed description
  Metadata: Object,      // Additional context (JSON)
  Timestamp: ISODate,
  IPAddress: String,
  UserAgent: String,
  SessionId: String,
  TraceId: String,       // For correlation with other logs
  IsSuccess: Boolean,
  ErrorMessage: String   // If IsSuccess = false
}
```

**Indexes**:
- `{ UserId: 1, Timestamp: -1 }` - User activity history
- `{ Timestamp: -1 }` - Recent activities
- `{ EntityType: 1, EntityId: 1, Timestamp: -1 }` - Entity audit trail
- `{ ActivityType: 1, Timestamp: -1 }` - Activity type filtering

---

## 🎨 WIDGET UI MOCKUP (Conceptual)

```
╔══════════════════════════════════════════════════════════╗
║  👤 User Activity Stream                    ⚙️ 🔄        ║
╠══════════════════════════════════════════════════════════╣
║  🔍 Filter: [All Activities ▼]  [All Users ▼]  [Today ▼]║
╠══════════════════════════════════════════════════════════╣
║  🟢 John Doe                             2 minutes ago   ║
║     Created property "Sunset Villa"                      ║
║     📍 Properties > Add New                              ║
╟──────────────────────────────────────────────────────────╢
║  🔵 Sarah Smith                          5 minutes ago   ║
║     Updated room "Room 101" status to Available          ║
║     📍 Rooms > Edit                                      ║
╟──────────────────────────────────────────────────────────╢
║  🟠 Mike Johnson                        10 minutes ago   ║
║     Deleted booking #B12345                              ║
║     📍 Bookings > Manage                                 ║
╟──────────────────────────────────────────────────────────╢
║  🟣 Admin User                          15 minutes ago   ║
║     Exported property report (25 records)                ║
║     📍 Properties > Reports                              ║
╟──────────────────────────────────────────────────────────╢
║  🟢 John Doe                            20 minutes ago   ║
║     Logged in from 192.168.1.100                         ║
║     📍 Authentication                                    ║
╚══════════════════════════════════════════════════════════╝
        [Load More]  [Export to CSV]
```

---

## 🚀 QUICK START DECISION MATRIX

**Option A - Full Implementation** (All 7 Phases)
- ⏱️ Total Time: 12-15 hours
- ✅ Complete solution with all features
- ✅ Real-time updates, filtering, export
- ✅ Production-ready

**Option B - MVP (Phases 1, 3, 5)** 
- ⏱️ Total Time: 5-7 hours
- ✅ Basic activity logging
- ✅ Simple widget display
- ✅ Manual logging (no middleware)
- ⚠️ No real-time, basic UI

**Option C - Enhanced MVP (Phases 1, 2, 3, 5)**
- ⏱️ Total Time: 8-10 hours
- ✅ Automatic activity logging
- ✅ Functional widget
- ✅ Good UI/UX
- ⚠️ No advanced features

---

## 🎯 RECOMMENDED APPROACH

**Start with ENHANCED MVP (Option C)** - Phases 1, 2, 3, 5

### Why?
1. Delivers core functionality quickly
2. Automatic logging = less maintenance
3. Good user experience
4. Can add Phases 6-7 later based on feedback

### Execution Order:
1. **Phase 1** → Build foundation (activity tracking model)
2. **Phase 3** → Create API (so we can test immediately)
3. **Phase 2** → Add middleware (automate logging)
4. **Phase 5** → Build widget (visualize data)

---

## 📝 NEXT STEPS - YOUR DECISION

Please choose one of the following:

### Choice 1: **Full Implementation** (All Phases)
- "Let's build the complete solution with all features"

### Choice 2: **Enhanced MVP** (Recommended)
- "Let's start with Phases 1, 2, 3, 5 and add features later"

### Choice 3: **Basic MVP**
- "Let's build the simplest version first (Phases 1, 3, 5)"

### Choice 4: **Custom**
- "I want to pick specific phases: [specify phases]"

---

## 🔗 INTEGRATION POINTS

**Existing Systems to Leverage**:
- ✅ AccessLogWorker (background processing)
- ✅ RabbitMQ (event messaging)
- ✅ MediatR (CQRS pattern)
- ✅ MongoDB (data storage)
- ✅ Dashboard infrastructure (widget framework)

**New Dependencies**:
- None! Uses existing tech stack

---

## 📋 DELIVERABLES (Enhanced MVP)

**Backend**:
- ✅ UserActivityLog domain model
- ✅ UserActivityRepository
- ✅ Activity logging attribute & filter
- ✅ Activity API endpoints (4 endpoints)
- ✅ MongoDB collection with indexes

**Frontend**:
- ✅ ActivityStreamWidgetComponent
- ✅ Activity models & interfaces
- ✅ Service methods in DashboardService
- ✅ Timeline-style UI with filtering

**Database**:
- ✅ Widget seed script for MongoDB
- ✅ Sample activity data for testing

**Documentation**:
- ✅ API documentation
- ✅ Usage guide
- ✅ Configuration guide

---

**Ready to proceed? Which option do you choose?** 🚀
