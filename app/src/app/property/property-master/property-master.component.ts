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

  searchText: string = '';
  statusFilter: string = 'all';
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
          console.log('Property object:', p);
          console.log('p.active:', p.active);
          console.log('p.Active:', p.Active);
          console.log('p.isActive:', p.isActive);

          return {
            id: p.id || p.Id,
            name: p.name || p.Name,
            isActive: p.active ?? p.Active ?? false,
            createdAt: p.createdAt || p.CreatedAt,
            createdBy: p.createdBy || p.CreatedBy,
            updatedAt: p.updatedAt || p.UpdatedAt,
            updatedBy: p.updatedBy || p.UpdatedBy,
            companyLogoURL: p.companyLogoURL || p.CompanyLogoURL || p.companyLogo || p.CompanyLogo,
            rooms: p.rooms || p.Rooms || []
          };
        });

        console.log('Mapped properties:', mapped);
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
      this.dataSource.data = properties;
      this.totalCount = properties.length;
      this.applyFilters();
    });
  }

  applyFilters(): void {
    let filtered = [...this.dataSource.data];

    // Status Filter
    if (this.statusFilter !== 'all') {
      const isActive = this.statusFilter === 'active';
      filtered = filtered.filter(p => p.isActive === isActive);
    }

    // Search
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
    this.statusFilter = 'all';
    this.searchText = '';
    this.applyFilters();
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

  getActiveCount(): number {
    return this.dataSource.data.filter(p => p.isActive).length;
  }

  getInactiveCount(): number {
    return this.dataSource.data.filter(p => !p.isActive).length;
  }
}
