# Quick Start: Testing Dashboard API

## 🚀 Setup Steps

### 1. Seed the Database

Run this in MongoDB (mongosh or Compass):

```bash
# Connect to your database
mongosh your-database-name

# Then paste and run the contents of:
MongoDB_DashboardCustomization_Schema.js
```

This creates:
- Widget library with 4 default widgets
- Indexes for performance
- Collections: DashboardConfigurations, WidgetLibrary, DashboardTemplates

---

### 2. Test API Endpoints

#### Get Your User ID
```javascript
// In browser console (F12) on your app
const token = localStorage.getItem('auth_tokenString');
const jwt = token.replace('Bearer ', '');
const payload = JSON.parse(atob(jwt.split('.')[1]));
console.log('User ID:', payload.sub);
```

#### Test with Postman/cURL

**Get Widget Library**
```bash
curl -X GET "https://localhost:5001/api/v1/dashboard/widgets" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Get User's Dashboard**
```bash
curl -X GET "https://localhost:5001/api/v1/dashboard/user/1?defaultOnly=true" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Create Dashboard**
```bash
curl -X POST "https://localhost:5001/api/v1/dashboard" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "dashboardName": "My First Custom Dashboard",
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
        },
        {
          "widgetId": "total-rooms",
          "widgetType": "kpi-card",
          "position": { "x": 3, "y": 0, "width": 3, "height": 2 },
          "settings": {
            "title": "Total Rooms",
            "icon": "meeting_room",
            "color": "#1976d2"
          }
        },
        {
          "widgetId": "bookings-today",
          "widgetType": "kpi-card",
          "position": { "x": 6, "y": 0, "width": 3, "height": 2 },
          "settings": {
            "title": "Bookings Today",
            "icon": "event_available",
            "color": "#1976d2"
          }
        },
        {
          "widgetId": "occupancy-rate",
          "widgetType": "kpi-card",
          "position": { "x": 9, "y": 0, "width": 3, "height": 2 },
          "settings": {
            "title": "Occupancy Rate",
            "icon": "people",
            "color": "#1976d2"
          }
        }
      ]
    }
  }'
```

---

### 3. Verify in MongoDB

```javascript
// Check if dashboard was created
db.DashboardConfigurations.find({ UserId: 1 }).pretty()

// Check widget library
db.WidgetLibrary.countDocuments()
// Should return: 4

// View all widgets
db.WidgetLibrary.find().pretty()
```

---

## 📋 API Endpoints Summary

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/v1/dashboard/user/{userId}` | Get user's default dashboard |
| GET | `/api/v1/dashboard/user/{userId}/all` | Get all user's dashboards |
| POST | `/api/v1/dashboard` | Create or update dashboard |
| DELETE | `/api/v1/dashboard/{id}` | Delete dashboard |
| POST | `/api/v1/dashboard/{id}/set-default` | Set as default dashboard |
| POST | `/api/v1/dashboard/reset-to-template` | Reset to template |
| GET | `/api/v1/dashboard/widgets` | Get widget library |
| GET | `/api/v1/dashboard/templates` | Get dashboard templates |

---

## 🧪 Test Scenarios

### Scenario 1: New User
```
1. User has no dashboard
2. GET /api/v1/dashboard/user/1
   → Returns 404 "Dashboard not found"
3. POST /api/v1/dashboard
   → Creates first dashboard
4. GET /api/v1/dashboard/user/1
   → Returns newly created dashboard
```

### Scenario 2: Multiple Dashboards
```
1. User creates "Analytics Dashboard"
2. User creates "Operations Dashboard"
3. User creates "Reports Dashboard"
4. GET /api/v1/dashboard/user/1/all
   → Returns all 3 dashboards
5. POST /api/v1/dashboard/{id}/set-default
   → Sets "Operations Dashboard" as default
6. GET /api/v1/dashboard/user/1?defaultOnly=true
   → Returns "Operations Dashboard"
```

### Scenario 3: Update Dashboard
```
1. GET /api/v1/dashboard/user/1
   → Get current dashboard (includes ID)
2. POST /api/v1/dashboard
   Body: { id: "existing-id", ... modified layout }
   → Updates existing dashboard
3. GET /api/v1/dashboard/user/1
   → Returns updated dashboard
```

---

## 🔍 Troubleshooting

### Issue: 404 on Widget Library
**Solution:** Run seed script to populate WidgetLibrary collection

### Issue: 401 Unauthorized
**Solution:** Check Authorization header has valid token

### Issue: Can't create dashboard
**Solution:** Verify userId matches token's user ID

### Issue: Dashboard not saving
**Check:**
1. MongoDB connection working?
2. Collection name correct? (DashboardConfigurations)
3. User has write permissions?
4. Check API logs for errors

---

## 📊 Sample Data

### Default 12-Column Grid Layout
```json
{
  "columns": 12,
  "rowHeight": 80,
  "widgets": [
    { "position": { "x": 0, "y": 0, "width": 3, "height": 2 } },  // Top-left
    { "position": { "x": 3, "y": 0, "width": 3, "height": 2 } },  // Top-center-left
    { "position": { "x": 6, "y": 0, "width": 3, "height": 2 } },  // Top-center-right
    { "position": { "x": 9, "y": 0, "width": 3, "height": 2 } },  // Top-right
    { "position": { "x": 0, "y": 2, "width": 12, "height": 4 } }  // Full-width below
  ]
}
```

### Widget Categories
- `KPI` - Key Performance Indicators (cards with numbers)
- `Analytics` - Charts and graphs
- `Bookings` - Booking-related widgets
- `Calendar` - Calendar views
- `List` - List/table widgets

---

## 🎯 Next Steps

After verifying API works:

1. ✅ Confirm widget library seeded
2. ✅ Test create dashboard via API
3. ✅ Test update dashboard via API
4. ✅ Verify data in MongoDB
5. 🔜 Implement Angular service
6. 🔜 Create dashboard builder UI
7. 🔜 Connect to API
8. 🔜 Test end-to-end

---

## 💡 Tips

1. **Use Swagger UI** for easy API testing
   - Navigate to: `https://localhost:5001/swagger`
   - All endpoints are documented there

2. **Postman Collection**
   - Import the endpoints into Postman
   - Create environment variables for baseUrl and token

3. **MongoDB Compass**
   - Great for visualizing dashboard data
   - Can manually edit documents for testing

4. **Browser DevTools**
   - Network tab shows API calls
   - Console shows any JavaScript errors
   - Application tab shows localStorage (token)

---

**Backend is ready! 🎉 Now let's build the frontend!**
