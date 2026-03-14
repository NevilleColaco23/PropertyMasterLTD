import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';

export interface ListItem {
  id: string | number;
  icon?: string;
  iconColor?: string;
  title: string;
  subtitle?: string;
  timestamp?: Date | string;
  metadata?: string;
}

export interface ListWidgetData {
  title: string;
  items: ListItem[];
  emptyMessage?: string;
}

@Component({
  selector: 'app-list-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatListModule,
    MatDividerModule
  ],
  template: `
    <mat-card class="list-widget">
      <mat-card-header>
        <mat-card-title>{{ data.title }}</mat-card-title>
        <div class="item-count">{{ data.items.length }} items</div>
      </mat-card-header>
      <mat-card-content>
        @if (data.items.length > 0) {
          <mat-list>
            @for (item of displayItems; track item.id) {
              <mat-list-item class="list-item">
                <mat-icon 
                  matListItemIcon 
                  [style.color]="item.iconColor || '#1976d2'">
                  {{ item.icon || 'circle' }}
                </mat-icon>
                <div matListItemTitle class="item-title">{{ item.title }}</div>
                @if (item.subtitle) {
                  <div matListItemLine class="item-subtitle">{{ item.subtitle }}</div>
                }
                <div matListItemMeta class="item-meta">
                  @if (item.timestamp) {
                    <span class="timestamp">{{ formatTimestamp(item.timestamp) }}</span>
                  }
                  @if (item.metadata) {
                    <span class="metadata">{{ item.metadata }}</span>
                  }
                </div>
              </mat-list-item>
              <mat-divider></mat-divider>
            }
          </mat-list>
        } @else {
          <div class="empty-state">
            <mat-icon>inbox</mat-icon>
            <p>{{ data.emptyMessage || 'No items to display' }}</p>
          </div>
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
      width: 100%;
    }

    .list-widget {
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
    }

    .item-count {
      font-size: 12px;
      color: #666;
      background: white;
      padding: 4px 12px;
      border-radius: 12px;
      font-weight: 500;
    }

    mat-card-content {
      flex: 1;
      overflow-y: auto;
      padding: 0 !important;
    }

    mat-list {
      padding: 0;
    }

    .list-item {
      padding: 12px 16px;
      transition: background-color 0.2s;
      cursor: pointer;
    }

    .list-item:hover {
      background-color: #f9f9f9;
    }

    .list-item mat-icon {
      margin-right: 12px;
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .item-title {
      font-weight: 500;
      color: #333;
      font-size: 14px;
    }

    .item-subtitle {
      color: #666;
      font-size: 12px;
      margin-top: 4px;
    }

    .item-meta {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 4px;
      font-size: 11px;
      color: #999;
    }

    .timestamp {
      white-space: nowrap;
    }

    .metadata {
      background: #e3f2fd;
      color: #1976d2;
      padding: 2px 8px;
      border-radius: 10px;
      font-weight: 500;
    }

    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 40px 20px;
      color: #999;
      text-align: center;
    }

    .empty-state mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 12px;
      opacity: 0.5;
    }

    .empty-state p {
      margin: 0;
      font-size: 14px;
    }

    mat-divider {
      margin: 0;
    }
  `]
})
export class ListWidgetComponent implements OnInit {
  @Input() data!: ListWidgetData;
  @Input() settings: any = {};
  
  displayItems: ListItem[] = [];
  maxItems: number = 10;

  ngOnInit(): void {
    // Apply settings
    this.maxItems = this.settings?.itemsToShow || 10;
    
    // Limit displayed items
    this.displayItems = this.data.items.slice(0, this.maxItems);
  }

  /**
   * Format timestamp to relative time (e.g., "2 hours ago")
   */
  formatTimestamp(timestamp: Date | string): string {
    const date = typeof timestamp === 'string' ? new Date(timestamp) : timestamp;
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    
    return date.toLocaleDateString();
  }
}
