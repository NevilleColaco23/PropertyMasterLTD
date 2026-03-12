import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import {BehaviorSubject, Observable, of } from 'rxjs';
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

  constructor(private _http: HttpClient, private router: Router) {
    this.signInState = this._signInState.asObservable();
    console.log('PRODUCTION:', environment.production);

    // Only access localStorage in the browser
    if (this.isBrowser) {
      const userData = this.getStoredUserData();
      if (userData != null) { 
        this._signInState.next(userData);
        this.checkTokenExpirationAndSignOut();
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
  }

  public signOut() : Observable<void> { // <-- Now returns Observable<void>
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
}
