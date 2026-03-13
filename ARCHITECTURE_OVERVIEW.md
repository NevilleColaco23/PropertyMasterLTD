# Widget-Based Dashboard - Architecture Overview

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         FRONTEND (Angular 18)                        │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │         Dashboard1Component (Smart Component)              │    │
│  │  - Manages state (loading, dashboardConfig, kpiCards)       │    │
│  │  - Calls DashboardService on init                          │    │
│  │  - Handles errors and loading states                       │    │
│  │  - Parses JWT token for userId                             │    │
│  └────────────────┬───────────────────────────────────────────┘    │
│                   │                                                  │
│                   │ uses                                             │
│                   ↓                                                  │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │          DashboardService (API Layer)                       │    │
│  │  ✅ getDashboardByUserId(userId, defaultOnly)              │    │
│  │  ✅ getUserDashboards(userId)                              │    │
│  │  ✅ saveDashboard(request)                                 │    │
│  │  ✅ deleteDashboard(id, userId)                            │    │
│  │  ✅ setDefaultDashboard(id, userId)                        │    │
│  │  ✅ getWidgetLibrary(params)                               │    │
│  │  ✅ getDashboardTemplates(params)                          │    │
│  │  ✅ resetDashboardToTemplate(templateId, userId)           │    │
│  └────────────────┬───────────────────────────────────────────┘    │
│                   │                                                  │
│                   │ renders                                          │
│                   ↓                                                  │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │       KpiCardWidgetComponent (Dumb Component)              │    │
│  │  - @Input() data: KpiCardData                              │    │
│  │  - Pure presentation component                             │    │
│  │  - Displays icon, title, value, trend                      │    │
│  │  - Hover animations                                         │    │
│  └────────────────────────────────────────────────────────────┘    │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │              TypeScript Models                              │    │
│  │  - DashboardConfiguration                                   │    │
│  │  - WidgetLibraryItem                                        │    │
│  │  - DashboardTemplate                                        │    │
│  │  - WidgetConfiguration, WidgetPosition                      │    │
│  └────────────────────────────────────────────────────────────┘    │
│                                                                       │
└───────────────────────────┬─────────────────────────────────────────┘
                            │
                            │ HTTP Requests
                            │ (JWT Bearer Token)
                            ↓
