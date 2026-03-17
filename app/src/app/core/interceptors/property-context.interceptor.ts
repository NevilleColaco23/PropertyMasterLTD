import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Interceptor that adds the selected property ID(s) to all HTTP requests
 * This allows the backend to automatically track which property context the user is working in
 */
export const propertyContextInterceptor: HttpInterceptorFn = (req, next) => {
  // Get selected property IDs from localStorage
  const storedIds = localStorage.getItem('selectedPropertyIds');
  
  if (!storedIds) {
    // No property selected, proceed normally
    return next(req);
  }

  try {
    const propertyIds: number[] = JSON.parse(storedIds);
    
    if (propertyIds.length > 0) {
      // For now, send the first selected property ID
      // (You can modify this if you want to send all IDs as comma-separated)
      const selectedPropertyId = propertyIds[0].toString();
      
      // Clone request and add custom header
      const modifiedReq = req.clone({
        setHeaders: {
          'X-Selected-Property': selectedPropertyId
        }
      });
      
      console.log('🏠 Adding selected property header:', selectedPropertyId);
      return next(modifiedReq);
    }
  } catch (error) {
    console.warn('⚠️ Failed to parse selected property IDs:', error);
  }

  // If parsing failed or no valid IDs, proceed normally
  return next(req);
};
