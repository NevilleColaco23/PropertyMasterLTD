import { Component, OnInit, ViewChild, inject, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { catchError, map, of, finalize } from 'rxjs';
import { APP_CONFIG, AppConfig } from '../../../../configuration/app.config.token';
import { ErrorHandlingService } from '../../../system/service/error-handling-service.service';
import { AuthService } from '../../../auth/services/auth.service';
import { PermissionDialogComponent, PermissionDialogData } from '../permission-dialog/permission-dialog.component';

interface User {
  id: number;
  username: string;
  email: string;
}

interface Menu {
  id: number;
  label: string;
  path: string;
  order: number;
  hasDropdown: boolean;
  subItems?: SubMenuItem[];
  isVisible: boolean;
}

interface SubMenuItem {
  subLabel: string;
  subPath: string;
}

interface MenuPermission {
  id: number;
  userId: number;
  menuId: number;
  accessLevel: 'read' | 'write';
  isActive: boolean;
  from: Date;
  to: Date;
}

interface MenuPermissionRow extends Menu {
  permissionId?: number;
  accessLevel?: 'read' | 'write';
  isActive?: boolean;
  from?: Date;
  to?: Date;
}

@Component({
  selector: 'app-menu-access-map',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatSelectModule,
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
  templateUrl: './menu-access-map.html',
  styleUrl: './menu-access-map.css',
})
export class MenuAccessMap implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private appConfig = inject<AppConfig>(APP_CONFIG);
  private pathAPI: string;

  displayedColumns: string[] = ['menuName', 'menuId', 'accessLevel', 'status', 'dateRange', 'actions'];
  dataSource = new MatTableDataSource<MenuPermissionRow>([]);

  users: User[] = [];
  allMenus: Menu[] = [];
  userPermissions: MenuPermission[] = [];
  allRows: MenuPermissionRow[] = [];

  selectedUserId: number | null = null;
  userSearchText: string = '';
  menuSearchText: string = '';
  accessLevelFilter: string = 'all';
  statusFilter: string = 'all';

  isLoading: boolean = false;
  totalCount: number = 0;
  pageSize: number = 25;

  constructor(
    private http: HttpClient,
    private errorHandling: ErrorHandlingService,
    private authService: AuthService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {
    this.pathAPI = this.appConfig.apiUrl;
  }

  ngOnInit(): void {
    this.loadUsers();
    this.loadMenus();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadUsers(): void {
    this.http.get<any>(`${this.pathAPI}/users/getall`).pipe(
      map(response => response.results || response || []),
      catchError(err => {
        console.error('Error loading users:', err);
        this.errorHandling.handleError(err);
        // Fallback to mock data if API fails
        return of([
          { id: 19, username: 'john.doe', email: 'john.doe@example.com' },
          { id: 20, username: 'jane.smith', email: 'jane.smith@example.com' },
          { id: 21, username: 'admin', email: 'admin@example.com' }
        ]);
      })
    ).subscribe(users => {
      console.log('Loaded users from API:', users);
      this.users = users;
    });
  }

  loadMenus(): void {
    this.http.get<any>(`${this.pathAPI}/menu/getall`).pipe(
      map(response => response.results || response || []),
      catchError(err => {
        this.errorHandling.handleError(err);
        return of([]);
      })
    ).subscribe(menus => {
      console.log('Loaded menus from API:', menus);
      this.allMenus = menus;
      console.log('this.allMenus set to:', this.allMenus);
      if (this.selectedUserId) {
        this.loadUserPermissions();
      }
    });
  }

  loadUserPermissions(): void {
    if (!this.selectedUserId) return;

    console.log('Loading permissions for user:', this.selectedUserId);
    this.isLoading = true;
    this.cdr.detectChanges(); // ✅ Force UI update to show spinner immediately

    const params = new HttpParams().set('userId', this.selectedUserId);

    this.http.get<any>(`${this.pathAPI}/menupermissions`, { params }).pipe(
      map(response => {
        console.log('Raw permissions response:', response);
        const permissions = response.results || response || [];

        // Map API response (PascalCase) to interface (camelCase)
        return permissions.map((p: any) => ({
          id: p.id || p.Id || p._id,
          userId: p.userId || p.UserId,
          menuId: p.menuId || p.MenuId || p.MenuID,
          accessLevel: p.accessLevel || p.AccessLevel,
          isActive: p.isActive !== undefined ? p.isActive : p.IsActive,
          from: p.from || p.From,
          to: p.to || p.To
        }));
      }),
      catchError(err => {
        console.error('Error loading permissions:', err);
        this.errorHandling.handleError(err);
        return of([]);
      }),
      finalize(() => {
        // Always set loading to false and trigger change detection
        this.isLoading = false;
        this.cdr.detectChanges(); // ✅ Force UI update to hide spinner
        console.log('Loading complete, isLoading:', this.isLoading);
      })
    ).subscribe(permissions => {
      console.log('Processed permissions:', permissions);
      this.userPermissions = permissions;
      this.mergeMenusWithPermissions();
    });
  }

  mergeMenusWithPermissions(): void {
    this.allRows = this.allMenus.map(menu => {
      const permission = this.userPermissions.find(p => p.menuId === menu.id);

      return {
        ...menu,
        permissionId: permission?.id,
        accessLevel: permission?.accessLevel as 'read' | 'write' | undefined,
        isActive: permission?.isActive,
        from: permission?.from ? new Date(permission.from) : undefined,
        to: permission?.to ? new Date(permission.to) : undefined
      };
    });

    this.applyFilters();
  }

  onUserChange(): void {
    if (this.selectedUserId) {
      this.loadUserPermissions();
    } else {
      this.dataSource.data = [];
      this.totalCount = 0;
    }
  }

  applyFilters(): void {
    let filtered = [...this.allRows];

    // Access Level Filter
    if (this.accessLevelFilter !== 'all') {
      filtered = filtered.filter(row => row.accessLevel === this.accessLevelFilter);
    }

    // Status Filter
    if (this.statusFilter !== 'all') {
      filtered = filtered.filter(row => {
        const status = this.getRowStatus(row);
        return status === this.statusFilter;
      });
    }

    // Menu Search
    if (this.menuSearchText) {
      const search = this.menuSearchText.toLowerCase();
      filtered = filtered.filter(row => 
        row.label.toLowerCase().includes(search)
      );
    }

    this.dataSource.data = filtered;
    this.totalCount = filtered.length;
  }

  filterMenus(): void {
    this.applyFilters();
  }

  clearMenuSearch(): void {
    this.menuSearchText = '';
    this.applyFilters();
  }

  resetFilters(): void {
    this.accessLevelFilter = 'all';
    this.statusFilter = 'all';
    this.menuSearchText = '';
    this.applyFilters();
  }

  getUserInitials(username: string): string {
    return username
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }

  getSelectedUsername(): string {
    const selectedUser = this.users.find(u => u.id === this.selectedUserId);
    return selectedUser ? selectedUser.username : '';
  }

  getAssignedMenuCount(): number {
    return this.allRows.filter(row => row.permissionId).length;
  }

  getRowStatus(row: MenuPermissionRow): string {
    if (!row.permissionId) return 'none';
    if (!row.isActive) return 'inactive';

    const now = new Date();
    const from = row.from ? new Date(row.from) : null;
    const to = row.to ? new Date(row.to) : null;

    // Debug logging
    console.log('getRowStatus for menu:', row.label);
    console.log('  permissionId:', row.permissionId);
    console.log('  isActive:', row.isActive);
    console.log('  now:', now);
    console.log('  from:', from);
    console.log('  to:', to);
    console.log('  now < from:', from && now < from);
    console.log('  now > to:', to && now > to);

    if (from && now < from) return 'inactive';
    if (to && now > to) return 'expired';

    return 'active';
  }

  getStatusClass(row: MenuPermissionRow): string {
    const status = this.getRowStatus(row);
    return `status-${status}`;
  }

  getStatusIcon(row: MenuPermissionRow): string {
    const status = this.getRowStatus(row);
    switch (status) {
      case 'active': return 'check_circle';
      case 'inactive': return 'cancel';
      case 'expired': return 'schedule';
      default: return 'block';
    }
  }

  getStatusText(row: MenuPermissionRow): string {
    const status = this.getRowStatus(row);
    switch (status) {
      case 'active': return 'Active';
      case 'inactive': return 'Inactive';
      case 'expired': return 'Expired';
      default: return 'Not Assigned';
    }
  }

  openAddPermissionDialog(): void {
    console.log('Opening add permission dialog for user:', this.selectedUserId);
    console.log('Current allMenus array:', this.allMenus);
    console.log('allMenus length:', this.allMenus.length);

    // If menus haven't loaded yet, wait for them
    if (this.allMenus.length === 0) {
      console.warn('Menus not loaded yet, loading now...');
      this.loadMenus();
      this.errorHandling.handleError('Please wait, loading menus...');
      return;
    }

    const dialogRef = this.dialog.open(PermissionDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      maxHeight: '90vh',
      data: {
        mode: 'add',
        userId: this.selectedUserId,
        allMenus: this.allMenus
      } as PermissionDialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.savePermission(result);
      }
    });
  }

  addPermissionForMenu(menu: MenuPermissionRow): void {
    console.log('Adding permission for menu:', menu.label);

    const dialogRef = this.dialog.open(PermissionDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      maxHeight: '90vh',
      data: {
        mode: 'add',
        userId: this.selectedUserId,
        menuId: menu.id,
        menuLabel: menu.label,
        allMenus: this.allMenus
      } as PermissionDialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.savePermission(result);
      }
    });
  }

  editPermission(row: MenuPermissionRow): void {
    console.log('Editing permission:', row.permissionId);

    const dialogRef = this.dialog.open(PermissionDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      maxHeight: '90vh',
      data: {
        mode: 'edit',
        userId: this.selectedUserId!,
        menuId: row.id,
        menuLabel: row.label,
        allMenus: this.allMenus,
        existingPermission: {
          id: row.permissionId!,
          menuId: row.id,
          accessLevel: row.accessLevel!,
          isActive: row.isActive!,
          from: row.from!,
          to: row.to!
        }
      } as PermissionDialogData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.updatePermission(result);
      }
    });
  }

  savePermission(data: any): void {
    const payload = {
      userId: data.userId,
      menuId: data.menuId,
      accessLevel: data.accessLevel,
      isActive: data.isActive,
      from: data.from,
      to: data.to
    };

    this.http.post(`${this.pathAPI}/menupermissions`, payload).pipe(
      catchError(err => {
        this.errorHandling.handleError(err);
        return of(null);
      })
    ).subscribe(result => {
      if (result !== null) {
        this.errorHandling.handleSuccess('Permission added successfully');
        this.loadUserPermissions();
      }
    });
  }

  updatePermission(data: any): void {
    const payload = {
      id: data.permissionId,
      userId: data.userId,
      menuId: data.menuId,
      accessLevel: data.accessLevel,
      isActive: data.isActive,
      from: data.from,
      to: data.to
    };

    this.http.put(`${this.pathAPI}/menupermissions/${data.permissionId}`, payload).pipe(
      catchError(err => {
        this.errorHandling.handleError(err);
        return of(null);
      })
    ).subscribe(result => {
      if (result !== null) {
        this.errorHandling.handleSuccess('Permission updated successfully');
        this.loadUserPermissions();
      }
    });
  }

  deletePermission(row: MenuPermissionRow): void {
    if (!confirm(`Are you sure you want to remove access to "${row.label}" for this user?`)) {
      return;
    }

    this.http.delete(`${this.pathAPI}/menupermissions/${row.permissionId}`).pipe(
      catchError(err => {
        this.errorHandling.handleError(err);
        return of(null);
      })
    ).subscribe(result => {
      if (result !== null) {
        this.errorHandling.handleSuccess('Permission removed successfully');
        this.loadUserPermissions();
      }
    });
  }
}
