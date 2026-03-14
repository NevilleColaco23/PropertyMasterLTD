import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

export interface KpiCardData {
  title: string;
  value: number | string;
  icon: string;
  color?: string;
  showTrend?: boolean;
  trendValue?: number;
  trendDirection?: 'up' | 'down';
}

@Component({
  selector: 'app-kpi-card-widget',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  template: `
    <mat-card class="kpi-card" [style.border-left-color]="data.color || '#1976d2'">
      <mat-card-content>
        <div class="kpi-header">
          <mat-icon [style.color]="data.color || '#1976d2'">{{ data.icon }}</mat-icon>
          <span class="kpi-title">{{ data.title }}</span>
        </div>
        <div class="kpi-value">{{ data.value }}</div>
        <div class="kpi-trend" *ngIf="data.showTrend && data.trendValue !== undefined">
          <mat-icon 
            [class.trend-up]="data.trendDirection === 'up'" 
            [class.trend-down]="data.trendDirection === 'down'">
            {{ data.trendDirection === 'up' ? 'trending_up' : 'trending_down' }}
          </mat-icon>
          <span>{{ data.trendValue }}%</span>
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

    .kpi-card {
      height: 100%;
      border-left: 4px solid;
      transition: transform 0.2s, box-shadow 0.2s;
    }

    .kpi-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 8px rgba(0,0,0,0.15);
    }

    mat-card-content {
      padding: 16px !important;
    }

    .kpi-header {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 12px;
    }

    .kpi-header mat-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
    }

    .kpi-title {
      font-size: 14px;
      font-weight: 500;
      color: #666;
      text-transform: uppercase;
    }

    .kpi-value {
      font-size: 32px;
      font-weight: 600;
      color: #333;
      margin-bottom: 8px;
    }

    .kpi-trend {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 14px;
    }

    .kpi-trend mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
    }

    .trend-up {
      color: #4caf50;
    }

    .trend-down {
      color: #f44336;
    }
  `]
})
export class KpiCardWidgetComponent implements OnInit {
  @Input() data!: KpiCardData;
  @Input() settings: any = {};

  ngOnInit(): void {
    console.log('🎯 KPI Widget initialized with data:', this.data);
    // Merge settings with data
    if (this.settings) {
      this.data = {
        title: this.settings.title || this.data?.title || 'KPI',
        value: this.data?.value || 0,
        icon: this.settings.icon || this.data?.icon || 'analytics',
        color: this.settings.color || this.data?.color || '#1976d2',
        showTrend: this.settings.showTrend || this.data?.showTrend || false,
        trendValue: this.data?.trendValue,
        trendDirection: this.data?.trendDirection
      };
      console.log('🎯 KPI Widget after settings merge:', this.data);
    }
  }
}
