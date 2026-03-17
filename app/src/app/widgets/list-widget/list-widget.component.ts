import { Component, Input, OnInit, OnChanges, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

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

export type ListViewType = 'bookings' | 'activity';

@Component({
  selector: 'app-list-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatListModule,
    MatDividerModule,
    MatButtonModule,
    MatTooltipModule
  ],
  template: `
    <mat-card class="list-widget">
      <mat-card-header>
        <button 
          mat-icon-button 
          class="nav-arrow" 
          (click)="toggleView('left')"
          [matTooltip]="currentView === 'bookings' ? 'Recent Activity' : 'Recent Bookings'">
          <mat-icon>chevron_left</mat-icon>
        </button>
        <mat-card-title>{{ getViewTitle() }}</mat-card-title>
        <button 
          mat-icon-button 
          class="nav-arrow" 
          (click)="toggleView('right')"
          [matTooltip]="currentView === 'bookings' ? 'Recent Activity' : 'Recent Bookings'">
          <mat-icon>chevron_right</mat-icon>
        </button>
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
      box-shadow: none !important;
    }

    mat-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 16px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-bottom: none;
      gap: 8px;
    }

    mat-card-title {
      font-size: 16px;
      font-weight: 600;
      margin: 0;
      color: white;
      flex: 1;
      text-align: center;
    }

    .nav-arrow {
      color: white;
      transition: transform 0.2s, opacity 0.2s;
    }

    .nav-arrow:hover {
      transform: scale(1.1);
      opacity: 0.8;
    }

    .item-count {
      font-size: 12px;
      color: white;
      background: rgba(255, 255, 255, 0.2);
      padding: 4px 12px;
      border-radius: 12px;
      font-weight: 600;
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
export class ListWidgetComponent implements OnInit, OnChanges {
  @Input() data!: ListWidgetData;
  @Input() settings: any = {};
  @Output() viewChange = new EventEmitter<ListViewType>();

  displayItems: ListItem[] = [];
  maxItems: number = 10;
  currentView: ListViewType = 'bookings'; // Default to Recent Bookings

  ngOnInit(): void {
    // Apply settings
    this.maxItems = this.settings?.itemsToShow || 10;

    // Limit displayed items
    this.updateDisplayItems();

    console.log('🎯 List Widget Init - currentView:', this.currentView, 'title:', this.getViewTitle());
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Detect when data input changes
    if (changes['data'] && !changes['data'].firstChange) {
      console.log('📊 List Widget data changed! Updating display items...');
      this.updateDisplayItems();
    }
  }

  private updateDisplayItems(): void {
    if (this.data && this.data.items) {
      this.displayItems = this.data.items.slice(0, this.maxItems);
      console.log('✅ Display items updated:', this.displayItems.length, 'items');
    }
  }

  toggleView(direction: 'left' | 'right'): void {
    console.log('🔄 Toggle View clicked! Current view:', this.currentView, '→ Switching to:', this.currentView === 'bookings' ? 'activity' : 'bookings');

    // Toggle between bookings and activity
    this.currentView = this.currentView === 'bookings' ? 'activity' : 'bookings';

    console.log('✅ View changed to:', this.currentView, 'Emitting event...');
    this.viewChange.emit(this.currentView);
  }

  getViewTitle(): string {
    return this.currentView === 'bookings' ? 'Recent Bookings' : 'Recent Activity';
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
