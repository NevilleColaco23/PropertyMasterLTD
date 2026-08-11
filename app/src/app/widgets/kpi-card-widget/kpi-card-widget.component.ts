import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

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
  imports: [CommonModule, MatCardModule, MatIconModule, MatButtonModule, MatTooltipModule],
  template: `
    <mat-card class="kpi-card" [style.border-left-color]="data.color || '#1976d2'">
      <mat-card-content>
        <div class="kpi-header">
          <span class="kpi-icon-badge" [style.background]="(data.color || '#1976d2') + '1a'">
            <mat-icon [style.color]="data.color || '#1976d2'">{{ data.icon }}</mat-icon>
          </span>
          <span class="kpi-title">{{ data.title }}</span>
          <button mat-icon-button (click)="onRefresh()" class="refresh-button" matTooltip="Refresh">
            <mat-icon>refresh</mat-icon>
          </button>
          <button mat-icon-button (click)="onMoreDetails()" class="more-details-button" matTooltip="More Details">
            <mat-icon>open_in_new</mat-icon>
          </button>
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
      border-left: none;
      border-radius: 14px;
      transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
      box-shadow: none !important;
    }

    .kpi-card:hover {
      transform: translateY(-2px);
    }

    mat-card-content {
      padding: 16px 20px !important;
      display: flex;
      flex-direction: column;
      justify-content: center;
      height: 100%;
    }

    .kpi-header {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 14px;
    }

    .kpi-icon-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      border-radius: 12px;
      flex-shrink: 0;
    }

    .kpi-header mat-icon {
      font-size: 22px;
      width: 22px;
      height: 22px;
    }

    .kpi-title {
      font-size: 13px;
      font-weight: 600;
      color: #666;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      flex: 1;
    }

    .refresh-button {
      width: 32px;
      height: 32px;
      line-height: 32px;
    }

    .refresh-button mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
      color: #999;
      transition: transform 0.3s ease;
    }

    .refresh-button:hover mat-icon {
      transform: rotate(180deg);
      color: #666;
    }

    .more-details-button {
      width: 32px;
      height: 32px;
      line-height: 32px;
    }

    .more-details-button mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
      color: #999;
      transition: all 0.2s ease;
    }

    .more-details-button:hover mat-icon {
      transform: scale(1.1);
      color: #666;
    }

    .kpi-value {
      font-size: 34px;
      font-weight: 700;
      color: #101828;
      margin-bottom: 6px;
      line-height: 1.1;
      letter-spacing: -0.5px;
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
  @Output() refresh = new EventEmitter<{ timestamp: Date }>();
  @Output() moreDetails = new EventEmitter<void>();

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

  onRefresh(): void {
    console.log('🔄 KPI Widget Refresh clicked!');
    this.refresh.emit({ timestamp: new Date() });
  }

  onMoreDetails(): void {
    console.log('📋 KPI Widget More Details clicked!');
    this.moreDetails.emit();
  }
}
