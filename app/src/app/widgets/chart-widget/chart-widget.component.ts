import { Component, Input, OnInit, OnChanges, SimpleChanges, ViewChild, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartType as ChartJsType } from 'chart.js';

export type ChartType = 'line' | 'bar' | 'pie' | 'doughnut';

export interface ChartDataset {
  label: string;
  data: number[];
  backgroundColor?: string | string[];
  borderColor?: string;
  borderWidth?: number;
}

export interface ChartWidgetData {
  title: string;
  labels: string[];
  datasets: ChartDataset[];
  chartType?: ChartType;
}

export interface ChartFilterChange {
  daysBack: number;
  groupBy: 'day' | 'week' | 'month';
}

export interface ChartRefreshEvent {
  timestamp: Date;
}

@Component({
  selector: 'app-chart-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
    MatTooltipModule,
    BaseChartDirective
  ],
  template: `
    <mat-card class="chart-widget">
      <mat-card-header>
        <mat-card-title>
          {{ data.title }}
          <span class="subtitle">{{ currentPeriodLabel }}</span>
        </mat-card-title>
        <button mat-icon-button (click)="onRefresh()" class="refresh-button" matTooltip="Refresh Widget">
          <mat-icon>refresh</mat-icon>
        </button>
        <button mat-icon-button (click)="onMoreDetails()" class="more-details-button" matTooltip="More Details">
          <mat-icon>open_in_new</mat-icon>
        </button>
        <button mat-icon-button [matMenuTriggerFor]="menu" class="chart-menu">
          <mat-icon>more_vert</mat-icon>
        </button>
        <mat-menu #menu="matMenu">
          <!-- Chart Type Options -->
          <div class="menu-section">
            <div class="menu-section-label">Chart Type</div>
            <button mat-menu-item (click)="changeChartType('line')" [class.active]="chartType === 'line'">
              <mat-icon>show_chart</mat-icon>
              <span>Line Chart</span>
              @if (chartType === 'line') {
                <mat-icon class="check-icon">check</mat-icon>
              }
            </button>
            <button mat-menu-item (click)="changeChartType('bar')" [class.active]="chartType === 'bar'">
              <mat-icon>bar_chart</mat-icon>
              <span>Bar Chart</span>
              @if (chartType === 'bar') {
                <mat-icon class="check-icon">check</mat-icon>
              }
            </button>
          </div>

          <mat-divider></mat-divider>

          <!-- Time Period Options -->
          <div class="menu-section">
            <div class="menu-section-label">Time Period</div>
            <button mat-menu-item (click)="changePeriod(7, 'day')" [class.active]="daysBack === 7">
              <mat-icon>date_range</mat-icon>
              <span>Last 7 Days</span>
              @if (daysBack === 7) {
                <mat-icon class="check-icon">check</mat-icon>
              }
            </button>
            <button mat-menu-item (click)="changePeriod(30, 'day')" [class.active]="daysBack === 30 && groupBy === 'day'">
              <mat-icon>calendar_today</mat-icon>
              <span>Last 30 Days</span>
              @if (daysBack === 30 && groupBy === 'day') {
                <mat-icon class="check-icon">check</mat-icon>
              }
            </button>
            <button mat-menu-item (click)="changePeriod(90, 'week')" [class.active]="daysBack === 90">
              <mat-icon>calendar_month</mat-icon>
              <span>Last 90 Days (Weekly)</span>
              @if (daysBack === 90) {
                <mat-icon class="check-icon">check</mat-icon>
              }
            </button>
            <button mat-menu-item (click)="changePeriod(180, 'month')" [class.active]="daysBack === 180">
              <mat-icon>event</mat-icon>
              <span>Last 6 Months</span>
              @if (daysBack === 180) {
                <mat-icon class="check-icon">check</mat-icon>
              }
            </button>
          </div>
        </mat-menu>
      </mat-card-header>
      <mat-card-content>
        <div class="chart-container">
          <canvas
            baseChart
            [type]="chartType"
            [data]="chartData"
            [options]="chartOptions">
          </canvas>
        </div>

        <!-- Data summary -->
        <div class="data-summary">
          @for (dataset of data.datasets; track dataset.label) {
            <div class="dataset-info">
              <div class="dataset-label">
                <span class="color-indicator" [style.background-color]="dataset.backgroundColor"></span>
                {{ dataset.label }}
              </div>
              <div class="dataset-stats">
                <span class="stat">
                  <small>Total:</small>
                  <strong>{{ calculateTotal(dataset.data) }}</strong>
                </span>
                <span class="stat">
                  <small>Avg:</small>
                  <strong>{{ calculateAverage(dataset.data) }}</strong>
                </span>
              </div>
            </div>
          }
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
      width: 100%;
    }

    .chart-widget {
      height: 100%;
      display: flex;
      flex-direction: column;
      box-shadow: none !important;
      border-radius: 14px;
      overflow: hidden;
    }

    mat-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 20px;
      background: #fbfbfe;
      border-bottom: 1px solid #eef0f4;
    }

    mat-card-title {
      font-size: 15px;
      font-weight: 700;
      margin: 0;
      flex: 1;
      color: #111827;
    }

    .chart-menu {
      margin-left: auto;
      color: #6b7280;
    }

    .refresh-button {
      color: #6b7280;
      transition: transform 0.3s ease;
    }

    .refresh-button:hover {
      transform: rotate(180deg);
      color: #1976d2;
    }

    .more-details-button {
      color: #6b7280;
      transition: all 0.2s ease;
    }

    .more-details-button:hover {
      transform: scale(1.1);
      color: #1976d2;
    }

    mat-card-content {
      flex: 1;
      padding: 16px !important;
      overflow: hidden;
      display: flex;
      flex-direction: column;
    }

    .chart-container {
      position: relative;
      height: 250px;
      margin-bottom: 16px;
      flex-shrink: 0;
    }

    canvas {
      width: 100% !important;
      height: 100% !important;
    }

    .chart-placeholder,
    .pie-chart-placeholder {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      color: #999;
      text-align: center;
      background: #f9f9f9;
      border: 2px dashed #e0e0e0;
      border-radius: 8px;
      padding: 20px;
    }

    .chart-placeholder mat-icon,
    .pie-chart-placeholder mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
      opacity: 0.5;
    }

    .chart-placeholder p,
    .pie-chart-placeholder p {
      margin: 8px 0;
      font-size: 16px;
      font-weight: 500;
      color: #666;
    }

    .chart-placeholder small,
    .pie-chart-placeholder small {
      font-size: 12px;
      color: #999;
      margin-top: 8px;
    }

    code {
      display: block;
      background: #f0f0f0;
      padding: 8px 12px;
      border-radius: 4px;
      margin-top: 8px;
      font-family: 'Courier New', monospace;
      font-size: 11px;
      color: #d32f2f;
    }

    .data-summary {
      display: flex;
      flex-direction: column;
      gap: 12px;
      flex-shrink: 0;
    }

    .dataset-info {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 14px;
      background: #f7f8fb;
      border-radius: 10px;
      border-left: none;
    }

    .dataset-label {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 500;
      color: #333;
    }

    .color-indicator {
      width: 12px;
      height: 12px;
      border-radius: 50%;
      border: 2px solid white;
      box-shadow: 0 0 0 1px rgba(0,0,0,0.1);
    }

    .dataset-stats {
      display: flex;
      gap: 16px;
    }

    .stat {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
    }

    .stat small {
      font-size: 10px;
      color: #999;
      text-transform: uppercase;
    }

    .stat strong {
      font-size: 16px;
      color: #1976d2;
    }

    .subtitle {
      display: block;
      font-size: 11px;
      font-weight: 500;
      color: #6b7280;
      margin-top: 2px;
    }

    .menu-section {
      padding: 8px 0;
    }

    .menu-section-label {
      padding: 8px 16px 4px;
      font-size: 11px;
      font-weight: 600;
      color: #999;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    ::ng-deep .mat-mdc-menu-content {
      padding: 4px 0 !important;
    }

    ::ng-deep .mat-mdc-menu-item {
      min-height: 40px !important;
      display: flex !important;
      align-items: center !important;
      gap: 12px !important;
    }

    ::ng-deep .mat-mdc-menu-item.active {
      background-color: rgba(25, 118, 210, 0.08);
      color: #1976d2;
    }

    ::ng-deep .mat-mdc-menu-item.active .mat-icon {
      color: #1976d2;
    }

    ::ng-deep .mat-mdc-menu-item .check-icon {
      margin-left: auto;
      font-size: 18px;
      width: 18px;
      height: 18px;
    }

    ::ng-deep .mat-divider {
      margin: 4px 0 !important;
    }
  `]
})
export class ChartWidgetComponent implements OnInit, OnChanges {
  @Input() data!: ChartWidgetData;
  @Input() settings: any = {};
  @Output() filterChange = new EventEmitter<ChartFilterChange>();
  @Output() refresh = new EventEmitter<ChartRefreshEvent>();
  @Output() moreDetails = new EventEmitter<void>();
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;

