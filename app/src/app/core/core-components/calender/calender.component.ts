import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { BookingService } from '../../booking/booking-service.service';
import { CalendarEvent, CalendarView } from 'angular-calendar';
import {  subDays, addWeeks, subWeeks, addMonths, subMonths, startOfWeek, endOfWeek, format,
  startOfDay, addDays, endOfDay, eachDayOfInterval, differenceInDays,startOfMonth,endOfMonth
 } from 'date-fns';

 export interface Room {
  name: string;
  bookings: CalendarEvent[];
}

@Component({
  selector: 'app-calender',
  templateUrl: './calender.component.html',
  styleUrls: ['./calender.component.scss']
})
export class CalenderComponent implements OnInit {
    view: CalendarView = CalendarView.Month;
    viewDate: Date = new Date();
    events: CalendarEvent[] = [];
    CalendarView = CalendarView;
    selectedDay: any;
    title: string;
    activeDayIsOpen: boolean = false;
    toggleViewEnabled: boolean = true; // Toggle view flag for Gantt chart
    rooms: Room[] = [];
    dates: Date[] = [];
  
    constructor(private cdr: ChangeDetectorRef) {}
  
    ngOnInit(): void {
      this.initializeDates();
      this.events = this.getHardcodedEvents();
      this.rooms = this.getHardcodedRooms();
      this.updateTitle();
      this.cdr.detectChanges();
    }
  
    handleDayClick(day: any): void {
      this.selectedDay = day;
      this.activeDayIsOpen = !this.activeDayIsOpen;
    }
  
    setView(view: CalendarView) {
      this.view = view;
      this.updateTitle();
    }
  
    previous(): void {
      if (this.view === CalendarView.Month) {
        this.viewDate = subMonths(this.viewDate, 1);
      } else if (this.view === CalendarView.Week) {
        this.viewDate = subWeeks(this.viewDate, 1);
      } else if (this.view === CalendarView.Day) {
        this.viewDate = subDays(this.viewDate, 1);
      }
      this.updateTitle();
      this.initializeDates(); // update dates for Gantt chart
      this.cdr.detectChanges();
    }
  
    today(): void {
      this.viewDate = new Date();
      this.updateTitle();
      this.initializeDates(); // update dates for Gantt chart
      this.cdr.detectChanges();
    }
  
    next(): void {
      if (this.view === CalendarView.Month) {
        this.viewDate = addMonths(this.viewDate, 1);
      } else if (this.view === CalendarView.Week) {
        this.viewDate = addWeeks(this.viewDate, 1);
      } else if (this.view === CalendarView.Day) {
        this.viewDate = addDays(this.viewDate, 1);
      }
      this.updateTitle();
      this.initializeDates(); // update dates for Gantt chart
      this.cdr.detectChanges();
    }
  
    toggleView(): void {
      this.toggleViewEnabled = !this.toggleViewEnabled;
      this.cdr.detectChanges();
    }
  
    initializeDates() {
      const start = startOfMonth(this.viewDate);
      const end = endOfMonth(this.viewDate); // Adjust for current month
      this.dates = eachDayOfInterval({ start, end });
    }
  
    calculateLeft(startDate: Date): string {
      const start = new Date(startDate).getTime();
      const firstDate = new Date(this.dates[0]).getTime();
      const dayWidth = 100 / this.dates.length; // Assuming equal width for each day
      const daysFromStart = (start - firstDate) / (1000 * 60 * 60 * 24);
      return `${daysFromStart * dayWidth}%`;
    }
    
    calculateWidth(startDate: Date, endDate: Date): string {
      const start = new Date(startDate).getTime();
      const end = new Date(endDate).getTime();
      const dayWidth = 100 / this.dates.length; // Assuming equal width for each day
      const duration = (end - start) / (1000 * 60 * 60 * 24);
      return `${duration * dayWidth}%`;
    }
  
    private updateTitle() {
      if (this.view === CalendarView.Month) {
        this.title = format(this.viewDate, 'MMMM yyyy');
      } else if (this.view === CalendarView.Week) {
        const start = startOfWeek(this.viewDate);
        const end = endOfWeek(this.viewDate);
        this.title = `${format(start, 'MMM dd')} - ${format(end, 'MMM dd, yyyy')}`;
      } else if (this.view === CalendarView.Day) {
        this.title = format(this.viewDate, 'MMMM dd, yyyy');
      }
    }
  
