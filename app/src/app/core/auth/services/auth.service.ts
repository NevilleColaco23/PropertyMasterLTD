import { Injectable,OnDestroy } from '@angular/core';
import { AuthenticationSuccessData } from '../models/login-data/login-data';
import {BehaviorSubject, Observable, Subscription} from 'rxjs';
import {HttpClient, HttpResponse} from '@angular/common/http';
//import { SocialAuthService, GoogleLoginProvider, SocialUser } from 'angularx-social-login';
import { environment } from '../../../environments/environment';
import { tap } from 'rxjs/operators';
import { shareReplay } from 'rxjs/operators';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService implements OnDestroy {
  
  public signInState: Observable<AuthenticationSuccessData>;
  private _signInState = new BehaviorSubject<AuthenticationSuccessData>(null);
  private socialAuthSub: Subscription;

  constructor(private _http: HttpClient) {
    this.signInState = this._signInState.asObservable();

    const userData = this.getStoredUserData();

    if (userData != null) { 
      this._signInState.next(userData);
    }   
    }

  ngOnDestroy(): void {
    if (this.socialAuthSub) { // Added null check for socialAuthSub
    this.socialAuthSub.unsubscribe();
  }
  }

  public authenticate(username: string, password: string): Observable<HttpResponse<AuthenticationSuccessData>> {
    const loginData = {
      username: username,
      password: password
    };

    return this._http.post<AuthenticationSuccessData>(`${environment.baseHost}/account/login`, loginData, { observe: 'response' })
      .pipe(
        tap(res => {
          if (res.body) {
            this.signIn(res.body);
          }
        }),
        shareReplay()
      );
  }

public signUp(username : string,email :string,password:string){
  const signUpmodel = {
    username: username,
    password: password,
    email:email
  };

 
  return this._http.post<AuthenticationSuccessData>(`${environment.baseHost}/account/SignUp`, signUpmodel, { observe: 'response' })
      .pipe(
        tap(res => {
          
          if (res.body) {
            this.signIn(res.body);
          }
        }),
        shareReplay()
      );
}

  // private authenticateExternalSignIn(user: SocialUser) {
  //   if (user == null) {
  //     this.signOut();
  //     return;
  //   }

  //   const externalTokenData = {
  //     idToken: user.idToken,
  //     provider: ExternalAuthenticationProviders[user.provider as keyof typeof ExternalAuthenticationProviders]
  //   };

  //   this._http.post<AuthenticationSuccessData>(`${environment.baseHost}/account/loginExternal`, externalTokenData, { observe: 'response' })
  //     .subscribe(res => {
  //       if (res.body) {
  //         this.signIn(res.body);
  //       }
  //     });
  // }
  

  private signIn(data: AuthenticationSuccessData) {
    const expiresAt = new Date();
    expiresAt.setTime(Date.now() + (data.expiresIn * 1000));

    localStorage.setItem('auth_userData', JSON.stringify(data));
    localStorage.setItem('auth_tokenString', `${data.tokenType} ${data.accessToken}`);
    localStorage.setItem('auth_tokenExpiresAt', expiresAt.getTime().toString());
    this._signInState.next(data);
  }

  public signOut() {
    localStorage.removeItem('auth_userData');
    localStorage.removeItem('auth_tokenString');
    localStorage.removeItem('auth_tokenExpiresAt');
    
    this._signInState.next(null);
  }

  public isSignedIn() {
    return this._signInState.value != null;
  }

  public getUserToken() {
    return localStorage.getItem('auth_tokenString');
  }

  public getUserId(): string | null {
    const token = this.getUserToken();
    if (!token) {
      return null;
    }

    try {
      // Remove 'Bearer ' prefix if present
      const tokenString = token.startsWith('Bearer ') ? token.substring(7) : token;
      
      // Decode the JWT token
      const decoded: any = jwtDecode(tokenString);
      
      // Return userId from the token claims
      // The userId can be in different claim names depending on implementation
      return decoded.userId || decoded.sub || decoded.nameid || null;
    } catch (error) {
      console.error('Error decoding JWT token:', error);
      return null;
    }
  }

  public getDecodedToken(): any | null {
    const token = this.getUserToken();
    if (!token) {
      return null;
    }

    try {
      // Remove 'Bearer ' prefix if present
      const tokenString = token.startsWith('Bearer ') ? token.substring(7) : token;
      return jwtDecode(tokenString);
    } catch (error) {
      console.error('Error decoding JWT token:', error);
      return null;
    }
  }

  public getValidityDays() {
    const expiresAt = localStorage.getItem('auth_tokenExpiresAt');
    return expiresAt ? (+expiresAt - Date.now()) / 1000 / (3600 * 24) : 0;
  }

  private getStoredUserData(): AuthenticationSuccessData {
    return JSON.parse(localStorage.getItem('auth_userData')) as AuthenticationSuccessData;
  }
}

// enum ExternalAuthenticationProviders {
//   Google
// }
