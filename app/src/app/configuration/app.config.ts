//This is a INTERFACE for application configuration

import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from '../app.routes';
import { APP_CONFIG } from '../configuration/app.config.token';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from '../core/auth/services/auth.interceptor';
import { environment } from '../environments/environment';
import { provideNativeDateAdapter } from '@angular/material/core';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideNativeDateAdapter(),
    {
      provide: APP_CONFIG,
      useValue: {
        apiUrl: environment.apiUrl
      }
    }
  ]
};
