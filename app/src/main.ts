import { bootstrapApplication } from '@angular/platform-browser';
import { AppConfig  } from './app/app.config';
import { AppComponent } from './app/app';

bootstrapApplication(AppComponent, { providers: [AppConfig] })
  .catch((err) => console.error(err));
  