# Auto-Redirect on Token Expiration

## What Was Implemented

I've added **automatic redirect to login** when your authentication token expires.

## How It Works

### 1. Auth Guard (`auth.guard.ts`)
- **Checks token validity** before allowing access to protected routes
- **Automatically redirects to login** if token is expired
- **Saves the attempted URL** so you can return there after logging in

### 2. Protected Routes
Updated `app.routes.ts` to protect these routes:
- ✅ `/propertySelector` - Property selection page
- ✅ `/bookings` - Bookings/reports page
- ✅ `/propertyLanding/**` - Dashboard and all sub-pages

### 3. Smart Login Redirect
- After successful login, automatically redirects you back to the page you were trying to access
- If no attempted page, goes to property selector as default

## What Happens Now

### Scenario 1: Token Expires While Browsing
```
1. You're on dashboard at /propertyLanding/dashboard1
2. Token expires
3. Auth service detects it
4. You're automatically logged out
5. Redirected to login page
```

### Scenario 2: Token Expired on Page Refresh
```
1. You refresh the page (F5)
2. Auth guard checks token
3. Token is expired
4. Auth guard saves current URL: /propertyLanding/dashboard1
5. Redirects to login
6. After login → automatically returns to dashboard!
```

### Scenario 3: Try to Access Protected Page Without Login
```
1. You try to go to /propertyLanding/dashboard1
2. Auth guard checks: Not logged in
3. Redirects to login
4. After login → returns to dashboard
```

## Console Messages

You'll see helpful messages in the console (F12):

```javascript
// When accessing protected route:
🔐 Auth Guard: Checking authentication...
✅ Auth Guard: User authenticated, access granted

// When token expired:
🔐 Auth Guard: Checking authentication...
⚠️ Auth Guard: Token expired or user not authenticated
🔄 Redirecting to login page...

// After login:
🔄 Redirecting to originally requested page: /propertyLanding/dashboard1
```

## Token Expiration Flow

```
┌─────────────────────────────────────────────────┐
│ 1. User tries to access /propertyLanding       │
│    ↓                                            │
│ 2. Auth Guard intercepts                       │
│    ↓                                            │
│ 3. Checks: authService.isSignedIn()            │
│    ├─ Valid token → Allow access ✅            │
│    └─ Expired/No token → Redirect to login ❌  │
│       ↓                                         │
│ 4. Save attempted URL in sessionStorage        │
│    ↓                                            │
│ 5. Navigate to /login                          │
│    ↓                                            │
│ 6. User logs in successfully                   │
│    ↓                                            │
│ 7. Check sessionStorage for saved URL          │
│    ├─ Found → Redirect there ✅                │
│    └─ Not found → Go to /propertySelector      │
└─────────────────────────────────────────────────┘
```

## Files Modified

1. **app/src/app/core/auth/guards/auth.guard.ts** ✨ NEW
   - Functional guard using Angular's new CanActivateFn
   - Checks token expiration on every route navigation
   - Saves attempted URL for post-login redirect

2. **app/src/app/app.routes.ts** 📝 UPDATED
   - Added `canActivate: [authGuard]` to protected routes
   - All authenticated pages now check token before access

3. **app/src/app/core/auth/login-form/login-form.ts** 📝 UPDATED
   - Checks for saved redirect URL after login
   - Automatically navigates to originally requested page
   - Falls back to property selector if no saved URL

## Token Expiration Check

The `authService.isSignedIn()` method already had token expiration checking:

```typescript
public isSignedIn(): boolean {
  const expiresAt = localStorage.getItem('auth_tokenExpiresAt');
  if (!expiresAt) return false;
  
  const now = Date.now();
  
  // Token expired? Auto logout
  if (now >= +expiresAt) {
    console.log('Token has expired locally. Signing out...');
    this.signOutInternalAndRedirect();
    return false;
  }
  
  return true;
}
```

## Testing

### Test Token Expiration
1. **Open browser console** (F12)
2. **Manually expire token**:
   ```javascript
   localStorage.setItem('auth_tokenExpiresAt', Date.now() - 1000);
   ```
3. **Try to navigate to dashboard** or **refresh page**
4. **You should be redirected to login** automatically

### Test Redirect After Login
1. While logged in, manually expire token (see above)
2. Try to access `/propertyLanding/dashboard1`
3. You'll be redirected to login
4. Login with valid credentials
5. You should automatically return to `/propertyLanding/dashboard1`

## Configuration

The auth guard is now protecting these routes automatically:
- Property Selector
- Property Landing (Dashboard & all sub-pages)
- Bookings/Reports

To protect additional routes, just add `canActivate: [authGuard]`:

```typescript
{ 
  path: 'some-new-page', 
  component: SomeComponent,
  canActivate: [authGuard]  // ← Add this
}
```

## Benefits

✅ **Automatic protection** - No manual token checks needed
✅ **Better UX** - Returns to intended page after login
✅ **Security** - Expired tokens can't access protected pages
✅ **Clear logging** - Console shows exactly what's happening
✅ **Centralized logic** - Auth check in one place (the guard)
✅ **Works on refresh** - Token checked on every page load

## Already Working Features

The auth service already has:
- ✅ Token expiration monitoring (checks every 60 seconds)
- ✅ Automatic logout when token expires
- ✅ Warning 5 minutes before expiration
- ✅ Session storage for logout messages

Now combined with:
- ✅ Route protection via auth guard
- ✅ Automatic redirect on token expiration
- ✅ Smart post-login navigation

## Summary

**Before:** Token could expire, but you could still access pages until API returned 401

**Now:** Token expiration is checked **before** you can access any protected route. If expired, you're automatically redirected to login!

No more accessing pages with expired tokens - the guard stops you at the door! 🛡️
