import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Property Selection Guard - Ensures user has selected properties before accessing property-specific routes
 * Redirects to property selector if no properties are selected
 */
export const propertySelectionGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  console.log('🏢 Property Selection Guard: Checking property selection...');
  
  // First, ensure user is authenticated
  if (!authService.isSignedIn()) {
    console.warn('⚠️ Property Selection Guard: User not authenticated');
    router.navigate(['/']);
    return false;
  }

  // Check if properties are selected in localStorage
  const selectedProperties = localStorage.getItem('selectedPropertyIds');
  
  if (selectedProperties && selectedProperties !== '[]' && selectedProperties !== 'null') {
    try {
      const propertyIds = JSON.parse(selectedProperties);
      if (Array.isArray(propertyIds) && propertyIds.length > 0) {
        console.log('✅ Property Selection Guard: Properties selected, access granted', propertyIds);
        return true;
      }
    } catch (e) {
      console.error('Error parsing selectedPropertyIds from localStorage:', e);
    }
  }

  // No properties selected - redirect to property selector
  console.warn('⚠️ Property Selection Guard: No properties selected');
  console.log('🔄 Redirecting to property selector...');
  
  // Store the attempted URL for redirect after property selection
  const attemptedUrl = state.url;
  if (attemptedUrl && attemptedUrl !== '/propertySelector') {
    sessionStorage.setItem('redirect_after_property_selection', attemptedUrl);
  }
  
  // Redirect to property selector
  router.navigate(['/propertySelector']);
  return false;
};
