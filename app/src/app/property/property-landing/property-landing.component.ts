import { Component, ViewChildren, QueryList, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { GetAllPropertiesServiceService, PropertyModel } from '../services/get-all-properties-service.service';
import { MatMenuTrigger, MatMenu } from '@angular/material/menu';
import { takeUntil, Subject } from 'rxjs';

@Component({
  selector: 'app-property-landing',
  templateUrl: './property-landing.component.html',
  styleUrls: ['./property-landing.component.css']
})
export class PropertyLandingComponent implements AfterViewInit, OnDestroy {
  @ViewChildren(MatMenuTrigger) megaMenuTriggerRefs!: QueryList<MatMenuTrigger>;
  @ViewChild('searchInput') searchInputRef!: ElementRef<HTMLInputElement>;

  searchQuery: string = '';
  hardcodedValues: string[] = [
    "Design", "Photos", "Videos", "Templates", "Presentations",
    "Social Media", "Marketing", "Documents", "Print Products", "Websites",
    "Education", "Personal", "Business", "Teams", "Free Stock Photos",
    "Free Videos", "Logos", "Flyers", "Posters", "Invitations", "Resumes",
    "Visual Suite", "Docs", "Whiteboards", "PDF editor", "Graphs and charts",
    "Sheets", "Video editor", "YouTube video editor", "Photo editor",
    "Photo collages", "Background remover", "Business cards", "Cards", "Mugs",
    "T-Shirts", "Hoodies", "Calendars", "Stickers", "Brochures",
    "mothers day" // Added from your image example
  ];
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

  navItems = [
    { label: 'Design spotlight', hasDropdown: true },
    { label: 'Business', hasDropdown: true },
    { label: 'Education', hasDropdown: true },
    { label: 'Plans and pricing', hasDropdown: true },
    { label: 'Learn', hasDropdown: true }
  ];

  constructor(
    private dropdownService: GetAllPropertiesServiceService,
  ) {}

  ngAfterViewInit() {
    this.megaMenuTriggerRefs.changes.pipe(takeUntil(this.destroy$)).subscribe(() => {
      // Logic if triggers change dynamically
    });
    document.addEventListener('click', this.onDocumentClick.bind(this));
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
    document.removeEventListener('click', this.onDocumentClick.bind(this));
  }

  onSearchInput(): void {
    const query = this.searchQuery.toLowerCase();
    if (query.length === 0) {
      this.filteredSuggestions = [];
      this.showSuggestionsList = false;
      this.activeSuggestionIndex = -1;
      return;
    }

    // Filter hardcoded values
    this.filteredSuggestions = this.hardcodedValues.filter(value =>
      value.toLowerCase().includes(query)
    );

    // Add the "Search for 'X'" option if the query isn't an exact match or part of any existing suggestion
    const isExactMatch = this.hardcodedValues.some(value => value.toLowerCase() === query);
    const isPartialMatch = this.filteredSuggestions.length > 0;

    // Only add "Search for" if the query is not an exact match of a hardcoded value,
    // and if there are either no filtered suggestions OR the query itself is unique enough.
    if (!isExactMatch && query.length > 0) {
      const searchForText = `Search for "${this.searchQuery}"`;
      // Prevent adding duplicate "Search for" if it's already there due to partial match logic
      if (!this.filteredSuggestions.includes(searchForText)) {
        // Decide where to put it. Usually at the end.
        this.filteredSuggestions.push(searchForText);
      }
    }


    this.showSuggestionsList = this.filteredSuggestions.length > 0;
    this.activeSuggestionIndex = -1;
  }

  onSearchFocus(): void {
    if (this.searchQuery.length > 0) {
      this.onSearchInput();
    }
  }

  selectSuggestion(suggestion: string): void {
    if (suggestion.startsWith('Search for "') && suggestion.endsWith('"')) {
      const actualQuery = suggestion.substring(12, suggestion.length - 1);
      this.searchQuery = actualQuery; // Set the input to the actual search term
      console.log('Performing a full search for:', actualQuery);
      // You would typically navigate to a search results page or trigger a full search here
      this.applyFilter(); // Call your existing filter method
    } else {
      this.searchQuery = suggestion;
      console.log('Selected suggestion:', suggestion);
      this.applyFilter();
    }
    this.filteredSuggestions = [];
    this.showSuggestionsList = false;
    this.activeSuggestionIndex = -1;
  }


  onKeyDown(event: KeyboardEvent): void {
    if (!this.showSuggestionsList || this.filteredSuggestions.length === 0) {
      return;
    }

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.activeSuggestionIndex = (this.activeSuggestionIndex + 1) % this.filteredSuggestions.length;
      this.scrollToActiveSuggestion();
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.activeSuggestionIndex = (this.activeSuggestionIndex - 1 + this.filteredSuggestions.length) % this.filteredSuggestions.length;
      this.scrollToActiveSuggestion();
    } else if (event.key === 'Enter') {
      event.preventDefault();
      if (this.activeSuggestionIndex > -1) {
        this.selectSuggestion(this.filteredSuggestions[this.activeSuggestionIndex]);
      } else {
        // If Enter is pressed without selecting a suggestion,
        // it means the user wants to search for the exact text they typed.
        this.selectSuggestion(`Search for "${this.searchQuery}"`); // Treat as a direct search
      }
    } else if (event.key === 'Escape') {
      this.showSuggestionsList = false;
      this.activeSuggestionIndex = -1;
    }
  }

  private scrollToActiveSuggestion(): void {
    const suggestionsListElement = document.getElementById('suggestions');
    if (suggestionsListElement && this.activeSuggestionIndex > -1) {
      const activeDiv = suggestionsListElement.children[this.activeSuggestionIndex] as HTMLElement;
      if (activeDiv) {
        activeDiv.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
      }
    }
    // Update input field with highlighted suggestion text for visual feedback
    if (this.activeSuggestionIndex > -1) {
        // Only update searchQuery if it's not the "Search for" text
        const selectedText = this.filteredSuggestions[this.activeSuggestionIndex];
        if (!selectedText.startsWith('Search for "')) {
            this.searchQuery = selectedText;
        }
    }
  }

  onDocumentClick(event: MouseEvent): void {
    const searchContainer = document.querySelector('.search-container-autocomplete');
    if (searchContainer && !searchContainer.contains(event.target as Node)) {
      this.showSuggestionsList = false;
      this.activeSuggestionIndex = -1;
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
}