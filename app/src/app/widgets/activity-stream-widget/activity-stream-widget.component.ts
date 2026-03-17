import { Component, Input, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivityService } from '../../services/activity.service';
import { ActivitySummaryDTO, UserActivityDTO, getActivityIcon, getActivityColor } from '../../models/activity.models';
import { Subject, takeUntil, interval, startWith, switchMap } from 'rxjs';
import { AuthService } from '../../core/auth/services/auth.service';

@Component({
  selector: 'app-activity-stream-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './activity-stream-widget.component.html',
  styleUrls: ['./activity-stream-widget.component.css']
})
export class ActivityStreamWidgetComponent implements OnInit, OnDestroy {
  @Input() settings: any = {};

  summary: ActivitySummaryDTO | null = null;
  loading = true;
  error: string | null = null;
  private destroy$ = new Subject<void>();

  // Period selection for filtering
  selectedPeriod: 'today' | 'yesterday' | 'thisMonth' | 'lastMonth' | 'previousMonth' | null = null;
  filteredActivities: UserActivityDTO[] = [];
  currentUserId: number | null = null;
  currentUsername: string | null = null;

  // Widget settings with defaults
  get title(): string {
    return this.settings?.title || 'User Activity';
  }

  get showStats(): boolean {
    return this.settings?.showStats !== false; // Default true
  }

  get maxActivities(): number {
    return this.settings?.maxActivities || 10;
  }

  get refreshInterval(): number {
    return this.settings?.refreshInterval || 60000; // 60 seconds
  }

  constructor(
    private activityService: ActivityService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {
    // Get current username for filtering (userId not available in token)
    this.authService.signInState.subscribe(userData => {
      this.currentUsername = userData?.username || null;
    });
  }

  ngOnInit(): void {
    // Setup auto-refresh using RxJS interval (runs inside Angular zone)
    if (this.refreshInterval > 0) {
      interval(this.refreshInterval)
        .pipe(
          startWith(0), // Start immediately
          switchMap(() => this.activityService.getActivitySummary(this.maxActivities, this.currentUsername || undefined)),
          takeUntil(this.destroy$)
        )
        .subscribe({
          next: (data) => {
            this.summary = data;
            this.filteredActivities = data.recentActivities;
            this.loading = false;
            this.error = null;
            this.cdr.markForCheck(); // Ensure change detection runs
          },
          error: (err) => {
            this.handleError(err);
            this.cdr.markForCheck(); // Ensure change detection runs
          }
        });
    } else {
      // No auto-refresh, load once
      this.loadData();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.loading = true;
    this.error = null;

    // Pass username to API for server-side filtering
    this.activityService.getActivitySummary(this.maxActivities, this.currentUsername || undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.summary = data;
          // No need to filter - already filtered on server
          this.filteredActivities = data.recentActivities;
          this.loading = false;
          this.cdr.markForCheck(); // Manually trigger change detection
        },
        error: (err) => {
          this.handleError(err);
          this.cdr.markForCheck(); // Manually trigger change detection
        }
      });
  }

  private handleError(err: any): void {
    console.error('Error loading activity summary:', err);
    // Check for authentication errors
    if (err.status === 401 || err.status === 403) {
      this.error = 'Session expired. Please log in again.';
    } else if (err.status === 0) {
      this.error = 'Unable to connect to server. Please check your connection.';
    } else {
      this.error = 'Failed to load activity data. Please try again.';
    }
    this.loading = false;
  }

  refresh(): void {
    this.loadData();
  }

  getIcon(activityType: string): string {
    return getActivityIcon(activityType);
  }

  getColor(activityType: string): string {
    return getActivityColor(activityType);
  }

  getActivityTypeEntries(): Array<{ type: string; count: number }> {
    if (!this.summary?.activityTypeCount) {
      return [];
    }
    
    return Object.entries(this.summary.activityTypeCount)
      .map(([type, count]) => ({ type, count }))
      .sort((a, b) => b.count - a.count)
      .slice(0, 5); // Top 5 activity types
  }

  getSuccessIcon(activity: UserActivityDTO): string {
    return activity.isSuccess ? 'check_circle' : 'error';
  }

  getSuccessColor(activity: UserActivityDTO): string {
    return activity.isSuccess ? '#4caf50' : '#f44336';
  }

