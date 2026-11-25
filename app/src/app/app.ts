import { Component, signal, inject } from '@angular/core';
import { Router, RouteConfigLoadStart, RouteConfigLoadEnd, RouterOutlet } from '@angular/router';
import { AsyncPipe } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { filter } from 'rxjs/operators';
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

  constructor() {
    // Only show the global loader while a lazy-loaded route module/chunk is
    // actually being fetched (e.g. first visit to a feature page). Regular
    // in-page navigations (tabs, query param changes, same-route clicks) do
    // NOT trigger this, so the overlay no longer appears on every click.
    // HTTP-request-driven loading is still handled separately by
    // loading.interceptor.ts using the same LoaderService.
    this.router.events
      .pipe(filter(e => e instanceof RouteConfigLoadStart || e instanceof RouteConfigLoadEnd))
      .subscribe(event => {
        if (event instanceof RouteConfigLoadStart) {
          this.loaderService.show();
        } else {
          this.loaderService.hide();
        }
      });
  }
}
