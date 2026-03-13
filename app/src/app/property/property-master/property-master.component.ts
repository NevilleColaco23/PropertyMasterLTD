import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { catchError, map, of, finalize } from 'rxjs';
import { APP_CONFIG, AppConfig } from '../../configuration/app.config.token';
import { ErrorHandlingService } from '../../core/system/service/error-handling-service.service';
import { PropertyDialogComponent, PropertyDialogData } from './property-dialog/property-dialog.component';

interface Property {
  id: number;
  name: string;
  isActive: boolean;
  createdAt?: Date;
  createdBy?: number;
  updatedAt?: Date;
  updatedBy?: number;
  companyLogoURL?: string;
  rooms?: RoomData[];
  isDeleted?: boolean;
  deletedAt?: Date;
  deletedBy?: number;
}

interface RoomData {
  id: string | number;
  roomName: string;
  roomCode: string;
  active: boolean;
  companyLogoURL?: string;
}

@Component({
  selector: 'app-property-master',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatTooltipModule,
    DatePipe
  ],
  templateUrl: './property-master.component.html',
  styleUrl: './property-master.component.css',
})
export class PropertyMasterComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private appConfig = inject<AppConfig>(APP_CONFIG);
  private pathAPI: string;

  displayedColumns: string[] = ['id', 'name', 'isActive', 'createdAt', 'actions'];
  dataSource = new MatTableDataSource<Property>([]);
  allProperties: Property[] = []; // Store original unfiltered data

  searchText: string = '';
  statusFilter: string = 'active'; // Default to 'active' to show only active properties
  isLoading: boolean = false;
  totalCount: number = 0;
  pageSize: number = 25;

  constructor(
    private http: HttpClient,
    private errorHandling: ErrorHandlingService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {
    this.pathAPI = this.appConfig.apiUrl;
  }

  ngOnInit(): void {
    this.loadProperties();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadProperties(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    this.http.get<any>(`${this.pathAPI}/property`).pipe(
      map(response => {
        console.log('Raw API Response:', response);
        const properties = response.results || response || [];
        console.log('Properties from API:', properties);

        const mapped = properties.map((p: any) => {
          console.log('=== Property mapping ===');
          console.log('Full property object:', p);
          console.log('Active field - p.active:', p.active, 'p.Active:', p.Active);
          console.log('IsDeleted field - p.isDeleted:', p.isDeleted, 'p.IsDeleted:', p.IsDeleted);

          // API returns camelCase due to JsonNamingPolicy.CamelCase in ApiStartup.cs
          const mappedProp = {
            id: p.id ?? p.Id,
            name: p.name ?? p.Name,
            isActive: p.active ?? p.Active ?? false, // API returns 'active' (camelCase)
            createdAt: p.createdAt ?? p.CreatedAt,
            createdBy: p.createdBy ?? p.CreatedBy,
            updatedAt: p.updatedAt ?? p.UpdatedAt,
            updatedBy: p.updatedBy ?? p.UpdatedBy,
            companyLogoURL: p.companyLogo ?? p.companyLogoURL ?? p.CompanyLogoURL ?? p.CompanyLogo ?? '',
            rooms: p.rooms ?? p.Rooms ?? [],
            isDeleted: p.isDeleted ?? p.IsDeleted ?? false, // API returns 'isDeleted' (camelCase)
            deletedAt: p.deletedAt ?? p.DeletedAt ?? null,
            deletedBy: p.deletedBy ?? p.DeletedBy ?? null
          };

          console.log('Mapped to:', mappedProp);
          console.log('========================');
          return mappedProp;
        });

        console.log('All mapped properties:', mapped);
        return mapped;
      }),
      catchError(err => {
        console.error('Error loading properties:', err);
        this.errorHandling.handleError(err);
        return of([]);
      }),
      finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      })
    ).subscribe(properties => {
      this.allProperties = properties;
      this.applyFilters();
    });
  }

  applyFilters(): void {
    let filtered = [...this.allProperties];

    // Apply status filters
    if (this.statusFilter === 'deleted') {
      // Show only deleted properties
      filtered = filtered.filter(p => p.isDeleted === true);
    } else if (this.statusFilter === 'all') {
      // Show ALL properties (active, inactive, and deleted)
      // No filtering needed - keep all properties
    } else {
      // For active/inactive filters, exclude deleted properties first
      filtered = filtered.filter(p => !p.isDeleted);

      // Then apply active/inactive filter
      if (this.statusFilter === 'active') {
        filtered = filtered.filter(p => p.isActive === true);
      } else if (this.statusFilter === 'inactive') {
        filtered = filtered.filter(p => p.isActive === false);
      }
    }

    // Search filter
    if (this.searchText) {
      const search = this.searchText.toLowerCase();
      filtered = filtered.filter(p => 
        p.name.toLowerCase().includes(search) ||
        p.id.toString().includes(search)
      );
    }

    this.dataSource.data = filtered;
    this.totalCount = filtered.length;
  }

  filterProperties(): void {
    this.applyFilters();
  }

  clearSearch(): void {
    this.searchText = '';
    this.applyFilters();
  }

  resetFilters(): void {
    this.statusFilter = 'active'; // Reset to 'active' instead of 'all'
    this.searchText = '';
    this.applyFilters();
  }

  getRowClass(property: Property): string {
    return property.isDeleted ? 'deleted-row' : '';
  }

  getStatusClass(property: Property): string {
    return property.isActive ? 'status-active' : 'status-inactive';
  }

  getStatusIcon(property: Property): string {
    return property.isActive ? 'check_circle' : 'cancel';
  }

  getStatusText(property: Property): string {
    return property.isActive ? 'Active' : 'Inactive';
  }

  openAddDialog(): void {
    const dialogRef = this.dialog.open(PropertyDialogComponent, {
      width: '700px',
      maxWidth: '95vw',
      height: 'auto',
      maxHeight: '95vh',
      disableClose: false,
      data: {
        mode: 'add'
      } as PropertyDialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.createProperty(result);
      }
    });
  }

  editProperty(property: Property): void {
    const dialogRef = this.dialog.open(PropertyDialogComponent, {
      width: '700px',
      maxWidth: '95vw',
      height: 'auto',
      maxHeight: '95vh',
      disableClose: false,
      data: {
        mode: 'edit',
        existingProperty: {
          id: property.id,
          name: property.name,
          isActive: property.isActive,
          companyLogoURL: property.companyLogoURL,
          rooms: property.rooms || []
        }
      } as PropertyDialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.updateProperty(result);
      }
    });
  }

  createProperty(data: any): void {
    const payload = {
      name: data.name,
      isActive: data.isActive,
      companyLogoURL: data.companyLogoURL || '',
      rooms: data.rooms || []
    };

    this.http.post(`${this.pathAPI}/property`, payload).pipe(
      catchError(err => {
        this.errorHandling.handleError(err);
        return of(null);
      })
    ).subscribe(result => {
      if (result !== null) {
        this.errorHandling.handleSuccess('Property created successfully');
        this.loadProperties();
      }
    });
  }

  updateProperty(data: any): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    const payload = {
      id: data.propertyId,
      name: data.name,
      isActive: data.isActive,
      companyLogoURL: data.companyLogoURL || '',
      rooms: data.rooms || []
    };

    this.http.put(`${this.pathAPI}/property/${data.propertyId}`, payload).pipe(
      catchError(err => {
        this.errorHandling.handleError(err);
        this.isLoading = false;
        this.cdr.detectChanges();
        return of(null);
      })
    ).subscribe(response => {
      if (response === null) {
        this.isLoading = false;
        this.cdr.detectChanges();
        return;
      }

      this.errorHandling.handleSuccess('Property updated successfully');
      setTimeout(() => {
        this.loadProperties();
      }, 300);
    });
  }

  deleteProperty(property: Property): void {
    if (!confirm(`Are you sure you want to delete property "${property.name}"?`)) {
      return;
    }

    this.isLoading = true;
    this.cdr.detectChanges();

    this.http.delete(`${this.pathAPI}/property/${property.id}`).pipe(
      catchError(err => {
        this.errorHandling.handleError(err);
        this.isLoading = false;
        this.cdr.detectChanges();
        return of(null);
      })
    ).subscribe(result => {
      if (result === null) {
        this.isLoading = false;
        this.cdr.detectChanges();
        return;
      }

      this.errorHandling.handleSuccess('Property deleted successfully');
      setTimeout(() => {
        this.loadProperties();
      }, 300);
    });
  }

  reinitiateProperty(property: Property): void {
    if (!property.isDeleted) {
      return;
    }

    if (!confirm(`Are you sure you want to reinitiate property "${property.name}"?`)) {
      return;
    }

    this.isLoading = true;
    this.cdr.detectChanges();

    // Create a payload to mark the property as not deleted
    const payload = {
      id: property.id,
      name: property.name,
      isActive: property.isActive,
      companyLogoURL: property.companyLogoURL || '',
      rooms: property.rooms || [],
      isDeleted: false,
      deletedAt: null,
      deletedBy: null
    };

    this.http.put(`${this.pathAPI}/property/${property.id}`, payload).pipe(
      catchError(err => {
        this.errorHandling.handleError(err);
        this.isLoading = false;
        this.cdr.detectChanges();
        return of(null);
      })
    ).subscribe(response => {
      if (response === null) {
        this.isLoading = false;
        this.cdr.detectChanges();
        return;
      }

      this.errorHandling.handleSuccess('Property reinitiated successfully');
      setTimeout(() => {
        this.loadProperties();
      }, 300);
    });
  }

  getActiveCount(): number {
    return this.allProperties.filter(p => p.isActive && !p.isDeleted).length;
  }

  getInactiveCount(): number {
    return this.allProperties.filter(p => !p.isActive && !p.isDeleted).length;
  }

  getDeletedCount(): number {
    return this.allProperties.filter(p => p.isDeleted).length;
  }
}
