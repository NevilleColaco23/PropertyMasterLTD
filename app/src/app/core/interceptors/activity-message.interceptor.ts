import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Interceptor that allows Angular services to attach human-readable activity messages
 * to API requests. These messages are used for activity logging on the backend.
 * 
 * Usage in service:
 * ```typescript
 * const headers = new HttpHeaders({
 *   'X-Activity-Message': 'Added Ocean View Suite to 3rd floor'
 * });
 * this.http.post(url, data, { headers });
 * ```
 */
export const activityMessageInterceptor: HttpInterceptorFn = (req, next) => {
  // This interceptor doesn't modify anything - it just passes through
  // The X-Activity-Message header is added directly by services when they have context
  
  // Optional: Log when activity messages are being sent
  const activityMessage = req.headers.get('X-Activity-Message');
  if (activityMessage) {
    console.log('📝 Activity message:', activityMessage);
  }

  return next(req);
};
