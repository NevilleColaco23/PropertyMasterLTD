import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import {BehaviorSubject, Observable, of, interval, Subscription } from 'rxjs';
import {HttpClient, HttpResponse} from '@angular/common/http';
import { tap,catchError,shareReplay } from 'rxjs/operators';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';

import { AuthenticationSuccessData } from '../model/login-data';
import { environment } from '../../../environments/environment';

export interface SignUpDto {
  username: string;
  email: string;
  password: string;
  phone: string;
}

@Injectable({
  providedIn: 'root' //makes it a singleton app-wide, no module registration needed.
})

export class AuthService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId); 

  public signInState: Observable<AuthenticationSuccessData | null>;
  private _signInState = new BehaviorSubject<AuthenticationSuccessData | null>(null);

  // Token expiration monitoring
  private tokenExpirationTimer?: Subscription;
  private readonly CHECK_INTERVAL = 60000; // Check every 60 seconds
  private readonly WARNING_BEFORE_EXPIRY = 5 * 60 * 1000; // Warn 5 minutes before expiry
  private hasShownWarning = false;

  constructor(private _http: HttpClient, private router: Router) {
    this.signInState = this._signInState.asObservable();
    console.log('PRODUCTION:', environment.production);

    // Only access localStorage in the browser
    if (this.isBrowser) {
      const userData = this.getStoredUserData();
      if (userData != null) { 
        this._signInState.next(userData);
        this.checkTokenExpirationAndSignOut();
        this.startTokenExpirationMonitoring(); // Start automatic monitoring
      }
    } 
    }


  public authenticate(username: string, password: string): Observable<HttpResponse<AuthenticationSuccessData>> {
    const loginData = {
      username: username,
      password: password
    };
    
    return this._http.post<AuthenticationSuccessData>(`${environment.apiUrl}/account/login`, loginData, { observe: 'response' })
      .pipe(
        tap(res => {
          if (res.body) {
            this.signIn(res.body);
          }
        }),
        shareReplay()
      );
  }
  
public signUp(data: SignUpDto){
  const signUpmodel = {
    username: data.username,
    password: data.password,
    email: data.email,
    Phone: data.phone
  };
  console.log('SignUp model:', signUpmodel);
  return this._http.post<AuthenticationSuccessData>(`${environment.apiUrl}/account/SignUp`, signUpmodel, { observe: 'response' })
      .pipe(
        tap(res => {
          
          if (res.body) {
            this.signIn(res.body);
          }
        }),
        shareReplay()
      );
}

  private signIn(data: AuthenticationSuccessData) {
    console.log('Received authentication data:', data);
    console.log('Signing in user:', `${data.tokenType} ${data.accessToken}`);
    const expiresAt = new Date();
    expiresAt.setTime(Date.now() + (data.expiresIn * 1000));

    localStorage.setItem('auth_userData', JSON.stringify(data));
    localStorage.setItem('auth_tokenString', `${data.tokenType} ${data.accessToken}`);
    localStorage.setItem('auth_tokenExpiresAt', expiresAt.getTime().toString());
    this._signInState.next(data);

    // Start monitoring token expiration
    this.hasShownWarning = false;
    this.startTokenExpirationMonitoring();
  }

  public signOut() : Observable<void> { // <-- Now returns Observable<void>
    // Stop monitoring token expiration
    this.stopTokenExpirationMonitoring();

    localStorage.removeItem('auth_userData');
    localStorage.removeItem('auth_tokenString');
    localStorage.removeItem('auth_tokenExpiresAt');
    this._signInState.next(null); // Update observable state immediately
    return of(undefined); // <-- Return an observable that immediately completes
  }

  public isSignedIn() : boolean {
    const expiresAtString = localStorage.getItem('auth_tokenExpiresAt');
    if (!expiresAtString) {
      return false; // No expiration time, so not signed in
    }

    const expiresAt = +expiresAtString; // Convert to number
    const now = Date.now();

    // If the token has expired, proactively sign out
    if (now >= expiresAt) {
      console.log('Token has expired locally. Signing out...');
      // We are calling signOut without subscribing here, as we want immediate action.
      // The HTTP Interceptor will handle the navigation if a subsequent API call is made.
      // For immediate redirection, you might need to adjust the signOut() signature
      // or call router.navigate directly after this signOut.
      // Let's refine this below.
      this.signOutInternalAndRedirect(); // New helper method
      return false;
    }
    return true; // Token is still valid locally
  }

