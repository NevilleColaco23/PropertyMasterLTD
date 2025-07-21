import { Component, ViewChildren, QueryList, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { GetAllPropertiesServiceService, PropertyModel } from '../services/get-all-properties-service.service';
import { MatMenuTrigger, MatMenu } from '@angular/material/menu';
import { takeUntil, Subject, catchError, map } from 'rxjs';
import { Router } from '@angular/router';
import { AppConfig } from '../../Appconfig';
import { HttpClient } from "@angular/common/http";
import { ErrorHandlingCommonServiceService } from '../../Common/Services/error-handling-common-service.service';
import { GetSearchResultsDTO } from '../../core/search-box-autocomplete/search-box-autocomplete.component';

@Component({
  selector: 'app-property-landing',
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
  private currentOpenTrigger: MatMenuTrigger | null = null;
  private isMenuPanelHovered: boolean = false;
  private pathAPI : string;

  navItems: any[] = [];

  constructor(
    private router: Router, private config: AppConfig
    ,private http: HttpClient, private errorHandling: ErrorHandlingCommonServiceService
  ) {this.pathAPI = this.config.setting['PathAPI'];}

onSearchSelected(selectedResult: GetSearchResultsDTO) {
  this.searchQuery = selectedResult.label; // Or selectedResult.path, depending on your filter's needs
  console.log('Search result selected from child:', selectedResult);

  // Handle navigation here in the parent
  this.router.navigateByUrl(selectedResult.path);

  this.applyFilter(); // Your existing filter logic
}

getMenuItems() {
  console.log('Fetching dropdown options from API:', this.pathAPI + 'v1/Menu');

  return this.http.get<any>(this.pathAPI + 'v1/menu/GetListByUserId').pipe(
    map(response => {
      console.log('Raw API response:', response.results);
      return response; // or map your data
    }),
    catchError((err) => this.errorHandling.handleError(err))
  );
}
 
ngAfterViewInit() {
    this.megaMenuTriggerRefs.changes.pipe(takeUntil(this.destroy$)).subscribe(() => {
      // Logic if triggers change dynamically
    });

     this.getMenuItems().subscribe({
    next: (data) => {
      this.navItems = data.results || [];  // <-- dynamically assign to navItems
      console.log('Menu data:', this.navItems);
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
    this.router.navigate(['/']);
  }
}

