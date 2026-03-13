import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { AuthService } from './auth.service';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getUserToken();

  // Skip adding auth header for Cloudinary requests
  if (req.url.includes('cloudinary.com')) {
    return next(req);
  }

  if (!token) {
    return next(req);
  }

  const authReq = req.clone({
    setHeaders: {
      Authorization: token
    }
  });

  // Handle HTTP errors, especially 401 Unauthorized
  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        console.warn('🚫 Received 401 Unauthorized - Token may be invalid or expired');

        // Automatically logout and redirect to login
        authService.signOut().subscribe({
          next: () => {
            sessionStorage.setItem('logout_message', 'Your session has expired. Please login again.');
            router.navigate(['/login']);
          }
        });
      }

      return throwError(() => error);
    })
  );
};
