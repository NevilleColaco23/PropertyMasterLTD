# 📊 Room Planner - Data Setup Complete!

## ✅ What Was Done

### **1. Created Sample Data Scripts**
- ✨ **`SampleData_RoomPlanner.js`** - Full dataset (7 rooms, 6 bookings)
- ✨ **`QuickInsert_RoomPlanner.js`** - Minimal dataset (3 rooms, 2 bookings)
- ✨ **`ROOM_PLANNER_SAMPLE_DATA_GUIDE.md`** - Detailed setup guide

### **2. Fixed MongoDB Query**
- 🔧 Updated `GetRoomsMongoQuery.cs` to support **both** PascalCase and camelCase
- 🔧 Now works with your actual Room schema
- 🔧 Handles soft-deleted rooms (excludes `IsDeleted: true`)
- 🔧 Maps fields correctly (PropertyId → propertyId, RoomCode → roomNumber, etc.)

---

## 🚀 How to Use

### **Option 1: Quick Test (Fastest)**
1. Open MongoDB Shell or Compass
2. Copy contents of `QuickInsert_RoomPlanner.js`
3. Paste and run
4. Navigate to Dashboard → Room Planner
5. Select Property 1

**Result**: 3 rooms with 2 bookings

### **Option 2: Full Demo (Recommended)**
1. Open MongoDB Shell or Compass
2. Copy contents of `SampleData_RoomPlanner.js`
3. Paste and run
4. Navigate to Dashboard → Room Planner
5. Select Property 1

**Result**: 7 rooms with 6 bookings showing various states

---

## 📊 Sample Data Overview

### **Quick Insert (3 rooms, 2 bookings)**
```
Room 101 (Deluxe) - OCCUPIED (3 days ago → 4 days from now)
Room 102 (Standard) - AVAILABLE
Room 103 (Suite) - FUTURE BOOKING (2 days from now → 7 days from now)
```

### **Full Insert (7 rooms, 6 bookings)**
```
Floor 1:
  Room 101 (Deluxe) - OCCUPIED
  Room 102 (Standard) - CHECK-IN TODAY
  Room 103 (Standard) - CHECK-OUT TODAY

Floor 2:
  Room 201 (Suite) - FUTURE BOOKING
  Room 202 (Deluxe) - FUTURE BOOKING

Floor 3:
  Room 301 (Presidential Suite) - LONG-TERM OCCUPIED
  Room 302 (Deluxe) - MAINTENANCE
```

---

## 🎨 Visual Guide

### **What You'll See:**

```
╔═══════════════════════════════════════════════════════════╗
║  ROOM PLANNER - GANTT CHART VIEW                          ║
╠═══════════════════════════════════════════════════════════╣
║  Room  │ 1  2  3  4  5  6  7  8  9  10 11 12 13 14 ...   ║
║────────┼──────────────────────────────────────────────    ║
║  101   │ 🔴 🔴 🔴 🔴 🔴 🟢 🟢 🟢 🟢 🟢 🟢 🟢 🟢 🟢 ...   ║
║  102   │ 🔵 🔴 🔴 🔴 🟢 🟢 🟢 🟢 🟢 🟢 🟢 🟢 🟢 🟢 ...   ║
║  103   │ 🟠 🟢 🟢 🟢 🟢 🟢 🟢 🟢 🟢 🟢 🟢 🟢 🟢 🟢 ...   ║
║  201   │ 🟢 🟢 🟢 🟢 🟢 🔵 🔴 🔴 🔴 🔴 🔴 🔴 🟠 🟢 ...   ║
║  301   │ 🔴 🔴 🔴 🔴 🔴 🔴 🔴 🔴 🔴 🔴 🔴 🔴 🔴 🔴 ...   ║
║  302   │ 🟣 🟣 🟣 🟣 🟣 🟣 🟣 🟣 🟣 🟣 🟣 🟣 🟣 🟣 ...   ║
╚═══════════════════════════════════════════════════════════╝

Legend:
🟢 Available    🔴 Occupied    🔵 Check-in    🟠 Check-out    🟣 Maintenance
```

