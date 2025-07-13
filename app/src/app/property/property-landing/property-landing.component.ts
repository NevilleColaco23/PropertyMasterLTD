import { Component, ViewChildren, QueryList, ViewChild, ElementRef, AfterViewInit,OnDestroy } from '@angular/core';
import { GetAllPropertiesServiceService, PropertyModel } from '../services/get-all-properties-service.service';
import { MatMenuTrigger, MatMenu } from '@angular/material/menu';
import { takeUntil, Subject } from 'rxjs'; // For cleaning up subscriptions

@Component({
  selector: 'app-property-landing',
  templateUrl: './property-landing.component.html',
  styleUrls: ['./property-landing.component.css']
})

export class PropertyLandingComponent implements AfterViewInit, OnDestroy {
  @ViewChildren(MatMenuTrigger) megaMenuTriggerRefs!: QueryList<MatMenuTrigger>;

  searchTerm: string = '';
  dropdownOptions: any[] = [];
  selectedCountry: string = '';

  private closeMenuTimeout: any;
  private openMenuTimeout: any;
  private destroy$ = new Subject<void>();

  activeMenuItem: any | null = null;
  private currentOpenTrigger: MatMenuTrigger | null = null;
  private isMenuPanelHovered: boolean = false; // New state to track menu panel hover

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

applyFilter() {
    console.log('Filtering with:', this.searchTerm);
    // Logic to filter your data goes here
  }

  ngAfterViewInit() {
    // Attach mouseleave/mouseenter to the menu panel only when it's opened.
    // This is handled by onMenuOpened/onMenuClosed events, not here.
    // However, we need to ensure MatMenuTrigger instances are available.
    this.megaMenuTriggerRefs.changes.pipe(takeUntil(this.destroy$)).subscribe(() => {
      // Logic if triggers change dynamically, but not strictly needed for stable nav items
    });
  }

  // --- Menu Hover Management ---

  startOpenMenu(item: any, index: number) {
    const trigger = this.megaMenuTriggerRefs.toArray()[index];

    // Clear any pending close timeout
    if (this.closeMenuTimeout) {
      clearTimeout(this.closeMenuTimeout);
      this.closeMenuTimeout = null;
    }

    // If a different menu is open, close it immediately
    if (this.currentOpenTrigger && this.currentOpenTrigger !== trigger && this.currentOpenTrigger.menuOpen) {
      this.currentOpenTrigger.closeMenu();
      this.currentOpenTrigger = null; // Clear old trigger reference
    }

    // Only attempt to open if not already open and not currently in a pending open
    if (!trigger.menuOpen && this.currentOpenTrigger !== trigger) {
      if (this.openMenuTimeout) {
        clearTimeout(this.openMenuTimeout);
      }

      this.openMenuTimeout = setTimeout(() => {
        // Double-check if still not open (e.g., rapid mouse movements might have closed it again)
        if (!trigger.menuOpen) {
          this.activeMenuItem = item;
          this.currentOpenTrigger = trigger;
          trigger.openMenu();
        }
        this.openMenuTimeout = null; // Clear timeout after execution
      }, 150); // Small delay to prevent flickering on quick mouse-overs
    }
  }

  startCloseMenu(item: any, index: number) {
    const trigger = this.megaMenuTriggerRefs.toArray()[index];

    // Clear any pending open timeout if the mouse leaves before it opens
    if (this.openMenuTimeout) {
      clearTimeout(this.openMenuTimeout);
      this.openMenuTimeout = null;
    }

    // Only proceed to close if this is the currently active menu trigger
    if (this.currentOpenTrigger === trigger) {
      // Set a timeout to close the menu
      this.closeMenuTimeout = setTimeout(() => {
        // Only close if the menu panel is NOT hovered and the current trigger is still the one that opened it
        if (!this.isMenuPanelHovered && this.currentOpenTrigger === trigger && trigger.menuOpen) {
          trigger.closeMenu();
          this.activeMenuItem = null;
          this.currentOpenTrigger = null; // Clear reference to closed trigger
        }
        this.closeMenuTimeout = null; // Clear timeout after execution
      }, 250); // Slightly longer delay to allow mouse to enter the menu
    }
  }