  formatDuration(durationMs?: number): string {
    if (!durationMs) return '';
    if (durationMs < 1000) return `${durationMs}ms`;
    return `${(durationMs / 1000).toFixed(2)}s`;
  }

  selectPeriod(period: 'today' | 'yesterday' | 'thisMonth' | 'lastMonth' | 'previousMonth'): void {
    // Toggle selection: if same period clicked, deselect it
    if (this.selectedPeriod === period) {
      this.selectedPeriod = null;
      // Reset to all user's activities (already filtered by server)
      this.filteredActivities = this.summary?.recentActivities || [];
    } else {
      this.selectedPeriod = period;
      this.filterActivitiesByPeriod(period);
    }
  }

  private filterActivitiesByPeriod(period: 'today' | 'yesterday' | 'thisMonth' | 'lastMonth' | 'previousMonth'): void {
    if (!this.summary?.recentActivities) {
      this.filteredActivities = [];
      return;
    }

    // User filtering is already done on server, only filter by period on client
    const activities = this.summary.recentActivities;

    // Use UTC dates to match server-side timestamps
    const now = new Date();
    const utcNow = new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate(), now.getHours(), now.getMinutes(), now.getSeconds()));

    const startOfToday = new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0, 0));
    const endOfToday = new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59, 999));

    const startOfYesterday = new Date(startOfToday);
    startOfYesterday.setUTCDate(startOfYesterday.getUTCDate() - 1);
    const endOfYesterday = new Date(startOfToday.getTime() - 1);

    const startOfThisMonth = new Date(Date.UTC(now.getFullYear(), now.getMonth(), 1, 0, 0, 0, 0));

    const startOfLastMonth = new Date(Date.UTC(now.getFullYear(), now.getMonth() - 1, 1, 0, 0, 0, 0));
    const endOfLastMonth = new Date(startOfThisMonth.getTime() - 1);

    const startOfPreviousMonth = new Date(Date.UTC(now.getFullYear(), now.getMonth() - 2, 1, 0, 0, 0, 0));
    const endOfPreviousMonth = new Date(startOfLastMonth.getTime() - 1);

    console.log(`Filtering for period: ${period}`);
    console.log(`Start of Last Month (UTC): ${startOfLastMonth.toISOString()}`);
    console.log(`End of Last Month (UTC): ${endOfLastMonth.toISOString()}`);
    console.log(`Total activities to filter: ${activities.length}`);

    this.filteredActivities = activities.filter(activity => {
      // Handle both Date objects and string timestamps
      const activityDate = activity.timestamp instanceof Date 
        ? activity.timestamp 
        : new Date(activity.timestamp);

      const activityTime = activityDate.getTime();

      console.log(`Checking activity: ${activityDate.toISOString()}, time: ${activityTime}`);

      switch (period) {
        case 'today':
          return activityTime >= startOfToday.getTime() && activityTime <= endOfToday.getTime();

        case 'yesterday':
          return activityTime >= startOfYesterday.getTime() && activityTime <= endOfYesterday.getTime();

        case 'thisMonth':
          return activityTime >= startOfThisMonth.getTime();

        case 'lastMonth':
          const result = activityTime >= startOfLastMonth.getTime() && activityTime <= endOfLastMonth.getTime();
          console.log(`Activity ${activityDate.toISOString()} lastMonth check: ${result} (${activityTime} between ${startOfLastMonth.getTime()} and ${endOfLastMonth.getTime()})`);
          return result;

        case 'previousMonth':
          return activityTime >= startOfPreviousMonth.getTime() && activityTime <= endOfPreviousMonth.getTime();

        default:
          return true;
      }
    });

    console.log(`Filtered ${this.filteredActivities.length} activities for period: ${period}`);
  }

  getSelectedPeriodLabel(): string {
    switch (this.selectedPeriod) {
      case 'today':
        return 'Today';
      case 'yesterday':
        return 'Yesterday';
      case 'thisMonth':
        return 'This Month';
      case 'lastMonth':
        return 'Last Month';
      case 'previousMonth':
        return this.getPreviousMonthName();
      default:
        return '';
    }
  }

  getPreviousMonthName(): string {
    const now = new Date();
    const twoMonthsAgo = new Date(now.getFullYear(), now.getMonth() - 2, 1);
    const monthNames = ['January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December'];
    return monthNames[twoMonthsAgo.getMonth()];
  }

  getPreviousMonthLabel(): string {
    return this.getPreviousMonthName();
  }
}
