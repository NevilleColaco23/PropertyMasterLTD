import { Injectable } from '@angular/core';
import {  HttpRequest,  HttpHandler,  HttpEvent,  HttpInterceptor} from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../../core/auth/services/auth.service';

@Injectable()
export class AccessTokenInterceptor implements HttpInterceptor {
  
  constructor(private authService: AuthService) {} 

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    
    const token = this.authService.getUserToken();

    if (token) {
      
      // 3. Clone the request and add the Authorization header
      const modifiedRequest = request.clone({
        setHeaders: { Authorization: `${token}` }
      });
      
      return next.handle(modifiedRequest);
    }
    
    return next.handle(request);
  }
}