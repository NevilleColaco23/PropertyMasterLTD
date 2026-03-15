import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatBadgeModule } from '@angular/material/badge';
import { AnalyticsService } from '../../services/analytics.service';
import {
  ActivityAnalyticsSummary,
  UserActivityStats,
  ActivityTypeDistribution,
  PeakUsageTime,
  DailyActivityTrend,
  SecurityAlertSummary,
  PerformanceMetrics
} from '../../models/analytics.models';
import { Subject, takeUntil, forkJoin } from 'rxjs';

@Component({
  selector: 'app-analytics-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatTabsModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatChipsModule,
    MatSelectModule,
    MatFormFieldModule,
    MatExpansionModule,
    MatBadgeModule
  ],
  templateUrl: './analytics-widget.component.html',
  styleUrls: ['./analytics-widget.component.css']
})
export class AnalyticsWidgetComponent implements OnInit, OnDestroy {
  @Input() settings: any = {};
  
  // Data properties
  summary: ActivityAnalyticsSummary | null = null;
  topUsers: UserActivityStats[] = [];
  distribution: ActivityTypeDistribution[] = [];
  peakTimes: PeakUsageTime[] = [];
  dailyTrends: DailyActivityTrend[] = [];
  securityAlerts: SecurityAlertSummary | null = null;
  performanceMetrics: PerformanceMetrics[] = [];
  
  // UI state
  loading = true;
  error: string | null = null;
  selectedTab = 0;
  timeRange: 'today' | 'week' | 'month' = 'week';
  
  private destroy$ = new Subject<void>();

  // Chart data (will be populated from analytics data)
  distributionChartData: any = null;
  peakTimesChartData: any = null;
  trendsChartData: any = null;

  // Table columns
  userColumns: string[] = ['rank', 'username', 'activities', 'successRate', 'lastActivity'];
  performanceColumns: string[] = ['activityType', 'avgDuration', 'minDuration', 'maxDuration', 'count', 'slowCount'];

  // Widget settings with defaults
  get title(): string {
    return this.settings?.title || 'Analytics Dashboard';
  }

  get showSummary(): boolean {
    return this.settings?.showSummary !== false;
  }

  get showCharts(): boolean {
    return this.settings?.showCharts !== false;
  }

  get showSecurity(): boolean {
    return this.settings?.showSecurity !== false;
  }

  get refreshInterval(): number {
    return this.settings?.refreshInterval || 300000; // 5 minutes
  }

