import { Component, Input, OnInit, OnDestroy } from '@angular/core';
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
import { Subject, takeUntil } from 'rxjs';

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

  constructor(private activityService: ActivityService) {}

  ngOnInit(): void {
    this.loadData();
    
    // Auto-refresh if interval is set
    if (this.refreshInterval > 0) {
      setInterval(() => this.loadData(), this.refreshInterval);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.loading = true;
    this.error = null;

    this.activityService.getActivitySummary(this.maxActivities)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.summary = data;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading activity summary:', err);
          this.error = 'Failed to load activity data';
          this.loading = false;
        }
      });
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
}
