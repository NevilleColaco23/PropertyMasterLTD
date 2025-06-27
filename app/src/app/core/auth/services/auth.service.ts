import { Injectable } from '@angular/core';
import { AuthenticationSuccessData } from '../model/login-data';
import {BehaviorSubject, Observable} from 'rxjs';
import {HttpClient, HttpResponse} from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { tap } from 'rxjs/operators';
import { shareReplay } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  
  public signInState: Observable<AuthenticationSuccessData | null>;
  private _signInState = new BehaviorSubject<AuthenticationSuccessData | null>(null);

  constructor(private _http: HttpClient) {
    this.signInState = this._signInState.asObservable();

    const userData = this.getStoredUserData();

    if (userData != null) { 
      this._signInState.next(userData);
    }   
    }


  public authenticate(username: string, password: string): Observable<HttpResponse<AuthenticationSuccessData>> {
    const loginData = {
      username: username,
      password: password
    };
console.log('Using API host:', environment.apiUrl);
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

public signUp(username : string,email :string,password:string){
  const signUpmodel = {
    username: username,
    password: password,
    email:email
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

  public getValidityDays() {
    const expiresAt = localStorage.getItem('auth_tokenExpiresAt');
    return expiresAt ? (+expiresAt - Date.now()) / 1000 / (3600 * 24) : 0;
  }

  private getStoredUserData(): AuthenticationSuccessData | null {
    const userData = localStorage.getItem('auth_userData');
    return userData ? (JSON.parse(userData) as AuthenticationSuccessData) : null;
  }
}