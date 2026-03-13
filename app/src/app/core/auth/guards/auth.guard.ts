import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Auth Guard - Protects routes from unauthorized access
 * Automatically redirects to login if token is expired or user not signed in
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  console.log('🔐 Auth Guard: Checking authentication...');
  
  // Check if user is signed in and token is valid
  if (authService.isSignedIn()) {
    console.log('✅ Auth Guard: User authenticated, access granted');
    return true;
  }

  // Token expired or not signed in - redirect to login
  console.warn('⚠️ Auth Guard: Token expired or user not authenticated');
  console.log('🔄 Redirecting to login page...');
  
  // Store the attempted URL for redirect after login
  const attemptedUrl = state.url;
  if (attemptedUrl && attemptedUrl !== '/') {
    sessionStorage.setItem('redirect_after_login', attemptedUrl);
  }
  
  // Redirect to login
  router.navigate(['/']);
  return false;
};