---

## 🔧 MongoDB Query Updates

### **Schema Mapping**
The query now handles your **PascalCase** schema:

| Your Field | Query Maps To | Fallback |
|------------|---------------|----------|
| `PropertyId` | `propertyId` | - |
| `RoomCode` | `roomNumber` | - |
| `RoomName` | `roomName` | `RoomCode` |
| `Active` | `isActive` | `true` |
| `IsDeleted` | (filtered out) | - |

### **Key Features**
✅ Supports both PascalCase and camelCase  
✅ Excludes soft-deleted rooms  
✅ Joins with Property collection  
✅ Provides sensible defaults  
✅ Handles missing fields gracefully  

---

## 🐛 Troubleshooting

### **Issue: No rooms showing**
**Solution:**
```javascript
// Check if rooms exist
db.Room.find({ PropertyId: 1, IsDeleted: { $ne: true } }).count()

// Should return > 0
```

### **Issue: No bookings showing**
**Solution:**
```javascript
// Check if bookings exist
db.Bookings.find({ 
  propertyId: 1, 
  Status: "Active",
  checkInDate: { $gte: new Date(new Date().getFullYear(), new Date().getMonth(), 1) }
}).count()

// Should return > 0
```

### **Issue: Wrong property selected**
**Solution:**
- Make sure **Property 1** is selected in property dropdown
- Or change `PropertyId: 1` to your actual property ID in the scripts

### **Issue: API errors**
**Check:**
1. Backend is running (`dotnet run` or F5)
2. MongoDB is connected
3. Browser console for errors (F12)
4. Backend logs for aggregation output

---

## 📝 Next Steps

### **1. Test Room Planner**
- ✅ Navigate months (prev/next buttons)
- ✅ View today indicator
- ✅ Hover over cells for tooltips
- ✅ Check statistics cards
- ✅ Test on mobile/tablet

### **2. Verify Backend**
Check backend console for:
```
🔧 Room Planner Query: GetRoomsMongoQuery - UserId: 1, PropertyIds: [1], ActiveOnly: True
🔧 Executing aggregation with 4 stages
✅ Aggregation returned 7 room documents
✅ Mapped 7 rooms for room planner
```

### **3. Add Real Data**
Once working:
1. Use real property IDs
2. Add actual rooms from your system
3. Real bookings will automatically display
4. Guest names will show (if guests exist)

---

## 🎉 Success Criteria

Your Room Planner is working if:

✅ Rooms are displayed in the grid  
✅ Room numbers show in left column  
✅ Days of month show in header  
✅ Booking cells are color-coded correctly  
✅ Statistics show accurate counts  
✅ Month navigation works  
✅ Property filtering works  
✅ Tooltips show on hover  
✅ No console errors  

---

## 📚 Documentation

- **Setup Guide**: `ROOM_PLANNER_SAMPLE_DATA_GUIDE.md`
- **Implementation**: `ROOM_PLANNER_IMPLEMENTATION.md`
- **Sample Data**: `SampleData_RoomPlanner.js`
- **Quick Test**: `QuickInsert_RoomPlanner.js`

---

## 💡 Tips

1. **Start with Quick Insert** for fastest testing
2. **Use Full Insert** for demo/presentation
3. **Property ID matters** - make sure it's selected
4. **Date range matters** - bookings must be in current month
5. **Active status matters** - only `Status: "Active"` bookings show

---

## 🔄 Clean Up

To remove sample data:

```javascript
// Remove sample rooms
db.Room.deleteMany({ 
  RoomId: { $in: [1001, 1002, 1003, 2001, 2002, 3001, 3002] } 
});

// Remove sample bookings
db.Bookings.deleteMany({ 
  bookingId: { $regex: /^(TEST|QUICK)/ } 
});
```

---

**🎊 Enjoy your beautiful Room Planner!** 🏨
