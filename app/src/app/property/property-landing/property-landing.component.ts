import {
  Component, ViewChildren, QueryList, ViewChild, ElementRef,  AfterViewInit, OnDestroy, inject,OnInit
,ChangeDetectorRef } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatMenuTrigger } from '@angular/material/menu';
import { takeUntil, Subject, catchError, map, Observable } from 'rxjs';
import { APP_CONFIG, AppConfig } from '../../configuration/app.config.token';
import { HttpClient, HttpParams } from "@angular/common/http";
import { ErrorHandlingService } from '../../core/system/service/error-handling-service.service';
import { GetSearchResultsDTO, SearchBoxAutocompleteComponent } from '../../core/auth/search-box-autocomplete/search-box-autocomplete.component';
import { LOG_LOGOUT } from '../../common/Constants/Constants';
import { LoggingService } from '../../core/system/service/logging.service';
import { SystemMessagesSnackbarComponent } from '../../core/system/system-messages-snackbar/system-messages-snackbar.component';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/auth/services/auth.service';

@Component({
  selector: 'app-property-landing',
  imports: [
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    SearchBoxAutocompleteComponent,
    SystemMessagesSnackbarComponent,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './property-landing.component.html',
  styleUrls: ['./property-landing.component.css']
})
export class PropertyLandingComponent implements AfterViewInit, OnDestroy, OnInit {
  @ViewChildren(MatMenuTrigger) megaMenuTriggerRefs!: QueryList<MatMenuTrigger>;
  @ViewChild('searchInput') searchInputRef!: ElementRef<HTMLInputElement>;

  searchQuery: string = '';
  hardcodedValues: string[] = [];
  filteredSuggestions: string[] = [];
  showSuggestionsList: boolean = false;
  activeSuggestionIndex: number = -1;
  dropdownOptions: any[] = [];
  selectedCountry: string = '';
  private closeMenuTimeout: any;
  private openMenuTimeout: any;
  private destroy$ = new Subject<void>();

  activeMenuItem: any | null = null;
  private pathAPI: string;

  navItems: any[] = [];
  logoPath: string | null = null;

  private appConfig = inject<AppConfig>(APP_CONFIG);

  constructor(
    private router: Router,
    private http: HttpClient,
    private errorHandling: ErrorHandlingService,
    private loggingService: LoggingService,
    private cdr: ChangeDetectorRef,
    private authService: AuthService
  ) {
    this.pathAPI = this.appConfig.apiUrl;
  }

  ngOnInit(): void {
    console.log('🚀 PropertyLandingComponent initialized');
    console.log('📍 API URL:', this.pathAPI);

    this.getMenuItems().subscribe({
      next: (data) => {
        console.log('✅ Menu data received:', data);
        console.log('📋 Menu results:', data.results);

        this.navItems = data.results || [];

        if (this.navItems.length === 0) {
          console.warn('⚠️ WARNING: No menu items received from API!');
          console.warn('This could mean:');
          console.warn('1. User has no menu permissions');
          console.warn('2. Menu data is not in database');
          console.warn('3. API endpoint returned empty results');
        } else {
          console.log(`✅ Loaded ${this.navItems.length} menu items:`, this.navItems);
        }

        // Force the template to update immediately
        this.cdr.detectChanges(); //issue 21.1
      },
      error: (err) => {
        console.error('❌ ERROR: Failed to fetch menu items');
        console.error('Error details:', err);
        console.error('Status:', err.status);
        console.error('Message:', err.message);

        // Show empty menu rather than crashing
        this.navItems = [];
        this.cdr.detectChanges();
      }
    });
  }

  onSearchSelected(selectedResult: GetSearchResultsDTO) {
    this.searchQuery = selectedResult.label;
    this.router.navigateByUrl(selectedResult.path);
    this.applyFilter();
  }

  goToProfile() {
    // Change this to your real profile route
    this.router.navigate(['/profile']);
  }

  getMenuItems() {
    const userId = this.authService.getUserId();

    if (!userId) {
      console.error('User ID not found. User may not be logged in.');
      this.router.navigate(['/login']);
      return new Observable(observer => observer.complete());
    }

    const params = new HttpParams().set('userId', userId);

    return this.http.get<any>(this.pathAPI + '/menu/GetinitialData', { params }).pipe(
      map(response => {
        this.logoPath = response?.results?.[0]?.property?.companyLogoURL || '';
        return response;
      }),
      catchError((err) => this.errorHandling.handleError(err))
    );
  }

  ngAfterViewInit(): void {
     this.megaMenuTriggerRefs.changes.pipe(takeUntil(this.destroy$)).subscribe(() => {});
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();

    if (this.closeMenuTimeout) clearTimeout(this.closeMenuTimeout);
    if (this.openMenuTimeout) clearTimeout(this.openMenuTimeout);

    const menuPanelElement = document.querySelector('.mat-menu-panel');
    if (menuPanelElement) {
      menuPanelElement.removeEventListener('mouseenter', this.handleMenuPanelMouseEnter as EventListener);
      menuPanelElement.removeEventListener('mouseleave', this.handleMenuPanelMouseLeave as EventListener);
    }
  }

  applyFilter() {
    console.log('Filtering with:', this.searchQuery);
  }

  // --- Existing Menu Methods (keep yours) ---
  startOpenMenu(item: any, index: number) { /* ... existing code ... */ }
  startCloseMenu(item: any, index: number) { /* ... existing code ... */ }
  onMenuOpened() { /* ... existing code ... */ }
  onMenuClosed() { /* ... existing code ... */ }
  handleMenuPanelMouseLeave = () => { /* ... existing code ... */ }
  handleMenuPanelMouseEnter = () => { /* ... existing code ... */ }
  fetchDropdownOptions(): void { /* ... existing code ... */ }

  signOut() {
    this.loggingService.logPageNavigation(`loginSuccess`, LOG_LOGOUT, `User logged out successfully`);
    this.router.navigate(['/']);
  }

  changeProperties() {
    // Navigate back to property selection page
    this.router.navigate(['/propertySelector']);
  }
}
