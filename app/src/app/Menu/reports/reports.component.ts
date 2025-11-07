import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
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
  displayedColumns: string[] = ['bookingId', 'guestId', 'roomNumber', 'bookingDate', 'totalPrice', 'isConfirmed', 'actions'];
  dataSource = new MatTableDataSource<Booking>();
  panelOpenState = true;
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  isLoading = false;
  private pathAPI: string;
  orderBy = 'lastModified'; // Default sorting column
  sortOrder = 'desc'; // Default sorting order
  filterString = '';

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
    
  this.sort.sortChange.subscribe((sort: Sort) => {
      this.onSortChange(sort);
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

onSortChange(sort: Sort): void {
    // Corrected: Map the sort direction to a value the API understands (e.g., 'asc' or 'desc')
    this.orderBy = sort.active;
    this.sortOrder = sort.direction === 'asc' ? 'asc' : 'desc';
    
    // Reset page index to 0 when a new sort is applied
    this.paginator.pageIndex = 0;
    this.pageIndex = 0;
    
    this.getBookings();
  }

  onFilterChange(event: Event): void {
    this.filterString = (event.target as HTMLInputElement).value.trim().toLowerCase();
    console.log('Filter string:', this.filterString);
    // Reset page index to 0 to start a new search from the first page
    this.paginator.pageIndex = 0;
    this.pageIndex = 0;
    
    this.getBookings();
  }

  getBookings(): void {
    this.isLoading = true;

    let params = new HttpParams();
    params = params.set('PageIndex', (this.pageIndex + 1).toString());
    params = params.set('PageSize', this.pageSize.toString());
    params = params.set('BookingId', '66');
    
    // Added dynamic sort parameters to the request
    params = params.set('OrderBy', this.orderBy);
    params = params.set('ActiveOrderBy', this.sortOrder);
    
    params = params.set('ActiveFilter', '10');
    params = params.set('SearchItem', this.filterString);
    params = params.set('ActiveSortDirection', this.sortOrder == 'asc' ? 1 : -1);
    
    
    this.http.get<any>(this.pathAPI + 'v1/Bookings/GetBookings', { params: params })
      .pipe(
        map(response => {
          console.log('API Response:', response);

          if (!response || !response.results) {
            this.errorHandling.handleError('Invalid API response format.');
            this.totalCount = 0;
            return [];
          }
          
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