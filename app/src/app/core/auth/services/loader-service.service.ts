// src/app/services/loader.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * Central loading indicator used for:
 *  - every HTTP request (via loadingInterceptor)
 *  - every router navigation (via app.ts)
 *
 * Uses a small "show delay" so fast operations don't flash the overlay,
 * and a "minimum visible time" once shown so it never flickers on/off
 * within a few hundred milliseconds.
 */
@Injectable({
  providedIn: 'root'
})
export class LoaderService {
  private activeRequests = 0;
  private isLoadingSubject = new BehaviorSubject<boolean>(false);
  isLoading$: Observable<boolean> = this.isLoadingSubject.asObservable();

  /** Delay before the overlay is actually shown (avoids flicker for instant loads). */
  private readonly showDelayMs = 150;
  /** Once shown, keep visible for at least this long (avoids rapid flicker). */
  private readonly minVisibleMs = 300;

  private showTimer: ReturnType<typeof setTimeout> | null = null;
  private hideTimer: ReturnType<typeof setTimeout> | null = null;
  private shownAt = 0;

  show(): void {
    this.activeRequests++;

    if (this.isLoadingSubject.value) {
      // Already visible (or about to be) — cancel any pending hide.
      if (this.hideTimer) {
        clearTimeout(this.hideTimer);
        this.hideTimer = null;
      }
      return;
    }

    if (this.showTimer) {
      return; // Already scheduled to show.
    }

    this.showTimer = setTimeout(() => {
      this.showTimer = null;
      if (this.activeRequests > 0) {
        this.shownAt = Date.now();
        this.isLoadingSubject.next(true);
      }
    }, this.showDelayMs);
  }

  hide(): void {
    this.activeRequests = Math.max(0, this.activeRequests - 1);
    if (this.activeRequests > 0) {
      return;
    }

    // Nothing left pending. If the overlay was never actually shown, just
    // cancel the pending show.
    if (this.showTimer) {
      clearTimeout(this.showTimer);
      this.showTimer = null;
      return;
    }

    if (!this.isLoadingSubject.value) {
      return;
    }

    const elapsed = Date.now() - this.shownAt;
    const remaining = Math.max(0, this.minVisibleMs - elapsed);

    if (this.hideTimer) {
      clearTimeout(this.hideTimer);
    }
    this.hideTimer = setTimeout(() => {
      this.hideTimer = null;
      if (this.activeRequests === 0) {
        this.isLoadingSubject.next(false);
      }
    }, remaining);
  }
}

