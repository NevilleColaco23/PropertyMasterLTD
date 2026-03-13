# 🚀 Quick Reference - Widget Dashboard System

## Start the Application

```bash
# Terminal 1: Start Backend (.NET API)
cd WebApi
dotnet run

# Terminal 2: Start Frontend (Angular)
cd app
npm start
```

Navigate to: `http://localhost:4200/propertyLanding/dashboard1`

---

## What You'll See

### First Load (No Saved Dashboard)
```
✅ Dashboard Header with dropdown
✅ 4 Default KPI Cards:
   - Total Properties: 24 (+12% ↗)
   - Total Rooms: 156
   - Bookings Today: 12
   - Occupancy Rate: 78% (+5% ↗)
```

### Browser Console
```
✅ Dashboard1 component initialized
✅ Dashboard config loaded: null
✅ Widget library loaded: [4 widgets]
```

### Network Tab
```
✅ GET /api/v1/dashboard/user/1?defaultOnly=true
✅ GET /api/v1/dashboard/widgets?activeOnly=true
```

---

## API Endpoints

### Get User's Dashboard
```bash
GET https://localhost:5001/api/v1/dashboard/user/1?defaultOnly=true
Authorization: Bearer YOUR_JWT_TOKEN
```

### Get Widget Library
```bash
GET https://localhost:5001/api/v1/dashboard/widgets?activeOnly=true
Authorization: Bearer YOUR_JWT_TOKEN
```

### Save Dashboard (Postman Example)
```bash
POST https://localhost:5001/api/v1/dashboard
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "userId": 1,
  "dashboardName": "My Dashboard",
  "isDefault": true,
  "layout": {
    "columns": 12,
    "rowHeight": 80,
    "widgets": [
      {
        "widgetId": "total-properties",
        "widgetType": "kpi-card",
        "position": { "x": 0, "y": 0, "width": 3, "height": 2 },
        "settings": {
          "title": "Total Properties",
          "icon": "hotel",
          "color": "#1976d2"
        }
      }
    ]
  }
}
```

---

## File Locations

### Frontend (Angular)
```
app/src/app/
├── models/dashboard.models.ts
├── services/dashboard.service.ts
├── widgets/kpi-card-widget/kpi-card-widget.component.ts
└── dashboard/dashboard1/
    ├── dashboard1.component.ts
    ├── dashboard1.component.html
    └── dashboard1.component.css
```

### Backend (C#)
```
classfiles/
├── Domain/Dashboard/DashboardModels.cs
├── Application/Dashboard/
│   ├── DTOs/DashboardDTOs.cs
│   ├── Commands/DashboardCommandHandlers.cs
│   └── Queries/DashboardQueryHandlers.cs
└── WebApi/API/V1/DashboardController.cs
```

### Database (MongoDB)
```
Collections:
- DashboardConfigurations  (empty - will populate when users save)
- WidgetLibrary           (4 widgets seeded ✅)
- DashboardTemplates      (empty - optional)
```

---

## Common Tasks

### Add a New Widget to Library
```javascript
// MongoDB Compass - run in mongosh tab
db.WidgetLibrary.insertOne({
  WidgetId: "revenue-chart",
  WidgetType: "chart",
  Name: "Revenue Chart",
  Description: "Monthly revenue visualization",
  Icon: "trending_up",
  Category: "Analytics",
  DefaultSettings: { chartType: "line" },
  DefaultSize: { width: 6, height: 4 },
  MinSize: { width: 4, height: 3 },
  MaxSize: { width: 12, height: 6 },
  RequiredPermissions: ["dashboard.view"],
  IsActive: true,
  CreatedAt: new Date()
});
```

### Check JWT Token
```javascript
// Browser Console
const token = localStorage.getItem('token');
if (token) {
  const payload = JSON.parse(atob(token.split('.')[1]));
  console.log('User ID:', payload.UserId);
  console.log('Expires:', new Date(payload.exp * 1000));
}
```

