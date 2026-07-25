import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * No-Guest Guard — blocks guest users from reaching write/admin routes.
 * Redirect guest back to the property landing dashboard.
 */
export const noGuestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isGuestUser()) {
    console.warn('🚫 No-Guest Guard: guest users cannot access this route.');
    router.navigate(['/propertyLanding']);
    return false;
  }

  return true;
};