  get topUsersLimit(): number {
    return this.settings?.topUsersLimit || 10;
  }

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit(): void {
    this.loadAllData();
    
    // Auto-refresh if interval is set
    if (this.refreshInterval > 0) {
      setInterval(() => this.loadAllData(), this.refreshInterval);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load all analytics data
   */
  loadAllData(): void {
    this.loading = true;
    this.error = null;

    const { startDate, endDate } = this.analyticsService.getDateRange(this.timeRange);

    // Load all analytics in parallel
    forkJoin({
      summary: this.analyticsService.getAnalyticsSummary(startDate, endDate),
      topUsers: this.analyticsService.getTopActiveUsers(this.topUsersLimit, startDate, endDate),
      distribution: this.analyticsService.getActivityDistribution(startDate, endDate),
      peakTimes: this.analyticsService.getPeakUsageTimes(startDate, endDate),
      dailyTrends: this.analyticsService.getDailyTrends(30),
      securityAlerts: this.analyticsService.getSecurityAlerts(24),
      performanceMetrics: this.analyticsService.getPerformanceMetrics(startDate, endDate)
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.summary = data.summary;
          this.topUsers = data.topUsers;
          this.distribution = data.distribution;
          this.peakTimes = data.peakTimes;
          this.dailyTrends = data.dailyTrends;
          this.securityAlerts = data.securityAlerts;
          this.performanceMetrics = data.performanceMetrics;
          
          this.prepareChartData();
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading analytics:', err);
          this.error = 'Failed to load analytics data';
          this.loading = false;
        }
      });
  }

  /**
   * Prepare chart data from analytics
   */
  prepareChartData(): void {
    // Distribution pie chart data
    if (this.distribution.length > 0) {
      this.distributionChartData = {
        labels: this.distribution.map(d => d.activityType),
        datasets: [{
          data: this.distribution.map(d => d.count),
          backgroundColor: this.generateColors(this.distribution.length),
          percentages: this.distribution.map(d => d.percentage)
        }]
      };
    }

    // Peak times bar chart data
    if (this.peakTimes.length > 0) {
      this.peakTimesChartData = {
        labels: this.peakTimes.map(p => p.hourLabel),
        datasets: [{
          label: 'Activity Count',
          data: this.peakTimes.map(p => p.activityCount),
          backgroundColor: '#1976d2',
          borderColor: '#1565c0',
          borderWidth: 1
        }]
      };
    }

    // Daily trends line chart data
    if (this.dailyTrends.length > 0) {
      this.trendsChartData = {
        labels: this.dailyTrends.map(t => t.dateLabel),
        datasets: [
          {
            label: 'Total Activities',
            data: this.dailyTrends.map(t => t.totalActivities),
            borderColor: '#1976d2',
            backgroundColor: 'rgba(25, 118, 210, 0.1)',
            tension: 0.4,
            fill: true
          },
          {
            label: 'Successful',
            data: this.dailyTrends.map(t => t.successfulActivities),
            borderColor: '#4caf50',
            backgroundColor: 'rgba(76, 175, 80, 0.1)',
            tension: 0.4,
            fill: true
          },
          {
            label: 'Failed',
            data: this.dailyTrends.map(t => t.failedActivities),
            borderColor: '#f44336',
            backgroundColor: 'rgba(244, 67, 54, 0.1)',
            tension: 0.4,
            fill: true
          }
        ]
      };
    }
  }

  /**
   * Generate colors for pie chart
   */
  generateColors(count: number): string[] {
    const baseColors = [
      '#1976d2', // Blue
      '#4caf50', // Green
      '#ff9800', // Orange
      '#9c27b0', // Purple
      '#f44336', // Red
      '#00bcd4', // Cyan
      '#ffeb3b', // Yellow
      '#795548', // Brown
      '#607d8b', // Blue Grey
      '#e91e63'  // Pink
    ];

    const colors: string[] = [];
    for (let i = 0; i < count; i++) {
      colors.push(baseColors[i % baseColors.length]);
    }
    return colors;
  }

  /**
   * Change time range filter
   */
  onTimeRangeChange(range: 'today' | 'week' | 'month'): void {
    this.timeRange = range;
    this.loadAllData();
  }

  /**
   * Refresh all data
   */
  refresh(): void {
    this.loadAllData();
  }

  /**
   * Export current analytics data
   */
  exportData(): void {
    const { startDate, endDate } = this.analyticsService.getDateRange(this.timeRange);
    
    this.analyticsService.exportActivities(
      undefined, // userId
      undefined, // activityType
      undefined, // entityType
      startDate,
      endDate
    )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          const filename = `analytics-export-${this.timeRange}-${new Date().toISOString().split('T')[0]}.csv`;
          this.analyticsService.downloadAsCSV(data, filename);
        },
        error: (err) => {
          console.error('Error exporting data:', err);
          this.error = 'Failed to export data';
        }
      });
  }

  /**
   * Get success rate color
   */
  getSuccessRateColor(rate: number): string {
    if (rate >= 95) return 'success';
    if (rate >= 80) return 'warning';
    return 'error';
  }

  /**
   * Get performance color based on duration
   */
  getPerformanceColor(avgDuration: number): string {
    if (avgDuration < 100) return 'success';
    if (avgDuration < 500) return 'warning';
    return 'error';
  }

  /**
   * Format duration in milliseconds
   */
  formatDuration(ms: number): string {
    if (ms < 1000) return `${ms.toFixed(0)}ms`;
    return `${(ms / 1000).toFixed(2)}s`;
  }

  /**
   * Format large numbers
   */
  formatNumber(num: number): string {
    if (num >= 1000000) return `${(num / 1000000).toFixed(1)}M`;
    if (num >= 1000) return `${(num / 1000).toFixed(1)}K`;
    return num.toString();
  }

  /**
   * Format percentage
   */
  formatPercentage(num: number): string {
    return `${num.toFixed(1)}%`;
  }

  /**
   * Get icon for activity type
   */
  getActivityIcon(activityType: string): string {
    const iconMap: { [key: string]: string } = {
      'Login': 'login',
      'Logout': 'logout',
      'View': 'visibility',
      'Create': 'add_circle',
      'Update': 'edit',
      'Delete': 'delete',
      'Search': 'search',
      'Export': 'download',
      'Import': 'upload',
      'Error': 'error'
    };
    return iconMap[activityType] || 'description';
  }

  /**
   * Get color for activity type
   */
  getActivityColor(activityType: string): string {
    const colorMap: { [key: string]: string } = {
      'Login': '#4caf50',
      'Logout': '#9e9e9e',
      'View': '#2196f3',
      'Create': '#4caf50',
      'Update': '#ff9800',
      'Delete': '#f44336',
      'Search': '#9c27b0',
      'Export': '#00bcd4',
      'Import': '#ffeb3b',
      'Error': '#f44336'
    };
    return colorMap[activityType] || '#607d8b';
  }

  /**
   * Format date
   */
  formatDate(date: Date | null): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleString();
  }

  /**
   * Get time range label
   */
  getTimeRangeLabel(): string {
    const labels = {
      'today': 'Today',
      'week': 'Last 7 Days',
      'month': 'Last 30 Days'
    };
    return labels[this.timeRange];
  }

  /**
   * Get max count from peak times for chart scaling
   */
  getMaxPeakCount(): number {
    if (this.peakTimes.length === 0) return 1;
    return Math.max(...this.peakTimes.map(p => p.activityCount));
  }

  /**
   * Get max count from trends for chart scaling
   */
  getMaxTrendCount(): number {
    if (this.dailyTrends.length === 0) return 1;
    return Math.max(...this.dailyTrends.map(t => t.totalActivities));
  }
}
