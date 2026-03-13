import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { WidgetLibraryItem } from '../../models/dashboard.models';

export interface WidgetPickerDialogData {
  availableWidgets: WidgetLibraryItem[];
  addedWidgetIds: string[];
}

@Component({
  selector: 'app-widget-picker-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>dashboard_customize</mat-icon>
      Add Widget to Dashboard
    </h2>

    <mat-dialog-content>
      <!-- Search -->
      <mat-form-field appearance="outline" class="search-field">
        <mat-label>Search widgets</mat-label>
        <input matInput [(ngModel)]="searchTerm" (ngModelChange)="filterWidgets()" placeholder="Search by name or category">
        <mat-icon matSuffix>search</mat-icon>
      </mat-form-field>

      <!-- Category Filter -->
      <div class="category-chips">
        <mat-chip-listbox [(ngModel)]="selectedCategory" (ngModelChange)="filterWidgets()">
          <mat-chip-option value="all">All</mat-chip-option>
          <mat-chip-option value="KPI">KPI</mat-chip-option>
          <mat-chip-option value="Analytics">Analytics</mat-chip-option>
          <mat-chip-option value="Bookings">Bookings</mat-chip-option>
          <mat-chip-option value="Activity">Activity</mat-chip-option>
        </mat-chip-listbox>
      </div>

      <!-- Widget Grid -->
      <div class="widget-grid">
        @for (widget of filteredWidgets; track widget.widgetId) {
          <mat-card 
            class="widget-card"
            [class.disabled]="isWidgetAdded(widget.widgetId)"
            (click)="!isWidgetAdded(widget.widgetId) && selectWidget(widget)">
            <mat-card-header>
              <mat-icon [style.color]="getCategoryColor(widget.category)">
                {{ widget.icon }}
              </mat-icon>
              <mat-card-title>{{ widget.name }}</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <p class="widget-description">{{ widget.description }}</p>
              <div class="widget-meta">
                <span class="category-badge" [style.background-color]="getCategoryColor(widget.category)">
                  {{ widget.category }}
                </span>
                <span class="widget-type">{{ widget.widgetType }}</span>
              </div>
            </mat-card-content>
            @if (isWidgetAdded(widget.widgetId)) {
              <div class="added-overlay">
                <mat-icon>check_circle</mat-icon>
                <span>Already Added</span>
              </div>
            }
          </mat-card>
        }
      </div>

      @if (filteredWidgets.length === 0) {
        <div class="no-results">
          <mat-icon>search_off</mat-icon>
          <p>No widgets found matching your criteria</p>
        </div>
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="cancel()">Cancel</button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 {
      display: flex;
      align-items: center;
      gap: 12px;
      color: #1976d2;
    }

    mat-dialog-content {
      min-width: 600px;
      max-height: 70vh;
      padding: 20px;
    }

    .search-field {
      width: 100%;
      margin-bottom: 16px;
    }

    .category-chips {
      margin-bottom: 20px;
    }

    .widget-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
      gap: 16px;
      margin-top: 16px;
    }

    .widget-card {
      cursor: pointer;
      transition: all 0.2s;
      position: relative;
      min-height: 180px;
    }

    .widget-card:hover:not(.disabled) {
      transform: translateY(-4px);
      box-shadow: 0 8px 16px rgba(0,0,0,0.2);
    }

    .widget-card.disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .widget-card mat-card-header {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 12px;
    }

    .widget-card mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
    }

    .widget-card mat-card-title {
      font-size: 16px;
      font-weight: 500;
    }

    .widget-description {
      font-size: 13px;
      color: #666;
      margin: 0 0 12px 0;
      line-height: 1.4;
    }

    .widget-meta {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 12px;
    }

    .category-badge {
      padding: 4px 8px;
      border-radius: 12px;
      font-size: 11px;
      font-weight: 600;
      color: white;
      text-transform: uppercase;
    }

    .widget-type {
      font-size: 11px;
      color: #999;
      text-transform: uppercase;
    }

    .added-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(76, 175, 80, 0.9);
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      color: white;
      font-weight: 600;
      gap: 8px;
    }

    .added-overlay mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
    }

    .no-results {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 40px;
      color: #999;
    }

    .no-results mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
    }
  `]
})
export class WidgetPickerDialogComponent implements OnInit {
  searchTerm = '';
  selectedCategory = 'all';
  filteredWidgets: WidgetLibraryItem[] = [];

  constructor(
    public dialogRef: MatDialogRef<WidgetPickerDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: WidgetPickerDialogData
  ) {}

  ngOnInit(): void {
    this.filteredWidgets = [...this.data.availableWidgets];
  }

  filterWidgets(): void {
    let filtered = [...this.data.availableWidgets];

    // Filter by category
    if (this.selectedCategory !== 'all') {
      filtered = filtered.filter(w => w.category === this.selectedCategory);
    }

    // Filter by search term
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(w =>
        w.name.toLowerCase().includes(term) ||
        w.description.toLowerCase().includes(term) ||
        w.category.toLowerCase().includes(term)
      );
    }

    this.filteredWidgets = filtered;
  }

  isWidgetAdded(widgetId: string): boolean {
    return this.data.addedWidgetIds.includes(widgetId);
  }

  selectWidget(widget: WidgetLibraryItem): void {
    this.dialogRef.close(widget);
  }

  cancel(): void {
    this.dialogRef.close();
  }

  getCategoryColor(category: string): string {
    const colors: { [key: string]: string } = {
      'KPI': '#1976d2',
      'Analytics': '#9c27b0',
      'Bookings': '#ff9800',
      'Activity': '#4caf50'
    };
    return colors[category] || '#757575';
  }
}
