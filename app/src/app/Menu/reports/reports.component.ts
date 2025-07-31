import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { Observable, catchError, map, of } from 'rxjs'; // For RxJS operations
import { AppConfig } from '../../Appconfig'; // For your API path
import { ErrorHandlingCommonServiceService } from '../../Common/Services/error-handling-common-service.service'; // Your error handling service

export interface Booking {
  _id: string; // MongoDB's default ID
  bookingId: string;
  customerName: string;
  serviceType: string;
  bookingDate: string; // Or Date, if you'll convert it
  status: string;
  amount: number;
  currency: string;
  notes?: string; // Optional field
}

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.css'
})

export class ReportsComponent implements OnInit, AfterViewInit {

  // Columns to display in the table, order matters
  displayedColumns: string[] = ['bookingId', 'customerName', 'serviceType', 'bookingDate', 'amount', 'status', 'actions'];

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
    private errorHandling: ErrorHandlingCommonServiceService
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

    // Apply filter predicate for searching across all columns
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
    this.http.get<any>(this.pathAPI + 'v1/Bookings') // Adjust your API endpoint
      .pipe(
        map(response => {
          // Assuming your API returns an object like { results: Booking[] }
          return response.results as Booking[];
        }),
        catchError(error => {
          this.isLoading = false; // Stop loading on error
          console.error('Error fetching bookings:', error);
          this.errorHandling.handleError(error); // Use your common error handling
          return of([]); // Return an empty array to prevent breaking the observable chain
        })
      )
      .subscribe({
        next: (data: Booking[]) => {
          this.dataSource.data = data; // Assign the fetched data to the dataSource
          this.isLoading = false; // Stop loading indicator
        },
        error: (err) => {
          // Error already handled by catchError, but this can catch final errors if needed
          console.error('Subscription error:', err);
        }
      });
  }

  // Method to apply filter from the search input
  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    // Reset pagination if filter changes
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  // Example action methods (implement these based on your needs)
  viewBooking(booking: Booking): void {
    console.log('View booking:', booking);
    // Navigate to a detail page, open a dialog, etc.
  }

  editBooking(booking: Booking): void {
    console.log('Edit booking:', booking);
    // Navigate to an edit page, open an edit form, etc.
  }

  deleteBooking(booking: Booking): void {
    if (confirm(`Are you sure you want to delete booking ID: ${booking.bookingId}?`)) {
      console.log('Delete booking:', booking);
      // Implement actual delete API call here
      // After successful deletion, refresh data or remove from dataSource
    }
  }
}