  chartType: ChartJsType = 'line';
  daysBack: number = 30;
  groupBy: 'day' | 'week' | 'month' = 'day';
  currentPeriodLabel: string = 'Last 30 Days';

  chartData: ChartConfiguration['data'] = {
    labels: [],
    datasets: []
  };
  chartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'bottom',
        labels: {
          usePointStyle: true,
          padding: 15,
          font: {
            size: 12
          }
        }
      },
      tooltip: {
        enabled: true,
        mode: 'index',
        intersect: false,
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        titleFont: {
          size: 14,
          weight: 'bold'
        },
        bodyFont: {
          size: 13
        },
        padding: 12,
        cornerRadius: 6
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          precision: 0,
          font: {
            size: 11
          }
        },
        grid: {
          color: 'rgba(0, 0, 0, 0.05)'
        }
      },
      x: {
        ticks: {
          font: {
            size: 11
          },
          maxRotation: 45,
          minRotation: 0
        },
        grid: {
          display: false
        }
      }
    },
    interaction: {
      mode: 'nearest',
      axis: 'x',
      intersect: false
    }
  };

  ngOnChanges(changes: SimpleChanges): void {
    // Detect when data input changes (e.g., from period filter change)
    if (changes['data'] && !changes['data'].firstChange) {
      console.log('📊 Chart data changed, updating chart...');
      this.initializeChart();
      // Trigger chart update after a brief delay to ensure DOM is ready
      setTimeout(() => {
        if (this.chart) {
          this.chart.update();
          console.log('✅ Chart updated with new data');
        }
      }, 100);
    }
  }

  ngOnInit(): void {
    // Apply settings
    this.chartType = (this.settings?.chartType || this.data?.chartType || 'line') as ChartJsType;
    this.initializeChart();
  }

  private initializeChart(): void {
    if (!this.data) return;

    // Prepare chart data
    this.chartData = {
      labels: this.data.labels,
      datasets: this.data.datasets.map(dataset => ({
        label: dataset.label,
        data: dataset.data,
        backgroundColor: this.chartType === 'line' 
          ? 'rgba(25, 118, 210, 0.1)' 
          : dataset.backgroundColor || '#1976d2',
        borderColor: dataset.borderColor || '#1976d2',
        borderWidth: dataset.borderWidth || 2,
        fill: this.chartType === 'line',
        tension: 0.4, // Smooth curves for line charts
        pointRadius: 3,
        pointHoverRadius: 5,
        pointBackgroundColor: '#fff',
        pointBorderColor: dataset.borderColor || '#1976d2',
        pointBorderWidth: 2
      }))
    };
  }

  changeChartType(type: 'line' | 'bar'): void {
    this.chartType = type as ChartJsType;
    this.initializeChart();
    this.chart?.update();
  }

  changePeriod(days: number, group: 'day' | 'week' | 'month'): void {
    this.daysBack = days;
    this.groupBy = group;
    this.updatePeriodLabel();
    this.filterChange.emit({ daysBack: days, groupBy: group });
  }

  private updatePeriodLabel(): void {
    if (this.daysBack === 7) {
      this.currentPeriodLabel = 'Last 7 Days';
    } else if (this.daysBack === 30 && this.groupBy === 'day') {
      this.currentPeriodLabel = 'Last 30 Days';
    } else if (this.daysBack === 90) {
      this.currentPeriodLabel = 'Last 90 Days (Weekly)';
    } else if (this.daysBack === 180) {
      this.currentPeriodLabel = 'Last 6 Months';
    }
  }

  calculateTotal(data: number[]): number {
    return data.reduce((sum, value) => sum + value, 0);
  }

  calculateAverage(data: number[]): number {
    if (data.length === 0) return 0;
    const total = this.calculateTotal(data);
    return Math.round(total / data.length);
  }

  onRefresh(): void {
    console.log('🔄 Chart Widget Refresh clicked!');
    this.refresh.emit({ timestamp: new Date() });
  }

  onMoreDetails(): void {
    console.log('📋 Chart Widget More Details clicked!');
    this.moreDetails.emit();
  }
}
