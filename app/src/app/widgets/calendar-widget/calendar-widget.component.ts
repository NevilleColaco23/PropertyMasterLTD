import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

export interface CalendarEvent {
  id: string | number;
  title: string;
  start: Date;
  end?: Date;
  color?: string;
  type?: string;
  description?: string;
}

export interface CalendarWidgetData {
  title: string;
  events: CalendarEvent[];
  currentDate?: Date;
}

@Component({
  selector: 'app-calendar-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule
  ],
  template: `
    <mat-card class="calendar-widget">
      <mat-card-header>
        <mat-card-title>{{ data.title }}</mat-card-title>
        <div class="calendar-controls">
          <button mat-icon-button (click)="previousMonth()">
            <mat-icon>chevron_left</mat-icon>
          </button>
          <span class="current-month">{{ currentMonthYear }}</span>
          <button mat-icon-button (click)="nextMonth()">
            <mat-icon>chevron_right</mat-icon>
          </button>
          <button mat-icon-button (click)="goToToday()" matTooltip="Today">
            <mat-icon>today</mat-icon>
          </button>
        </div>
      </mat-card-header>
      <mat-card-content>
        <div class="calendar-grid">
          <!-- Day headers -->
          <div class="calendar-header">
            @for (day of weekDays; track day) {
              <div class="day-header">{{ day }}</div>
            }
          </div>
          
          <!-- Calendar days -->
          <div class="calendar-body">
            @for (week of calendarWeeks; track week) {
              <div class="calendar-week">
                @for (day of week; track day.date) {
                  <div 
                    class="calendar-day"
                    [class.other-month]="!day.isCurrentMonth"
                    [class.today]="day.isToday"
                    [class.has-events]="day.events.length > 0">
                    <div class="day-number">{{ day.dayNumber }}</div>
                    @if (day.events.length > 0) {
                      <div class="day-events">
                        @for (event of day.events.slice(0, 2); track event.id) {
                          <div 
                            class="event-indicator"
                            [style.background-color]="event.color || '#1976d2'"
                            [matTooltip]="event.title">
                          </div>
                        }
                        @if (day.events.length > 2) {
                          <div class="more-events">+{{ day.events.length - 2 }}</div>
                        }
                      </div>
                    }
                  </div>
                }
              </div>
            }
          </div>
        </div>

        <!-- Event legend -->
        <div class="event-legend">
          <div class="legend-title">This Month</div>
          <div class="legend-stats">
            <div class="stat-item">
              <span class="stat-value">{{ monthEventCount }}</span>
              <span class="stat-label">Events</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">{{ todayEventCount }}</span>
              <span class="stat-label">Today</span>
            </div>
          </div>
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

    .calendar-widget {
      height: 100%;
      display: flex;
      flex-direction: column;
      box-shadow: none !important;
    }

    mat-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 20px;
      background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
      border-bottom: none;
    }

    mat-card-title {
      font-size: 16px;
      font-weight: 600;
      margin: 0;
      color: white;
    }

    .calendar-controls {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .calendar-controls button {
      color: white;
    }

    .current-month {
      font-size: 14px;
      font-weight: 600;
      color: white;
      min-width: 120px;
      text-align: center;
    }

    mat-card-content {
      flex: 1;
      padding: 12px !important;
      overflow-y: auto;
    }

    .calendar-grid {
      display: flex;
      flex-direction: column;
    }

    .calendar-header {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: 2px;
      margin-bottom: 4px;
    }

    .day-header {
      text-align: center;
      font-size: 11px;
      font-weight: 600;
      color: #666;
      padding: 8px 4px;
      text-transform: uppercase;
    }

    .calendar-body {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .calendar-week {
      display: grid;
      grid-template-columns: repeat(7, 1fr);
      gap: 2px;
    }

    .calendar-day {
      aspect-ratio: 1;
      background: white;
      border: 1px solid #e0e0e0;
      border-radius: 4px;
      padding: 4px;
      display: flex;
      flex-direction: column;
      position: relative;
      cursor: pointer;
      transition: all 0.2s;
    }

    .calendar-day:hover {
      background: #f5f5f5;
      transform: scale(1.05);
      z-index: 1;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .calendar-day.other-month {
      background: #fafafa;
      opacity: 0.5;
    }

    .calendar-day.today {
      background: #e3f2fd;
      border-color: #1976d2;
      border-width: 2px;
    }

    .calendar-day.today .day-number {
      color: #1976d2;
      font-weight: 700;
    }

    .calendar-day.has-events {
      background: #f9f9f9;
    }

    .day-number {
      font-size: 12px;
      font-weight: 500;
      color: #333;
      text-align: center;
      margin-bottom: 2px;
    }

    .day-events {
      display: flex;
      flex-direction: column;
      gap: 2px;
      flex: 1;
    }

    .event-indicator {
      height: 4px;
      border-radius: 2px;
      width: 100%;
    }

    .more-events {
      font-size: 9px;
      color: #666;
      text-align: center;
      margin-top: 2px;
    }

    .event-legend {
      margin-top: 12px;
      padding: 12px;
      background: #f5f5f5;
      border-radius: 6px;
    }

    .legend-title {
      font-size: 12px;
      font-weight: 600;
      color: #666;
      margin-bottom: 8px;
      text-transform: uppercase;
    }

    .legend-stats {
      display: flex;
      gap: 16px;
    }

    .stat-item {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .stat-value {
      font-size: 24px;
      font-weight: 700;
      color: #1976d2;
    }

    .stat-label {
      font-size: 11px;
      color: #999;
      text-transform: uppercase;
    }
  `]
})
export class CalendarWidgetComponent implements OnInit {
  @Input() data!: CalendarWidgetData;
  @Input() settings: any = {};
  
