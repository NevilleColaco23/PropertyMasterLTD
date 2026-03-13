// ============================================
// JWT Token Debugging Script
// ============================================
// Run this in your browser's Developer Console (F12) to:
//   - Verify you're logged in
//   - Check if your token has expired
//   - Find your User ID for database queries
// 
// When to use:
//   - Menus not loading and you get 401 errors
//   - Need to verify which user is logged in
//   - Want to check token expiration time
// ============================================

console.log("🔍 Checking User ID from JWT Token...");
console.log("─────────────────────────────────────");

// Get the token from localStorage
const tokenString = localStorage.getItem('auth_tokenString');

if (!tokenString) {
  console.error("❌ ERROR: No auth token found in localStorage!");
  console.log("This means you are not logged in.");
  console.log("Please login first, then run this script again.");
} else {
  console.log("✅ Token found in localStorage");
  
  try {
    // Remove "Bearer " prefix if present
    const jwt = tokenString.replace('Bearer ', '').trim();
    
    // Decode JWT (split by . and decode middle part)
    const parts = jwt.split('.');
    
    if (parts.length !== 3) {
      console.error("❌ ERROR: Invalid JWT format!");
      console.log("Expected format: header.payload.signature");
      console.log("Actual parts:", parts.length);
    } else {
      const payload = parts[1];
      const decodedPayload = JSON.parse(atob(payload));
      
      console.log("\n✅ JWT Decoded Successfully!");
      console.log("─────────────────────────────────────");
      console.log("Full Payload:", decodedPayload);
      console.log("\n📋 User Information:");
      console.log("  User ID (sub):", decodedPayload.sub || "NOT FOUND");
      console.log("  Email:", decodedPayload.email || decodedPayload.Email || "NOT FOUND");
      console.log("  Username:", decodedPayload.username || decodedPayload.Username || "NOT FOUND");
      console.log("  Name:", decodedPayload.name || decodedPayload.Name || "NOT FOUND");
      
      // Check expiration
      if (decodedPayload.exp) {
        const expDate = new Date(decodedPayload.exp * 1000);
        const now = new Date();
        const isExpired = now > expDate;
        
        console.log("\n⏰ Token Expiration:");
        console.log("  Expires:", expDate.toLocaleString());
        console.log("  Status:", isExpired ? "❌ EXPIRED" : "✅ Valid");
        
        if (isExpired) {
          console.warn("\n⚠️  WARNING: Your token has expired!");
          console.log("Please logout and login again to get a new token.");
        } else {
          const timeLeft = Math.floor((expDate - now) / 1000 / 60);
          console.log(`  Time left: ${timeLeft} minutes`);
        }
      }
      
      // Show user ID for reference
      const userId = decodedPayload.sub;
      if (userId) {
        console.log("\n✅ Your User ID is:", userId);
        console.log("─────────────────────────────────────");
        console.log("\n💡 Tip: If menus aren't loading, check:");
        console.log(`   - MongoDB query: db.MenuPermissions.find({ UserId: ${userId} })`);
        console.log("   - Verify permissions exist for this user");
      } else {
        console.error("❌ ERROR: User ID (sub) not found in token!");
        console.log("This is unusual. Please check your authentication setup.");
      }
    }
  } catch (error) {
    console.error("❌ ERROR: Failed to decode JWT token");
    console.error("Error details:", error);
    console.log("\nThis could mean:");
    console.log("1. Token is corrupted");
    console.log("2. Token format is invalid");
    console.log("3. Token encoding is incorrect");
    console.log("\nTry logging out and logging in again.");
  }
}

console.log("\n─────────────────────────────────────");
console.log("✅ Script completed");