    private getHardcodedEvents(): CalendarEvent[] {
      return [
        {
          start: new Date('2024-11-15'),
          end: new Date('2024-11-18'),
          title: 'Meeting',
          color: { primary: '#ad2121', secondary: '#FAE3E3' }
        },
        {
          start: new Date('2024-11-20'),
          end: new Date('2024-11-22'),
          title: 'Conference',
          color: { primary: '#1e90ff', secondary: '#D1E8FF' }
        }
      ];
    }
  
    private getHardcodedRooms(): Room[] {
      return [
        {
          name: 'Room A',
          bookings: [
            {
              start: new Date('2024-11-15'),
              end: new Date('2024-11-16'),
              title: 'Book11',
              color: { primary: '#ad2121', secondary: '#FAE3E3' }
            },
            {
              start: new Date('2024-11-17'),
              end: new Date('2024-11-20'),
              title: 'Book2',
              color: { primary: '#008000', secondary: '#FAE3E3' }
            },
            {
              start: new Date('2024-11-20'),
              end: new Date('2024-11-22'),
              title: 'Book3',
              color: { primary: '#1e90ff', secondary: '#D1E8FF' }
            }
          ]
        },
        {
          name: 'Room B',
          bookings: [
            {
              start: new Date('2024-11-12'),
              end: new Date('2024-11-21'),
              title: 'Booking5',
              color: { primary: '#e3bc08', secondary: '#FDF1BA' }
            },
            {
              start: new Date('2024-11-23'),
              end: new Date('2024-11-25'),
              title: 'Booking6',
              color: { primary: '#1e90ff', secondary: '#D1E8FF' }
            }
          ]
        },
        {
          name: 'Room C',
          bookings: [
            {
              start: new Date('2024-11-25'),
              end: new Date('2024-11-27'),
              title: 'Booking8',
              color: { primary: '#e3bc08', secondary: '#FDF1BA' }
            },
            {
              start: new Date('2024-11-28'),
              end: new Date('2024-11-30'),
              title: 'Booking9',
              color: { primary: '#1e90ff', secondary: '#D1E8FF' }
            }
          ]
        }
      ];
    }

    private calculateOverlapColor(bookings: CalendarEvent[], currentBooking: CalendarEvent)
    : { color: string, overlapCount: number, overlapDates: Date[] } 
    {
      const overlapColor = '#FF00FF'; // Magenta color for overlapping bookings
      const defaultColor = '#000000'; // Default color (black)
      let overlapCount = 0; 
      let overlapDates: Date[] = [];
    
      console.log('currentBooking');
      if (Array.isArray(bookings)) {
      for (let booking of bookings) {
        if (booking !== currentBooking && this.isOverlapping(booking, currentBooking)) {
          console.log('booking');
          console.log(booking);
          console.log('currentBooking');
          console.log(currentBooking);
          const overlappingDates = this.getOverlappingDates(booking, currentBooking);
          overlapCount += overlappingDates.length;
          overlapDates = overlapDates.concat(overlappingDates);
        }
      }
    }
      console.log(`Overlap count for ${currentBooking.title}: ${overlapCount}`);
      console.log(`Overlap dates for ${currentBooking.title}:`, overlapDates);
    
      return {
        color: overlapCount > 0 ? overlapColor : currentBooking?.color?.primary || defaultColor,
        overlapCount: overlapCount,
        overlapDates: overlapDates
      };
    }

    private isOverlapping(booking1: CalendarEvent, booking2: CalendarEvent): boolean {
      const isOverlap = booking1.start < booking2.end && booking1.end >= booking2.start;
      console.log(`Checking overlap between ${booking1.title} and ${booking2.title}: ${isOverlap}`);
      return isOverlap;
    }

    private getOverlappingDates(booking1: CalendarEvent, booking2: CalendarEvent): Date[] {
      const start = booking1.start > booking2.start ? booking1.start : booking2.start;
      const end = booking1.end < booking2.end ? booking1.end : booking2.end;
    
      const overlappingDates: Date[] = [];
      let currentDate = new Date(start);
    
      while (currentDate <= end) {
        overlappingDates.push(new Date(currentDate));
        currentDate.setDate(currentDate.getDate() + 1);
      }
    
      console.log(`Overlapping dates between ${booking1.title} and ${booking2.title}:`, overlappingDates);
      return overlappingDates;
    }
  }
  