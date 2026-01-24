import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { provideServerRendering, withRoutes } from '@angular/ssr';
import { appConfig } from './app.config'; 

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering()
  ]
};

// Merge the client config with server config
export const config = mergeApplicationConfig(appConfig, serverConfig);