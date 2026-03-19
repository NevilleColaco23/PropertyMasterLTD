# 🏨 Room Planner Sample Data - Setup Guide

## 📋 Overview
This guide will help you insert sample data into your MongoDB database so you can see the Room Planner in action!

---

## 🚀 Quick Setup (3 Steps)

### **Step 1: Connect to MongoDB**
Open MongoDB Compass, Studio 3T, or MongoDB Shell and connect to your database.

### **Step 2: Run the Sample Data Script**
1. Open the file: `SampleData_RoomPlanner.js`
2. Copy the entire contents
3. Run it in MongoDB Shell or your MongoDB client

**MongoDB Shell:**
```bash
mongosh "your-connection-string"
use PropertyMaster
load("SampleData_RoomPlanner.js")
```

**MongoDB Compass:**
1. Click on your database
2. Open "MongoSH" tab at bottom
3. Paste the script content
4. Press Enter

### **Step 3: Verify and Test**
1. Start your backend API
2. Navigate to Dashboard → Room Planner tab
3. Select **Property 1** in the property selector
4. You should see 7 rooms with bookings!

---

## 📊 What Gets Inserted

### **Rooms (7 total)**
| Room | Type | Floor | Status | Price/Night |
|------|------|-------|--------|-------------|
| 101 | Deluxe | 1 | Available | $120 |
| 102 | Standard | 1 | Available | $80 |
| 103 | Standard | 1 | Available | $80 |
| 201 | Suite | 2 | Available | $250 |
| 202 | Deluxe | 2 | Available | $120 |
| 301 | Presidential Suite | 3 | Available | $500 |
| 302 | Deluxe | 3 | **Maintenance** | $120 |

### **Bookings (6 total)**
| Booking ID | Room | Status | Check-in | Check-out |
|------------|------|--------|----------|-----------|
| TEST001 | 101 | Occupied | 5 days ago | In 2 days |
| TEST002 | 102 | Check-in Today | Today | In 3 days |
| TEST003 | 103 | Check-out Today | 7 days ago | Today |
| TEST004 | 201 | Future | In 5 days | In 12 days |
| TEST005 | 301 | Occupied | 10 days ago | In 18 days |
| TEST006 | 202 | Future | In 10 days | In 14 days |

---

## 🎨 What You'll See in Room Planner

### **Visual Representation:**
- **🟢 Green cells** = Room available
- **🔴 Red cells** = Room occupied
- **🔵 Blue cells** = Check-in day (with arrow →)
- **🟠 Orange cells** = Check-out day (with arrow ←)
- **🟣 Purple striped cells** = Room under maintenance

### **Guest Names:**
Guest names will appear on occupied cells (if you have guests in your database)

### **Statistics Cards:**
- Total Rooms: 7
- Occupied Today: 3
- Available Today: 3 (1 is in maintenance)
- Occupancy Rate: ~43%

---

## 🔧 Schema Compatibility

The updated `GetRoomsMongoQuery` now supports **both** PascalCase and camelCase field names:

### **Supported Field Mappings:**
| Query Output | Your Schema (PascalCase) | Alternative (camelCase) |
|--------------|--------------------------|-------------------------|
| roomId | RoomId | roomId |
| roomNumber | RoomCode | roomNumber |
| roomName | RoomName | roomName |
| propertyId | PropertyId | propertyId |
| isActive | Active | isActive |

### **Features:**
✅ Works with your existing Room schema (PascalCase)  
✅ Also works with camelCase fields  
✅ Excludes soft-deleted rooms (IsDeleted = true)  
✅ Joins with Property collection for property names  
✅ Provides fallback values for missing fields  

---

## 🐛 Troubleshooting

### **No rooms showing?**
1. Check if Property 1 is selected in property selector
2. Verify rooms inserted: `db.Room.find({ PropertyId: 1 })`
3. Check browser console for API errors
4. Verify backend is running and MongoDB is connected

### **No bookings showing?**
1. Verify bookings inserted: `db.Bookings.find({ propertyId: 1 })`
2. Check date range - bookings span current month
3. Look for console logs: `🔧 Executing aggregation...`

### **API errors?**
Check backend logs for:
- MongoDB connection issues
- Collection not found errors
- Aggregation pipeline errors

---

## 🧹 Clean Up (Optional)

To remove sample data:

```javascript
// Remove sample rooms
db.Room.deleteMany({ RoomId: { $in: [1001, 1002, 1003, 2001, 2002, 3001, 3002] } });

// Remove sample bookings
db.Bookings.deleteMany({ bookingId: { $in: ["TEST001", "TEST002", "TEST003", "TEST004", "TEST005", "TEST006"] } });
```

---

## 📝 Customization

### **Add More Rooms:**
Copy the room template and change:
- `_id` (unique)
- `RoomId` (unique)
- `roomNumber` / `RoomCode`
- `PropertyId` (your property ID)

### **Add More Bookings:**
Copy the booking template and change:
- `_id` (unique)
- `bookingId` (unique)
- `roomNumber` (must match existing room)
- `propertyId` (must match room's property)
- Dates (check-in/check-out)

---

## 🎉 Next Steps

Once you see data in the Room Planner:

1. ✅ **Test Navigation** - Use prev/next month buttons
2. ✅ **Test Filtering** - Select different properties
3. ✅ **Test Interactions** - Hover over cells for tooltips
4. ✅ **Check Stats** - Verify occupancy calculations
5. ✅ **Mobile View** - Test responsive design

---

## 💡 Pro Tips

1. **Real Data**: Once working, replace test data with real bookings
2. **Property Filter**: Always test with property selector
3. **Date Range**: Bookings must be in the visible month to show
4. **Active Status**: Only active bookings with `Status: "Active"` display
5. **Guest Names**: Link guest data to see names on booking cells

---

**Need help?** Check the `ROOM_PLANNER_IMPLEMENTATION.md` for full documentation!
