import { Component, signal, inject } from '@angular/core';
import { Router, NavigationEnd, RouterOutlet } from '@angular/router';
import { AsyncPipe } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { filter, first } from 'rxjs/operators';
import { LoaderService } from './core/auth/services/loader-service.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AsyncPipe, MatProgressSpinnerModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('propertyMaster');
  loaderService = inject(LoaderService);
  router = inject(Router);

  // Set to true once the first navigation completes. Used to avoid showing
  // the large global loader after the app has already rendered — allow
  // smaller component-level loaders to run instead.
  initialLoadCompleted = false;

  constructor() {
    // Mark initial load completed after the first successful navigation end
    // so subsequent background requests do not trigger the full-page loader.
    this.router.events.pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd), first()).subscribe(() => {
      this.initialLoadCompleted = true;
    });
  }
}
