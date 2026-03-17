import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivityService } from '../../services/activity.service';
import { Subject, takeUntil, interval, startWith, switchMap } from 'rxjs';

/**
 * Lightweight activity feed widget
 * Displays only DisplayMessage, Timestamp, and Action
 */
export interface ActivityWidgetData {
  displayMessage: string;
  timestamp: Date;
  action: string;
  timeAgo?: string;
}

@Component({
  selector: 'app-activity-feed-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './activity-feed-widget.component.html',
  styleUrls: ['./activity-feed-widget.component.css']
})
export class ActivityFeedWidgetComponent implements OnInit, OnDestroy {
  @Input() settings: any = {};
  
  activities: ActivityWidgetData[] = [];
  loading = true;
  error: string | null = null;
  private destroy$ = new Subject<void>();

  // Widget settings with defaults
  get title(): string {
    return this.settings?.title || 'Recent Activity Feed';
  }

  get count(): number {
    return this.settings?.count || 15;
  }

  get refreshInterval(): number {
    return this.settings?.refreshInterval || 30000; // 30 seconds
  }

  get showRefreshButton(): boolean {
    return this.settings?.showRefreshButton !== false; // Default true
  }

  constructor(private activityService: ActivityService) {}

  ngOnInit(): void {
    this.loadData();
    
    // Auto-refresh if interval is set
    if (this.refreshInterval > 0) {
      interval(this.refreshInterval)
        .pipe(
          startWith(0),
          switchMap(() => this.activityService.getWidgetData(this.count)),
          takeUntil(this.destroy$)
        )
        .subscribe({
          next: (data) => {
            this.activities = data.map(a => ({
              ...a,
              timestamp: new Date(a.timestamp),
              timeAgo: this.getTimeAgo(new Date(a.timestamp))
            }));
            this.loading = false;
            this.error = null;
          },
          error: (err) => {
            console.error('Error loading activity widget data:', err);
            this.error = 'Failed to load activity feed';
            this.loading = false;
          }
        });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.loading = true;
    this.error = null;

    this.activityService.getWidgetData(this.count)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.activities = data.map(a => ({
            ...a,
            timestamp: new Date(a.timestamp),
            timeAgo: this.getTimeAgo(new Date(a.timestamp))
          }));
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading activity widget data:', err);
          this.error = 'Failed to load activity feed';
          this.loading = false;
        }
      });
  }

  refresh(): void {
    this.loadData();
  }

  getTimeAgo(timestamp: Date): string {
    const now = new Date();
    const diff = now.getTime() - timestamp.getTime();
    const seconds = Math.floor(diff / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);
    const months = Math.floor(days / 30);
    const years = Math.floor(days / 365);

    if (seconds < 60) return 'just now';
    if (minutes < 60) return `${minutes} minute${minutes !== 1 ? 's' : ''} ago`;
    if (hours < 24) return `${hours} hour${hours !== 1 ? 's' : ''} ago`;
    if (days < 30) return `${days} day${days !== 1 ? 's' : ''} ago`;
    if (months < 12) return `${months} month${months !== 1 ? 's' : ''} ago`;
    return `${years} year${years !== 1 ? 's' : ''} ago`;
  }

  getActionIcon(action: string): string {
    const lowerAction = action.toLowerCase();
    
    if (lowerAction.includes('view') || lowerAction.includes('select')) return 'visibility';
    if (lowerAction.includes('create') || lowerAction.includes('add')) return 'add_circle';
    if (lowerAction.includes('update') || lowerAction.includes('edit')) return 'edit';
    if (lowerAction.includes('delete') || lowerAction.includes('remove')) return 'delete';
    if (lowerAction.includes('login') || lowerAction.includes('signin')) return 'login';
    if (lowerAction.includes('logout') || lowerAction.includes('signout')) return 'logout';
    if (lowerAction.includes('export')) return 'download';
    if (lowerAction.includes('import')) return 'upload';
    if (lowerAction.includes('search')) return 'search';
    if (lowerAction.includes('filter')) return 'filter_list';
    
    return 'info'; // Default icon
  }

  getActionColor(action: string): string {
    const lowerAction = action.toLowerCase();
    
    if (lowerAction.includes('create') || lowerAction.includes('add')) return 'success';
    if (lowerAction.includes('update') || lowerAction.includes('edit')) return 'primary';
    if (lowerAction.includes('delete') || lowerAction.includes('remove')) return 'warn';
    if (lowerAction.includes('view') || lowerAction.includes('select')) return 'accent';
    
    return 'default';
  }
}
