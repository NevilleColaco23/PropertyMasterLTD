import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';

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

@Component({
  selector: 'app-chart-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule
  ],
  template: `
    <mat-card class="chart-widget">
      <mat-card-header>
        <mat-card-title>{{ data.title }}</mat-card-title>
        <button mat-icon-button [matMenuTriggerFor]="menu" class="chart-menu">
          <mat-icon>more_vert</mat-icon>
        </button>
        <mat-menu #menu="matMenu">
          <button mat-menu-item (click)="changeChartType('line')">
            <mat-icon>show_chart</mat-icon>
            <span>Line Chart</span>
          </button>
          <button mat-menu-item (click)="changeChartType('bar')">
            <mat-icon>bar_chart</mat-icon>
            <span>Bar Chart</span>
          </button>
          <button mat-menu-item (click)="changeChartType('pie')">
            <mat-icon>pie_chart</mat-icon>
            <span>Pie Chart</span>
          </button>
        </mat-menu>
      </mat-card-header>
      <mat-card-content>
        <div class="chart-container">
          @if (chartType === 'line' || chartType === 'bar') {
            <canvas #chartCanvas></canvas>
          } @else if (chartType === 'pie' || chartType === 'doughnut') {
            <div class="pie-chart-placeholder">
              <mat-icon>pie_chart</mat-icon>
              <p>{{ chartType | titlecase }} Chart</p>
              <small>Install ng2-charts for chart rendering</small>
            </div>
          } @else {
            <div class="chart-placeholder">
              <mat-icon>analytics</mat-icon>
              <p>Chart visualization</p>
              <small>Install ng2-charts library:</small>
              <code>npm install ng2-charts chart.js</code>
            </div>
          }
        </div>
        
        <!-- Simple data visualization (no library required) -->
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
    }

    mat-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px;
      background: #f5f5f5;
      border-bottom: 1px solid #e0e0e0;
    }

    mat-card-title {
      font-size: 16px;
      font-weight: 500;
      margin: 0;
      flex: 1;
    }

    .chart-menu {
      margin-left: auto;
    }

    mat-card-content {
      flex: 1;
      padding: 16px !important;
      overflow-y: auto;
    }

    .chart-container {
      position: relative;
      height: 250px;
      margin-bottom: 16px;
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
    }

    .dataset-info {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px;
      background: #f9f9f9;
      border-radius: 6px;
      border-left: 4px solid #1976d2;
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
  `]
})
export class ChartWidgetComponent implements OnInit {
  @Input() data!: ChartWidgetData;
  @Input() settings: any = {};
  
  chartType: ChartType = 'line';

  ngOnInit(): void {
    // Apply settings
    this.chartType = this.settings?.chartType || this.data?.chartType || 'line';
    
    // In a real implementation, you would initialize Chart.js here
    // this.initializeChart();
  }

  changeChartType(type: ChartType): void {
    this.chartType = type;
    // In a real implementation, you would re-render the chart
    // this.updateChart();
  }

  calculateTotal(data: number[]): number {
    return data.reduce((sum, value) => sum + value, 0);
  }

  calculateAverage(data: number[]): number {
    if (data.length === 0) return 0;
    const total = this.calculateTotal(data);
    return Math.round(total / data.length);
  }

  // Private method to initialize Chart.js (when library is installed)
  // private initializeChart(): void {
  //   // Chart.js initialization code
  // }
}