### Debug Dashboard Loading
```javascript
// Browser Console - check component state
const component = ng.getComponent(document.querySelector('app-dashboard1'));
console.log('Loading:', component.loading);
console.log('Config:', component.dashboardConfig);
console.log('KPI Cards:', component.kpiCards);
console.log('Widget Library:', component.widgetLibrary);
```

---

## Troubleshooting

### Issue: "Failed to load dashboard"
**Cause**: API not running or wrong URL  
**Fix**: Check `app/src/app/environments/environment.ts`
```typescript
export const environment = {
  apiUrl: 'https://localhost:5001' // ← Verify this
};
```

### Issue: No widgets showing
**Cause**: API returning 401 Unauthorized  
**Fix**: Check JWT token exists
```javascript
localStorage.getItem('token') // Should return a token
```

### Issue: CORS error
**Cause**: Backend not allowing frontend origin  
**Fix**: Check backend CORS settings
```csharp
// WebApi/CORS/CorsStartup.cs
options.AddDefaultPolicy(policy => {
    policy.WithOrigins("http://localhost:4200")
          .AllowAnyHeader()
          .AllowAnyMethod();
});
```

### Issue: "cloudinary" property error (pre-existing)
**Status**: ⚠️ Not related to dashboard  
**Impact**: None on dashboard functionality  
**Fix**: Add to environment.ts if needed:
```typescript
cloudinary: {
  cloudName: 'your-cloud-name',
  uploadPreset: 'your-preset'
}
```

---

## Next Steps Checklist

### Test Phase 1 ✅
- [ ] Start both backend and frontend
- [ ] Navigate to dashboard
- [ ] Check browser console for logs
- [ ] Verify 4 KPI cards render
- [ ] Check network tab for API calls
- [ ] Test dashboard type selector dropdown

### Phase 2: More Widgets
- [ ] Create chart-widget.component.ts
- [ ] Create calendar-widget.component.ts
- [ ] Create list-widget.component.ts
- [ ] Update dashboard to render all types

### Phase 3: Customization
- [ ] Install: `npm install angular-gridster2`
- [ ] Add edit mode toggle
- [ ] Implement drag-and-drop
- [ ] Add save button
- [ ] Create widget picker modal

### Phase 4: Real Data
- [ ] Create property count API
- [ ] Create room count API
- [ ] Create booking count API
- [ ] Create occupancy API
- [ ] Add auto-refresh timer

---

## Key Files to Remember

| Purpose | File | Status |
|---------|------|--------|
| TypeScript Models | `app/src/app/models/dashboard.models.ts` | ✅ Created |
| API Service | `app/src/app/services/dashboard.service.ts` | ✅ Created |
| Widget Component | `app/src/app/widgets/kpi-card-widget/...` | ✅ Created |
| Dashboard Component | `app/src/app/dashboard/dashboard1/...` | ✅ Updated |
| Backend Controller | `WebApi/API/V1/DashboardController.cs` | ✅ Created |
| Command Handlers | `classfiles/.../Commands/DashboardCommandHandlers.cs` | ✅ Created |
| Query Handlers | `classfiles/.../Queries/DashboardQueryHandlers.cs` | ✅ Created |
| Domain Models | `classfiles/Domain/Dashboard/DashboardModels.cs` | ✅ Created |
| MongoDB Seed Script | `MongoDB_Compass_Dashboard_Seed.js` | ✅ Run in Compass |

---

## Support Documentation

1. **PHASE1_COMPLETE_TESTING_GUIDE.md** - How to test the implementation
2. **ARCHITECTURE_OVERVIEW.md** - System architecture diagrams
3. **ANGULAR_FRONTEND_PHASE1_SUMMARY.md** - Phase 1 detailed summary
4. **WIDGET_DASHBOARD_IMPLEMENTATION_GUIDE.md** - Complete implementation guide
5. **QUICK_START_DASHBOARD_API.md** - API testing guide
6. **DASHBOARD_DEPENDENCY_INJECTION_FIX.md** - Technical fix explanation

---

**🎉 You're all set! Run the app and see your dashboard in action!**