┌─────────────────────────────────────────────────────────────────────┐
│                      BACKEND (.NET 6 / C#)                           │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │         DashboardController (API/V1)                        │    │
│  │  [Authorize]                                                │    │
│  │  ✅ GET  /api/v1/dashboard/user/{userId}                    │    │
│  │  ✅ GET  /api/v1/dashboard/user/{userId}/all                │    │
│  │  ✅ POST /api/v1/dashboard                                  │    │
│  │  ✅ DELETE /api/v1/dashboard/{id}                           │    │
│  │  ✅ PUT  /api/v1/dashboard/{id}/default                     │    │
│  │  ✅ GET  /api/v1/dashboard/widgets                          │    │
│  │  ✅ GET  /api/v1/dashboard/templates                        │    │
│  │  ✅ POST /api/v1/dashboard/reset-to-template                │    │
│  └────────────────┬───────────────────────────────────────────┘    │
│                   │                                                  │
│                   │ uses MediatR                                     │
│                   ↓                                                  │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │            CQRS Handlers (Application Layer)                │    │
│  │                                                              │    │
│  │  Commands:                                                   │    │
│  │  ✅ SaveDashboardConfigurationCommandHandler                │    │
│  │  ✅ DeleteDashboardConfigurationCommandHandler              │    │
│  │  ✅ SetDefaultDashboardCommandHandler                       │    │
│  │  ✅ ResetDashboardToTemplateCommandHandler                  │    │
│  │                                                              │    │
│  │  Queries:                                                    │    │
│  │  ✅ GetDashboardByUserIdQueryHandler                        │    │
│  │  ✅ GetUserDashboardsQueryHandler                           │    │
│  │  ✅ GetWidgetLibraryQueryHandler                            │    │
│  │  ✅ GetDashboardTemplatesQueryHandler                       │    │
│  └────────────────┬───────────────────────────────────────────┘    │
│                   │                                                  │
│                   │ accesses                                         │
│                   ↓                                                  │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │              IMongoDatabase                                 │    │
│  │  - Direct collection access                                 │    │
│  │  - GetCollection<T>(collectionName)                         │    │
│  └────────────────┬───────────────────────────────────────────┘    │
│                   │                                                  │
└───────────────────┼──────────────────────────────────────────────────┘
                    │
                    │ MongoDB Driver
                    ↓
┌─────────────────────────────────────────────────────────────────────┐
│                        DATABASE (MongoDB)                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │  DashboardConfigurations Collection                         │    │
│  │  - UserId, DashboardName, IsDefault                         │    │
│  │  - Layout { columns, rowHeight, widgets[] }                 │    │
│  │  - CreatedAt, UpdatedAt                                     │    │
│  │  Indexes: (UserId, IsDefault), (UserId, DashboardName)      │    │
│  │  Status: ⚠️ EMPTY (will populate when users save)           │    │
│  └────────────────────────────────────────────────────────────┘    │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │  WidgetLibrary Collection                                   │    │
│  │  - WidgetId, WidgetType, Name, Description                  │    │
│  │  - Icon, Category, DefaultSettings                          │    │
│  │  - DefaultSize, MinSize, MaxSize                            │    │
│  │  - RequiredPermissions, IsActive                            │    │
│  │  Indexes: (WidgetId - unique), (Category, IsActive)         │    │
│  │  Status: ✅ SEEDED (4 widgets)                              │    │
│  │    • total-properties                                        │    │
│  │    • total-rooms                                             │    │
│  │    • bookings-today                                          │    │
│  │    • occupancy-rate                                          │    │
│  └────────────────────────────────────────────────────────────┘    │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │  DashboardTemplates Collection                              │    │
│  │  - TemplateName, Description, RoleId                        │    │
│  │  - Layout (same structure as DashboardConfigurations)       │    │
│  │  - IsPublic, PreviewImage                                   │    │
│  │  Indexes: (RoleId), (IsPublic)                              │    │
│  │  Status: ⚠️ EMPTY (optional, for future)                    │    │
│  └────────────────────────────────────────────────────────────┘    │
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘
```

## 📊 Data Flow Example

### Scenario: User Loads Dashboard

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. User navigates to /propertyLanding/dashboard1               │
└───────────────────────────┬─────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 2. Dashboard1Component.ngOnInit()                               │
│    - Extract userId from JWT token                              │
│    - Call loadDashboard()                                       │
│    - Call loadWidgetLibrary()                                   │
└───────────────────────────┬─────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 3. DashboardService.getDashboardByUserId(userId, true)          │
│    HTTP: GET /api/v1/dashboard/user/1?defaultOnly=true          │
│    Headers: Authorization: Bearer <JWT>                         │
└───────────────────────────┬─────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 4. Backend: DashboardController.GetDashboardByUserId()          │
│    - Extract userId from route                                  │
│    - Create GetDashboardByUserIdQuery                           │
│    - Send to MediatR                                            │
└───────────────────────────┬─────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 5. GetDashboardByUserIdQueryHandler.Handle()                    │
│    - Get MongoDB collection                                     │
│    - Build filter: UserId == 1 && IsDefault == true             │
│    - Execute query                                              │
│    - Return: null (no saved dashboard yet)                      │
└───────────────────────────┬─────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 6. Dashboard1Component receives null                            │
│    - No saved dashboard found                                   │
│    - Call loadDefaultKpiCards()                                 │
│    - Create 4 KPI cards with mock data                          │
└───────────────────────────┬─────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 7. DashboardService.getWidgetLibrary({ activeOnly: true })      │
│    HTTP: GET /api/v1/dashboard/widgets?activeOnly=true          │
└───────────────────────────┬─────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 8. Backend: DashboardController.GetWidgetLibrary()              │
│    - GetWidgetLibraryQueryHandler                               │
│    - Query MongoDB WidgetLibrary collection                     │
│    - Filter: IsActive == true                                   │
│    - Return: [4 widgets array]                                  │
└───────────────────────────┬─────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 9. Dashboard1Component stores widget library                    │
│    - widgetLibrary = [4 widgets]                                │
│    - Available for future customization                         │
└───────────────────────────┬─────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│ 10. Render Dashboard                                            │
│     - Show 4 default KPI cards                                  │
│     - Display with animations                                   │
│     - Ready for user interaction                                │
└─────────────────────────────────────────────────────────────────┘
```

## 🎨 UI Components Hierarchy

```
Dashboard1Component
│
├── Dashboard Header
│   ├── Title & Subtitle
│   └── Dashboard Type Selector Dropdown
│
├── @if (loading)
│   └── Loading Spinner + Message
│
├── @if (kpiCards.length > 0)
│   └── Dashboard Grid
│       ├── KpiCardWidgetComponent (Total Properties)
│       ├── KpiCardWidgetComponent (Total Rooms)
│       ├── KpiCardWidgetComponent (Bookings Today)
│       └── KpiCardWidgetComponent (Occupancy Rate)
│
└── @if (empty state)
    └── Empty State
        ├── Icon
        ├── Title
        ├── Description
        └── "Add Widgets" Button
```

## 🔐 Security Flow

```
┌─────────────────────────────────────────────────────────────────┐
│ User Login                                                       │
│   ↓                                                              │
│ JWT Token Generated                                             │
│   ↓                                                              │
│ Token Stored in localStorage                                    │
└───────────────────────────┬─────────────────────────────────────┘
                            ↓
                ┌───────────────────────┐
                │ Every API Call        │
                └───────────┬───────────┘
                            │
         ┌──────────────────┼──────────────────┐
         ↓                  ↓                  ↓
    Extract Token    Add to Headers    Backend Validates
         │                  │                  │
         │         Authorization:              │
         │         Bearer <JWT>          [Authorize]
         │                  │             Attribute
         │                  │                  │
         └──────────────────┼──────────────────┘
                            ↓
                   ✅ Request Allowed
                   or
                   ❌ 401 Unauthorized
```

## 📦 File Structure

```
PropertyMasterV4.0/
│
├── app/src/app/
│   ├── models/
│   │   └── dashboard.models.ts              ✅ NEW
│   │
│   ├── services/
│   │   └── dashboard.service.ts             ✅ NEW
│   │
│   ├── widgets/
│   │   └── kpi-card-widget/
│   │       └── kpi-card-widget.component.ts ✅ NEW
│   │
│   └── dashboard/
│       └── dashboard1/
│           ├── dashboard1.component.ts      ✅ UPDATED
│           ├── dashboard1.component.html    ✅ UPDATED
│           └── dashboard1.component.css     ✅ UPDATED
│
└── classfiles/
    ├── Domain/Dashboard/
    │   └── DashboardModels.cs               ✅ NEW
    │
    ├── Application/
    │   ├── Dashboard/
    │   │   ├── DTOs/DashboardDTOs.cs        ✅ NEW
    │   │   ├── Commands/
    │   │   │   └── DashboardCommandHandlers.cs ✅ NEW
    │   │   └── Queries/
    │   │       └── DashboardQueryHandlers.cs   ✅ NEW
    │   │
    │   └── MongoCollections.cs              ✅ UPDATED
    │
    └── WebApi/API/V1/
        └── DashboardController.cs           ✅ NEW
```

## 🎯 Implementation Status

### ✅ Phase 1: Foundation (COMPLETE)
- [x] Backend API (8 endpoints)
- [x] CQRS handlers (4 commands + 4 queries)
- [x] MongoDB schema & seeding
- [x] Angular service layer
- [x] TypeScript models
- [x] KPI widget component
- [x] Dashboard integration

### ⏳ Phase 2: Widget Variety (TODO)
- [ ] Chart widget (line, bar, pie)
- [ ] Calendar widget (booking view)
- [ ] List widget (recent activity)
- [ ] Widget factory/renderer

### ⏳ Phase 3: Customization UI (TODO)
- [ ] Install gridster2 library
- [ ] Drag-and-drop layout
- [ ] Edit mode toggle
- [ ] Save dashboard functionality
- [ ] Widget picker modal
- [ ] Resize handles

### ⏳ Phase 4: Real Data (TODO)
- [ ] Property count API
- [ ] Room count API
- [ ] Booking count API
- [ ] Occupancy rate API
- [ ] Auto-refresh mechanism

## 🚀 Ready to Launch!

Your widget-based dashboard system is now functional! The foundation is solid:
- ✅ Backend fully implemented
- ✅ Frontend connected to API
- ✅ Database seeded with widgets
- ✅ UI components rendering

**Next**: Run `npm start` and see it in action! 🎉
