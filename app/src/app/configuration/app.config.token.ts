import { InjectionToken } from '@angular/core';

export interface AppConfig {
  apiUrl: string;
  // add other config properties
}

export const APP_CONFIG = new InjectionToken<AppConfig>('app.config');