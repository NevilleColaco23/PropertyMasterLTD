import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import {catchError, map, of } from 'rxjs';
import { AppConfig } from '../../Appconfig';
import { ErrorHandlingService } from '../../core/system-messages-snackbar/service/error-handling-service.service';

export interface Booking {
  _id: string;
  bookingId: string;
  guestId: number;
  roomNumber: string;
  BookingDate: Date;
  totalPrice: number;
  isConfirmed: boolean;
}

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.css'
})

export class ReportsComponent implements OnInit, AfterViewInit {

  // Columns to display in the table, order matters
  displayedColumns: string[] = ['bookingId', 'guestId', 'serviceType', 'bookingDate', 'amount', 'isConfirmed', 'actions'];

  // DataSource for the Material Table
  dataSource = new MatTableDataSource<Booking>();

  // ViewChild decorators to get references to MatPaginator and MatSort
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  isLoading = false; // To show a loading indicator

  private pathAPI: string;

  constructor(
    private http: HttpClient,
    private appConfig: AppConfig,
    private errorHandling: ErrorHandlingService
  ) {
    this.pathAPI = this.appConfig.setting['PathAPI'];
  }

  ngOnInit(): void {
    this.getBookings();
  }

  ngAfterViewInit(): void {
    // Connect the MatTableDataSource to the paginator and sort after the view is initialized
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

    // Apply filter predicate fo r searching across all columns
    this.dataSource.filterPredicate = (data: Booking, filter: string): boolean => {
      // Convert all string values of the booking object to lowercase for case-insensitive matching
      const dataStr = Object.keys(data).reduce((currentTerm: string, key: string) => {
        // Exclude _id and notes if they are not relevant to search, or include them
        if (key !== '_id' && key !== 'notes') {
            return currentTerm + (data as any)[key] + ' ';
        }
        return currentTerm;
      }, '').toLowerCase();

      const transformedFilter = filter.trim().toLowerCase();
      return dataStr.indexOf(transformedFilter) !== -1;
    };
  }

  getBookings(): void {
    this.isLoading = true; // Start loading indicator

  // 1. Create a new HttpParams instance
  let params = new HttpParams();

  // 2. Append each query parameter using the .set() method
  params = params.set('BookingId', '66');
  params = params.set('PageIndex', '9');
  params = params.set('PageSize', '7');
  params = params.set('OrderBy', '4');
  params = params.set('Filter', '10');
    this.http.get<any>(this.pathAPI + 'v1/Bookings/GetBookings', { params: params })
      .pipe(
        map(response => {
          return response.results as Booking[];
        }),
        catchError(error => {
          this.isLoading = false; 
          this.errorHandling.handleError(error);
          return this.errorHandling.handleError(error);
        })
      )
      .subscribe({
        next: (data: Booking[]) => {
          this.dataSource.data = data;
          this.isLoading = false;
          this.errorHandling.handleSuccess(data.length + ' Bookings loaded successfully!');
        },
        error: (err) => {
          console.error('Subscription error:', err);
        }
      });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  viewBooking(booking: Booking): void {
    console.log('View booking:', booking);
  }

  editBooking(booking: Booking): void {
    console.log('Edit booking:', booking);
  }

  deleteBooking(booking: Booking): void {
    if (confirm(`Are you sure you want to delete booking ID: ${booking.bookingId}?`)) {
      console.log('Delete booking:', booking);
    }
  }
}
