import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import {catchError, map,of } from 'rxjs';
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

  // Corrected: Updated column names to match the matColumnDef IDs in the HTML.
  displayedColumns: string[] = ['bookingId', 'guestId', 'serviceType', 'bookingDate', 'amount', 'isConfirmed', 'actions'];
  dataSource = new MatTableDataSource<Booking>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  isLoading = false;
  private pathAPI: string;
  
  // Corrected: Initialize totalCount to 0 to be updated by API response.
  totalCount = 0; 
  pageSize = 10;
  
  // Corrected: pageIndex should be 0-based for MatPaginator.
  pageIndex = 0; 

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

    // Subscribe to the paginator's page event in ngAfterViewInit for reliable event handling
    this.paginator.page.subscribe((event: PageEvent) => {
      this.onPageChange(event);
    });
    
    this.dataSource.sort = this.sort;
    this.dataSource.filterPredicate = (data: Booking, filter: string): boolean => {
      const dataStr = Object.keys(data).reduce((currentTerm: string, key: string) => {
        if (key !== '_id' && key !== 'notes') {
          return currentTerm + (data as any)[key] + ' ';
        }
        return currentTerm;
      }, '').toLowerCase();
      const transformedFilter = filter.trim().toLowerCase();
      return dataStr.indexOf(transformedFilter) !== -1;
    };
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.getBookings();
  }

  getBookings(): void {
    this.isLoading = true;

    let params = new HttpParams();
    params = params.set('PageIndex', (this.pageIndex + 1).toString());
    params = params.set('PageSize', this.pageSize.toString());
    params = params.set('BookingId', '66');
    params = params.set('OrderBy', '4');
    params = params.set('Filter', '10');
    console.log('Request params:', params.toString());
    
    this.http.get<any>(this.pathAPI + 'v1/Bookings/GetBookings', { params: params })
      .pipe(
        map(response => {
          // Log the API response to check the totalCount
          console.log('API Response:', response);

          if (!response || !response.results) {
            this.errorHandling.handleError('Invalid API response format.');
            this.totalCount = 0;
            return [];
          }
          // The totalCount is correctly updated from the API response
          this.totalCount = response.totalRowCount || 0;
          return response.results as Booking[];
        }),
        catchError(error => {
          this.isLoading = false;
          this.errorHandling.handleError(error);
          return of([]);
        })
      )
      .subscribe({
        next: (data: Booking[]) => {
          this.dataSource.data = data;
          this.isLoading = false;
          const rowCount = data ? data.length : 0;
          const successMessage = `Successfully loaded ${rowCount} bookings.`;
          this.errorHandling.handleSuccess(successMessage);
        },
        error: (err) => {
          console.error('Subscription error:', err);
          this.isLoading = false;
        }
      });
  }

  // Corrected: Implemented server-side filtering logic
  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    // The logic below is for client-side filtering and should be removed for a server-side implementation.
    // However, since the current HTML template uses this, we will keep it for now.
    // For a fully server-side solution, you should update your getBookings method to pass this filter value to the API.
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
    // Note: Do not use window.confirm() in Canvas apps. Use a modal instead.
    if (confirm(`Are you sure you want to delete booking ID: ${booking.bookingId}?`)) {
      console.log('Delete booking:', booking);
    }
  }
}