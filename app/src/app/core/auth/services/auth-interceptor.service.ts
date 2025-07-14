// src/app/core/auth/services/auth-interceptor.service.ts (or wherever your interceptor file is located)
import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service'; // Adjust path if needed
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor { // <-- Make sure 'export' is here!

  constructor(private authService: AuthService, private router: Router) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Optionally: Add the token to outgoing requests if you don't do it elsewhere
    // This part ensures every outgoing request gets the token
    const token = this.authService.getUserToken(); // "Bearer <token>"
    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: token
        }
      });
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 || error.status === 403) {
          console.warn('Unauthorized or Forbidden response received. Token might be expired or invalid.');
          this.authService.signOut().subscribe({
            next: () => {
              console.log('Client-side logout performed due to token expiration.');
              this.router.navigate(['/login']);
            },
            error: (logoutError) => {
              console.error('Error during client-side logout after 401/403:', logoutError);
              this.router.navigate(['/login']);
            }
          });
        }
        return throwError(() => error);
      })
    );
  }
}