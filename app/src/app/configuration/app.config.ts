//This is a INTERFACE for application configuration

import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from '../app.routes';
import { APP_CONFIG } from '../configuration/app.config.token';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from '../core/auth/services/auth.interceptor';
import { propertyContextInterceptor } from '../core/interceptors/property-context.interceptor';
import { activityMessageInterceptor } from '../core/interceptors/activity-message.interceptor';
import { loadingInterceptor } from '../core/interceptors/loading.interceptor';
import { environment } from '../environments/environment';
import { provideNativeDateAdapter } from '@angular/material/core';

// Import Chart.js configuration
import './chart.config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([
      loadingInterceptor,
      authInterceptor,
      propertyContextInterceptor,
      activityMessageInterceptor
    ])),
    provideNativeDateAdapter(),
    {
      provide: APP_CONFIG,
      useValue: {
        apiUrl: environment.apiUrl
      }
    }
  ]
};