  onMenuOpened() {
    // When the menu opens, clear any pending close timeouts (mouse entered menu)
    if (this.closeMenuTimeout) {
      clearTimeout(this.closeMenuTimeout);
      this.closeMenuTimeout = null;
    }

    // Find the actual menu panel element which is rendered in the overlay
    // Use a small timeout to ensure the menu panel is rendered in the DOM
    setTimeout(() => {
      const menuPanelElement = document.querySelector('.mat-menu-panel');
      if (menuPanelElement) {
        // Add event listeners directly to the menu panel
        menuPanelElement.addEventListener('mouseenter', this.handleMenuPanelMouseEnter as EventListener);
        menuPanelElement.addEventListener('mouseleave', this.handleMenuPanelMouseLeave as EventListener);
        this.isMenuPanelHovered = true; // Assume it's hovered when opened
      }
    }, 50);
  }

  onMenuClosed() {
    this.activeMenuItem = null;
    this.currentOpenTrigger = null; // Ensure this is nullified when menu closes

    // Remove event listeners from the menu panel to prevent memory leaks
    const menuPanelElement = document.querySelector('.mat-menu-panel');
    if (menuPanelElement) {
      menuPanelElement.removeEventListener('mouseenter', this.handleMenuPanelMouseEnter as EventListener);
      menuPanelElement.removeEventListener('mouseleave', this.handleMenuPanelMouseLeave as EventListener);
    }
    this.isMenuPanelHovered = false; // Reset the hover state
  }

  // Use arrow functions for handlers to maintain 'this' context
  handleMenuPanelMouseLeave = () => {
    this.isMenuPanelHovered = false;
    // If the menu panel is no longer hovered, and no trigger is hovered, close the menu
    // We re-trigger a close attempt based on the original logic
    if (this.currentOpenTrigger && this.currentOpenTrigger.menuOpen) {
      this.closeMenuTimeout = setTimeout(() => {
        const isAnyTriggerHovered = this.megaMenuTriggerRefs.some(t => {
          const triggerElement = (t as any)._elementRef?.nativeElement;
          return triggerElement && triggerElement.matches(':hover');
        });

        if (!isAnyTriggerHovered && !this.isMenuPanelHovered && this.currentOpenTrigger && this.currentOpenTrigger.menuOpen) {
          this.currentOpenTrigger.closeMenu();
          this.activeMenuItem = null;
          this.currentOpenTrigger = null;
        }
        this.closeMenuTimeout = null;
      }, 100); // Small delay before closing after leaving the panel
    }
  }

  handleMenuPanelMouseEnter = () => {
    this.isMenuPanelHovered = true;
    // When mouse enters the menu panel, clear any pending close timeouts
    if (this.closeMenuTimeout) {
      clearTimeout(this.closeMenuTimeout);
      this.closeMenuTimeout = null;
    }
  }

  // --- Other Methods ---

  fetchDropdownOptions(): void {
    console.log('Fetching dropdown options...');
    this.dropdownService.getDropdownOptions().pipe(takeUntil(this.destroy$)).subscribe(
      (options: PropertyModel[]) => {
        console.log('Dropdown options fetched:', options);
        this.dropdownOptions = options;
      },
      (error: any) => {
        console.error('Error fetching dropdown options', error);
      }
    );
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
    // Ensure all event listeners are removed
    const menuPanelElement = document.querySelector('.mat-menu-panel');
    if (menuPanelElement) {
      menuPanelElement.removeEventListener('mouseenter', this.handleMenuPanelMouseEnter as EventListener);
      menuPanelElement.removeEventListener('mouseleave', this.handleMenuPanelMouseLeave as EventListener);
    }
  }
}