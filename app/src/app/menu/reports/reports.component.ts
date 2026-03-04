  import { Component, OnInit, ViewChild, AfterViewInit,Inject, inject } from '@angular/core';
  import { HttpClient, HttpParams } from '@angular/common/http';
  import { MatTableDataSource } from '@angular/material/table';
  import { MatPaginator, PageEvent } from '@angular/material/paginator';
  import { MatSort, Sort, MatSortModule } from '@angular/material/sort';
  import { MatInputModule } from '@angular/material/input';
  import {catchError, map,of } from 'rxjs';
  import { MatExpansionModule } from '@angular/material/expansion';
  import { MatProgressBarModule } from '@angular/material/progress-bar'; // For loading indicator
  import { MatFormFieldModule } from '@angular/material/form-field'; // For search input
  import { MatTableModule } from '@angular/material/table';
  import { MatCheckboxModule } from '@angular/material/checkbox';
  import { MatIconModule } from '@angular/material/icon'; // For search icon
  import { MatPaginatorModule } from '@angular/material/paginator';
  import { DatePipe, CurrencyPipe, CommonModule } from '@angular/common'; 

  import { APP_CONFIG, AppConfig } from '../../configuration/app.config.token';
  import { ErrorHandlingService } from '../../core/system/service/error-handling-service.service';
  import { LoggingService } from '../../core/system/service/logging.service';
  import { LOG_DELETE_BOOKING, LOG_EDIT_GRID, LOG_EDIT_VIEW } from '../../common/Constants/Constants';

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
    standalone: true,
    imports: [MatExpansionModule, MatProgressBarModule, MatFormFieldModule, MatTableModule, MatCheckboxModule, MatIconModule
      , MatPaginatorModule, DatePipe, CurrencyPipe, CommonModule, MatInputModule, MatSortModule],
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
    private appConfig = inject<AppConfig>(APP_CONFIG);

    constructor(
      private http: HttpClient,
      private errorHandling: ErrorHandlingService,private loggingService: LoggingService) {

      this.pathAPI = this.appConfig.apiUrl;
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
      this.loggingService.logPageNavigation(`viewBooking`, LOG_EDIT_VIEW, `User viewed booking with ID: ${booking.bookingId}`);
    }

    editBooking(booking: Booking): void {
      this.loggingService.logPageNavigation(`editBooking`, LOG_EDIT_GRID, `User edited booking with ID: ${booking.bookingId}`);
    }

    deleteBooking(booking: Booking): void {
      if (confirm(`Are you sure you want to delete booking ID: ${booking.bookingId}?`)) {
        this.loggingService.logPageNavigation(`deleteBooking`, LOG_DELETE_BOOKING, `User deleted booking with ID: ${booking.bookingId}`);
      }
    }
  }