  currentDate: Date = new Date();
  weekDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  calendarWeeks: any[][] = [];
  monthEventCount = 0;
  todayEventCount = 0;

  ngOnInit(): void {
    this.currentDate = this.data?.currentDate || new Date();
    this.generateCalendar();
    this.calculateStats();
  }

  get currentMonthYear(): string {
    return this.currentDate.toLocaleDateString('en-US', { 
      month: 'long', 
      year: 'numeric' 
    });
  }

  previousMonth(): void {
    this.currentDate = new Date(
      this.currentDate.getFullYear(),
      this.currentDate.getMonth() - 1,
      1
    );
    this.generateCalendar();
    this.calculateStats();
  }

  nextMonth(): void {
    this.currentDate = new Date(
      this.currentDate.getFullYear(),
      this.currentDate.getMonth() + 1,
      1
    );
    this.generateCalendar();
    this.calculateStats();
  }

  goToToday(): void {
    this.currentDate = new Date();
    this.generateCalendar();
    this.calculateStats();
  }

  private generateCalendar(): void {
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth();
    
    // First day of the month
    const firstDay = new Date(year, month, 1);
    const startingDayOfWeek = firstDay.getDay();
    
    // Last day of the month
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    
    // Previous month
    const prevMonthLastDay = new Date(year, month, 0).getDate();
    
    const weeks: any[][] = [];
    let currentWeek: any[] = [];
    
    // Fill in days from previous month
    for (let i = startingDayOfWeek - 1; i >= 0; i--) {
      currentWeek.push(this.createDayObject(
        prevMonthLastDay - i,
        year,
        month - 1,
        false
      ));
    }
    
    // Fill in days of current month
    for (let day = 1; day <= daysInMonth; day++) {
      if (currentWeek.length === 7) {
        weeks.push(currentWeek);
        currentWeek = [];
      }
      currentWeek.push(this.createDayObject(day, year, month, true));
    }
    
    // Fill in days from next month
    let nextMonthDay = 1;
    while (currentWeek.length < 7) {
      currentWeek.push(this.createDayObject(
        nextMonthDay++,
        year,
        month + 1,
        false
      ));
    }
    weeks.push(currentWeek);
    
    this.calendarWeeks = weeks;
  }

  private createDayObject(day: number, year: number, month: number, isCurrentMonth: boolean): any {
    const date = new Date(year, month, day);
    const today = new Date();
    const isToday = date.toDateString() === today.toDateString();
    
    // Get events for this day
    const dayEvents = this.data?.events?.filter(event => {
      const eventDate = new Date(event.start);
      return eventDate.toDateString() === date.toDateString();
    }) || [];
    
    return {
      dayNumber: day,
      date,
      isCurrentMonth,
      isToday,
      events: dayEvents
    };
  }

  private calculateStats(): void {
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth();
    const today = new Date();
    
    // Count events in current month
    this.monthEventCount = this.data?.events?.filter(event => {
      const eventDate = new Date(event.start);
      return eventDate.getFullYear() === year && eventDate.getMonth() === month;
    }).length || 0;
    
    // Count events today
    this.todayEventCount = this.data?.events?.filter(event => {
      const eventDate = new Date(event.start);
      return eventDate.toDateString() === today.toDateString();
    }).length || 0;
  }
}
