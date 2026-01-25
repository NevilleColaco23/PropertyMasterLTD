import { Component, ViewChildren, QueryList, ViewChild, ElementRef, AfterViewInit, OnDestroy, Inject } from '@angular/core';
import { Router, RouterOutlet, RouterLink } from '@angular/router';
import { MatMenuTrigger } from '@angular/material/menu';
import { takeUntil, Subject, catchError, map } from 'rxjs';
import { APP_CONFIG, AppConfig } from '../../app.config.token';
import { HttpClient, HttpParams } from "@angular/common/http";
import { ErrorHandlingService } from '../../core/system-messages-snackbar/service/error-handling-service.service';
import { GetSearchResultsDTO } from '../../core/search-box-autocomplete/search-box-autocomplete.component';
import { LOG_LOGOUT } from '../../Common/Constants/Constants';
import { LoggingService } from '../../core/auth/services/logging.service';
import { SearchBoxAutocompleteComponent } from '../../core/search-box-autocomplete/search-box-autocomplete.component';
import { SystemMessagesSnackbarComponent } from '../../core/system-messages-snackbar/system-messages-snackbar.component';
import { MatButtonModule } from '@angular/material/button';


@Component({
  selector: 'app-property-landing',
  imports: [RouterLink, RouterOutlet, SearchBoxAutocompleteComponent, SystemMessagesSnackbarComponent, MatButtonModule],
  templateUrl: './property-landing.component.html',
  styleUrls: ['./property-landing.component.css']
})
export class PropertyLandingComponent implements AfterViewInit, OnDestroy {
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
  private pathAPI : string;
  navItems: any[] = [];
  logoPath: string | null = null;

  constructor(
    private router: Router,private http: HttpClient, private errorHandling: ErrorHandlingService, private loggingService: LoggingService
    , @Inject(APP_CONFIG) private appConfig: AppConfig
  ) { this.pathAPI = this.appConfig.apiUrl; }

onSearchSelected(selectedResult: GetSearchResultsDTO) {
  this.searchQuery = selectedResult.label;

  this.router.navigateByUrl(selectedResult.path);

  this.applyFilter();
}

getMenuItems() {
 let params = new HttpParams().set('userId', 10); //TODO : to remove. get userid from token on server

  return this.http.get<any>(this.pathAPI + 'v1/menu/GetinitialData', { params }).pipe(
    map(response => {
      this.logoPath = response.results[0]?.property?.companyLogoURL || '';
      return response;
    }),   
    catchError((err) => this.errorHandling.handleError(err))
  );
}
 
ngAfterViewInit() {
    this.megaMenuTriggerRefs.changes.pipe(takeUntil(this.destroy$)).subscribe(() => {
    });

     this.getMenuItems().subscribe({
    next: (data) => {
      this.navItems = data.results || [];
    },
    error: (err) => console.error('Error occurred while fetching menu items:', err)
  });

  this.megaMenuTriggerRefs.changes.pipe(takeUntil(this.destroy$)).subscribe(() => {});
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.closeMenuTimeout) {
      clearTimeout(this.closeMenuTimeout);
    }
    if (this.openMenuTimeout) {
      clearTimeout(this.openMenuTimeout);
    }
    const menuPanelElement = document.querySelector('.mat-menu-panel');
    if (menuPanelElement) {
      menuPanelElement.removeEventListener('mouseenter', this.handleMenuPanelMouseEnter as EventListener);
      menuPanelElement.removeEventListener('mouseleave', this.handleMenuPanelMouseLeave as EventListener);
    }
  }


  applyFilter() {
    console.log('Filtering with:', this.searchQuery);
    // This is where you would trigger a proper search/filter on your data
    // based on `this.searchQuery`.
    // For a real application, this would typically involve:
    // 1. Calling a service to fetch/filter data.
    // 2. Updating a list of results displayed on the page.
    // 3. Potentially navigating to a search results page.
  }

  // --- Existing Menu Methods (Unchanged) ---
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
}