// New private helper method for internal sign out and redirection
  private signOutInternalAndRedirect() {
    this.signOut().subscribe({
        next: () => {
            console.log('Proactive logout successful.');
            this.router.navigate(['/login']);
        },
        error: (err) => {
            console.error('Proactive logout failed but redirecting:', err);
            this.router.navigate(['/login']);
        }
    });
  }

 // Optional: A public method to trigger a manual check
  public checkTokenExpirationAndSignOut() {
    this.isSignedIn(); // This will trigger signOutInternalAndRedirect if expired
  }

  public getUserToken() {
    if (!this.isBrowser) {
      return null;
    }
    return localStorage.getItem('auth_tokenString');
  }

  public getValidityDays() {
    const expiresAt = localStorage.getItem('auth_tokenExpiresAt');
    return expiresAt ? (+expiresAt - Date.now()) / 1000 / (3600 * 24) : 0;
  }

  public getUserId(): number | null {
    if (!this.isBrowser) {
      return null;
    }

    const token = localStorage.getItem('auth_tokenString');
    if (!token) {
      return null;
    }

    try {
      // Extract the JWT token (remove "Bearer " prefix if present)
      const jwt = token.replace('Bearer ', '');

      // Decode the JWT payload (middle part between dots)
      const payload = jwt.split('.')[1];
      const decodedPayload = JSON.parse(atob(payload));

      // The userId is stored in the 'sub' (subject) claim
      return decodedPayload.sub ? parseInt(decodedPayload.sub, 10) : null;
    } catch (error) {
      console.error('Error decoding JWT token:', error);
      return null;
    }
  }

  private getStoredUserData(): AuthenticationSuccessData | null {
    const userData = localStorage.getItem('auth_userData');
    return userData ? (JSON.parse(userData) as AuthenticationSuccessData) : null;
  }

  /**
   * Start automatic token expiration monitoring
   * Checks token validity every minute and automatically logs out when expired
   */
  private startTokenExpirationMonitoring(): void {
    // Stop any existing timer first
    this.stopTokenExpirationMonitoring();

    if (!this.isBrowser) {
      return;
    }

    console.log('🔐 Started token expiration monitoring');

    // Check immediately
    this.checkTokenExpiration();

    // Then check every minute
    this.tokenExpirationTimer = interval(this.CHECK_INTERVAL).subscribe(() => {
      this.checkTokenExpiration();
    });
  }

  /**
   * Stop the token expiration monitoring timer
   */
  private stopTokenExpirationMonitoring(): void {
    if (this.tokenExpirationTimer) {
      this.tokenExpirationTimer.unsubscribe();
      this.tokenExpirationTimer = undefined;
      console.log('🔓 Stopped token expiration monitoring');
    }
  }

  /**
   * Check if token is about to expire or has expired
   */
  private checkTokenExpiration(): void {
    const expiresAtString = localStorage.getItem('auth_tokenExpiresAt');
    if (!expiresAtString) {
      return;
    }

    const expiresAt = +expiresAtString;
    const now = Date.now();
    const timeUntilExpiry = expiresAt - now;

    // Token has expired - logout immediately
    if (timeUntilExpiry <= 0) {
      console.warn('⏰ Token has expired! Logging out automatically...');
      this.autoLogout('Your session has expired. Please login again.');
      return;
    }

    // Token is about to expire - show warning
    if (timeUntilExpiry <= this.WARNING_BEFORE_EXPIRY && !this.hasShownWarning) {
      this.hasShownWarning = true;
      const minutesLeft = Math.ceil(timeUntilExpiry / 60000);
      console.warn(`⚠️ Token will expire in ${minutesLeft} minutes`);

      // Silent warning - logged to console only
      // You can integrate with Angular Material Snackbar here if needed
    }
  }

  /**
   * Automatically logout the user and redirect to login page
   */
  private autoLogout(message?: string): void {
    this.stopTokenExpirationMonitoring();

    this.signOut().subscribe({
      next: () => {
        console.log('🚪 Auto logout successful');
        if (message) {
          // Store message to show after redirect
          sessionStorage.setItem('logout_message', message);
        }
        this.router.navigate(['/login']);
      },
      error: (err) => {
        console.error('❌ Auto logout failed:', err);
        this.router.navigate(['/login']);
      }
    });
  }

  /**
   * Get remaining time until token expires (in milliseconds)
   */
  public getTimeUntilExpiry(): number {
    const expiresAtString = localStorage.getItem('auth_tokenExpiresAt');
    if (!expiresAtString) {
      return 0;
    }
    return (+expiresAtString) - Date.now();
  }

  /**
   * Check if token will expire soon (within WARNING_BEFORE_EXPIRY time)
   */
  public isTokenExpiringSoon(): boolean {
    const timeUntilExpiry = this.getTimeUntilExpiry();
    return timeUntilExpiry > 0 && timeUntilExpiry <= this.WARNING_BEFORE_EXPIRY;
  }
}
