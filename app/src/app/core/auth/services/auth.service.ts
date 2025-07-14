import { Injectable } from '@angular/core';
import { AuthenticationSuccessData } from '../model/login-data';
import {BehaviorSubject, Observable, of } from 'rxjs';
import {HttpClient, HttpResponse} from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { tap,catchError } from 'rxjs/operators';
import { shareReplay } from 'rxjs/operators';
import { Router } from '@angular/router';
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  
  public signInState: Observable<AuthenticationSuccessData | null>;
  private _signInState = new BehaviorSubject<AuthenticationSuccessData | null>(null);

  constructor(private _http: HttpClient, private router: Router) {
    this.signInState = this._signInState.asObservable();
    console.log('PRODUCTION:', environment.production);

    const userData = this.getStoredUserData();

    if (userData != null) { 
      this._signInState.next(userData);
      this.checkTokenExpirationAndSignOut();
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
  
public signUp(username : string,email :string,password:string,phone:string){
  const signUpmodel = {
    username: username,
    password: password,
    email:email,
    phonenumber: phone
  };
  
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
    const expiresAt = new Date();
    expiresAt.setTime(Date.now() + (data.expiresIn * 1000));
    console.log('token valid till:', expiresAt.setTime(Date.now() + (data.expiresIn * 1000)));

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
    return localStorage.getItem('auth_tokenString');
  }

  public getValidityDays() {
    const expiresAt = localStorage.getItem('auth_tokenExpiresAt');
    return expiresAt ? (+expiresAt - Date.now()) / 1000 / (3600 * 24) : 0;
  }

  private getStoredUserData(): AuthenticationSuccessData | null {
    const userData = localStorage.getItem('auth_userData');
    return userData ? (JSON.parse(userData) as AuthenticationSuccessData) : null;
  }
}