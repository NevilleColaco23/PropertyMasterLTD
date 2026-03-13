# Authentication & Routing System - File Reference Guide

## Overview
This document provides detailed information about each file in the authentication and routing system, explaining what they do, when they run, and how they work together.

---

## Table of Contents
1. [Authentication Files](#authentication-files)
2. [Routing & Navigation Files](#routing--navigation-files)
3. [Dashboard Files](#dashboard-files)
4. [How They Work Together](#how-they-work-together)
5. [Flow Diagrams](#flow-diagrams)

---

## Authentication Files

### 1. `app/src/app/core/auth/guards/auth.guard.ts`

**Purpose:** Protects routes from unauthorized access by checking token validity before navigation

**Type:** Functional Route Guard (CanActivateFn)

**When it runs:** 
- Before every navigation to protected routes
- On page refresh/reload
- When user directly types URL in browser

**What it does:**
```typescript
1. Intercepts navigation attempt
2. Injects AuthService and Router
3. Calls authService.isSignedIn() to check token
4. If valid:
   ✅ Returns true → Allow navigation
5. If invalid/expired:
   ❌ Saves attempted URL in sessionStorage
   ❌ Redirects to login page
   ❌ Returns false → Block navigation
```

**Key Features:**
- ✅ Checks token expiration locally (no API call = fast)
- ✅ Saves attempted URL for post-login redirect
- ✅ Prevents loading pages with expired tokens
- ✅ Console logging for debugging

**Code Example:**
```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isSignedIn()) {
    return true; // Allow navigation
  }

  // Save URL and redirect to login
  sessionStorage.setItem('redirect_after_login', state.url);
  router.navigate(['/']);
  return false; // Block navigation
};
```

**Used in:** `app.routes.ts` - Applied to protected routes

**Dependencies:**
- `AuthService` - For token validation
- `Router` - For redirection
- `sessionStorage` - For saving redirect URL

---

### 2. `app/src/app/core/auth/services/auth.interceptor.ts`

**Purpose:** Handles HTTP requests and responses for authentication

**Type:** HTTP Interceptor (HttpInterceptorFn)

**When it runs:**
- On every HTTP request made by the application
- Automatically when API calls are made
- Before request is sent and after response is received

**What it does:**

**Before Request (Request Handling):**
```typescript
1. Intercepts outgoing HTTP request
2. Gets user token from AuthService
3. Skips Cloudinary URLs (external service)
4. If token exists:
   ✅ Clones request
   ✅ Adds Authorization header: "Bearer {token}"
5. Sends modified request to server
```

**After Response (Error Handling):**
```typescript
1. Catches HTTP error responses
2. If 401 Unauthorized:
   ✅ Logs user out via authService.signOut()
   ✅ Saves logout message in sessionStorage
   ✅ Redirects to /login
3. Throws error for further handling
```

**Key Features:**
- ✅ Automatically adds token to API requests
- ✅ Centralized token management (no manual headers)
- ✅ Handles server-side token rejection
- ✅ Automatic logout on 401 errors
- ✅ Skips external services (Cloudinary)

**Code Example:**
```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getUserToken();

  // Add Authorization header
  const authReq = req.clone({
    setHeaders: { Authorization: token }
  });

  // Handle 401 errors
  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authService.signOut().subscribe();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};
```

**Configured in:** `app.config.ts` via `provideHttpClient(withInterceptors([authInterceptor]))`

**Dependencies:**
- `AuthService` - For token retrieval and logout
- `Router` - For redirection on 401
- `HttpClient` - Intercepts all HTTP calls

---

### 3. `app/src/app/core/auth/services/auth.service.ts`

**Purpose:** Manages user authentication state and token lifecycle

**Type:** Injectable Service (Singleton)

**When it runs:**
- Initialized when app starts
- On login/logout
- Every 60 seconds (token monitoring)
- On demand when methods are called

**What it does:**

**Authentication Methods:**
```typescript
authenticate(username, password)
  → POST /account/login
  → Stores token in localStorage
  → Updates signInState observable
  → Returns Observable<HttpResponse>

signIn(data)
  → Stores auth_userData, auth_tokenString, auth_tokenExpiresAt
  → Updates BehaviorSubject
  → Starts token monitoring

signOut()
  → Removes all auth data from localStorage
  → Stops token monitoring
  → Updates signInState to null
  → Returns Observable<void>

signUp(data)
  → POST /account/SignUp
  → Automatically signs in on success
```

**Token Validation:**
```typescript
isSignedIn()
  → Checks localStorage for auth_tokenExpiresAt
  → Compares with Date.now()
  → If expired: Auto logout + redirect
  → Returns boolean

getUserToken()
  → Returns 'Bearer {token}' string
  → Used by interceptor

getUserId()
  → Decodes JWT token
  → Extracts 'sub' claim (user ID)
  → Returns number | null
```

**Token Monitoring:**
```typescript
startTokenExpirationMonitoring()
  → Runs every 60 seconds (CHECK_INTERVAL)
  → Checks token expiration
  → Warns 5 minutes before expiry
  → Auto logout on expiration

checkTokenExpiration()
  → Calculates time until expiry
  → Shows warning at 5 minutes
  → Calls autoLogout() if expired

autoLogout(message)
  → Logs out user
  → Saves logout message
  → Redirects to login
```

**Key Features:**
- ✅ Centralized auth state management
- ✅ Automatic token monitoring
- ✅ JWT decoding for user info
- ✅ Observable-based state updates
- ✅ SSR-safe (checks isBrowser)
- ✅ Proactive logout on expiration

**State Management:**
```typescript
// Observable pattern
signInState: Observable<AuthenticationSuccessData | null>
private _signInState = new BehaviorSubject<...>(null)

// Components can subscribe:
authService.signInState.subscribe(user => {
  if (user) {
    console.log('User logged in:', user);
  }
});
```

**Storage Structure:**
```typescript
localStorage:
  - auth_userData: JSON object with user details
  - auth_tokenString: "Bearer {jwt_token}"
  - auth_tokenExpiresAt: Timestamp in milliseconds

sessionStorage:
  - logout_message: Message to show after logout
  - redirect_after_login: URL to redirect after login
```

**Dependencies:**
- `HttpClient` - For API calls
- `Router` - For redirects
- `BehaviorSubject` - For state management
- `interval` - For token monitoring

---

## Routing & Navigation Files

### 4. `app/src/app/app.routes.ts`

**Purpose:** Defines application route configuration with protection

**Type:** Route Configuration Array

**When it runs:**
- At application initialization
- On every route navigation
- When guards are evaluated

**Route Structure:**
```typescript
export const routes: Routes = [
  // Public routes (no guard)
  { 
    path: '', 
    component: LoginFormComponent, 
    pathMatch: 'full' 
  },
  { 
    path: 'create-user', 
    component: CreateUserComponent 
  },
  { 
    path: 'activate', 
    component: ActivateAccountComponent 
  },

  // Protected routes (with authGuard)
  { 
    path: 'propertySelector', 
    component: PropertySelectionComponent, 
    canActivate: [authGuard] 
  },
  { 
    path: 'bookings', 
    component: ReportsComponent, 
    canActivate: [authGuard] 
  },
  {
    path: 'propertyLanding',
    canActivate: [authGuard], // Protects all child routes
    loadChildren: () => import('./property/property-landing/property-landing.routes')
      .then(m => m.PROPERTY_LANDING_ROUTES),
  },
];
```

**Navigation Flow:**
```
User navigates to /propertyLanding/dashboard1
    ↓
Route matches: propertyLanding (parent)
    ↓
authGuard runs (checks token)
    ↓
If valid: Load property-landing.routes.ts
    ↓
Match child route: dashboard1
    ↓
Load Dashboard1Component
```

**Key Features:**
- ✅ Lazy loading for performance (loadChildren)
- ✅ Route protection via canActivate
- ✅ Hierarchical route structure
- ✅ Guard inheritance (parent guard protects all children)

**Protected Routes:**
1. `/propertySelector` - Property selection page
2. `/bookings` - Bookings/reports page
3. `/propertyLanding/**` - All property landing pages including:
   - `/propertyLanding/dashboard1` - Dashboard
   - `/propertyLanding/MenuAccessmapping` - Menu access management
   - `/propertyLanding/property-master` - Property management
   - `/propertyLanding/bookingsReport` - Reports

**Public Routes:**
1. `/` - Login page
2. `/create-user` - User registration
3. `/activate` - Email activation

---

### 5. `app/src/app/property/property-landing/property-landing.routes.ts`

**Purpose:** Child routes for property landing section

**Type:** Route Configuration Array

**Routes Defined:**
```typescript
export const PROPERTY_LANDING_ROUTES: Routes = [
  {
    path: '',
    component: PropertyLandingComponent,
    children: [
      // Default redirect
      { path: '', redirectTo: 'dashboard1', pathMatch: 'full' },

      // Dashboard
      { path: 'dashboard1', component: Dashboard1Component },

      // Reports
      { path: 'bookingsReport', component: ReportsComponent },

      // Settings
      { path: 'MenuAccessmapping', component: MenuAccessMap },
      { path: 'property-master', component: PropertyMasterComponent },
    ],
  },
];
```

**Layout Structure:**
```
PropertyLandingComponent (parent - has navigation menu)
    ├── Header (logo, search, user buttons)
    ├── Navigation Bar (Home, Settings, Front Office)
    └── <router-outlet> (child routes render here)
           ├── Dashboard1Component
           ├── ReportsComponent
           ├── MenuAccessMap
           └── PropertyMasterComponent
```

**Key Features:**
- ✅ Shared layout (navigation menu) across all child routes
- ✅ Default redirect to dashboard
- ✅ Nested routing structure
- ✅ Inherits auth protection from parent route

---

### 6. `app/src/app/core/auth/login-form/login-form.ts`

**Purpose:** Handles user login and post-login navigation

**Type:** Standalone Component

**What it does:**

**On Submit:**
```typescript
1. Validates form (email + password)
2. Disables form (prevents double submission)
3. Shows loader
4. Calls authService.authenticate(email, password)
5. On success:
   ✅ Logs event via loggingService
   ✅ Checks sessionStorage for redirect_after_login
   ✅ If found: Navigate to saved URL
   ✅ If not found: Navigate to /propertySelector
   ✅ Enables form
   ✅ Shows success state
6. On error:
   ✅ Re-enables form
   ✅ Shows error state (401 = wrong credentials, other = server error)
   ✅ Special handling for unactivated accounts
```

**Smart Redirect Logic:**
```typescript
// Check for saved redirect URL from auth guard
const redirectUrl = sessionStorage.getItem('redirect_after_login');
if (redirectUrl) {
  console.log('Redirecting to originally requested page:', redirectUrl);
  sessionStorage.removeItem('redirect_after_login');
  this.router.navigate([redirectUrl]); // Go back to intended page
} else {
  this.router.navigate(['/propertySelector']); // Default
}
```

**Form States:**
```typescript
enum LocalLoginState {
  None,           // Initial state
  Waiting,        // API call in progress
  Success,        // Login successful
  ErrorWrongData, // 401 - Invalid credentials
  ErrorOther      // Other errors (500, network, etc.)
}
```

**Key Features:**
- ✅ Form validation (email format, required fields)
- ✅ Loading state management
- ✅ Smart post-login redirect
- ✅ Error handling with user feedback
- ✅ Auto-fill from environment (for development)
- ✅ Logging for audit trail

**Template Bindings:**
```html
<form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
  <input formControlName="email" />
  <input formControlName="password" [type]="hidePassword ? 'password' : 'text'" />
  <button [disabled]="!loginForm.valid || localLoginState === Waiting">
    {{ localLoginState === Waiting ? 'Logging in...' : 'Login' }}
  </button>
</form>
```

---

### 7. `app/src/app/property/property-landing/property-landing.component.ts`

**Purpose:** Main layout component for authenticated section with navigation menu

**Type:** Standalone Component

**What it does:**

**On Initialization (ngOnInit):**
```typescript
1. Logs initialization
2. Gets user ID from authService.getUserId()
3. Calls getMenuItems() with user ID
4. On success:
   ✅ Sets navItems array
   ✅ Extracts logoPath from response
   ✅ Forces change detection
   ✅ Logs number of menu items loaded
5. On error:
   ✅ Logs error details
   ✅ Sets navItems to empty array (graceful failure)
```

**Menu Loading:**
```typescript
getMenuItems() {
  const userId = this.authService.getUserId();
  const params = new HttpParams().set('userId', userId);
  
  return this.http.get('/menu/GetinitialData', { params }).pipe(
    map(response => {
      this.logoPath = response?.results?.[0]?.property?.companyLogoURL;
      return response;
    }),
    catchError(err => this.errorHandling.handleError(err))
  );
}
```

**Navigation Structure:**
```typescript
navItems: Array<{
  label: string,          // "Home", "Settings", etc.
  path: string,           // "/propertyLanding/dashboard1"
  order: number,          // Display order
  hasDropdown: boolean,   // Has submenu?
  subItems: Array<{       // Optional submenu items
    subLabel: string,
    subPath: string
  }>,
  isVisible: boolean
}>
```

**Key Features:**
- ✅ Dynamic menu loading from API
- ✅ User-specific menu items (based on permissions)
- ✅ Support for dropdown menus
- ✅ Company logo display
- ✅ Search functionality
- ✅ User profile actions (Change Properties, My Profile, Sign Out)
- ✅ Graceful error handling (empty menu on failure)
- ✅ Change detection forcing (fixes Angular 18 issue)

**Template Sections:**
```html
<header class="site-header">
  <!-- Top Bar: Logo, Search, User Buttons -->
  <div class="topbar">...</div>
  
  <!-- Navigation Bar: Dynamic Menu Items -->
  <div class="mainbar">
    <nav>
      @for (item of navItems; track item.label) {
        <a [routerLink]="item.path">{{ item.label }}</a>
      }
    </nav>
  </div>
</header>

<!-- Child Route Content -->
<main class="app-main">
  <router-outlet></router-outlet>
</main>
```

**User Actions:**
```typescript
signOut()
  → Logs event
  → Navigates to / (login page)

changeProperties()
  → Navigates to /propertySelector

goToProfile()
  → Navigates to /profile (future feature)

onSearchSelected(result)
  → Navigates to result.path
```

---

## Dashboard Files

### 8. `app/src/app/dashboard/dashboard1/dashboard1.component.ts`

**Purpose:** Main dashboard with KPI cards and dashboard type selector

**Type:** Standalone Component

**What it displays:**
```
┌─────────────────────────────────────────────┐
│ Dashboard Header                             │
│ [Dashboard Icon] Dashboard    [Selector ▼]   │
│ Welcome message                              │
├─────────────────────────────────────────────┤
│ [Total Properties] [Total Rooms]            │
│ [Bookings Today]   [Occupancy Rate]         │
├─────────────────────────────────────────────┤
│ Quick Actions                                │
│ [New Booking] [View Calendar] [View Reports] │
├─────────────────────────────────────────────┤
│ Recent Activity                              │
│ (No recent activity to display)             │
└─────────────────────────────────────────────┘
```

**Dashboard Selector:**
```typescript
dashboardTypes: DashboardType[] = [
  {
    value: 'dashboard1',
    label: 'Overview Dashboard',
    icon: 'dashboard',
    route: '/propertyLanding/dashboard1'
  },
  {
    value: 'dashboard2',
    label: 'Analytics Dashboard',
    icon: 'analytics',
    route: '/propertyLanding/dashboard2'
  },
  {
    value: 'dashboard3',
    label: 'Reports Dashboard',
    icon: 'assessment',
    route: '/propertyLanding/dashboard3'
  }
];

onDashboardChange(event: MatSelectChange) {
  const selected = this.dashboardTypes.find(d => d.value === event.value);
  // Currently warns for unimplemented dashboards
  // Will navigate when dashboard2/3 are created
}
```

**Key Features:**
- ✅ Dashboard type selector dropdown
- ✅ KPI summary cards (Properties, Rooms, Bookings, Occupancy)
- ✅ Quick action buttons
- ✅ Recent activity section
- ✅ Material Design components
- ✅ Responsive layout
- ✅ Extensible for multiple dashboard types

**Future Integration Points:**
```typescript
// Connect to real data via service
ngOnInit(): void {
  this.dashboardService.getStats().subscribe(stats => {
    this.totalProperties = stats.totalProperties;
    this.totalRooms = stats.totalRooms;
    this.bookingsToday = stats.bookingsToday;
    this.occupancyRate = stats.occupancyRate;
  });
}
```

---

## How They Work Together

### Complete Authentication Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    APPLICATION STARTUP                       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ AuthService Constructor                                      │
│ - Checks localStorage for existing token                    │
│ - If found: Starts token monitoring                         │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    USER NAVIGATION ATTEMPT                   │
│ URL: /propertyLanding/dashboard1                            │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 1. AUTH GUARD (Route Protection)                            │
│    - Runs before loading page                               │
│    - Calls authService.isSignedIn()                         │
│    - Checks localStorage token expiration                   │
│                                                              │
│    If Valid:                                                 │
│    ✅ Return true → Allow navigation                        │
│                                                              │
│    If Invalid/Expired:                                       │
│    ❌ Save attempted URL → sessionStorage                   │
│    ❌ Navigate to /login                                    │
│    ❌ Return false → Block navigation                       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. COMPONENT LOADS (if guard allowed)                       │
│    - PropertyLandingComponent initialized                   │
│    - Makes API call to /menu/GetinitialData                 │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. AUTH INTERCEPTOR (API Request)                           │
│    - Intercepts /menu/GetinitialData request                │
│    - Gets token from authService.getUserToken()             │
│    - Adds header: Authorization: Bearer {token}             │
│    - Sends request to server                                │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. SERVER VALIDATION                                         │
│    - Receives request with Authorization header             │
│    - Validates token (signature, expiration, claims)        │
│                                                              │
│    If Valid:                                                 │
│    ✅ Returns menu data (200 OK)                            │
│                                                              │
│    If Invalid:                                               │
│    ❌ Returns 401 Unauthorized                              │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. AUTH INTERCEPTOR (Response Handling)                     │
│    - Catches 401 error                                      │
│    - Calls authService.signOut()                            │
│    - Saves logout message → sessionStorage                  │
│    - Navigates to /login                                    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. USER LOGS IN AGAIN                                       │
│    - Enters credentials                                     │
│    - LoginFormComponent.onSubmit()                          │
│    - Calls authService.authenticate()                       │
│    - On success:                                            │
│      ✅ Checks sessionStorage for redirect URL              │
│      ✅ Navigates back to /propertyLanding/dashboard1       │
└─────────────────────────────────────────────────────────────┘
```

### Token Lifecycle

```
┌─────────────────────────────────────────────────────────────┐
│                         LOGIN                                │
│ authService.authenticate(username, password)                 │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    STORE TOKEN DATA                          │
│ localStorage.setItem('auth_userData', JSON)                  │
│ localStorage.setItem('auth_tokenString', 'Bearer xyz')       │
│ localStorage.setItem('auth_tokenExpiresAt', timestamp)       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│              START TOKEN MONITORING                          │
│ interval(60000) - Checks every 60 seconds                    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                  TOKEN VALIDATION CHECKS                     │
│                                                              │
│ Every Route Navigation:                                      │
│ → Auth Guard checks token expiration                        │
│                                                              │
│ Every API Call:                                              │
│ → Interceptor adds token to headers                         │
│ → Server validates token                                    │
│                                                              │
│ Every 60 Seconds:                                            │
│ → Token monitoring checks expiration                        │
│                                                              │
│ Every Page Refresh:                                          │
│ → Guard checks before loading page                          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                  TOKEN EXPIRATION                            │
│                                                              │
│ Detected By:                                                 │
│ 1. Auth Guard (on navigation)                               │
│ 2. Auth Service monitoring (every 60s)                      │
│ 3. API 401 response (server-side validation)               │
│                                                              │
│ Action Taken:                                                │
│ → authService.signOut()                                     │
│ → Clear localStorage                                        │
│ → Stop monitoring                                           │
│ → Navigate to /login                                        │
│ → Save logout message                                       │
└─────────────────────────────────────────────────────────────┘
```

### Multi-Layer Security

```
Layer 1: AUTH GUARD (Client-Side Pre-Navigation)
├─ Runs: Before page loads
├─ Checks: Local token expiration
├─ Fast: No API call needed
└─ Blocks: Navigation to protected routes

Layer 2: AUTH INTERCEPTOR (Request)
├─ Runs: On every API call
├─ Adds: Authorization header
├─ Automatic: No manual header management
└─ Centralized: One place for all requests

Layer 3: SERVER VALIDATION (Backend)
├─ Runs: On API server
├─ Validates: Token signature & expiration
├─ Authoritative: Final security check
└─ Returns: 401 if invalid

Layer 4: AUTH INTERCEPTOR (Response)
├─ Runs: After API response
├─ Catches: 401 errors
├─ Action: Auto logout & redirect
└─ Graceful: Handles token rejection

Layer 5: TOKEN MONITORING (Background)
├─ Runs: Every 60 seconds
├─ Checks: Token expiration
├─ Warns: 5 minutes before expiry
└─ Auto-logout: On expiration
```

---

## Flow Diagrams

### Login Flow
```
User opens app
    ↓
Login page (/)
    ↓
User enters credentials
    ↓
LoginFormComponent.onSubmit()
    ↓
authService.authenticate()
    ↓
POST /account/login
    ↓
Server validates credentials
    ↓
Returns: token + user data
    ↓
authService.signIn(data)
    ├─ Store in localStorage
    ├─ Update signInState
    └─ Start token monitoring
    ↓
LoginFormComponent checks redirect
    ├─ sessionStorage has URL? → Navigate there
    └─ No saved URL? → Navigate to /propertySelector
    ↓
User selects property
    ↓
Navigate to /propertyLanding
    ↓
Auth Guard checks token → Valid ✅
    ↓
PropertyLandingComponent loads
    ↓
Calls /menu/GetinitialData
    ↓
Interceptor adds token header
    ↓
Menu data received
    ↓
Navigate to /propertyLanding/dashboard1 (default)
    ↓
Dashboard loads ✅
```

### Token Expiration Flow
```
User browsing dashboard
    ↓
Token expires (detected by monitoring)
    ↓
authService.autoLogout()
    ├─ Stops monitoring
    ├─ Clears localStorage
    ├─ Saves logout message
    └─ Redirects to /login
    ↓
User clicks on dashboard link
    ↓
Auth Guard intercepts
    ↓
Checks token → Expired ❌
    ├─ Saves attempted URL
    └─ Redirects to /login
    ↓
User logs in
    ↓
LoginFormComponent checks sessionStorage
    ↓
Finds saved URL: /propertyLanding/dashboard1
    ↓
Navigates back to dashboard ✅
```

### API Call Flow
```
Component makes API call
    ↓
http.get('/api/properties')
    ↓
Auth Interceptor intercepts
    ↓
Gets token: authService.getUserToken()
    ↓
Clones request + adds header
    ↓
Authorization: Bearer xyz123
    ↓
Sends to server
    ↓
Server validates token
    ├─ Valid → Returns data (200)
    └─ Invalid → Returns 401
    ↓
Interceptor catches 401
    ├─ Calls authService.signOut()
    ├─ Saves logout message
    └─ Navigates to /login
```

---

## Summary Table

| File | Type | Runs When | Primary Purpose | Checks Token | Makes API Call | Redirects |
|------|------|-----------|-----------------|--------------|----------------|-----------|
| **auth.guard.ts** | Route Guard | Before navigation | Block access to protected routes | ✅ Yes (local) | ❌ No | ✅ Yes (to login) |
| **auth.interceptor.ts** | HTTP Interceptor | During API calls | Add token header & handle 401 | ❌ No | ❌ No (intercepts) | ✅ Yes (on 401) |
| **auth.service.ts** | Service | Various | Manage auth state & token | ✅ Yes (continuous) | ✅ Yes (login/logout) | ✅ Yes (on expire) |
| **app.routes.ts** | Configuration | At startup | Define routes & protection | ❌ No | ❌ No | ❌ No |
| **login-form.ts** | Component | On login | Handle login form | ❌ No | ✅ Yes (via service) | ✅ Yes (after login) |
| **property-landing.component.ts** | Component | After login | Show menu & layout | ❌ No | ✅ Yes (menu data) | ❌ No |
| **dashboard1.component.ts** | Component | When navigated | Display dashboard | ❌ No | ❌ No (yet) | ❌ No |

---

## Key Takeaways

### Auth Guard vs Auth Interceptor

**Auth Guard:**
- ✅ Prevents loading pages
- ✅ Client-side check (fast)
- ✅ Runs before navigation
- ✅ No API call required

**Auth Interceptor:**
- ✅ Handles API security
- ✅ Server-side validation
- ✅ Runs during requests
- ✅ Catches 401 errors

**Both are essential for complete security!**

### Security Layers

1. **Local Check** (Guard) → Fast, prevents unnecessary page loads
2. **API Check** (Interceptor + Server) → Authoritative, catches server-side issues
3. **Monitoring** (Service) → Proactive, warns before expiration
4. **Auto-logout** (All layers) → Graceful handling of expired tokens

### User Experience

- ✅ Fast initial check (no API delay)
- ✅ Automatic token injection (no manual headers)
- ✅ Smart redirect after login (returns to intended page)
- ✅ Graceful error handling (clear messages)
- ✅ Proactive warnings (5 minutes before expiry)
- ✅ Consistent behavior across all protected routes

---

## Debugging Tips

### Check Auth Guard
```javascript
// In browser console
console.log('Token expiry:', localStorage.getItem('auth_tokenExpiresAt'));
console.log('Time now:', Date.now());
console.log('Expired?', Date.now() > +localStorage.getItem('auth_tokenExpiresAt'));
```

### Check Interceptor
```javascript
// Open Network tab (F12)
// Look for API requests
// Check Headers section
// Look for: Authorization: Bearer xyz...
```

### Check Token Monitoring
```javascript
// In auth.service.ts, add breakpoint or log in:
private checkTokenExpiration(): void {
  console.log('Token monitoring check running...');
  // Runs every 60 seconds
}
```

### Manually Trigger Expiration
```javascript
// Force token to expire
localStorage.setItem('auth_tokenExpiresAt', Date.now() - 1000);
// Then try navigating or refreshing
```

---

**Document Created:** For comprehensive understanding of authentication & routing system
**Last Updated:** During dashboard and auth guard implementation
**Maintained By:** Development Team
