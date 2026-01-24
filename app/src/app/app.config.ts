//This is a INTERFACE for application configuration

import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { APP_CONFIG } from './app.config.token';


export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    {
      provide: APP_CONFIG,
      useValue: {
        apiUrl: 'https://localhost:44346/'
      }
    }
  ]
};