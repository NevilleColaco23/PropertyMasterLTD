import { Component, ViewChildren, QueryList, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { GetAllPropertiesServiceService, PropertyModel } from '../services/get-all-properties-service.service';
import { MatMenuTrigger, MatMenu } from '@angular/material/menu'; // Keep if you're using MatMenu for other parts
import { takeUntil, Subject } from 'rxjs'; // For cleaning up subscriptions

@Component({
  selector: 'app-property-landing',
  templateUrl: './property-landing.component.html',
  styleUrls: ['./property-landing.component.css']
})
export class PropertyLandingComponent implements AfterViewInit, OnDestroy {
  @ViewChildren(MatMenuTrigger) megaMenuTriggerRefs!: QueryList<MatMenuTrigger>;
  @ViewChild('searchInput') searchInputRef!: ElementRef<HTMLInputElement>; // Reference to the search input

  // --- Autocomplete Properties ---
  searchQuery: string = '';
  hardcodedValues: string[] = [
    "Design", "Photos", "Videos", "Templates", "Presentations",
    "Social Media", "Marketing", "Documents", "Print Products", "Websites",
    "Education", "Personal", "Business", "Teams", "Free Stock Photos",
    "Free Videos", "Logos", "Flyers", "Posters", "Invitations", "Resumes",
    "Visual Suite", "Docs", "Whiteboards", "PDF editor", "Graphs and charts",
    "Sheets", "Video editor", "YouTube video editor", "Photo editor",
    "Photo collages", "Background remover", "Business cards", "Cards", "Mugs",
    "T-Shirts", "Hoodies", "Calendars", "Stickers", "Brochures"
  ];
  filteredSuggestions: string[] = [];
  showSuggestionsList: boolean = false;
  activeSuggestionIndex: number = -1;

  // --- Existing Properties ---
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
    private dropdownService: GetAllPropertiesServiceService, // Assuming this is still used for other dropdowns
  ) {}

  ngAfterViewInit() {
    this.megaMenuTriggerRefs.changes.pipe(takeUntil(this.destroy$)).subscribe(() => {
      // Logic if triggers change dynamically
    });

    // Handle clicks outside the search bar to close suggestions
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
    document.removeEventListener('click', this.onDocumentClick.bind(this)); // Clean up global listener
  }

  // --- Autocomplete Methods ---

  onSearchInput(): void {
    const query = this.searchQuery.toLowerCase();
    if (query.length === 0) {
      this.filteredSuggestions = [];
      this.showSuggestionsList = false;
      this.activeSuggestionIndex = -1;
      return;
    }

    this.filteredSuggestions = this.hardcodedValues.filter(value =>
      value.toLowerCase().includes(query)
    );
    this.showSuggestionsList = this.filteredSuggestions.length > 0;
    this.activeSuggestionIndex = -1; // Reset active index on new input
  }

  onSearchFocus(): void {
    // Show suggestions again if the input is focused and has content
    if (this.searchQuery.length > 0) {
      this.onSearchInput(); // Re-filter and show if needed
    }
  }

  selectSuggestion(suggestion: string): void {
    this.searchQuery = suggestion;
    this.filteredSuggestions = [];
    this.showSuggestionsList = false;
    this.activeSuggestionIndex = -1;
    // Optionally, trigger an actual search or navigation here
    console.log('Selected suggestion:', suggestion);
    this.applyFilter(); // Call your existing filter method
  }

  onKeyDown(event: KeyboardEvent): void {
    if (!this.showSuggestionsList || this.filteredSuggestions.length === 0) {
      return; // No suggestions to navigate
    }

    if (event.key === 'ArrowDown') {
      event.preventDefault(); // Prevent cursor movement
      this.activeSuggestionIndex = (this.activeSuggestionIndex + 1) % this.filteredSuggestions.length;
      this.scrollToActiveSuggestion();
    } else if (event.key === 'ArrowUp') {
      event.preventDefault(); // Prevent cursor movement
      this.activeSuggestionIndex = (this.activeSuggestionIndex - 1 + this.filteredSuggestions.length) % this.filteredSuggestions.length;
      this.scrollToActiveSuggestion();
    } else if (event.key === 'Enter') {
      event.preventDefault(); // Prevent form submission
      if (this.activeSuggestionIndex > -1) {
        this.selectSuggestion(this.filteredSuggestions[this.activeSuggestionIndex]);
      } else {
        // If Enter is pressed without selecting a suggestion,
        // you might want to perform a direct search for the current input value.
        console.log('Searching for:', this.searchQuery);
        this.showSuggestionsList = false;
        this.applyFilter(); // Call your existing filter method
      }
    } else if (event.key === 'Escape') {
      this.showSuggestionsList = false;
      this.activeSuggestionIndex = -1;
    }
  }

  private scrollToActiveSuggestion(): void {
    // This is a bit tricky without direct template reference to suggestion divs.
    // A simple approach is to find the .suggestions-list and scroll its content.
    // If you need more precise scrolling, consider using @ViewChildren for the suggestion divs.
    const suggestionsListElement = document.getElementById('suggestions');
    if (suggestionsListElement && this.activeSuggestionIndex > -1) {
      const activeDiv = suggestionsListElement.children[this.activeSuggestionIndex] as HTMLElement;
      if (activeDiv) {
        activeDiv.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
      }
    }
    // Update input field with highlighted suggestion text for visual feedback
    if (this.activeSuggestionIndex > -1) {
      this.searchQuery = this.filteredSuggestions[this.activeSuggestionIndex];
    }
  }

  onDocumentClick(event: MouseEvent): void {
    // Check if the click occurred outside the search container
    const searchContainer = document.querySelector('.search-container-autocomplete');
    if (searchContainer && !searchContainer.contains(event.target as Node)) {
      this.showSuggestionsList = false;
      this.activeSuggestionIndex = -1;
    }
  }

  // --- Existing Methods ---
  applyFilter() {
    console.log('Filtering with:', this.searchQuery);
    // Logic to filter your data goes here, using this.searchQuery
    // You would typically call a service to filter your data based on this.searchQuery
  }

  startOpenMenu(item: any, index: number) { /* ... existing code ... */ }
  startCloseMenu(item: any, index: number) { /* ... existing code ... */ }
  onMenuOpened() { /* ... existing code ... */ }
  onMenuClosed() { /* ... existing code ... */ }
  handleMenuPanelMouseLeave = () => { /* ... existing code ... */ }
  handleMenuPanelMouseEnter = () => { /* ... existing code ... */ }
  fetchDropdownOptions(): void { /* ... existing code ... */ }
}